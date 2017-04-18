using System;
using System.Linq;
using System.Reflection;
using parser.parsergenerator.parser;
using System.Collections.Generic;
using cpg.parser.parsgenerator.generator;

using static cpg.parser.parsgenerator.generator.Functions;
using parser.parsergenerator.syntax;
using cpg.parser.parsgenerator.parser.llparser;
using cpg.parser.parsgenerator.parser;

namespace parser.parsergenerator.generator
{

    public enum ParserType
    {
        LL = 1,
        LR = 2,

        COMBINATOR = 3
    }


    public class ParserConfiguration<T>
    {
        public Dictionary<string, ReductionFunction> Functions { get; set; }
        public Dictionary<string, NonTerminal<T>> NonTerminals { get; set; }
    }

    public class ParserGenerator
    {

        #region API
        public static ISyntaxParser<T> BuildSyntaxParser<T>(ParserConfiguration<T> conf, ParserType parserType, string rootRule)
        {
            ISyntaxParser<T> parser = null;
            switch (parserType)
            {
                case ParserType.LL:
                    {
                        parser = new LLSyntaxParser<T>(conf, rootRule);
                        break;
                    }
                default:
                    {
                        parser = null;
                        break;
                    }
            }
            return parser;
        }

        public static Parser<T> BuildParser<T>(Type parserClass, ParserType parserType, string rootRule)
        {
            ParserConfiguration<T> configuration = ExtractParserConfiguration<T>(parserClass,rootRule);
            ISyntaxParser<T> syntaxParser = BuildSyntaxParser<T>(configuration, parserType, rootRule);
            ConcreteSyntaxTreeVisitor<T> visitor = new ConcreteSyntaxTreeVisitor<T>(configuration);
            Parser<T> parser = new Parser<T>(syntaxParser, visitor);
            return parser;
        }

        #endregion

        #region CONFIGURATION

        static Tuple<string, string> ExtractNTAndRule(string ruleString)
        {
            Tuple<string, string> result = null;
            string nt = "";
            string rule = "";
            int i = ruleString.IndexOf(":");
            if (i > 0)
            {
                nt = ruleString.Substring(0, i).Trim();
                rule = ruleString.Substring(i + 1);
                result = new Tuple<string, string>(nt, rule);
            }

            return result;
        }


        static private ParserConfiguration<T> ExtractParserConfiguration<T>(Type parserClass, string rootRule)
        {
            ParserConfiguration<T> conf = new ParserConfiguration<T>();
            Dictionary<string, Functions.ReductionFunction> functions = new Dictionary<string, ReductionFunction>();
            Dictionary<string, NonTerminal<T>> nonTerminals = new Dictionary<string, NonTerminal<T>>();
            List<MethodInfo> methods = parserClass.GetMethods().ToList<MethodInfo>();
            methods = methods.Where(m =>
            {
                List<Attribute> attributes = m.GetCustomAttributes().ToList<Attribute>().ToList<Attribute>();
                Attribute attr = attributes.Find(a => a.GetType() == typeof(ReductionAttribute));
                return attr != null;
            }).ToList<MethodInfo>();

            parserClass.GetMethods();
            methods.ForEach(m =>
            {
                ReductionAttribute[] attributes = (ReductionAttribute[])m.GetCustomAttributes(typeof(ReductionAttribute), true);

                foreach (ReductionAttribute attr in attributes)
                {
                    Tuple<string, string> ntAndRule = ExtractNTAndRule(attr.RuleString);
                    string key = ntAndRule.Item1 + "_" + ntAndRule.Item2.Replace(" ", "_");
                    var delegMethod = Delegate.CreateDelegate(typeof(Functions.ReductionFunction), m);
                    functions[key] = delegMethod as ReductionFunction;

                    Rule<T> r = BuildNonTerminal<T>(ntAndRule);
                    NonTerminal<T> nonT = null;
                    if (!nonTerminals.ContainsKey(ntAndRule.Item1))
                    {
                        nonT = new NonTerminal<T>(ntAndRule.Item1, new List<Rule<T>>());
                    }
                    else
                    {
                        nonT = nonTerminals[ntAndRule.Item1];
                    }
                    nonT.Rules.Add(r);
                    nonTerminals[ntAndRule.Item1] = nonT;
                }



            });

           

            conf.Functions = functions;
            conf.NonTerminals = nonTerminals;

            InitializeStartingTokens(conf, rootRule);
            return conf;
        }

        static private Rule<T> BuildNonTerminal<T>(Tuple<string, string> ntAndRule)
        {
            Rule<T> rule = new Rule<T>();

            List<Clause<T>> clauses = new List<Clause<T>>();
            string ruleString = ntAndRule.Item2;
            string[] clausesString = ruleString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in clausesString)
            {
                Clause<T> clause = null;
                bool isTerminal = false;
                T token = default(T);
                //T token = Enum.Parse(typeof(T), item, out isTerminal);
                try
                {
                    token = (T)Enum.Parse(typeof(T), item);
                    isTerminal = true;
                }
                catch (Exception e)
                {
                    isTerminal = false;
                }
                if (isTerminal)
                {
                    clause = new TerminalClause<T>(token);
                }
                else
                {
                    clause = new NonTerminalClause<T>(item);
                }
                if (clause != null)
                {
                    clauses.Add(clause);
                }
            }
            rule.Clauses = clauses;
            rule.Key = ntAndRule.Item1 + "_" + ntAndRule.Item2.Replace(" ", "_");

            return rule;
        }

        static private void InitializeStartingTokens<T>(ParserConfiguration<T> configuration, string root)
        {
            

            Dictionary<string, NonTerminal<T>> nts = configuration.NonTerminals;


            InitStartingTokensForNT(nts, root);
        }

        static private void InitStartingTokensForNT<T>(Dictionary<string, NonTerminal<T>> nonTerminals, string name)
        {
            if (nonTerminals.ContainsKey(name))
            {
                NonTerminal<T> nt = nonTerminals[name];
                nt.Rules.ForEach(r => InitStartingTokensForRule<T>(nonTerminals, r));
             }
            return;
        }

        static private void InitStartingTokensForRule<T>(Dictionary<string, NonTerminal<T>> nonTerminals, Rule<T> rule)
        {
           if (rule.PossibleLeadingTokens == null || rule.PossibleLeadingTokens.Count == 0)
            {
                rule.PossibleLeadingTokens = new List<T>();
                Clause<T> first = rule.Clauses[0];
                if (first is TerminalClause<T>)
                {
                    TerminalClause<T> term = first as TerminalClause<T>;
                    rule.PossibleLeadingTokens.Add(term.ExpectedToken);
                }
                else
                {
                    NonTerminalClause<T> nonterm = first as NonTerminalClause<T>;
                    InitStartingTokensForNT<T>(nonTerminals, nonterm.NonTerminalName);
                    NonTerminal<T> firstNonTerminal = nonTerminals[nonterm.NonTerminalName];
                    firstNonTerminal.Rules.ForEach(r =>
                    {
                        rule.PossibleLeadingTokens.AddRange(r.PossibleLeadingTokens);
                    });
                }
                
            }
        }

        #endregion



    }
}