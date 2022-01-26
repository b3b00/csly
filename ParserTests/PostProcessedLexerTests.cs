using System;
using System.Collections.Generic;
using Xunit;
using postProcessedLexerParser;
using postProcessedLexerParser.expressionModel;

namespace ParserTests
{
    public class PostProcessedLexerTests
    {
        [Fact]
        public void TestPostLexerProcessing()
        {
            var Parser = postProcessedLexerParser.PostProcessedLexerParserBuilder.buildPostProcessedLexerParser();
            
            var r = Parser.Parse("2 * x");
            Assert.False(r.IsError);
            var res = r.Result.Evaluate(new ExpressionContext(new Dictionary<string, double>()
                { { "x", 2 } }));
          Assert.NotNull(res);
          Assert.Equal(4,res.Value);
            
            
            r = Parser.Parse("2  x");
            Assert.False(r.IsError);
            res = r.Result.Evaluate(new ExpressionContext(new Dictionary<string, double>()
                { { "x", 2 } }));
            Assert.NotNull(res);
            Assert.Equal(4,res.Value);
            
            
            r = Parser.Parse("2 ( x ) ");
            Assert.False(r.IsError);
            res = r.Result.Evaluate(new ExpressionContext(new Dictionary<string, double>()
                { { "x", 2 } }));
            Assert.NotNull(res);
            Assert.Equal(4,res.Value);
            
        }
    }
}