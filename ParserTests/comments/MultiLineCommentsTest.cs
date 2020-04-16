using sly.buildresult;
using sly.lexer;
using Xunit;

namespace ParserTests.comments
{
    public enum MultiLineCommentsToken
    {
        [Lexeme(GenericToken.Int)] INT = 1,

        [Lexeme(GenericToken.Double)] DOUBLE = 2,

        [Lexeme(GenericToken.Identifier)] ID = 3,

        [MultiLineComment("/*", "*/")] COMMENT = 4
    }
    
    public class MultiLineCommentsTest
    {
        [Fact]
        public void NotEndingMultiComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<MultiLineCommentsToken>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<MultiLineCommentsToken>;


            var dump = lexer.ToString();

            var code = @"1
2 /* not ending
comment";

            var r = lexer.Tokenize(code);
            Assert.True(r.IsOk);
            var tokens = r.Tokens;

            Assert.Equal(4, tokens.Count);

            var token1 = tokens[0];
            var token2 = tokens[1];
            var token3 = tokens[2];

            Assert.Equal(MultiLineCommentsToken.INT, token1.TokenID);
            Assert.Equal("1", token1.Value);
            Assert.Equal(0, token1.Position.Line);
            Assert.Equal(0, token1.Position.Column);

            Assert.Equal(MultiLineCommentsToken.INT, token2.TokenID);
            Assert.Equal("2", token2.Value);
            Assert.Equal(1, token2.Position.Line);
            Assert.Equal(0, token2.Position.Column);

            Assert.Equal(MultiLineCommentsToken.COMMENT, token3.TokenID);
            Assert.Equal(@" not ending
comment", token3.Value);
            Assert.Equal(1, token3.Position.Line);
            Assert.Equal(2, token3.Position.Column);
        }

        [Fact]
        public void TestGenericMultiLineComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<MultiLineCommentsToken>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<MultiLineCommentsToken>;


            var dump = lexer.ToString();

            var code = @"1
2 /* multi line 
comment on 2 lines */ 3.0";

            var r = lexer.Tokenize(code);
            Assert.True(r.IsOk);
            var tokens = r.Tokens;

            Assert.Equal(5, tokens.Count);

            var intToken1 = tokens[0];
            var intToken2 = tokens[1];
            var multiLineCommentToken = tokens[2];
            var doubleToken = tokens[3];

            Assert.Equal(MultiLineCommentsToken.INT, intToken1.TokenID);
            Assert.Equal("1", intToken1.Value);
            Assert.Equal(0, intToken1.Position.Line);
            Assert.Equal(0, intToken1.Position.Column);

            Assert.Equal(MultiLineCommentsToken.INT, intToken2.TokenID);
            Assert.Equal("2", intToken2.Value);
            Assert.Equal(1, intToken2.Position.Line);
            Assert.Equal(0, intToken2.Position.Column);
            Assert.Equal(MultiLineCommentsToken.COMMENT, multiLineCommentToken.TokenID);
            Assert.Equal(@" multi line 
comment on 2 lines ", multiLineCommentToken.Value);
            Assert.Equal(1, multiLineCommentToken.Position.Line);
            Assert.Equal(2, multiLineCommentToken.Position.Column);
            Assert.Equal(MultiLineCommentsToken.DOUBLE, doubleToken.TokenID);
            Assert.Equal("3.0", doubleToken.Value);
            Assert.Equal(2, doubleToken.Position.Line);
            Assert.Equal(22, doubleToken.Position.Column);
        }

        [Fact]
        public void TestGenericSingleLineComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<MultiLineCommentsToken>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<MultiLineCommentsToken>;

            var dump = lexer.ToString();

            var r = lexer.Tokenize(@"1
2 // single line comment
3.0");
            Assert.True(r.IsError);
            Assert.Equal('/', r.Error.UnexpectedChar);
        }

        [Fact]
        public void TestInnerMultiComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<MultiLineCommentsToken>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<MultiLineCommentsToken>;

            var dump = lexer.ToString();

            var code = @"1
2 /* inner */ 3
4
            ";

            var r = lexer.Tokenize(code);
            Assert.True(r.IsOk);
            var tokens = r.Tokens;
            Assert.Equal(6, tokens.Count);

            var token1 = tokens[0];
            var token2 = tokens[1];
            var token3 = tokens[2];
            var token4 = tokens[3];
            var token5 = tokens[4];

            Assert.Equal(MultiLineCommentsToken.INT, token1.TokenID);
            Assert.Equal("1", token1.Value);
            Assert.Equal(0, token1.Position.Line);
            Assert.Equal(0, token1.Position.Column);

            Assert.Equal(MultiLineCommentsToken.INT, token2.TokenID);
            Assert.Equal("2", token2.Value);
            Assert.Equal(1, token2.Position.Line);
            Assert.Equal(0, token2.Position.Column);

            Assert.Equal(MultiLineCommentsToken.COMMENT, token3.TokenID);
            Assert.Equal(@" inner ", token3.Value);
            Assert.Equal(1, token3.Position.Line);
            Assert.Equal(2, token3.Position.Column);

            Assert.Equal(MultiLineCommentsToken.INT, token4.TokenID);
            Assert.Equal("3", token4.Value);
            Assert.Equal(1, token4.Position.Line);
            Assert.Equal(14, token4.Position.Column);

            Assert.Equal(MultiLineCommentsToken.INT, token5.TokenID);
            Assert.Equal("4", token5.Value);
            Assert.Equal(2, token5.Position.Line);
            Assert.Equal(0, token5.Position.Column);
        }

        [Fact]
        public void TestMixedEOLComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<MultiLineCommentsToken>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<MultiLineCommentsToken>;

            var dump = lexer.ToString();
            var code = "1\n2\r\n/* multi line \rcomment on 2 lines */ 3.0";
            var r = lexer.Tokenize(code);
            Assert.True(r.IsOk);
            var tokens = r.Tokens;

            Assert.Equal(5, tokens.Count);

            var token1 = tokens[0];
            var token2 = tokens[1];
            var token3 = tokens[2];
            var token4 = tokens[3];

            Assert.Equal(MultiLineCommentsToken.INT, token1.TokenID);
            Assert.Equal("1", token1.Value);
            Assert.Equal(0, token1.Position.Line);
            Assert.Equal(0, token1.Position.Column);

            Assert.Equal(MultiLineCommentsToken.INT, token2.TokenID);
            Assert.Equal("2", token2.Value);
            Assert.Equal(1, token2.Position.Line);
            Assert.Equal(0, token2.Position.Column);
            Assert.Equal(MultiLineCommentsToken.COMMENT, token3.TokenID);
            Assert.Equal(" multi line \rcomment on 2 lines ", token3.Value);
            Assert.Equal(2, token3.Position.Line);
            Assert.Equal(0, token3.Position.Column);
            Assert.Equal(MultiLineCommentsToken.DOUBLE, token4.TokenID);
            Assert.Equal("3.0", token4.Value);
            Assert.Equal(3, token4.Position.Line);
            Assert.Equal(22, token4.Position.Column);
        }
    }
}