using System.Linq;
using NFluent;
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

        [SingleLineComment("//",channel:0)] COMMENT
    }
    
    public class SingleLineCommentsTest
    {
     

        [Fact]
        public void TestGenericMultiLineCommentWithSingleLineComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<SingleLineCommentsToken>>());
            Check.That(lexerRes).IsOk();
            
            var lexer = lexerRes.Result as GenericLexer<SingleLineCommentsToken>;

            var dump = lexer.ToString();

            var r =lexer?.Tokenize(@"1
2 /* multi line 
comment on 2 lines */ 3.0");
            Check.That(r.IsError).IsTrue();
            
            var tokens = r.Tokens;
            Check.That(r.Error.UnexpectedChar).IsEqualTo('*');
        }
        
        [Fact]
        public void TestCommentOnLastLine()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<SingleLineCommentsToken>>());
            Check.That(lexerRes).IsOk();
            
            var lexer = lexerRes.Result as GenericLexer<SingleLineCommentsToken>;

            var dump = lexer.ToString();

            var r =lexer?.Tokenize(@"
1.0
// this is the final comment");
            Check.That(r).IsOkLexing();
            var comment = r.Tokens[1];
            Check.That(comment).IsNotNull();
            Check.That(comment.TokenID).IsEqualTo(SingleLineCommentsToken.COMMENT);
            Check.That(comment.Value).Contains("this is the final comment");

        }

        [Fact]
        public void TestGenericSingleLineComment()
        {
            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<SingleLineCommentsToken>>());
            Check.That(lexerRes).IsOk();
            var lexer = lexerRes.Result as GenericLexer<SingleLineCommentsToken>;

            var dump = lexer.ToString();

            var r = lexer.Tokenize(@"1
2 // single line comment
3.0");
            Check.That(r.IsOk).IsTrue();
            var tokens = r.Tokens;

            Check.That(tokens).CountIs(5);
         
            var expectations = new (SingleLineCommentsToken UnexpectedTokenSyntaxError, string expectedValue, int line, int column)[]
            {
                (SingleLineCommentsToken.INT, "1", 0, 0),
                (SingleLineCommentsToken.INT, "2", 1, 0),
                (SingleLineCommentsToken.COMMENT, "single line comment", 1, 2),
                (SingleLineCommentsToken.DOUBLE, "3.0", 2, 0)
            };

            Check.That(tokens.Extracting(x => (x.TokenID, x.Value.Trim(), x.Position.Line, x.Position.Column)))
                .Contains(expectations);
            
            
        }

       
    }
}