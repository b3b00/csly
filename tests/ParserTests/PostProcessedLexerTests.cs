using System.Collections.Generic;
using NFluent;
using Xunit;
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
            Check.That(r.IsError).IsFalse();
            
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