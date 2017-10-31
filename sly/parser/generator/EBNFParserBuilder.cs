using sly.lexer;

using sly.parser.generator;
using System.Reflection;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Principal;
using System.Xml;
using sly.parser.syntax;
using sly.parser.llparser;
using sly.buildresult;

namespace sly.parser.generator
{


    /// <summary>
    /// this class provides API to build parser
    /// </summary>
    internal class EBNFParserBuilder<IN,OUT> : ParserBuilder<IN,OUT> where IN :struct
    {

        public EBNFParserBuilder()
        {

        }


        public override BuildResult<Parser<IN,OUT>> BuildParser(object parserInstance, ParserType parserType, string rootRule)
        {
            
            RuleParser<IN> ruleparser = new RuleParser<IN>();
            ParserBuilder<EbnfToken, GrammarNode<IN>> builder = new ParserBuilder<EbnfToken, GrammarNode<IN>>();

            Parser<EbnfToken,GrammarNode<IN>> grammarParser = builder.BuildParser(ruleparser, ParserType.LL_RECURSIVE_DESCENT, "rule").Result;

            BuildResult<Parser<IN, OUT>> result = new BuildResult<Parser<IN, OUT>>();

            ParserConfiguration<IN,OUT> configuration =
                ExtractEbnfParserConfiguration(parserInstance.GetType(), grammarParser);
            configuration.StartingRule = rootRule;

            ISyntaxParser<IN, OUT> syntaxParser = BuildSyntaxParser(configuration, parserType, rootRule);

            SyntaxTreeVisitor<IN, OUT> visitor = null;
            if (parserType == ParserType.LL_RECURSIVE_DESCENT)
            {
                new SyntaxTreeVisitor<IN, OUT>(configuration, parserInstance);
            }
            else if (parserType == ParserType.EBNF_LL_RECURSIVE_DESCENT)
            {
                visitor = new EBNFSyntaxTreeVisitor<IN,OUT>(configuration, parserInstance);
            }
            Parser<IN,OUT> parser = new Parser<IN,OUT>(syntaxParser, visitor);
            parser.Configuration = configuration;
            var lexerResult = BuildLexer();
            if (lexerResult.IsError)
            {
                result.AddErrors(lexerResult.Errors);
            }
            else
            {
                parser.Lexer = lexerResult.Result;
            }
            parser.Instance = parserInstance;
            result.Result = parser;
            return result;
        }



        protected override ISyntaxParser<IN,OUT> BuildSyntaxParser(ParserConfiguration<IN,OUT> conf, ParserType parserType,
            string rootRule)
        {
            ISyntaxParser<IN,OUT> parser = null;
            switch (parserType)
            {
                case ParserType.LL_RECURSIVE_DESCENT:
                    {
                        parser = (ISyntaxParser<IN, OUT>)(new RecursiveDescentSyntaxParser<IN,OUT>(conf, rootRule));
                        break;
                    }
                case ParserType.EBNF_LL_RECURSIVE_DESCENT:
                {
                    parser = (ISyntaxParser<IN, OUT>)(new EBNFRecursiveDescentSyntaxParser<IN,OUT>(conf, rootRule));
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


        #region configuration

        protected virtual ParserConfiguration<IN,OUT> ExtractEbnfParserConfiguration(Type parserClass,
            Parser<EbnfToken,GrammarNode<IN>> grammarParser)
        {
            ParserConfiguration<IN,OUT> conf = new ParserConfiguration<IN,OUT>();
            Dictionary<string, NonTerminal<IN>> nonTerminals = new Dictionary<string, NonTerminal<IN>>();
            List<MethodInfo> methods = parserClass.GetMethods().ToList<MethodInfo>();
            methods = methods.Where(m =>
            {
                List<Attribute> attributes = m.GetCustomAttributes().ToList<Attribute>();
                Attribute attr = attributes.Find(a => a.GetType() == typeof(ProductionAttribute));
                return attr != null;
            }).ToList<MethodInfo>();

            methods.ForEach(m =>
            {
                ProductionAttribute[] attributes =
                    (ProductionAttribute[])m.GetCustomAttributes(typeof(ProductionAttribute), true);

                foreach (ProductionAttribute attr in attributes)
                {

                    string ruleString = attr.RuleString;
                    ParseResult<EbnfToken,GrammarNode<IN>> parseResult = grammarParser.Parse(ruleString);
                    if (!parseResult.IsError)
                    {
                        Rule<IN> rule = (Rule<IN>)parseResult.Result;
                        rule.SetVisitor(m);
                        NonTerminal<IN> nonT = null;
                        if (!nonTerminals.ContainsKey(rule.NonTerminalName))
                        {
                            nonT = new NonTerminal<IN>(rule.NonTerminalName, new List<Rule<IN>>());
                        }
                        else
                        {
                            nonT = nonTerminals[rule.NonTerminalName];
                        }
                        nonT.Rules.Add(rule);
                        nonTerminals[rule.NonTerminalName] = nonT;
                    }
                    else
                    {
                        string message = parseResult
                            .Errors
                            .Select(e => e.ErrorMessage)
                            .Aggregate<string>((e1, e2) => e1 + "\n" + e2);
                        throw new ParserConfigurationException(message);
                    }
                }



            });

            conf.NonTerminals = nonTerminals;

            return conf;
        }

        #endregion

      

    }


}