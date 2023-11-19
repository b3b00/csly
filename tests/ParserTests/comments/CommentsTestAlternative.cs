using NFluent;
using sly.buildresult;
using sly.lexer;
using Xunit;

namespace ParserTests.comments
{
    
    public enum CommentsTokenAlternative
    {
        [Lexeme(GenericToken.Int, channel:0)] INT,

        [Lexeme(GenericToken.Double, channel:0)] DOUBLE,

        [Lexeme(GenericToken.Identifier,channel:0)] ID,

        [SingleLineComment("//",channel:0)] [MultiLineComment("/*", "*/",channel:0)] COMMENT
    }
    
    public class CommentsTestAlternative
    {
        [Fact]
        public void NotEndingMultiComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenAlternative>>());
            Check.That(lexerRes).IsOk();
            var lexer = lexerRes.Result as GenericLexer<CommentsTokenAlternative>;

            var code = @"1
2 /* not ending
comment";

            var r = lexer.Tokenize(code);
            Check.That(r).IsOkLexing();
            var tokens = r.Tokens;
            Check.That(tokens).CountIs(4);


            var expectations = new []
            {
                (CommentsTokenAlternative.INT, "1", 0, 0),
                (CommentsTokenAlternative.INT, "2", 1, 0),
                (CommentsTokenAlternative.COMMENT, @" not ending
comment", 1, 2),
            };
            Check.That(tokens.Extracting(x => (x.TokenID, x.Value, x.Position.Line, x.Position.Column)))
                .Contains(expectations);
        }

        [Fact]
        public void TestGenericMultiLineComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenAlternative>>());
            Check.That(lexerRes).IsOk();
            var lexer = lexerRes.Result as GenericLexer<CommentsTokenAlternative>;

            var code = @"1
2 /* multi line 
comment on 2 lines */ 3.0";

            var r = lexer.Tokenize(code);
            Check.That(r).IsOkLexing();
            var tokens = r.Tokens;

            Check.That(tokens).CountIs(5);
            
            var expectations = new []
            {
                (CommentsTokenAlternative.INT, "1", 0, 0),
                (CommentsTokenAlternative.INT, "2", 1, 0),
                (CommentsTokenAlternative.COMMENT, @" multi line 
comment on 2 lines ", 1, 2),
                (CommentsTokenAlternative.DOUBLE, "3.0", 2, 22),
            };
            Check.That(tokens.Extracting(x => (x.TokenID, x.Value, x.Position.Line, x.Position.Column)))
                .Contains(expectations);
        }

        [Fact]
        public void TestGenericSingleLineComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenAlternative>>());
            Check.That(lexerRes).IsOk();
            var lexer = lexerRes.Result as GenericLexer<CommentsTokenAlternative>;

            var dump = lexer.ToString();

            var r = lexer.Tokenize(@"1
2 // single line comment
3.0");
            Check.That(r).IsOkLexing();
            var tokens = r.Tokens;

            Check.That(tokens).CountIs(5);

            var expectations = new []
            {
                (CommentsTokenAlternative.INT, "1", 0, 0),
                (CommentsTokenAlternative.INT, "2", 1, 0),
                (CommentsTokenAlternative.COMMENT, " single line comment", 1, 2),
                (CommentsTokenAlternative.DOUBLE, "3.0", 2, 0),
            };
            Check.That(tokens.Extracting(x => (x.TokenID, x.Value, x.Position.Line, x.Position.Column)))
                .Contains(expectations);

        }

        [Fact]
        public void TestInnerMultiComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenAlternative>>());
            Check.That(lexerRes).IsOk();
            var lexer = lexerRes.Result as GenericLexer<CommentsTokenAlternative>;

            var dump = lexer.ToString();

            var code = @"1
2 /* inner */ 3
4
            ";

            var r = lexer.Tokenize(code);
            Check.That(r).IsOkLexing();
            var tokens = r.Tokens;
            Check.That(tokens).CountIs(6);

            var expectations = new []
            {
                (CommentsTokenAlternative.INT, "1", 0, 0),
                (CommentsTokenAlternative.INT, "2", 1, 0),
                (CommentsTokenAlternative.COMMENT, " inner ", 1, 2),
                (CommentsTokenAlternative.INT, "3", 1, 14),
                (CommentsTokenAlternative.INT, "4", 2, 0),
            };
            Check.That(tokens.Extracting(x => (x.TokenID, x.Value, x.Position.Line, x.Position.Column)))
                .Contains(expectations);
        }

        [Fact]
        public void TestMixedEOLComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsTokenAlternative>>());
            Check.That(lexerRes).IsOk();
            var lexer = lexerRes.Result as GenericLexer<CommentsTokenAlternative>;

            var dump = lexer.ToString();
            var code = "1\n2\r\n/* multi line \rcomment on 2 lines */ 3.0";
            var r = lexer.Tokenize(code);
            Check.That(r).IsOkLexing();
            var tokens = r.Tokens;

            Check.That(tokens).CountIs(5);


            var expectations = new[]
            {
                (CommentsTokenAlternative.INT, "1", 0, 0),
                (CommentsTokenAlternative.INT, "2", 1, 0),
                (CommentsTokenAlternative.COMMENT, " multi line \rcomment on 2 lines ", 2, 0),
                (CommentsTokenAlternative.DOUBLE, "3.0", 3, 22),
            };
            Check.That(tokens.Extracting(x => (x.TokenID, x.Value, x.Position.Line, x.Position.Column)))
                .Contains(expectations);

        }
    }
}