
using System.Collections.Generic;
using sly.buildresult;
using sly.lexer;
using sly.parser.generator;
using sly.parser.generator.visitor;
using sly.parser.parser;

namespace sly.parser
{
    public class Parser<IN, OUT> where IN : struct
    {
        public Parser(ISyntaxParser<IN, OUT> syntaxParser, SyntaxTreeVisitor<IN, OUT> visitor)
        {
            SyntaxParser = syntaxParser;
            Visitor = visitor;
        }

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
            exprResult = ExpressionRulesGenerator.BuildExpressionRules(Configuration, Instance.GetType(), exprResult);
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

            var tokens = lexingResult.Tokens;
            var position = 0;
            var tokensWithoutComments = new List<Token<IN>>();
            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                if (!token.IsComment)
                {
                    token.PositionInTokenFlow = position;
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
            syntaxResult = cleaner.CleanSyntaxTree(syntaxResult);
            if (!syntaxResult.IsError && syntaxResult.Root != null)
            {
                var r = Visitor.VisitSyntaxTree(syntaxResult.Root,parsingContext);
                result.Result = r;
                result.SyntaxTree = syntaxResult.Root;
                result.IsError = false;
            }
            else
            {
                result.Errors = new List<ParseError>();
                result.Errors.AddRange(syntaxResult.Errors);
                result.IsError = true;
            }

            return result;
        }
    }
}