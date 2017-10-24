using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using sly.parser.llparser;
using sly.lexer;
using sly.parser.syntax;

namespace sly.parser.generator
{

   


  
    /// <summary>
    /// this class provides API to build parser
    /// </summary>
    public class ParserBuilder<IN,OUT> where IN : struct
    {
        #region API

        /// <summary>
        /// Builds a parser (lexer, syntax parser and syntax tree visitor) according to a parser definition instance
        /// </summary>
        /// <typeparam name="IN"></typeparam>
        /// <param name="parserInstance"> a parser definition instance , containing 
        /// [Reduction] methods for grammar rules 
        /// <param name="parserType">a ParserType enum value stating the analyser type (LR, LL ...) for now only LL recurive descent parser available </param>
        /// <param name="rootRule">the name of the root non terminal of the grammar</param>
        /// <returns></returns>
        public virtual Parser<IN,OUT> BuildParser(object parserInstance, ParserType parserType, string rootRule)
        {
            Parser<IN,OUT> parser = null;
            if (parserType == ParserType.LL_RECURSIVE_DESCENT)
            {
                ParserConfiguration<IN,OUT> configuration = ExtractParserConfiguration(parserInstance.GetType());
                ISyntaxParser<IN,OUT> syntaxParser = BuildSyntaxParser(configuration, parserType, rootRule);
                SyntaxTreeVisitor<IN,OUT> visitor = new SyntaxTreeVisitor<IN,OUT>(configuration, parserInstance);
                parser = new Parser<IN,OUT>(syntaxParser, visitor);
                parser.Lexer = BuildLexer();
                parser.Instance = parserInstance;
                parser.Configuration = configuration;
            }
            else if (parserType == ParserType.EBNF_LL_RECURSIVE_DESCENT)
            {
                EBNFParserBuilder<IN,OUT> builder = new EBNFParserBuilder<IN,OUT>();
                parser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, rootRule);
            }
            return parser;
        }

        protected virtual  ISyntaxParser<IN,OUT> BuildSyntaxParser(ParserConfiguration<IN,OUT> conf, ParserType parserType, string rootRule)
        {
            ISyntaxParser<IN,OUT> parser = null;
            switch (parserType)
            {
                case ParserType.LL_RECURSIVE_DESCENT:
                    {
                        parser = new RecursiveDescentSyntaxParser<IN,OUT>(conf, rootRule);
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

        #endregion

       



        #region CONFIGURATION



        private Tuple<string, string> ExtractNTAndRule(string ruleString)
        {
            Tuple<string, string> result = null;
            if (ruleString != null)
            {
                string nt = "";
                string rule = "";
                int i = ruleString.IndexOf(":");
                if (i > 0)
                {
                    nt = ruleString.Substring(0, i).Trim();
                    rule = ruleString.Substring(i + 1);
                    result = new Tuple<string, string>(nt, rule);
                }

            }
            return result;
        }


        protected ILexer<IN> BuildLexer() 
        {
            ILexer<IN> lexer = LexerBuilder.BuildLexer<IN>();            
            return lexer;
        }

       

        protected virtual  ParserConfiguration<IN,OUT> ExtractParserConfiguration(Type parserClass)
        {
            ParserConfiguration<IN,OUT> conf = new ParserConfiguration<IN,OUT>();
            Dictionary<string, MethodInfo> functions = new Dictionary<string, MethodInfo>();
            Dictionary<string, NonTerminal<IN>> nonTerminals = new Dictionary<string, NonTerminal<IN>>();
            List<MethodInfo> methods = parserClass.GetMethods().ToList<MethodInfo>();
            methods = methods.Where(m =>
            {
                List<Attribute> attributes = m.GetCustomAttributes().ToList<Attribute>();
                Attribute attr = attributes.Find(a => a.GetType() == typeof(ProductionAttribute));
                return attr != null;
            }).ToList<MethodInfo>();

            parserClass.GetMethods();
            methods.ForEach(m =>
            {
                ProductionAttribute[] attributes = (ProductionAttribute[])m.GetCustomAttributes(typeof(ProductionAttribute), true);

                foreach (ProductionAttribute attr in attributes)
                {
                    Tuple<string, string> ntAndRule = ExtractNTAndRule(attr.RuleString);
                    

                    

                    Rule<IN> r = BuildNonTerminal(ntAndRule);
                    string key = ntAndRule.Item1 + "__" + r.Key;
                    functions[key] = m;
                    NonTerminal<IN> nonT = null;
                    if (!nonTerminals.ContainsKey(ntAndRule.Item1))
                    {
                        nonT = new NonTerminal<IN>(ntAndRule.Item1, new List<Rule<IN>>());
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
            
            return conf;
        }

        private Rule<IN> BuildNonTerminal(Tuple<string, string> ntAndRule) 
        {
            Rule<IN> rule = new Rule<IN>();

            List<IClause<IN>> clauses = new List<IClause<IN>>();
            string ruleString = ntAndRule.Item2;
            string[] clausesString = ruleString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in clausesString)
            {
                IClause<IN> clause = null;
                bool isTerminal = false;
                IN token = default(IN);                              
                try
                {
                    token = (IN)Enum.Parse(typeof(IN) , item, false);
                    isTerminal = true;
                }
                catch(Exception e) 
                {
                    isTerminal = false;
                }
                if (isTerminal)
                {
                    clause = new TerminalClause<IN>(token);
                }
                else
                {
                    clause = new NonTerminalClause<IN>(item);
                }
                if (clause != null)
                {
                    clauses.Add(clause);
                }
            }
            rule.Clauses = clauses;
            //rule.Key = ntAndRule.Item1 + "_" + ntAndRule.Item2.Replace(" ", "_");

            return rule;
        }

       

        #endregion



    }
}