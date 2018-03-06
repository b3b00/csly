using Xunit;
using sly.parser;
using sly.parser.generator;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using jsonparser;
using jsonparser.JsonModel;
using sly.lexer;
using sly.buildresult;
using System.Linq;
using System;

namespace ParserTests
{

    public enum CommentsToken
    {

        [Lexeme(GenericToken.Int)]
        INT,

        [Lexeme(GenericToken.Double)]
        DOUBLE,

        [Lexeme(GenericToken.Identifier)]
        ID,

        [Comment("//", "/*", "*/")]
        COMMENT

    }

    public class CommentsTest
    {

        public CommentsTest()
        {

        }

        [Fact]
        public void TestGenericSingleLineComment()
        {
            var lexerRes = LexerBuilder.BuildLexer<CommentsToken>(new BuildResult<ILexer<CommentsToken>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<CommentsToken>;

            string dump = lexer.ToString();

            var tokens = lexer.Tokenize(@"1
2 // single line comment
3.0").ToList();

            Assert.Equal(4, tokens.Count);

            var token1 = tokens[0];            
            var token2 = tokens[1];
            var token3 = tokens[2];
            var token4 = tokens[3];

            
            Assert.Equal(CommentsToken.INT,token1.TokenID);
            Assert.Equal("1",token1.Value);
            Assert.Equal(0,token1.Position.Line);
            Assert.Equal(0,token1.Position.Column);
            Assert.Equal(CommentsToken.INT,token2.TokenID);
            Assert.Equal("2",token2.Value);
            Assert.Equal(1,token2.Position.Line);
            Assert.Equal(0,token2.Position.Column);
            Assert.Equal(CommentsToken.COMMENT,token3.TokenID);
            Assert.Equal(" single line comment",token3.Value);
            Assert.Equal(1,token3.Position.Line);
            Assert.Equal(2,token3.Position.Column);
            Assert.Equal(CommentsToken.DOUBLE,token4.TokenID);
            Assert.Equal("3.0",token4.Value);
            Assert.Equal(2,token4.Position.Line);
            Assert.Equal(0,token4.Position.Column);

        }


[Fact]
        public void TestGenericMultiLineComment()
        {
            var lexerRes = LexerBuilder.BuildLexer<CommentsToken>(new BuildResult<ILexer<CommentsToken>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<CommentsToken>;

           
            string dump = lexer.ToString();

            string code = @"1
2 /* multi line 
comment on 2 lines */ 3.0";
            

            var tokens = lexer.Tokenize(code).ToList();

            Assert.Equal(4, tokens.Count);

             var token1 = tokens[0];            
            var token2 = tokens[1];
            var token3 = tokens[2];
            var token4 = tokens[3];

            Assert.Equal(CommentsToken.INT,token1.TokenID);
            Assert.Equal("1",token1.Value);
             Assert.Equal(0,token1.Position.Line);
            Assert.Equal(0,token1.Position.Column);

            Assert.Equal(CommentsToken.INT,token2.TokenID);
            Assert.Equal("2",token2.Value);
             Assert.Equal(1,token2.Position.Line);
            Assert.Equal(0,token2.Position.Column);
            Assert.Equal(CommentsToken.COMMENT,token3.TokenID);
            Assert.Equal(@" multi line 
comment on 2 lines ",token3.Value);
            Assert.Equal(1,token3.Position.Line);
            Assert.Equal(2,token3.Position.Column);
            Assert.Equal(CommentsToken.DOUBLE,token4.TokenID);
            Assert.Equal("3.0",token4.Value);
            Assert.Equal(2,token4.Position.Line);
            Assert.Equal(22,token4.Position.Column);

        }

        [Fact]
        public void TestInnerMultiComment() {
            var lexerRes = LexerBuilder.BuildLexer<CommentsToken>(new BuildResult<ILexer<CommentsToken>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<CommentsToken>;

           
            string dump = lexer.ToString();

            string code = @"1
2 /* inner */ 3
4
            ";

            var tokens = lexer.Tokenize(code).ToList();            

            Assert.Equal(5, tokens.Count);

             var token1 = tokens[0];            
            var token2 = tokens[1];
            var token3 = tokens[2];
            var token4 = tokens[3];
            var token5 = tokens[4];


            Assert.Equal(CommentsToken.INT,token1.TokenID);
            Assert.Equal("1",token1.Value);
             Assert.Equal(0,token1.Position.Line);
            Assert.Equal(0,token1.Position.Column);

            Assert.Equal(CommentsToken.INT,token2.TokenID);
            Assert.Equal("2",token2.Value);
             Assert.Equal(1,token2.Position.Line);
            Assert.Equal(0,token2.Position.Column);

            Assert.Equal(CommentsToken.COMMENT,token3.TokenID);
            Assert.Equal(@" inner ",token3.Value);
            Assert.Equal(1,token3.Position.Line);
            Assert.Equal(2,token3.Position.Column);

            Assert.Equal(CommentsToken.INT,token4.TokenID);
            Assert.Equal("3",token4.Value);
            Assert.Equal(1,token4.Position.Line);
            Assert.Equal(14,token4.Position.Column);

            Assert.Equal(CommentsToken.INT,token5.TokenID);
            Assert.Equal("4",token5.Value);
            Assert.Equal(2,token5.Position.Line);
            Assert.Equal(0,token5.Position.Column);
        }

        [Fact]
        public void NotEndingMultiComment() {
            var lexerRes = LexerBuilder.BuildLexer<CommentsToken>(new BuildResult<ILexer<CommentsToken>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<CommentsToken>;

           
            string dump = lexer.ToString();

            string code = @"1
2 /* not ending
comment";

            var tokens = lexer.Tokenize(code).ToList();            

            Assert.Equal(3, tokens.Count);

             var token1 = tokens[0];            
            var token2 = tokens[1];
            var token3 = tokens[2];
            

            Assert.Equal(CommentsToken.INT,token1.TokenID);
            Assert.Equal("1",token1.Value);
             Assert.Equal(0,token1.Position.Line);
            Assert.Equal(0,token1.Position.Column);

            Assert.Equal(CommentsToken.INT,token2.TokenID);
            Assert.Equal("2",token2.Value);
             Assert.Equal(1,token2.Position.Line);
            Assert.Equal(0,token2.Position.Column);

            Assert.Equal(CommentsToken.COMMENT,token3.TokenID);
            Assert.Equal(@" not ending
comment",token3.Value);
            Assert.Equal(1,token3.Position.Line);
            Assert.Equal(2,token3.Position.Column);

            
        }



    }
}
