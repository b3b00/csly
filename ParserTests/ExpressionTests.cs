﻿using System.Linq;
using Xunit;
using sly.parser;
using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;
using expressionparser;

namespace ParserTests
{
    
    public class ExpressionTests
    {

        private Parser<ExpressionToken> Parser;
      
        public ExpressionTests()
        {
            ExpressionParser parserInstance = new ExpressionParser();
            ParserBuilder builder = new ParserBuilder();
            Parser = builder.BuildParser<ExpressionToken>(parserInstance, ParserType.LL_RECURSIVE_DESCENT, "expression");
        }


        [Fact]
        public void TestSingleValue()
        {
            ParseResult<ExpressionToken> r = Parser.Parse("1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.Equal(1, (int)r.Result);
        }

        [Fact]
        public void TestSingleNegativeValue()
        {
            ParseResult <ExpressionToken> r = Parser.Parse("-1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.Equal(-1, (int)r.Result);
        }

        [Fact]
        public void TestTermPlus()
        {
            ParseResult<ExpressionToken> r = Parser.Parse("1 + 1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.Equal(2, (int)r.Result);
        }

        [Fact]
        public void TestTermMinus()
        {
            ParseResult<ExpressionToken> r = Parser.Parse("1 - 1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.Equal(0, (int)r.Result);
        }

        [Fact]
        public void TestFactorTimes()
        {
            ParseResult<ExpressionToken> r = Parser.Parse("2*2");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.Equal(4, (int)r.Result);
        }

        [Fact]
        public void TestFactorDivide()
        {
            ParseResult<ExpressionToken> r = Parser.Parse("42/2");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.Equal(21, (int)r.Result);
        }

        [Fact]
        public void TestGroup()
        {
            ParseResult<ExpressionToken> r = Parser.Parse("(2 + 2)");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.Equal(4, (int)r.Result);
        }

        [Fact]
        public void TestGroup2()
        {
            ParseResult<ExpressionToken> r = Parser.Parse("6 * (2 + 2)");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.Equal(24, (int)r.Result);
        }

        [Fact]
        public void TestPrecedence()
        {
            ParseResult<ExpressionToken> r = Parser.Parse("6 * 2 + 2");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.Equal(14, (int)r.Result);
        }


        

      
    }
}
