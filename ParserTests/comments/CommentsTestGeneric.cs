using NFluent;
using sly.buildresult;
using sly.lexer;
using Xunit;

namespace ParserTests.comments
{
    
    public enum CommentsToken
    {
        [Lexeme(GenericToken.Int,channel:0)] INT,

        [Lexeme(GenericToken.Double,channel:0)] DOUBLE,

        [Lexeme(GenericToken.Identifier,channel:0)] ID,

        [Comment("//", "/*", "*/",channel:0)] COMMENT
    }
    
    public class CommentsTestGeneric
    {
        [Fact]
        public void NotEndingMultiComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsToken>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result as GenericLexer<CommentsToken>;

            var dump = lexer.ToString();

            var code = @"1
2 /* not ending
comment";

            var r = lexer.Tokenize(code);
            Check.That(r.IsOk).IsTrue();
            var tokens = r.Tokens;

            Check.That(tokens).CountIs(4);

            var expectations = new (CommentsToken token, string Value, int line, int column)[]
            {
                (CommentsToken.INT, "1", 0, 0),
                (CommentsToken.INT, "2", 1, 0),
                (CommentsToken.COMMENT, @" not ending
comment", 1, 2)
            };

            Check.That(tokens.Extracting(x => (x.TokenID, x.Value, x.Position.Line, x.Position.Column)))
                .Contains(expectations);
       
        }
        
        [Fact]
        public void EmptySingleLineComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsToken>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result as GenericLexer<CommentsToken>;

            var dump = lexer.ToString();

            var code = @"//
1 
//
2 
//";

            var r = lexer.Tokenize(code);
            Check.That(r.IsOk).IsTrue();
            var tokens = r.Tokens;

            Check.That(tokens).CountIs(6);

            var expectations = new (CommentsToken token, string Value, int line, int column)[]
            {
                (CommentsToken.COMMENT, @"", 0, 0),
                (CommentsToken.INT, "1", 1, 0),
                (CommentsToken.COMMENT, @"", 2, 0),
                (CommentsToken.INT, "2", 3, 0),
                (CommentsToken.COMMENT, @"", 4, 0)
            };
            
            

            Check.That(tokens.Extracting(x => (x.TokenID, x.Value, x.Position.Line, x.Position.Column)))
                .Contains(expectations);
       
        }

        [Fact]
        public void TestGenericMultiLineComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsToken>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result as GenericLexer<CommentsToken>;


            var dump = lexer.ToString();

            var code = @"1
2 /* multi line 
comment on 2 lines */ 3.0";

            var r = lexer.Tokenize(code);
            Check.That(r.IsOk).IsTrue();
            var tokens = r.Tokens;

            Check.That(tokens).CountIs(5);

            var expectations = new (CommentsToken token, string Value, int line, int column)[]
            {
                (CommentsToken.INT, "1", 0, 0),
                (CommentsToken.INT, "2", 1, 0),
                (CommentsToken.COMMENT, @" multi line 
comment on 2 lines ", 1, 2),

                (CommentsToken.DOUBLE, "3.0", 2, 22)
            };
            
            Check.That(tokens.Extracting(x => (x.TokenID, x.Value, x.Position.Line, x.Position.Column)))
                .Contains(expectations);
        }

        [Fact]
        public void TestGenericSingleLineComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsToken>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result as GenericLexer<CommentsToken>;

            var dump = lexer.ToString();


            var r = lexer.Tokenize(@"1
2 // single line comment
3.0");
            Check.That(r.IsOk).IsTrue();
            var tokens = r.Tokens;

            Check.That(tokens).CountIs(5);

            var expectations = new (CommentsToken token, string Value, int line, int column)[]
            {
                (CommentsToken.INT, "1", 0, 0),
                (CommentsToken.INT, "2", 1, 0),
                (CommentsToken.COMMENT, "single line comment", 1, 2),
                (CommentsToken.DOUBLE, "3.0", 2, 0)
            };
            
            Check.That(tokens.Extracting(x => (x.TokenID, x.Value.Trim(), x.Position.Line, x.Position.Column)))
                .Contains(expectations);
        }

        [Fact]
        public void TestInnerMultiComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsToken>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result as GenericLexer<CommentsToken>;


            var dump = lexer.ToString();

            var code = @"1
2 /* inner */ 3
4
            ";

            var r = lexer.Tokenize(code);
            Check.That(r.IsOk).IsTrue();
            var tokens = r.Tokens;

            Check.That(tokens).CountIs(6);

            var expectations = new (CommentsToken token, string Value, int line, int column)[]
            {
                (CommentsToken.INT, @"1", 0, 0),
                (CommentsToken.INT, @"2", 1, 0),
                (CommentsToken.COMMENT, @" inner ", 1, 2),
                (CommentsToken.INT, "3", 1, 14),
                (CommentsToken.INT, "4", 2, 0)
            };
            
            Check.That(tokens.Extracting(x => (x.TokenID, x.Value, x.Position.Line, x.Position.Column)))
                .Contains(expectations);
        }

        [Fact]
        public void TestMixedEOLComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<CommentsToken>>());
            Check.That(lexerRes.IsError).IsFalse();
            var lexer = lexerRes.Result as GenericLexer<CommentsToken>;


            var dump = lexer.ToString();
            var code = "1\n2\r\n/* multi line \rcomment on 2 lines */ 3.0";
            
            var r = lexer.Tokenize(code);
            Check.That(r.IsOk).IsTrue();
            var tokens = r.Tokens;

            Check.That(tokens).CountIs(5);

            var expectations = new (CommentsToken token, string Value, int line, int column)[]
            {
                (CommentsToken.INT, "1", 0, 0),
                (CommentsToken.INT, "2", 1, 0),
                (CommentsToken.COMMENT, " multi line \rcomment on 2 lines ", 2, 0),
                (CommentsToken.DOUBLE, "3.0", 3, 22)
            };
            
            Check.That(tokens.Extracting(x => (x.TokenID, x.Value, x.Position.Line, x.Position.Column)))
                .Contains(expectations);
        }
    }
}