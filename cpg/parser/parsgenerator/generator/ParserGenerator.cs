using System;
using System.Linq;
using System.Reflection;
using parser.parsergenerator.parser;
using System.Collections.Generic;
using cpg.parser.parsgenerator.generator;

using static cpg.parser.parsgenerator.generator.Functions;
using parser.parsergenerator.syntax;

namespace parser.parsergenerator.generator
{

    public enum ParserType {
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


        public static ISyntaxParser<T> BuildSyntaxParser<T>(Type parserClass, ParserType parserType, string rootRule)
        {
            
            return null;
        }

        public static object BuildParser<T>(Type parserClass, ParserType parserType, string rootRule) {
            ParserConfiguration<T> configuration = ExtractParserInformation<T>(parserClass);
            ISyntaxParser<T> syntaxParser = BuildSyntaxParser<T>(parserClass,parserType,rootRule);                        
            // todo build visitor
            // todo build wrapper arround visitor and syntaxparser
            return null;
        }


        static Tuple<string,string> ExtractNTAndRule(string ruleString)
        {
            Tuple<string, string> result = null;
            string nt = "";
            string rule = "";
            int i = ruleString.IndexOf(":");
            if (i > 0)
            {
                nt = ruleString.Substring(0, i);
                rule = ruleString.Substring(i + 1);
                result = new Tuple<string, string>(nt, rule);
            }
            
            return result;
        }


        static private ParserConfiguration<T> ExtractParserInformation<T>(Type parserClass)
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
                ReductionAttribute attr = (ReductionAttribute)m.GetCustomAttributes(typeof(ReductionAttribute), true)[0];
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



            });
            conf.Functions = functions;
            conf.NonTerminals = nonTerminals;
            return conf;
        }

        static private Rule<T> BuildNonTerminal<T>(Tuple<string, string> ntAndRule)
        {
            Rule<T> rule = new Rule<T>(); 
            
            List<Clause<T>> clauses = new List<Clause<T>>();
            string ruleString = ntAndRule.Item2;
            string[] clausesString = ruleString.Split(new char[] { ' ' },StringSplitOptions.RemoveEmptyEntries);
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

           
            return rule;
        }

    }
}