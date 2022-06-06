using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using indented;
using jsonparser;
using jsonparser.JsonModel;
using simpleExpressionParser;
using sly.buildresult;
using sly.parser;
using sly.parser.generator;
using sly.parser.generator.visitor;
using sly.parser.llparser;
using sly.parser.parser;
using sly.parser.syntax.grammar;
using Xunit;
using String = System.String;

namespace ParserTests
{
    public class ImplicitTokensTests
    {
        private Parser<ImplicitTokensTokens, double> Parser;

        private BuildResult<Parser<ImplicitTokensTokens, double>> BuildParser()
        {
            var parserInstance = new ImplicitTokensParser();
            var builder = new ParserBuilder<ImplicitTokensTokens, double>();
            var result = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "expression");
            return result;
        }
        
        private BuildResult<Parser<ImplicitTokensTokens, double>> BuildExpressionParser()
        {
            var parserInstance = new ImplicitTokensExpressionParser();
            var builder = new ParserBuilder<ImplicitTokensTokens, double>();
            var result = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, nameof(ImplicitTokensExpressionParser)+"_expression");
            return result;
        }

        [Fact]
        public void BuildParserTest()
        {
            var parser = BuildParser();
            Assert.True(parser.IsOk);
            Assert.NotNull(parser.Result);
            var r = parser.Result.Parse("2.0 - 2.0 + bozzo  + Test");
            Assert.True(r.IsOk);
            // grammar is left associative so expression really is 
            // (2.0 - (2.0 + (bozzo  + Test))) = 2 - ( 2 + (42 + 0)) = 2 - (2 + 42) = 2 - 44 = -42
            Assert.Equal(-42.0,r.Result);
        }
        
        [Fact]
        public void BuildExpressionParserTest()
        {
            var parser = BuildExpressionParser();
            Assert.True(parser.IsOk);
            Assert.NotNull(parser.Result);
            var r = parser.Result.Parse("2.0 - 2.0 + bozzo  + Test");
            Assert.True(r.IsOk);
             
            
            Assert.Equal(2 - 2 + 42 + 0,r.Result);
        }
    }
    
}
