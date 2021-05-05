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
            Assert.Single(lexerRes6.Errors);
            var expectedErrors = new[]
            {
                "too many comment lexem"
            };
            foreach (var expectedError in expectedErrors)
            {
                Assert.True(lexerRes6.Errors.Exists(x => x.Level == ErrorLevel.FATAL && x.Message == expectedError));
            }

            var lexerRes5 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError5>>());
            Assert.True(lexerRes5.IsError);
            Assert.Single(lexerRes5.Errors);
            expectedErrors = new[]
            {
                "too many multi-line comment lexem"
            };
            foreach (var expectedError in expectedErrors)
            {
                Assert.True(lexerRes5.Errors.Exists(x => x.Level == ErrorLevel.FATAL && x.Message == expectedError));
            }
            

            var lexerRes4 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError4>>());
            Assert.True(lexerRes4.IsError);
            Assert.Single(lexerRes4.Errors);
            expectedErrors = new[]
            {
                "too many single-line comment lexem"
            };
            foreach (var expectedError in expectedErrors)
            {
                Assert.True(lexerRes4.Errors.Exists(x => x.Level == ErrorLevel.FATAL && x.Message == expectedError));
            }

        }

        [Fact]
        public void RedundantAttributes()
        {
            var lexerRes3 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError3>>());
            Assert.True(lexerRes3.IsError);
            Assert.Single(lexerRes3.Errors);
            Assert.Equal(ErrorLevel.FATAL, lexerRes3.Errors[0].Level);
            Assert.Equal("comment lexem can't be used together with single-line or multi-line comment lexems", lexerRes3.Errors[0].Message);

            var lexerRes2 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError2>>());
            Assert.True(lexerRes2.IsError);
            Assert.Single(lexerRes2.Errors);
            Assert.Equal(ErrorLevel.FATAL, lexerRes2.Errors[0].Level);
            Assert.Equal("comment lexem can't be used together with single-line or multi-line comment lexems", lexerRes2.Errors[0].Message);

            var lexerRes1 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError1>>());
            Assert.True(lexerRes1.IsError);
            Assert.Single(lexerRes1.Errors);
            Assert.Equal(ErrorLevel.FATAL, lexerRes1.Errors[0].Level);
            Assert.Equal("comment lexem can't be used together with single-line or multi-line comment lexems", lexerRes1.Errors[0].Message);
        }

        [Fact]
        public void MixedErrors()
        {
            var lexerRes10 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError10>>(),lang:"en");
            Assert.True(lexerRes10.IsError);
            Assert.Equal(4, lexerRes10.Errors.Count);
            var expectedErrors = new[]
            {
                "too many comment lexem", "too many multi-line comment lexem", "too many single-line comment lexem",
                "comment lexem can't be used together with single-line or multi-line comment lexems"
            };
            foreach (var expectedError in expectedErrors)
            {
                Assert.True(lexerRes10.Errors.Exists(x => x.Level == ErrorLevel.FATAL && x.Message == expectedError));
            }

            expectedErrors = new[] {"too many multi-line comment lexem", "too many single-line comment lexem"};

            var lexerRes9 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError9>>(),lang:"en");
            Assert.True(lexerRes9.IsError);
            Assert.Equal(2, lexerRes9.Errors.Count);
            foreach (var expectedError in expectedErrors)
            {
                Assert.True(lexerRes9.Errors.Exists(x => x.Level == ErrorLevel.FATAL && x.Message == expectedError));
            }

            expectedErrors = new[] {"too many multi-line comment lexem","comment lexem can't be used together with single-line or multi-line comment lexems"};
            
            var lexerRes8 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError8>>(),lang:"en");
            Assert.True(lexerRes8.IsError);
            Assert.Equal(2, lexerRes8.Errors.Count);
            foreach (var expectedError in expectedErrors)
            {
                Assert.True(lexerRes8.Errors.Exists(x => x.Level == ErrorLevel.FATAL && x.Message == expectedError));
            }

            expectedErrors = new[] {"too many single-line comment lexem" , "comment lexem can't be used together with single-line or multi-line comment lexems"};
            var lexerRes7 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError7>>());
            Assert.True(lexerRes7.IsError);
            Assert.Equal(2, lexerRes7.Errors.Count);
            foreach (var expectedError in expectedErrors)
            {
                Assert.True(lexerRes7.Errors.Exists(x => x.Level == ErrorLevel.FATAL && x.Message == expectedError));
            }
        }
    }
}