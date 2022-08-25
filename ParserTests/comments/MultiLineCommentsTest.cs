using System.Linq;
using NFluent;
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

        [MultiLineComment("/*", "*/",channel:0)] COMMENT = 4
    }
    
    public class MultiLineCommentsTest
    {
        [Fact]
        public void NotEndingMultiComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<MultiLineCommentsToken>>());
            Check.That(lexerRes).IsOk();
            var lexer = lexerRes.Result as GenericLexer<MultiLineCommentsToken>;


            var dump = lexer.ToString();

            var code = @"1
2 /* not ending
comment";

            var r = lexer.Tokenize(code);
            Check.That(r).IsOkLexing();
            
            var tokens = r.Tokens;
            Check.That(tokens).CountIs(4);

            var expectations = new[]
            {
                (MultiLineCommentsToken.INT, "1", 0, 0),
                (MultiLineCommentsToken.INT, "2", 1, 0),
                (MultiLineCommentsToken.COMMENT, @" not ending
comment", 1, 2)
            };

            Check.That(tokens.Extracting(x => (x.TokenID, x.Value, x.Position.Line, x.Position.Column)))
                .Contains(expectations);
        }

        [Fact]
        public void TestGenericMultiLineComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<MultiLineCommentsToken>>());
            Check.That(lexerRes).IsOk();
            var lexer = lexerRes.Result as GenericLexer<MultiLineCommentsToken>;


            var dump = lexer.ToString();

            var code = @"1
2 /* multi line 
comment on 2 lines */ 3.0";

            var r = lexer.Tokenize(code);
            Check.That(r).IsOkLexing();
            var tokens = r.Tokens;

            Check.That(tokens).CountIs(5);

            var expectations = new[]
            {
                (MultiLineCommentsToken.INT, "1", 0, 0),
                (MultiLineCommentsToken.INT, "2", 1, 0),
                (MultiLineCommentsToken.COMMENT, @" multi line 
comment on 2 lines ", 1, 2),
                (MultiLineCommentsToken.DOUBLE, "3.0", 2, 22)
            };

            Check.That(tokens.Extracting(x => (x.TokenID, x.Value, x.Position.Line, x.Position.Column)))
                .Contains(expectations);
        }

        [Fact]
        public void TestGenericSingleLineComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<MultiLineCommentsToken>>());
            Check.That(lexerRes).IsOk();
            var lexer = lexerRes.Result as GenericLexer<MultiLineCommentsToken>;

            var dump = lexer.ToString();

            var r = lexer.Tokenize(@"1
2 // single line comment
3.0");
            Check.That(r).Not.IsOkLexing();
            
            Check.That(r.Error.UnexpectedChar).IsEqualTo('/');
        }

        [Fact]
        public void TestInnerMultiComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<MultiLineCommentsToken>>());
            Check.That(lexerRes).IsOk();
            var lexer = lexerRes.Result as GenericLexer<MultiLineCommentsToken>;

            var dump = lexer.ToString();

            var code = @"1
2 /* inner */ 3
4
            ";

            var r = lexer.Tokenize(code);
            Check.That(r).IsOkLexing();
            var tokens = r.Tokens;
            Check.That(tokens).CountIs(6);

            var expectations = new[]
            {
                (MultiLineCommentsToken.INT, "1", 0, 0),
                (MultiLineCommentsToken.INT, "2", 1, 0),
                (MultiLineCommentsToken.COMMENT, " inner ", 1, 2),
                (MultiLineCommentsToken.INT, "3", 1, 14),
                (MultiLineCommentsToken.INT, "4", 2, 0),
            };

            Check.That(tokens.Extracting(x => (x.TokenID, x.Value, x.Position.Line, x.Position.Column)))
                .Contains(expectations);
        }

        [Fact]
        public void TestMixedEOLComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<MultiLineCommentsToken>>());
            Check.That(lexerRes).IsOk();
            var lexer = lexerRes.Result as GenericLexer<MultiLineCommentsToken>;

            var dump = lexer.ToString();
            var code = "1\n2\r\n/* multi line \rcomment on 2 lines */ 3.0";
            var r = lexer.Tokenize(code);
            Check.That(r).IsOkLexing();;
            var tokens = r.Tokens;

            Check.That(tokens).CountIs(5);

            var expectations = new[]
            {
                (MultiLineCommentsToken.INT, "1", 0, 0),
                (MultiLineCommentsToken.INT, "2", 1, 0),
                (MultiLineCommentsToken.COMMENT, " multi line \rcomment on 2 lines ", 2, 0),
                (MultiLineCommentsToken.DOUBLE, "3.0", 3, 22)
            };

            Check.That(tokens.Extracting(x => (x.TokenID, x.Value, x.Position.Line, x.Position.Column)))
                .Contains(expectations);
        }
    }
}