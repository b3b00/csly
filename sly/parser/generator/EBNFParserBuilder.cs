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


namespace sly.parser.generator
{


    /// <summary>
    /// this class provides API to build parser
    /// </summary>
    internal class EBNFParserBuilder<IN,OUT> : ParserBuilder
    {


        private Parser<EbnfToken,GrammarNode<IN>> GrammarParser;

        public EBNFParserBuilder()
        {

        }


        public virtual Parser<IN,OUT> BuildParser(object parserInstance, ParserType parserType, string rootRule)
        {

            ParserBuilder builder = new ParserBuilder();
            EBNFParserBuilder<EbnfToken,GrammarNode<IN>> parserGrammar = new EBNFParserBuilder<EbnfToken,GrammarNode<IN>>();
            if (GrammarParser == null)
            {
                GrammarParser = builder.BuildParser<EbnfToken,GrammarNode<IN>>(parserGrammar, ParserType.LL_RECURSIVE_DESCENT, "rule");
            }
            ParserConfiguration<IN,OUT> configuration =
                ExtractEbnfParserConfiguration(parserInstance.GetType(), GrammarParser);

            ISyntaxParser<IN> syntaxParser = BuildSyntaxParser<IN,OUT>(configuration, parserType, rootRule);

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
            parser.Lexer = BuildLexer<IN>(parserInstance.GetType(), parserInstance);
            parser.Instance = parserInstance;
            return parser;
        }

        [LexerConfiguration]
        public ILexer<EbnfToken> BuildEbnfLexer(ILexer<EbnfToken> lexer)
        {
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.COLON, ":"));
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.ONEORMORE, "\\+"));
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.ZEROORMORE, "\\*"));
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.IDENTIFIER,
                "[A-Za-z0-9_��������][A-Za-z0-9_��������]*"));
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.COLON, ":"));
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.WS, "[ \\t]+", true));
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.EOL, "[\\n\\r]+", true, true));
            return lexer;
        }




        protected override ISyntaxParser<IN> BuildSyntaxParser<IN,OUT>(ParserConfiguration<IN,OUT> conf, ParserType parserType,
            string rootRule)
        {
            ISyntaxParser<IN> parser = null;
            switch (parserType)
            {
                case ParserType.LL_RECURSIVE_DESCENT:
                    {
                        parser = (ISyntaxParser<IN>)(new RecursiveDescentSyntaxParser<IN,OUT>(conf, rootRule));
                        break;
                    }
                case ParserType.EBNF_LL_RECURSIVE_DESCENT:
                {
                    parser = (ISyntaxParser<IN>)(new EBNFRecursiveDescentSyntaxParser<IN,OUT>(conf, rootRule));
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
            Dictionary<string, MethodInfo> functions = new Dictionary<string, MethodInfo>();
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
                        functions[rule.NonTerminalName+"__"+rule.Key] = m;


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



            conf.Functions = functions;
            conf.NonTerminals = nonTerminals;

            return conf;
        }

        #endregion

        #region rules grammar

        [Production("rule : IDENTIFIER COLON clauses")]
        public object Root(Token<EbnfToken> name, Token<EbnfToken> discarded, List<IClause<IN>> clauses)
        {
            Rule<IN> rule = new Rule<IN>();
            rule.NonTerminalName = name.Value;
            rule.Clauses = clauses;
            return rule;
        }


        [Production("clauses : clause clauses")]

        public object Clauses(IClause<IN> clause, List<IClause<IN>> clauses)
        {
            List<IClause<IN>> list = new List<IClause<IN>> { clause };
            if (clauses != null)
            {
                list.AddRange(clauses);
            }
            return list;
        }

        [Production("clauses : clause ")]
        public object SingleClause(IClause<IN> clause)
        {
            return new List<IClause<IN>> { clause };
        }



       

        [Production("clause : IDENTIFIER ZEROORMORE")]
        public IClause<IN> ZeroMoreClause(Token<EbnfToken> id, Token<EbnfToken> discarded)
        {
            IClause<IN> innerClause = BuildTerminalOrNonTerimal(id.Value);
            return new ZeroOrMoreClause<IN>(innerClause);
        }

        [Production("clause : IDENTIFIER ONEORMORE")]
        public IClause<IN> OneMoreClause(Token<EbnfToken> id, Token<EbnfToken> discarded)
        {
            IClause<IN> innerClause = BuildTerminalOrNonTerimal(id.Value);
            return new OneOrMoreClause<IN>(innerClause);
        }

        [Production("clause : IDENTIFIER ")]
        public IClause<IN> SimpleClause(Token<EbnfToken> id)
        {
            IClause<IN> clause = BuildTerminalOrNonTerimal(id.Value);
            return clause;
        }

        private IClause<IN> BuildTerminalOrNonTerimal(string name)
        {

            IN token = default(IN);
            IClause<IN> clause;
            bool isTerminal = false;
            try
            {
                token = (IN)Enum.Parse(typeof(IN), name, false);
                isTerminal = true;
            }
            catch
            {
                isTerminal = false;
            }
            if (isTerminal)
            {
                clause = new TerminalClause<IN>(token);
            }
            else
            {
                clause = new NonTerminalClause<IN>(name);
            }
            return clause;
        }

        #endregion

    }


}