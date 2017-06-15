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
    internal class EBNFParserBuilder<T> : ParserBuilder
    {


        private Parser<EbnfToken> GrammarParser;

        public EBNFParserBuilder()
        {

        }


        public virtual Parser<T> BuildParser(object parserInstance, ParserType parserType, string rootRule)
        {

            ParserBuilder builder = new ParserBuilder();
            EBNFParserBuilder<T> parserGrammar = new EBNFParserBuilder<T>();
            if (GrammarParser == null)
            {
                GrammarParser = builder.BuildParser<EbnfToken>(parserGrammar, ParserType.LL_RECURSIVE_DESCENT, "rule");
            }
            ParserConfiguration<T> configuration =
                ExtractEbnfParserConfiguration(parserInstance.GetType(), GrammarParser);

            ISyntaxParser<T> syntaxParser = BuildSyntaxParser<T>(configuration, parserType, rootRule);

            SyntaxTreeVisitor<T> visitor = new SyntaxTreeVisitor<T>(configuration, parserInstance);
            if (parserType == ParserType.EBNF_LL_RECURSIVE_DESCENT)
            {
                visitor = new EBNFSyntaxTreeVisitor<T>(configuration, parserInstance);
            }
            Parser<T> parser = new Parser<T>(syntaxParser, visitor);
            parser.Configuration = configuration;
            parser.Lexer = BuildLexer<T>(parserInstance.GetType(), parserInstance);
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
                "[A-Za-z0-9_авйиклоф][A-Za-z0-9_авйиклоф]*"));
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.COLON, ":"));
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.WS, "[ \\t]+", true));
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.EOL, "[\\n\\r]+", true, true));
            return lexer;
        }




        protected override ISyntaxParser<T> BuildSyntaxParser<T>(ParserConfiguration<T> conf, ParserType parserType,
            string rootRule)
        {
            ISyntaxParser<T> parser = null;
            switch (parserType)
            {
                case ParserType.LL_RECURSIVE_DESCENT:
                    {
                        parser = (ISyntaxParser<T>)(new RecursiveDescentSyntaxParser<T>(conf, rootRule));
                        break;
                    }
                case ParserType.EBNF_LL_RECURSIVE_DESCENT:
                {
                    parser = (ISyntaxParser<T>)(new EBNFRecursiveDescentSyntaxParser<T>(conf, rootRule));
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

        protected virtual ParserConfiguration<T> ExtractEbnfParserConfiguration(Type parserClass,
            Parser<EbnfToken> grammarParser)
        {
            ParserConfiguration<T> conf = new ParserConfiguration<T>();
            Dictionary<string, MethodInfo> functions = new Dictionary<string, MethodInfo>();
            Dictionary<string, NonTerminal<T>> nonTerminals = new Dictionary<string, NonTerminal<T>>();
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
                    ParseResult<EbnfToken> parseResult = grammarParser.Parse(ruleString);
                    if (!parseResult.IsError)
                    {
                        Rule<T> rule = (Rule<T>)parseResult.Result;

                        
                        
                        functions[rule.NonTerminalName+"__"+rule.Key] = m;


                        NonTerminal<T> nonT = null;
                        if (!nonTerminals.ContainsKey(rule.NonTerminalName))
                        {
                            nonT = new NonTerminal<T>(rule.NonTerminalName, new List<Rule<T>>());
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
        public object Root(Token<EbnfToken> name, Token<EbnfToken> discarded, List<IClause<T>> clauses)
        {
            Rule<T> rule = new Rule<T>();
            rule.NonTerminalName = name.Value;
            rule.Clauses = clauses;
            return rule;
        }


        [Production("clauses : clause clauses")]

        public object Clauses(IClause<T> clause, List<IClause<T>> clauses)
        {
            List<IClause<T>> list = new List<IClause<T>> { clause };
            if (clauses != null)
            {
                list.AddRange(clauses);
            }
            return list;
        }

        [Production("clauses : clause ")]
        public object SingleClause(IClause<T> clause)
        {
            return new List<IClause<T>> { clause };
        }



       

        [Production("clause : IDENTIFIER ZEROORMORE")]
        public IClause<T> ZeroMoreClause(Token<EbnfToken> id, Token<EbnfToken> discarded)
        {
            IClause<T> innerClause = BuildTerminalOrNonTerimal(id.Value);
            return new ZeroOrMoreClause<T>(innerClause);
        }

        [Production("clause : IDENTIFIER ONEORMORE")]
        public IClause<T> OneMoreClause(Token<EbnfToken> id, Token<EbnfToken> discarded)
        {
            IClause<T> innerClause = BuildTerminalOrNonTerimal(id.Value);
            return new OneOrMoreClause<T>(innerClause);
        }

        [Production("clause : IDENTIFIER ")]
        public IClause<T> SimpleClause(Token<EbnfToken> id)
        {
            IClause<T> clause = BuildTerminalOrNonTerimal(id.Value);
            return clause;
        }

        private IClause<T> BuildTerminalOrNonTerimal(string name)
        {

            T token = default(T);
            IClause<T> clause;
            bool isTerminal = false;
            try
            {
                token = (T)Enum.Parse(typeof(T), name, false);
                isTerminal = true;
            }
            catch
            {
                isTerminal = false;
            }
            if (isTerminal)
            {
                clause = new TerminalClause<T>(token);
            }
            else
            {
                clause = new NonTerminalClause<T>(name);
            }
            return clause;
        }

        #endregion

    }


}