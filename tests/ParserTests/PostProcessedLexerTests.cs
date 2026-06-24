using System.Collections.Generic;
using NFluent;
using postProcessedLexerParser;
using Xunit;
using postProcessedLexerParser.expressionModel;
using sly.parser.generator;

namespace ParserTests
{
    public class PostProcessedLexerTests
    {
        [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public void TestPostLexerProcessing(ParserType parserType)
        {
            var Parser = postProcessedLexerParser.PostProcessedLexerParserBuilder.buildPostProcessedLexerParser(parserType);
            
            var parserInstance = new FormulaParser();
            var builder = new ParserBuilder<FormulaToken, Expression>();
            var build = builder.BuildParser(parserInstance, parserType, $"{nameof(FormulaParser)}_expressions",
                lexerPostProcess: PostProcessedLexerParserBuilder.postProcessFormula);
            Check.That(build).IsOk();
            var r = Parser.Parse("2 * x");
            Check.That(r).IsOkParsing();
            
            var res = r.Result.Evaluate(new ExpressionContext(new Dictionary<string, double>()
                { { "x", 2 } }));
            Check.That(res).IsNotNull();
            Check.That(res.Value).IsEqualTo(4);
          
            
            
            r = Parser.Parse("2  x");
            Check.That(r.IsError).IsFalse();
            res = r.Result.Evaluate(new ExpressionContext(new Dictionary<string, double>()
                { { "x", 2 } }));
            Check.That(res).IsNotNull();
            Check.That(res.Value).IsEqualTo(4);
            
            
            r = Parser.Parse("2 ( x ) ");
            Check.That(r.IsError).IsFalse();
            res = r.Result.Evaluate(new ExpressionContext(new Dictionary<string, double>()
                { { "x", 2 } }));
            Check.That(res).IsNotNull();
            Check.That(res.Value).IsEqualTo(4);
            
        }
    }
}