using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using sly.parser.llparser;
using sly.lexer;
using sly.parser.syntax;
using sly.buildresult;

namespace sly.parser.generator
{



    public delegate BuildResult<Parser<IN, OUT>> ParserChecker<IN,OUT>(BuildResult<Parser<IN, OUT>> result, NonTerminal<IN> nonterminal) where IN : struct;
  
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
        public virtual BuildResult<Parser<IN, OUT>> BuildParser(object parserInstance, ParserType parserType, string rootRule)
        {

            Parser<IN, OUT> parser = null;
            BuildResult<Parser<IN, OUT>> result = new BuildResult<Parser<IN, OUT>>();
            if (parserType == ParserType.LL_RECURSIVE_DESCENT)
            {
                ParserConfiguration<IN, OUT> configuration = ExtractParserConfiguration(parserInstance.GetType());
                ISyntaxParser<IN, OUT> syntaxParser = BuildSyntaxParser(configuration, parserType, rootRule);
                SyntaxTreeVisitor<IN, OUT> visitor = new SyntaxTreeVisitor<IN, OUT>(configuration, parserInstance);
                parser = new Parser<IN, OUT>(syntaxParser, visitor);
                var lexerResult = BuildLexer();
                parser.Lexer = lexerResult.Result;
                if (lexerResult.IsError)
                {
                    result.AddErrors(lexerResult.Errors);
                }
                parser.Instance = parserInstance;
                parser.Configuration = configuration;
                result.Result = parser;
            }
            else if (parserType == ParserType.EBNF_LL_RECURSIVE_DESCENT)
            {
                EBNFParserBuilder<IN, OUT> builder = new EBNFParserBuilder<IN, OUT>();
                result = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, rootRule);
            }
            parser = result.Result;
            var expressionResult = parser.BuildExpressionParser(result, rootRule);
            if (expressionResult.IsError)
            {
                result.AddErrors(expressionResult.Errors);
            }
            result.Result.Configuration = expressionResult.Result;

            result = CheckParser(result);

            return result;
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


        protected BuildResult<ILexer<IN>> BuildLexer() 
        {
            var lexer = LexerBuilder.BuildLexer<IN>(new BuildResult<ILexer<IN>>());            
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
                    r.SetVisitor(m);
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
                    var tIn = typeof(IN);
                    bool b = Enum.TryParse<IN>(item, out token);
                    if (b)
                    {
                        isTerminal = true;
                        ;
                    }

                    //token = (IN)Enum.Parse(tIn , item);
                    //isTerminal = true;
                }
                catch(ArgumentException) 
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

        #region parser checking
        private BuildResult<Parser<IN, OUT>> CheckParser(BuildResult<Parser<IN, OUT>> result)
        {
            var checkers = new List<ParserChecker<IN, OUT>>();
            checkers.Add(CheckUnreachable);
            checkers.Add(CheckNotFound);

            if (result.Result != null && !result.IsError)
            {
                foreach (var checker in checkers)
                {
                    if (checker != null)
                    {
                        result.Result.Configuration.NonTerminals.Values.ToList<NonTerminal<IN>>()
                            .ForEach(nt => result = checker(result, nt));
                    }
                }
                //ParserChecker<IN, OUT> c = CheckUnreachable;
                

                // WARN : unreachable non terminals


                // ERROR / FATAL ?: unknown non term / term
            }
            return result;
        }

        private static BuildResult<Parser<IN,OUT>> CheckUnreachable(BuildResult<Parser<IN,OUT>> result, NonTerminal<IN> nonTerminal)
        {
            var conf = result.Result.Configuration;
            bool found = false;
            foreach(var nt in result.Result.Configuration.NonTerminals.Values.ToList<NonTerminal<IN>>())
            {
                if (nt.Name != nonTerminal.Name)
                {
                    found = NonTerminalReferences(nt,nonTerminal.Name);
                    if (found)
                    {
                        break;
                    }
                }
            }
            if (!found)
            {
                result.AddError(new ParserInitializationError(ErrorLevel.WARN, $"non terminal {nonTerminal.Name} is never used."));
            }
            return result;
        }


        private static bool NonTerminalReferences(NonTerminal<IN> nonTerminal, string referenceName)
        {
            bool found = false;
            int iRule = 0;
            while (iRule < nonTerminal.Rules.Count && !found)
            {
                var rule = nonTerminal.Rules[iRule];
                int iClause = 0;
                while (iClause < rule.Clauses.Count && !found)
                {
                    var clause = rule.Clauses[iClause];
                    found = found || clause is NonTerminalClause<IN> ntClause && ntClause.NonTerminalName == nonTerminal.Name;
                    iClause++;
                }
                iRule++;
            }
            return found;
        }

        

        private static BuildResult<Parser<IN, OUT>> CheckNotFound(BuildResult<Parser<IN, OUT>> result, NonTerminal<IN> nonTerminal)
        {
            var conf = result.Result.Configuration;
            foreach(var rule in nonTerminal.Rules)
            {
                foreach(var clause in rule.Clauses)
                {
                    if (clause is NonTerminalClause<IN> ntClause)
                    {
                        if (!conf.NonTerminals.ContainsKey(ntClause.NonTerminalName))
                        {
                            result.AddError(new ParserInitializationError(ErrorLevel.ERROR, $"{ntClause.NonTerminalName} references from {rule.RuleString} does not exist."));
                        }
                    }                    
                }
            }
            return result;
        }



        #endregion

    }
}