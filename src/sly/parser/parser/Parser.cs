
using System;
using System.Collections.Generic;
using System.Linq;
using sly.buildresult;
using sly.lexer;
using sly.parser.generator;
using sly.parser.generator.visitor;
using sly.parser.parser;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser
{
    public class Parser<IN, OUT> where IN : struct
    {   
        public Action<ISyntaxNode<IN>> SyntaxParseCallback { get; set; }

        public Dictionary<IN, Dictionary<string, string>> LexemeLabels => Lexer.LexemeLabels;

        public Parser(string i18n, ISyntaxParser<IN, OUT> syntaxParser, SyntaxTreeVisitor<IN, OUT> visitor)
        {
            I18n = i18n;
            SyntaxParser = syntaxParser;
            Visitor = visitor;
        }

        public string I18n { get; set; }
        
        public ILexer<IN> Lexer { get; set; }
        public object Instance { get; set; }
        public ISyntaxParser<IN, OUT> SyntaxParser { get; set; }
        public SyntaxTreeVisitor<IN, OUT> Visitor { get; set; }
        public ParserConfiguration<IN, OUT> Configuration { get; set; }


        #region expression generator

        public virtual BuildResult<ParserConfiguration<IN, OUT>> BuildExpressionParser(
            BuildResult<Parser<IN, OUT>> result, string startingRule = null)
        {
            var exprResult = new BuildResult<ParserConfiguration<IN, OUT>>(Configuration);
            var expressionGenerator = new ExpressionRulesGenerator<IN, OUT>(I18n);
            exprResult = expressionGenerator.BuildExpressionRules(Configuration, Instance.GetType(), exprResult);
            Configuration = exprResult.Result;
            SyntaxParser.Init(exprResult.Result, startingRule);
            if (startingRule != null)
            {
                Configuration.StartingRule = startingRule;
                SyntaxParser.StartingNonTerminal = startingRule;
            }

            if (exprResult.IsError)
                result.AddErrors(exprResult.Errors);
            else
                result.Result.Configuration = Configuration;
            return exprResult;
        }

        #endregion



        public ParseResult<IN, OUT> Parse(string source, string startingNonTerminal = null)
        {
            return ParseWithContext(source,new NoContext(),startingNonTerminal);
        }


        public ParseResult<IN, OUT> ParseWithContext(string source, object context, string startingNonTerminal = null)
        {
            ParseResult<IN, OUT> result = null;
            var lexingResult = Lexer.Tokenize(source);
            if (lexingResult.IsError)
            {
                result = new ParseResult<IN, OUT>();
                result.IsError = true;
                result.Errors = new List<ParseError>();
                result.Errors.Add(lexingResult.Error);
                return result;
            }

            var tokens = lexingResult.Tokens.Tokens;
            if (Lexer.LexerPostProcess != null)
            {
                tokens = Lexer.LexerPostProcess(tokens);
            }
            var position = 0;
            var tokensWithoutComments = new List<Token<IN>>();
            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                if (!token.IsComment || token.Notignored)
                {
                    token.PositionInTokenVisibleFlow = position;
                    tokensWithoutComments.Add(token);
                    position++;
                }
            }

            result = ParseWithContext(tokensWithoutComments, context, startingNonTerminal);


            return result;
        }




        public ParseResult<IN, OUT> ParseWithContext(IList<Token<IN>> tokens, object parsingContext = null, string startingNonTerminal = null)
        {
            var result = new ParseResult<IN, OUT>();

            var cleaner = new SyntaxTreeCleaner<IN>();
            var syntaxResult = SyntaxParser.Parse(tokens, startingNonTerminal);
            syntaxResult.UsesOperations = Configuration.UsesOperations;
            syntaxResult = cleaner.CleanSyntaxTree(syntaxResult);
            if (!syntaxResult.IsError && syntaxResult.Root != null)
            {
                if (SyntaxParseCallback != null)
                {
                    SyntaxParseCallback(syntaxResult.Root);
                }
                var r = Visitor.VisitSyntaxTree(syntaxResult.Root,parsingContext);
                result.Result = r;
                result.SyntaxTree = syntaxResult.Root;
                result.IsError = false;
            }
            else
            {
                result.Errors = new List<ParseError>();
                var unexpectedTokens = syntaxResult.Errors.ToList();
                var byEnding = unexpectedTokens.GroupBy(x => x.UnexpectedToken.Position).OrderBy(x => x.Key);
                var errors = new List<ParseError>();  
                foreach (var expecting in byEnding)
                {
                    var expectingTokens = expecting.SelectMany(x => x.ExpectedTokens ?? new List<LeadingToken<IN>>()).Distinct();
                    var expectedTokens =  expectingTokens != null && expectingTokens.Any() ? expectingTokens?.ToArray() : null;
                    if (expectedTokens != null)
                    {
                        var expected = new UnexpectedTokenSyntaxError<IN>(expecting.First().UnexpectedToken, LexemeLabels, I18n,
                            expectedTokens);
                        errors.Add(expected);
                    }
                    else
                    {
                        var expected = new UnexpectedTokenSyntaxError<IN>(expecting.First().UnexpectedToken, LexemeLabels, I18n,
                            new LeadingToken<IN>[]{});
                        errors.Add(expected);
                    }
                }
                
                result.Errors.AddRange(errors);
                result.IsError = true;
            }

            return result;
        }
    }
}