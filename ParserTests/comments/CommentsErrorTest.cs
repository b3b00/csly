using sly.buildresult;
using sly.lexer;
using Xunit;

namespace ParserTests.comments
{
    
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
}