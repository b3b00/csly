using System.Collections.Generic;
using System.Linq;
using sly.buildresult;
using sly.lexer;
using Xunit;

namespace ParserTests
{
    public enum CommentsToken
    {
        [Lexeme(GenericToken.Int)] INT,

        [Lexeme(GenericToken.Double)] DOUBLE,

        [Lexeme(GenericToken.Identifier)] ID,

        [Comment("//", "/*", "*/")] COMMENT
    }

    public enum CommentsTokenAlternative
    {
        [Lexeme(GenericToken.Int)] INT,

        [Lexeme(GenericToken.Double)] DOUBLE,

        [Lexeme(GenericToken.Identifier)] ID,

        [SingleLineComment("//")] [MultiLineComment("/*", "*/")] COMMENT
    }

    public enum SingleLineCommentsToken
    {
        [Lexeme(GenericToken.Int)] INT,

        [Lexeme(GenericToken.Double)] DOUBLE,

        [Lexeme(GenericToken.Identifier)] ID,

        [SingleLineComment("//")] COMMENT
    }

    public enum MultiLineCommentsToken
    {
        [Lexeme(GenericToken.Int)] INT,

        [Lexeme(GenericToken.Double)] DOUBLE,

        [Lexeme(GenericToken.Identifier)] ID,

        [MultiLineComment("/*", "*/")] COMMENT
    }

    public enum CommentsTokenError1
    {
        [Lexeme(GenericToken.Int)] INT,

        [Lexeme(GenericToken.Double)] DOUBLE,

        [Lexeme(GenericToken.Identifier)] ID,

        [Comment("//", "/*", "*/")] [SingleLineComment("//")] COMMENT
    }

    public enum CommentsTokenError2
    {
        [Lexeme(GenericToken.Int)] INT,

        [Lexeme(GenericToken.Double)] DOUBLE,

        [Lexeme(GenericToken.Identifier)] ID,

        [Comment("//", "/*", "*/")] [MultiLineComment("/*", "*/")] COMMENT
    }

    public enum CommentsTokenError3
    {
        [Lexeme(GenericToken.Int)] INT,

        [Lexeme(GenericToken.Double)] DOUBLE,

        [Lexeme(GenericToken.Identifier)] ID,

        [Comment("//", "/*", "*/")] [SingleLineComment("//")] [MultiLineComment("/*", "*/")] COMMENT
    }

    public enum CommentsTokenError4
    {
        [Lexeme(GenericToken.Int)] INT,

        [Lexeme(GenericToken.Double)] DOUBLE,

        [Lexeme(GenericToken.Identifier)] ID,

        [SingleLineComment("//")] [SingleLineComment("//")] COMMENT
    }

    public enum CommentsTokenError5
    {
        [Lexeme(GenericToken.Int)] INT,

        [Lexeme(GenericToken.Double)] DOUBLE,

        [Lexeme(GenericToken.Identifier)] ID,

        [MultiLineComment("/*", "*/")] [MultiLineComment("/*", "*/")] COMMENT
    }

    public enum CommentsTokenError6
    {
        [Lexeme(GenericToken.Int)] INT,

        [Lexeme(GenericToken.Double)] DOUBLE,

        [Lexeme(GenericToken.Identifier)] ID,

        [Comment("//", "/*", "*/")] [Comment("//", "/*", "*/")] COMMENT
    }

    public enum CommentsTokenError7
    {
        [Lexeme(GenericToken.Int)] INT,

        [Lexeme(GenericToken.Double)] DOUBLE,

        [Lexeme(GenericToken.Identifier)] ID,

        [Comment("//", "/*", "*/")] [SingleLineComment("//")] [SingleLineComment("//")] COMMENT
    }

    public enum CommentsTokenError8
    {
        [Lexeme(GenericToken.Int)] INT,

        [Lexeme(GenericToken.Double)] DOUBLE,

        [Lexeme(GenericToken.Identifier)] ID,

        [Comment("//", "/*", "*/")] [MultiLineComment("/*", "*/")] [MultiLineComment("/*", "*/")] COMMENT
    }

    public enum CommentsTokenError9
    {
        [Lexeme(GenericToken.Int)] INT,

        [Lexeme(GenericToken.Double)] DOUBLE,

        [Lexeme(GenericToken.Identifier)] ID,

        [MultiLineComment("/*", "*/")] [MultiLineComment("/*", "*/")] [SingleLineComment("//")] [SingleLineComment("//")] COMMENT
    }

    public enum CommentsTokenError10
    {
        [Lexeme(GenericToken.Int)] INT,

        [Lexeme(GenericToken.Double)] DOUBLE,

        [Lexeme(GenericToken.Identifier)] ID,

        [Comment("//", "/*", "*/")] [Comment("//", "/*", "*/")] [MultiLineComment("/*", "*/")] [MultiLineComment("/*", "*/")] [SingleLineComment("//")] [SingleLineComment("//")] COMMENT
    }

    public class CommentsErrorTest
    {
        [Fact]
        public void MultipleAttributes()
        {
            var lexerRes6 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError6>>());
            Assert.True(lexerRes6.IsError);
            Assert.Equal(2, lexerRes6.Errors.Count);
            Assert.Equal(ErrorLevel.FATAL, lexerRes6.Errors[1].Level);
            Assert.Equal("too many comment lexem", lexerRes6.Errors[1].Message);

            var lexerRes5 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError5>>());
            Assert.True(lexerRes5.IsError);
            Assert.Equal(2, lexerRes5.Errors.Count);
            Assert.Equal(ErrorLevel.FATAL, lexerRes5.Errors[1].Level);
            Assert.Equal("too many multi-line comment lexem", lexerRes5.Errors[1].Message);

            var lexerRes4 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError4>>());
            Assert.True(lexerRes4.IsError);
            Assert.Equal(2, lexerRes4.Errors.Count);
            Assert.Equal(ErrorLevel.FATAL, lexerRes4.Errors[1].Level);
            Assert.Equal("too many single-line comment lexem", lexerRes4.Errors[1].Message);
        }

        [Fact]
        public void RedundantAttributes()
        {
            var lexerRes3 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError3>>());
            Assert.True(lexerRes3.IsError);
            Assert.Equal(2, lexerRes3.Errors.Count);
            Assert.Equal(ErrorLevel.FATAL, lexerRes3.Errors[1].Level);
            Assert.Equal("comment lexem can't be used together with single-line or multi-line comment lexems", lexerRes3.Errors[1].Message);

            var lexerRes2 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError2>>());
            Assert.True(lexerRes2.IsError);
            Assert.Equal(2, lexerRes2.Errors.Count);
            Assert.Equal(ErrorLevel.FATAL, lexerRes2.Errors[1].Level);
            Assert.Equal("comment lexem can't be used together with single-line or multi-line comment lexems", lexerRes2.Errors[1].Message);

            var lexerRes1 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError1>>());
            Assert.True(lexerRes1.IsError);
            Assert.Equal(2, lexerRes1.Errors.Count);
            Assert.Equal(ErrorLevel.FATAL, lexerRes1.Errors[1].Level);
            Assert.Equal("comment lexem can't be used together with single-line or multi-line comment lexems", lexerRes1.Errors[1].Message);
        }

        [Fact]
        public void MixedErrors()
        {
            var lexerRes10 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError10>>());
            Assert.True(lexerRes10.IsError);
            Assert.Equal(5, lexerRes10.Errors.Count);
            Assert.Equal(ErrorLevel.FATAL, lexerRes10.Errors[1].Level);
            Assert.Equal("too many comment lexem", lexerRes10.Errors[1].Message);
            Assert.Equal(ErrorLevel.FATAL, lexerRes10.Errors[2].Level);
            Assert.Equal("too many multi-line comment lexem", lexerRes10.Errors[2].Message);
            Assert.Equal(ErrorLevel.FATAL, lexerRes10.Errors[3].Level);
            Assert.Equal("too many single-line comment lexem", lexerRes10.Errors[3].Message);
            Assert.Equal(ErrorLevel.FATAL, lexerRes10.Errors[4].Level);
            Assert.Equal("comment lexem can't be used together with single-line or multi-line comment lexems", lexerRes10.Errors[4].Message);

            var lexerRes9 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError9>>());
            Assert.True(lexerRes9.IsError);
            Assert.Equal(3, lexerRes9.Errors.Count);
            Assert.Equal(ErrorLevel.FATAL, lexerRes9.Errors[1].Level);
            Assert.Equal("too many multi-line comment lexem", lexerRes9.Errors[1].Message);
            Assert.Equal(ErrorLevel.FATAL, lexerRes9.Errors[2].Level);
            Assert.Equal("too many single-line comment lexem", lexerRes9.Errors[2].Message);

            var lexerRes8 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError8>>());
            Assert.True(lexerRes8.IsError);
            Assert.Equal(3, lexerRes8.Errors.Count);
            Assert.Equal(ErrorLevel.FATAL, lexerRes8.Errors[1].Level);
            Assert.Equal("too many multi-line comment lexem", lexerRes8.Errors[1].Message);
            Assert.Equal(ErrorLevel.FATAL, lexerRes8.Errors[2].Level);
            Assert.Equal("comment lexem can't be used together with single-line or multi-line comment lexems", lexerRes8.Errors[2].Message);

            var lexerRes7 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError7>>());
            Assert.True(lexerRes7.IsError);
            Assert.Equal(3, lexerRes7.Errors.Count);
            Assert.Equal(ErrorLevel.FATAL, lexerRes7.Errors[1].Level);
            Assert.Equal("too many single-line comment lexem", lexerRes7.Errors[1].Message);
            Assert.Equal(ErrorLevel.FATAL, lexerRes7.Errors[2].Level);
            Assert.Equal("comment lexem can't be used together with single-line or multi-line comment lexems", lexerRes7.Errors[2].Message);
        }
    }

    public class CommentsTestGeneric
    {
        [Fact]
        public void NotEndingMultiComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsToken>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<CommentsToken>;

            var dump = lexer.ToString();

            var code = @"1
2 /* not ending
comment";

            var tokens = lexer.Tokenize(code).ToList();

            Assert.Equal(4, tokens.Count);

            var token1 = tokens[0];
            var token2 = tokens[1];
            var token3 = tokens[2];

            Assert.Equal(CommentsToken.INT, token1.TokenID);
            Assert.Equal("1", token1.Value);
            Assert.Equal(0, token1.Position.Line);
            Assert.Equal(0, token1.Position.Column);

            Assert.Equal(CommentsToken.INT, token2.TokenID);
            Assert.Equal("2", token2.Value);
            Assert.Equal(1, token2.Position.Line);
            Assert.Equal(0, token2.Position.Column);

            Assert.Equal(CommentsToken.COMMENT, token3.TokenID);
            Assert.Equal(@" not ending
comment", token3.Value);
            Assert.Equal(1, token3.Position.Line);
            Assert.Equal(2, token3.Position.Column);
        }

        [Fact]
        public void TestGenericMultiLineComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsToken>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<CommentsToken>;


            var dump = lexer.ToString();

            var code = @"1
2 /* multi line 
comment on 2 lines */ 3.0";

            var tokens = lexer.Tokenize(code).ToList();

            Assert.Equal(5, tokens.Count);

            var intToken1 = tokens[0];
            var intToken2 = tokens[1];
            var multiLineCommentToken = tokens[2];
            var doubleToken = tokens[3];

            Assert.Equal(CommentsToken.INT, intToken1.TokenID);
            Assert.Equal("1", intToken1.Value);
            Assert.Equal(0, intToken1.Position.Line);
            Assert.Equal(0, intToken1.Position.Column);

            Assert.Equal(CommentsToken.INT, intToken2.TokenID);
            Assert.Equal("2", intToken2.Value);
            Assert.Equal(1, intToken2.Position.Line);
            Assert.Equal(0, intToken2.Position.Column);
            Assert.Equal(CommentsToken.COMMENT, multiLineCommentToken.TokenID);
            Assert.Equal(@" multi line 
comment on 2 lines ", multiLineCommentToken.Value);
            Assert.Equal(1, multiLineCommentToken.Position.Line);
            Assert.Equal(2, multiLineCommentToken.Position.Column);
            Assert.Equal(CommentsToken.DOUBLE, doubleToken.TokenID);
            Assert.Equal("3.0", doubleToken.Value);
            Assert.Equal(2, doubleToken.Position.Line);
            Assert.Equal(22, doubleToken.Position.Column);
        }

        [Fact]
        public void TestGenericSingleLineComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsToken>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<CommentsToken>;

            var dump = lexer.ToString();


            var tokens = lexer.Tokenize(@"1
2 // single line comment
3.0").ToList();

            Assert.Equal(5, tokens.Count);

            var token1 = tokens[0];
            var token2 = tokens[1];
            var token3 = tokens[2];
            var token4 = tokens[3];


            Assert.Equal(CommentsToken.INT, token1.TokenID);
            Assert.Equal("1", token1.Value);
            Assert.Equal(0, token1.Position.Line);
            Assert.Equal(0, token1.Position.Column);
            Assert.Equal(CommentsToken.INT, token2.TokenID);
            Assert.Equal("2", token2.Value);
            Assert.Equal(1, token2.Position.Line);
            Assert.Equal(0, token2.Position.Column);
            Assert.Equal(CommentsToken.COMMENT, token3.TokenID);
            Assert.Equal(" single line comment", token3.Value.Replace("\r","").Replace("\n",""));
            Assert.Equal(1, token3.Position.Line);
            Assert.Equal(2, token3.Position.Column);
            Assert.Equal(CommentsToken.DOUBLE, token4.TokenID);
            Assert.Equal("3.0", token4.Value);
            Assert.Equal(2, token4.Position.Line);
            Assert.Equal(0, token4.Position.Column);
        }

        [Fact]
        public void TestInnerMultiComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsToken>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<CommentsToken>;


            var dump = lexer.ToString();

            var code = @"1
2 /* inner */ 3
4
            ";

            var tokens = lexer.Tokenize(code).ToList();

            Assert.Equal(6, tokens.Count);

            var token1 = tokens[0];
            var token2 = tokens[1];
            var token3 = tokens[2];
            var token4 = tokens[3];
            var token5 = tokens[4];


            Assert.Equal(CommentsToken.INT, token1.TokenID);
            Assert.Equal("1", token1.Value);
            Assert.Equal(0, token1.Position.Line);
            Assert.Equal(0, token1.Position.Column);

            Assert.Equal(CommentsToken.INT, token2.TokenID);
            Assert.Equal("2", token2.Value);
            Assert.Equal(1, token2.Position.Line);
            Assert.Equal(0, token2.Position.Column);

            Assert.Equal(CommentsToken.COMMENT, token3.TokenID);
            Assert.Equal(@" inner ", token3.Value);
            Assert.Equal(1, token3.Position.Line);
            Assert.Equal(2, token3.Position.Column);

            Assert.Equal(CommentsToken.INT, token4.TokenID);
            Assert.Equal("3", token4.Value);
            Assert.Equal(1, token4.Position.Line);
            Assert.Equal(14, token4.Position.Column);

            Assert.Equal(CommentsToken.INT, token5.TokenID);
            Assert.Equal("4", token5.Value);
            Assert.Equal(2, token5.Position.Line);
            Assert.Equal(0, token5.Position.Column);
        }

        [Fact]
        public void TestMixedEOLComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsToken>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<CommentsToken>;


            var dump = lexer.ToString();
            var code = "1\n2\r\n/* multi line \rcomment on 2 lines */ 3.0";
            List<Token<CommentsToken>> tokens = null;
            tokens = lexer.Tokenize(code).ToList();

            Assert.Equal(5, tokens.Count);

            var token1 = tokens[0];
            var token2 = tokens[1];
            var token3 = tokens[2];
            var token4 = tokens[3];

            Assert.Equal(CommentsToken.INT, token1.TokenID);
            Assert.Equal("1", token1.Value);
            Assert.Equal(0, token1.Position.Line);
            Assert.Equal(0, token1.Position.Column);

            Assert.Equal(CommentsToken.INT, token2.TokenID);
            Assert.Equal("2", token2.Value);
            Assert.Equal(1, token2.Position.Line);
            Assert.Equal(0, token2.Position.Column);
            Assert.Equal(CommentsToken.COMMENT, token3.TokenID);
            Assert.Equal(" multi line \rcomment on 2 lines ", token3.Value);
            Assert.Equal(2, token3.Position.Line);
            Assert.Equal(0, token3.Position.Column);
            Assert.Equal(CommentsToken.DOUBLE, token4.TokenID);
            Assert.Equal("3.0", token4.Value);
            Assert.Equal(3, token4.Position.Line);
            Assert.Equal(22, token4.Position.Column);
        }
    }

    public class CommentsTestAlternative
    {
        [Fact]
        public void NotEndingMultiComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenAlternative>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<CommentsTokenAlternative>;


            var dump = lexer.ToString();

            var code = @"1
2 /* not ending
comment";

            var tokens = lexer.Tokenize(code).ToList();

            Assert.Equal(4, tokens.Count);

            var token1 = tokens[0];
            var token2 = tokens[1];
            var token3 = tokens[2];

            Assert.Equal(CommentsTokenAlternative.INT, token1.TokenID);
            Assert.Equal("1", token1.Value);
            Assert.Equal(0, token1.Position.Line);
            Assert.Equal(0, token1.Position.Column);

            Assert.Equal(CommentsTokenAlternative.INT, token2.TokenID);
            Assert.Equal("2", token2.Value);
            Assert.Equal(1, token2.Position.Line);
            Assert.Equal(0, token2.Position.Column);

            Assert.Equal(CommentsTokenAlternative.COMMENT, token3.TokenID);
            Assert.Equal(@" not ending
comment", token3.Value);
            Assert.Equal(1, token3.Position.Line);
            Assert.Equal(2, token3.Position.Column);
        }

        [Fact]
        public void TestGenericMultiLineComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenAlternative>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<CommentsTokenAlternative>;


            var dump = lexer.ToString();

            var code = @"1
2 /* multi line 
comment on 2 lines */ 3.0";

            var tokens = lexer.Tokenize(code).ToList();

            Assert.Equal(5, tokens.Count);

            var intToken1 = tokens[0];
            var intToken2 = tokens[1];
            var multiLineCommentToken = tokens[2];
            var doubleToken = tokens[3];

            Assert.Equal(CommentsTokenAlternative.INT, intToken1.TokenID);
            Assert.Equal("1", intToken1.Value);
            Assert.Equal(0, intToken1.Position.Line);
            Assert.Equal(0, intToken1.Position.Column);

            Assert.Equal(CommentsTokenAlternative.INT, intToken2.TokenID);
            Assert.Equal("2", intToken2.Value);
            Assert.Equal(1, intToken2.Position.Line);
            Assert.Equal(0, intToken2.Position.Column);
            Assert.Equal(CommentsTokenAlternative.COMMENT, multiLineCommentToken.TokenID);
            Assert.Equal(@" multi line 
comment on 2 lines ", multiLineCommentToken.Value);
            Assert.Equal(1, multiLineCommentToken.Position.Line);
            Assert.Equal(2, multiLineCommentToken.Position.Column);
            Assert.Equal(CommentsTokenAlternative.DOUBLE, doubleToken.TokenID);
            Assert.Equal("3.0", doubleToken.Value);
            Assert.Equal(2, doubleToken.Position.Line);
            Assert.Equal(22, doubleToken.Position.Column);
        }

        [Fact]
        public void TestGenericSingleLineComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenAlternative>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<CommentsTokenAlternative>;

            var dump = lexer.ToString();

            var tokens = lexer.Tokenize(@"1
2 // single line comment
3.0").ToList();

            Assert.Equal(5, tokens.Count);

            var token1 = tokens[0];
            var token2 = tokens[1];
            var token3 = tokens[2];
            var token4 = tokens[3];

            Assert.Equal(CommentsTokenAlternative.INT, token1.TokenID);
            Assert.Equal("1", token1.Value);
            Assert.Equal(0, token1.Position.Line);
            Assert.Equal(0, token1.Position.Column);
            Assert.Equal(CommentsTokenAlternative.INT, token2.TokenID);
            Assert.Equal("2", token2.Value);
            Assert.Equal(1, token2.Position.Line);
            Assert.Equal(0, token2.Position.Column);
            Assert.Equal(CommentsTokenAlternative.COMMENT, token3.TokenID);
            Assert.Equal(" single line comment", token3.Value.Replace("\r", "").Replace("\n", ""));
            Assert.Equal(1, token3.Position.Line);
            Assert.Equal(2, token3.Position.Column);
            Assert.Equal(CommentsTokenAlternative.DOUBLE, token4.TokenID);
            Assert.Equal("3.0", token4.Value);
            Assert.Equal(2, token4.Position.Line);
            Assert.Equal(0, token4.Position.Column);
        }

        [Fact]
        public void TestInnerMultiComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenAlternative>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<CommentsTokenAlternative>;

            var dump = lexer.ToString();

            var code = @"1
2 /* inner */ 3
4
            ";

            var tokens = lexer.Tokenize(code).ToList();

            Assert.Equal(6, tokens.Count);

            var token1 = tokens[0];
            var token2 = tokens[1];
            var token3 = tokens[2];
            var token4 = tokens[3];
            var token5 = tokens[4];

            Assert.Equal(CommentsTokenAlternative.INT, token1.TokenID);
            Assert.Equal("1", token1.Value);
            Assert.Equal(0, token1.Position.Line);
            Assert.Equal(0, token1.Position.Column);

            Assert.Equal(CommentsTokenAlternative.INT, token2.TokenID);
            Assert.Equal("2", token2.Value);
            Assert.Equal(1, token2.Position.Line);
            Assert.Equal(0, token2.Position.Column);

            Assert.Equal(CommentsTokenAlternative.COMMENT, token3.TokenID);
            Assert.Equal(@" inner ", token3.Value);
            Assert.Equal(1, token3.Position.Line);
            Assert.Equal(2, token3.Position.Column);

            Assert.Equal(CommentsTokenAlternative.INT, token4.TokenID);
            Assert.Equal("3", token4.Value);
            Assert.Equal(1, token4.Position.Line);
            Assert.Equal(14, token4.Position.Column);

            Assert.Equal(CommentsTokenAlternative.INT, token5.TokenID);
            Assert.Equal("4", token5.Value);
            Assert.Equal(2, token5.Position.Line);
            Assert.Equal(0, token5.Position.Column);
        }

        [Fact]
        public void TestMixedEOLComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenAlternative>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<CommentsTokenAlternative>;

            var dump = lexer.ToString();
            var code = "1\n2\r\n/* multi line \rcomment on 2 lines */ 3.0";
            List<Token<CommentsTokenAlternative>> tokens = null;
            tokens = lexer.Tokenize(code).ToList();

            Assert.Equal(5, tokens.Count);

            var token1 = tokens[0];
            var token2 = tokens[1];
            var token3 = tokens[2];
            var token4 = tokens[3];

            Assert.Equal(CommentsTokenAlternative.INT, token1.TokenID);
            Assert.Equal("1", token1.Value);
            Assert.Equal(0, token1.Position.Line);
            Assert.Equal(0, token1.Position.Column);

            Assert.Equal(CommentsTokenAlternative.INT, token2.TokenID);
            Assert.Equal("2", token2.Value);
            Assert.Equal(1, token2.Position.Line);
            Assert.Equal(0, token2.Position.Column);
            Assert.Equal(CommentsTokenAlternative.COMMENT, token3.TokenID);
            Assert.Equal(" multi line \rcomment on 2 lines ", token3.Value);
            Assert.Equal(2, token3.Position.Line);
            Assert.Equal(0, token3.Position.Column);
            Assert.Equal(CommentsTokenAlternative.DOUBLE, token4.TokenID);
            Assert.Equal("3.0", token4.Value);
            Assert.Equal(3, token4.Position.Line);
            Assert.Equal(22, token4.Position.Column);
        }
    }

    public class SingleLineCommentsTest
    {
        [Fact]
        public void NotEndingMultiComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<SingleLineCommentsToken>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<SingleLineCommentsToken>;

            var dump = lexer.ToString();

			var error = Assert.Throws<LexerException>(() =>
			{
				lexer?.Tokenize(@"1
2 /* not ending
comment").ToList();
			});
			Assert.Equal('*', error.Error.UnexpectedChar);
		}

        [Fact]
        public void TestGenericMultiLineComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<SingleLineCommentsToken>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<SingleLineCommentsToken>;

            var dump = lexer.ToString();

			var error = Assert.Throws<LexerException>(() =>
			{
				lexer?.Tokenize(@"1
2 /* multi line 
comment on 2 lines */ 3.0").ToList();
			});
			Assert.Equal('*', error.Error.UnexpectedChar);
		}

        [Fact]
        public void TestGenericSingleLineComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<SingleLineCommentsToken>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<SingleLineCommentsToken>;

            var dump = lexer.ToString();

            var tokens = lexer.Tokenize(@"1
2 // single line comment
3.0").ToList();

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

        [Fact]
        public void TestInnerMultiComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<SingleLineCommentsToken>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<SingleLineCommentsToken>;

            var dump = lexer.ToString();

			var error = Assert.Throws<LexerException>(() =>
			{
				lexer?.Tokenize(@"1
2 /* inner */ 3
4
            ").ToList();
			});
			Assert.Equal('*', error.Error.UnexpectedChar);
		}

        [Fact]
        public void TestMixedEOLComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<SingleLineCommentsToken>>());
            Assert.False(lexerRes.IsError);
            var lexer = lexerRes.Result as GenericLexer<SingleLineCommentsToken>;

            var dump = lexer.ToString();

			var error = Assert.Throws<LexerException>(() =>
			{
				lexer?.Tokenize("1\n2\r\n/* multi line \rcomment on 2 lines */ 3.0").ToList();
			});
			Assert.Equal('*', error.Error.UnexpectedChar);
		}
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

			var tokens = lexer.Tokenize(code).ToList();

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

			var tokens = lexer.Tokenize(code).ToList();

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

			var error = Assert.Throws<LexerException>(() =>
			{
				lexer.Tokenize(@"1
2 // single line comment
3.0");
			});
			Assert.Equal('/', error.Error.UnexpectedChar);
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

            var tokens = lexer.Tokenize(code).ToList();

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
            List<Token<MultiLineCommentsToken>> tokens = null;
            tokens = lexer.Tokenize(code).ToList();

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