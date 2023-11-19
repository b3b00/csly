using NFluent;
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
            Check.That(lexerRes6).Not.IsOk();
            Check.That(lexerRes6.Errors).CountIs(1);
            var expectedErrors = new[]
            {
                "too many comment lexem"
            };

            Check.That(lexerRes6.Errors.Extracting(x => x.Message)).Contains(expectedErrors);
            Check.That(lexerRes6.Errors.Extracting(x => x.Level)).Contains(ErrorLevel.FATAL);
            
            

            var lexerRes5 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError5>>());
            Check.That(lexerRes5).Not.IsOk();
            Check.That(lexerRes5.Errors).CountIs(1);
            expectedErrors = new[]
            {
                "too many multi-line comment lexem"
            };
            Check.That(lexerRes5.Errors.Extracting(x => x.Message)).Contains(expectedErrors);
            Check.That(lexerRes5.Errors.Extracting(x => x.Level)).Contains(ErrorLevel.FATAL);
            

            var lexerRes4 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError4>>());
            Check.That(lexerRes4).Not.IsOk();
            Check.That(lexerRes4.Errors).CountIs(1);
            expectedErrors = new[]
            {
                "too many single-line comment lexem"
            };
            Check.That(lexerRes4.Errors.Extracting(x => x.Message)).Contains(expectedErrors);
            Check.That(lexerRes4.Errors.Extracting(x => x.Level)).Contains(ErrorLevel.FATAL);

        }

        [Fact]
        public void RedundantAttributes()
        {
            var lexerRes3 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError3>>());
            Check.That(lexerRes3).Not.IsOk();
            Check.That(lexerRes3.Errors).CountIs(1);
            Check.That(lexerRes3.Errors[0].Level).IsEqualTo(ErrorLevel.FATAL);
            Check.That(lexerRes3.Errors[0].Message).IsEqualTo("comment lexem can't be used together with single-line or multi-line comment lexems");

            var lexerRes2 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError2>>());
            Check.That(lexerRes2).Not.IsOk();
            Check.That(lexerRes2.Errors).CountIs(1);
            Check.That(lexerRes2.Errors[0].Level).IsEqualTo(ErrorLevel.FATAL);
            Check.That(lexerRes2.Errors[0].Message).IsEqualTo("comment lexem can't be used together with single-line or multi-line comment lexems");

            var lexerRes1 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError1>>());
            Check.That(lexerRes1).Not.IsOk();
            Check.That(lexerRes1.Errors).CountIs(1);
            Check.That(lexerRes1.Errors[0].Level).IsEqualTo(ErrorLevel.FATAL);
            Check.That(lexerRes1.Errors[0].Message).IsEqualTo("comment lexem can't be used together with single-line or multi-line comment lexems");
        }

        [Fact]
        public void MixedErrors()
        {
            var lexerRes10 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError10>>(),lang: "en");
            Check.That(lexerRes10).Not.IsOk();
            Check.That(lexerRes10.Errors).CountIs(4);
            (string message,ErrorLevel level)[] expectedErrors = new (string message,ErrorLevel level)[]
            {
                ("too many comment lexem", ErrorLevel.FATAL),
                ("too many multi-line comment lexem", ErrorLevel.FATAL),
                ("too many single-line comment lexem", ErrorLevel.FATAL),
                ("comment lexem can't be used together with single-line or multi-line comment lexems", ErrorLevel.FATAL)
            };
            
            Check.That(lexerRes10.Errors.Extracting(x => (x.Message, x.Level))).Contains(expectedErrors);

            expectedErrors = new[] {("too many multi-line comment lexem", ErrorLevel.FATAL), ("too many single-line comment lexem", ErrorLevel.FATAL)};

            var lexerRes9 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError9>>(),lang: "en");
            Check.That(lexerRes9).Not.IsOk();
            Check.That(lexerRes9.Errors).CountIs(2);
            Check.That(lexerRes9.Errors.Extracting(x => (x.Message, x.Level))).Contains(expectedErrors);

            expectedErrors = new[] {
                ("too many multi-line comment lexem", ErrorLevel.FATAL),
                ("comment lexem can't be used together with single-line or multi-line comment lexems", ErrorLevel.FATAL)
            };
            
            var lexerRes8 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError8>>(),lang: "en");
            Check.That(lexerRes8).Not.IsOk();
            Check.That(lexerRes8.Errors).CountIs(2);
            Check.That(lexerRes8.Errors.Extracting(x => (x.Message, x.Level))).Contains(expectedErrors);

            expectedErrors = new[] {
                ("too many single-line comment lexem", ErrorLevel.FATAL) , 
                ("comment lexem can't be used together with single-line or multi-line comment lexems", ErrorLevel.FATAL)
            };
            var lexerRes7 = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenError7>>());
            Check.That(lexerRes7).Not.IsOk();
            Check.That(lexerRes7.Errors).CountIs(2);
            Check.That(lexerRes7.Errors.Extracting(x => (x.Message, x.Level))).Contains(expectedErrors);
        }
    }
}