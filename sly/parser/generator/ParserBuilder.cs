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
    public class ParserBuilder
    {
        #region API

        /// <summary>
        /// Builds a parser (lexer, syntax parser and syntax tree visitor) according to a parser definition instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parserInstance"> a parser definition instance , containing 
        /// [Reduction] methods for grammar rules 
        /// [LexerConfigurationAttribute] method for token definition</param>
        /// <param name="parserType"></param>
        /// <param name="rootRule"></param>
        /// <returns></returns>
        public static Parser<T> BuildParser<T>(object parserInstance, ParserType parserType, string rootRule)
        {
            ParserConfiguration<T> configuration = ExtractParserConfiguration<T>(parserInstance.GetType(),rootRule);
            ISyntaxParser<T> syntaxParser = BuildSyntaxParser<T>(configuration, parserType, rootRule);
            ConcreteSyntaxTreeVisitor<T> visitor = new ConcreteSyntaxTreeVisitor<T>(configuration, parserInstance);
            Parser<T> parser = new Parser<T>(syntaxParser, visitor);
            parser.Lexer = BuildLexer<T>(parserInstance.GetType(),parserInstance);
            parser.Instance = parserInstance;
            return parser;
        }

        private static ISyntaxParser<T> BuildSyntaxParser<T>(ParserConfiguration<T> conf, ParserType parserType, string rootRule)
        {
            ISyntaxParser<T> parser = null;
            switch (parserType)
            {
                case ParserType.LL_RECURSIVE_DESCENT:
                    {
                        parser = new RecursiveDescentSyntaxParser<T>(conf, rootRule);
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


        static private Lexer<T> BuildLexer<T>(Type parserClass, object parserInstance = null)
        {
            TypeInfo typeInfo = parserClass.GetTypeInfo();
            Lexer<T> lexer = null;
            List < MethodInfo > methods = typeInfo.DeclaredMethods.ToList<MethodInfo>();
            methods = methods.Where(m =>
            {
                List<Attribute> attributes = m.GetCustomAttributes().ToList<Attribute>().ToList<Attribute>();
                Attribute attr = attributes.Find(a => a.GetType() == typeof(LexerConfigurationAttribute));
                return attr != null;
            }).ToList<MethodInfo>();
            if (methods.Count > 0)
            {
                MethodInfo lexerConfigurerMethod = methods[0];
                lexer = new Lexer<T>();
                object res = lexerConfigurerMethod.Invoke(parserInstance, new object[] { lexer });
            }
            return lexer;
        }

       

        static private ParserConfiguration<T> ExtractParserConfiguration<T>(Type parserClass, string rootRule)
        {
            ParserConfiguration<T> conf = new ParserConfiguration<T>();
            Dictionary<string, MethodInfo> functions = new Dictionary<string, MethodInfo>();
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

                    functions[key] = m;

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
                try
                {
                    token = (T)Enum.Parse(typeof(T) , item, true);
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

       

        #endregion



    }
}