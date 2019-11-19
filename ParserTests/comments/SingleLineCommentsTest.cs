using sly.buildresult;
using sly.lexer;
using Xunit;

namespace ParserTests.comments
{
    
    public enum SingleLineCommentsToken
    {
        [Lexeme(GenericToken.Int)] INT,

        [Lexeme(GenericToken.Double)] DOUBLE,

        [Lexeme(GenericToken.Identifier)] ID,

        [SingleLineComment("//")] COMMENT
    }
    
    public class SingleLineCommentsTest
    {
     

        [Fact]
        public void TestGenericMultiLineCommentWithSingleLineComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<SingleLineCommentsToken>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<SingleLineCommentsToken>;

            var dump = lexer.ToString();

            var r =lexer?.Tokenize(@"1
2 /* multi line 
comment on 2 lines */ 3.0");
            Assert.True(r.IsError);
            var tokens = r.Tokens;
            Assert.Equal('*', r.Error.UnexpectedChar);
        }

        [Fact]
        public void TestGenericSingleLineComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<SingleLineCommentsToken>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<SingleLineCommentsToken>;

            var dump = lexer.ToString();

            var r = lexer.Tokenize(@"1
2 // single line comment
3.0");
            Assert.True(r.IsOk);
            var tokens = r.Tokens;

            Assert.Equal(5, tokens.Count);

            var token1 = tokens[0];
            var token2 = tokens[1];
            var token3 = tokens[2];
            var token4 = tokens[3];

            Assert.Equal(SingleLineCommentsToken.INT, token1.TokenID);
            Assert.Equal("1", token1.Value);
            Assert.Equal(0, token1.Position.Line);
            Assert.Equal(0, token1.Position.Column);
            Assert.Equal(SingleLineCommentsToken.INT, token2.TokenID);
            Assert.Equal("2", token2.Value);
            Assert.Equal(1, token2.Position.Line);
            Assert.Equal(0, token2.Position.Column);
            Assert.Equal(SingleLineCommentsToken.COMMENT, token3.TokenID);
            Assert.Equal(" single line comment", token3.Value.Replace("\r", "").Replace("\n", ""));
            Assert.Equal(1, token3.Position.Line);
            Assert.Equal(2, token3.Position.Column);
            Assert.Equal(SingleLineCommentsToken.DOUBLE, token4.TokenID);
            Assert.Equal("3.0", token4.Value);
            Assert.Equal(2, token4.Position.Line);
            Assert.Equal(0, token4.Position.Column);
        }

       
    }
}