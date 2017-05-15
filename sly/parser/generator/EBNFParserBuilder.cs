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
    public class EBNFParserBuilder<T> : ParserBuilder
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
                ExtractEbnfParserConfiguration(parserInstance.GetType().GetTypeInfo(), GrammarParser);

            ISyntaxParser<T> syntaxParser = BuildSyntaxParser<T>(configuration, parserType, rootRule);

            SyntaxTreeVisitor<T> visitor = new SyntaxTreeVisitor<T>(configuration, parserInstance);
            Parser<T> parser = new Parser<T>(syntaxParser, visitor);
            parser.Lexer = BuildLexer<T>(parserInstance.GetType(), parserInstance);
            parser.Instance = parserInstance;
            return parser;
        }

        [LexerConfigurationAttribute]
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




        protected virtual ISyntaxParser<T> BuildSyntaxParser(ParserConfiguration<T> conf, ParserType parserType,
            string rootRule)
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


        #region configuration

        protected virtual ParserConfiguration<T> ExtractEbnfParserConfiguration(TypeInfo parserClass,
            Parser<EbnfToken> grammarParser)
        {
            ParserConfiguration<T> conf = new ParserConfiguration<T>();
            Dictionary<string, MethodInfo> functions = new Dictionary<string, MethodInfo>();
            Dictionary<string, NonTerminal<T>> nonTerminals = new Dictionary<string, NonTerminal<T>>();
            List<MethodInfo> methods = parserClass.GetMethods().ToList<MethodInfo>();
            methods = methods.Where(m =>
            {
                List<Attribute> attributes = m.GetCustomAttributes().ToList<Attribute>();
                Attribute attr = attributes.Find(a => a.GetType() == typeof(ReductionAttribute));
                return attr != null;
            }).ToList<MethodInfo>();

            methods.ForEach(m =>
            {
                ReductionAttribute[] attributes =
                    (ReductionAttribute[])m.GetCustomAttributes(typeof(ReductionAttribute), true);

                foreach (ReductionAttribute attr in attributes)
                {

                    string ruleString = attr.RuleString;
                    ParseResult<EbnfToken> parseResult = grammarParser.Parse(ruleString);
                    if (!parseResult.IsError)
                    {
                        Rule<T> rule = (Rule<T>)parseResult.Result;

                        functions[rule.Key] = m;


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

        [Reduction("rule : IDENTIFIER COLON clauses")]
        public object Root(Token<EbnfToken> name, Token<EbnfToken> discarded, List<IClause<T>> clauses)
        {
            Rule<T> rule = new Rule<T>();
            rule.NonTerminalName = name.Value;
            rule.Clauses = clauses;
            return rule;
        }


        [Reduction("clauses : clause clauses")]

        public object Clauses(IClause<T> clause, List<IClause<T>> clauses)
        {
            List<IClause<T>> list = new List<IClause<T>> { clause };
            if (clauses != null)
            {
                list.AddRange(clauses);
            }
            return list;
        }

        [Reduction("clauses : clause ")]
        public object SingleClause(IClause<T> clause)
        {
            return new List<IClause<T>> { clause };
        }



       

        [Reduction("clause : IDENTIFIER ZEROORMORE")]
        public IClause<T> ZeroMoreClause(Token<EbnfToken> id, Token<EbnfToken> discarded)
        {
            IClause<T> innerClause = BuildTerminalOrNonTerimal(id.Value);
            return new ZeroOrMoreClause<T>(innerClause);
        }

        [Reduction("clause : IDENTIFIER ONEORMORE")]
        public IClause<T> OneMoreClause(Token<EbnfToken> id, Token<EbnfToken> discarded)
        {
            IClause<T> innerClause = BuildTerminalOrNonTerimal(id.Value);
            return new OneOrMoreClause<T>(innerClause);
        }

        [Reduction("clause : IDENTIFIER ")]
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