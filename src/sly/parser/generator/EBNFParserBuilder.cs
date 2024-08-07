using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sly.buildresult;
using sly.i18n;
using sly.lexer;
using sly.lexer.fsm;
using sly.parser.generator.visitor;
using sly.parser.llparser.bnf;
using sly.parser.llparser.ebnf;
using sly.parser.syntax.grammar;

namespace sly.parser.generator
{
    /// <summary>
    ///     this class provides API to build parser
    /// </summary>
    internal class EBNFParserBuilder<IN, OUT> : ParserBuilder<IN, OUT> where IN : struct
    {
        public EBNFParserBuilder(string i18n = null) : base(i18n)
        {
        }

        public override BuildResult<Parser<IN, OUT>> BuildParser(object parserInstance, ParserType parserType,
            string rootRule = null, Action<IN, LexemeAttribute, GenericLexer<IN>> extensionBuilder = null,
            LexerPostProcess<IN> lexerPostProcess = null)
        {
            if (string.IsNullOrEmpty(rootRule))
            {
                var rootAttribute = parserInstance.GetType().GetCustomAttribute<ParserRootAttribute>();
                if (rootAttribute != null)
                {
                    rootRule = rootAttribute.RootRule;
                }
            }

            bool useMemoization = parserInstance.GetType().GetCustomAttribute<UseMemoizationAttribute>() != null;
            
            bool broadenTokenWindow = parserInstance.GetType().GetCustomAttribute<BroadenTokenWindowAttribute>() != null;

            bool autoCloseIndentations = parserInstance.GetType().GetCustomAttribute<AutoCloseIndentationsAttribute>() != null;

            var ruleparser = new RuleParser<IN,OUT>();

            //using AOT
            var b = new AotRuleParser<IN, OUT>();
            var grammarParser = b.BuildParser(this.I18N);
            // var builder = new ParserBuilder<EbnfTokenGeneric, GrammarNode<IN,OUT>>(I18N);
            //
            // var grammarParser = builder.BuildParser(ruleparser, ParserType.LL_RECURSIVE_DESCENT, "rule").Result;


            var result = new BuildResult<Parser<IN, OUT>>();

            ParserConfiguration<IN, OUT> configuration = null;
    
            try
            {
                configuration = ExtractEbnfParserConfiguration(parserInstance.GetType(), grammarParser);
                configuration.UseMemoization = useMemoization;
                configuration.BroadenTokenWindow = broadenTokenWindow;
                configuration.AutoCloseIndentations = autoCloseIndentations;
                LeftRecursionChecker<IN, OUT> recursionChecker = new LeftRecursionChecker<IN, OUT>();

                // check left recursion.
                var (foundRecursion, recursions) = LeftRecursionChecker<IN, OUT>.CheckLeftRecursion(configuration);
                if (foundRecursion)
                {
                    var recs = string.Join("\n", recursions.Select<List<string>, string>(x => string.Join(" > ", x)));
                    result.AddError(new ParserInitializationError(ErrorLevel.FATAL,
                        i18n.I18N.Instance.GetText(I18N, I18NMessage.LeftRecursion, recs),
                        ErrorCodes.PARSER_LEFT_RECURSIVE));
                    return result;
                }

                configuration.StartingRule = rootRule;
            }
            catch (Exception e)
            {
                result.AddError(new ParserInitializationError(ErrorLevel.ERROR, e.Message,
                    ErrorCodes.PARSER_UNKNOWN_ERROR));
                return result;
            }

            var syntaxParser = BuildSyntaxParser(configuration, parserType, rootRule);

            SyntaxTreeVisitor<IN, OUT> visitor = null;
            visitor = new EBNFSyntaxTreeVisitor<IN, OUT>(configuration, parserInstance);
            var parser = new Parser<IN, OUT>(I18N, syntaxParser, visitor);
            parser.Configuration = configuration;
            var lexerResult = BuildLexer(extensionBuilder, lexerPostProcess,
                configuration.GetAllExplicitTokenClauses().Select(x => x.ExplicitToken).Distinct().ToList());
            if (lexerResult.IsError)
            {
                foreach (var lexerResultError in lexerResult.Errors)
                {
                    result.AddError(lexerResultError);
                }

                return result;
            }
            else
            {
                parser.Lexer = lexerResult.Result;
                parser.Instance = parserInstance;
                result.Result = parser;
                return result;
            }
        }


        internal override ISyntaxParser<IN, OUT> BuildSyntaxParser(ParserConfiguration<IN, OUT> conf,
            ParserType parserType,
            string rootRule)
        {
            ISyntaxParser<IN, OUT> parser = null;
            switch (parserType)
            {
                case ParserType.LL_RECURSIVE_DESCENT:
                {
                    parser = new RecursiveDescentSyntaxParser<IN, OUT>(conf, rootRule, I18N);
                    break;
                }
                case ParserType.EBNF_LL_RECURSIVE_DESCENT:
                {
                    parser = new EBNFRecursiveDescentSyntaxParser<IN, OUT>(conf, rootRule, I18N);
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

        protected virtual ParserConfiguration<IN, OUT> ExtractEbnfParserConfiguration(Type parserClass,
            Parser<EbnfTokenGeneric, GrammarNode<IN,OUT>> grammarParser)
        {
            var conf = new ParserConfiguration<IN, OUT>();
            var nonTerminals = new Dictionary<string, NonTerminal<IN,OUT>>();
            var methods = parserClass.GetMethods().ToList<MethodInfo>();
            methods = methods.Where<MethodInfo>(m =>
            {
                var attributes = m.GetCustomAttributes().ToList<Attribute>();
                var attr = attributes.Find(a => a.GetType() == typeof(ProductionAttribute));
                return attr != null;
            }).ToList<MethodInfo>();

            methods.ForEach(m =>
            {
                var attributes =
                    (ProductionAttribute[])m.GetCustomAttributes(typeof(ProductionAttribute), true);

                var nodeNames = (NodeNameAttribute[])m.GetCustomAttributes(typeof(NodeNameAttribute), true);
                string nodeName = nodeNames != null && nodeNames.Any() ? nodeNames[0].Name : null;
               
                foreach (var attr in attributes)
                {
                    var ruleString = attr.RuleString;
                    var parseResult = grammarParser.Parse(ruleString);
                    if (!parseResult.IsError)
                    {
                        var rule = (Rule<IN,OUT>)parseResult.Result;
                        rule.NodeName = nodeName;
                        rule.RuleString = ruleString;
                        rule.SetVisitor(m);
                        NonTerminal<IN,OUT> nonT = null;
                        if (!nonTerminals.ContainsKey(rule.NonTerminalName))
                            nonT = new NonTerminal<IN,OUT>(rule.NonTerminalName, new List<Rule<IN,OUT>>());
                        else
                            nonT = nonTerminals[rule.NonTerminalName];
                        nonT.Rules.Add(rule);
                        nonTerminals[rule.NonTerminalName] = nonT;
                    }
                    else
                    {
                        var message = parseResult
                            .Errors
                            .Select<ParseError, string>(e => e.ErrorMessage)
                            .Aggregate<string>((e1, e2) => e1 + "\n" + e2);
                        message = $"rule error [{ruleString}] : {message}";
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