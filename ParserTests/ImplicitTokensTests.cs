﻿using System;
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
        DOUBLE = 4,
        
        [Keyword("Test")] 
        TEST = 5
       
    }

    public class ImplicitTokensParser
    {
        [Production("primary: DOUBLE")]
        public double Primary(Token<ImplicitTokensTokens> doubleToken)
        {
            return doubleToken.DoubleValue;
        }
        
        [Production("primary : 'bozzo'[d]")]
        public double Bozzo()
        {
            return 42.0;
        }

        [Production("primary : TEST[d]")]
        public double Test()
        {
            return 0.0;
        } 


        [Production("expression : primary ['+' | '-'] expression")]
        
        
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


        [Production("expression : primary ")]
        public double Simple(double value)
        {
            return value;
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
            var r = parser.Result.Parse("2.0 - 2.0 + bozzo  + Test");
            Assert.True(r.IsOk);
            // grammar is left associative so expression really is 
            // (2.0 - (2.0 + (bozzo  + Test))) = 2 - ( 2 + (42 + 0)) = 2 - (2 + 42) = 2 - 44 = -42
            Assert.Equal(-42.0,r.Result);
        }
    }
    
}