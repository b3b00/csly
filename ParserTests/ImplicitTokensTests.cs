using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using expressionparser;
using indented;
using jsonparser;
using jsonparser.JsonModel;
using simpleExpressionParser;
using sly.buildresult;
using sly.lexer;
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
    
    [Lexer(IgnoreWS = true, IgnoreEOL = true)]
    public enum ImplicitTokensTokens
    {
        [MultiLineComment("/*","*/")]
        MULTILINECOMMENT = 1,
        
        [SingleLineComment("//")]
        SINGLELINECOMMENT = 2,

        [Lexeme(GenericToken.Identifier, IdentifierType.AlphaNumeric)]
        ID = 3,
        
        [Lexeme(GenericToken.Double, channel:101)]
        DOUBLE = 4
    }

    public class ImplicitTokensParser
    {
        [Production("primary: DOUBLE")]
        public double Primary(Token<ImplicitTokensTokens> doubleToken)
        {
            return doubleToken.DoubleValue;
        }


        [Production("expression : term ['+' | '-'] expression")]
        
        public double Expression(double left, Token<ImplicitTokensTokens> operatorToken, double right)
        {
            double result = 0.0;


            switch (operatorToken.StringWithoutQuotes)
            {
                case "+":
                {
                    result = left + right;
                    break;
                }
                case "-":
                {
                    result = left - right;
                    break;
                }
            }

            return result;
        }

        [Production("expression : term")]
        public double Expression_Term(double termValue)
        {
            return termValue;
        }

        [Production("term : factor ['*' | '/'] term")]
        public double Term(double left, Token<ImplicitTokensTokens> operatorToken, double right)
        {
            var result = 0d;


            switch (operatorToken.StringWithoutQuotes)
            {
                case "*":
                {
                    result = left * right;
                    break;
                }
                case "/":
                {
                    result = left / right;
                    break;
                }
            }

            return result;
        }

        [Production("term : factor")]
        public double Term_Factor(double factorValue)
        {
            return factorValue;
        }

        [Production("factor : primary")]
        public double primaryFactor(double primValue)
        {
            return primValue;
        }

        [Production("factor : '-'[d] factor")]
        public double Factor(double factorValue)
        {
            return -factorValue;
        }
    }


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

        [Fact]
        public void BuildParserTest()
        {
            var parser = BuildParser();
            Assert.True(parser.IsOk);
            Assert.NotNull(parser.Result);
        }
    }
    
}
