using System;
using System.Linq;
using sly.lexer;
using Xunit;

namespace ParserTests
{
    
    public enum Token218
    {
        EOS = 0,
        [Lexeme(GenericToken.Double)]
        DOUBLE,
        [Lexeme(GenericToken.Int)]
        INT,
        [Lexeme(GenericToken.String, "\"", "\\")]
        STRING,
        [Lexeme(GenericToken.Identifier, IdentifierType.AlphaNumeric)]
        IDENTIFIER,
        [Lexeme(GenericToken.SugarToken, ";")]
        SEMICOLON,
        [Lexeme(GenericToken.SugarToken, ",")]
        COMMA,
        [Lexeme(GenericToken.SugarToken, "=")]
        ASSIGNMENT,
        [Lexeme(GenericToken.SugarToken, "(")]
        LPAREN,
        [Lexeme(GenericToken.SugarToken, ")")]
        RPAREN,
        [Lexeme(GenericToken.SugarToken, "{")]
        LBRACE,
        [Lexeme(GenericToken.SugarToken, "}")]
        RBRACE,
        [Lexeme(GenericToken.SugarToken, "[")]
        LBRACKET,
        [Lexeme(GenericToken.SugarToken, "]")]
        RBRACKET,
        [Lexeme(GenericToken.SugarToken, ">")]
        GREATER,
        [Lexeme(GenericToken.SugarToken, "<")]
        LESSER,
        [Lexeme(GenericToken.SugarToken, "==")]
        DOUBLEEQUALS,
        [Lexeme(GenericToken.SugarToken, "!=")]
        DIFFERENT,
        [Lexeme(GenericToken.SugarToken, "+")]
        PLUS,
        [Lexeme(GenericToken.SugarToken, "-")]
        MINUS,
        [Lexeme(GenericToken.SugarToken, "*")]
        TIMES,
        [Lexeme(GenericToken.SugarToken, "/")]
        DIVIDE,
        [Lexeme(GenericToken.SugarToken, "%")]
        MODULUS,
        [Lexeme(GenericToken.SugarToken, "++")]
        PLUSPLUS,
        [Lexeme(GenericToken.SugarToken, "--")]
        MINUSMINUS,
        [Lexeme(GenericToken.KeyWord, "if")]
        IF,
        [Lexeme(GenericToken.KeyWord, "else")]
        ELSE,
        [Lexeme(GenericToken.KeyWord, "loop")]
        LOOP,
        [Lexeme(GenericToken.KeyWord, "while")]
        WHILE,
        [Lexeme(GenericToken.KeyWord, "true")]
        TRUE,
        [Lexeme(GenericToken.KeyWord, "false")]
        FALSE,
        [Lexeme(GenericToken.KeyWord, "break")]
        BREAK,
        [Lexeme(GenericToken.KeyWord, "continue")]
        CONTINUE,
        [Lexeme(GenericToken.KeyWord, "return")]
        RETURN,
        [Lexeme(GenericToken.KeyWord, "function")]
        FUNCTION,
        [Lexeme(GenericToken.KeyWord, "import")]
        IMPORT,
        [Lexeme(GenericToken.KeyWord, "new")]
        NEW,
        [Comment("//", "/*", "*/")]
        COMMENT
    }
    
    public class IssuesTests
    {

        [Fact]
        public static void Issue218()
        {
            var lexerResult = LexerBuilder.BuildLexer<Token218>();
            Assert.True(lexerResult.IsOk);
            var lexer = lexerResult.Result;
            var result = lexer.Tokenize("a = 0.0;");
            Assert.True(result.IsOk);
            var tokens = result.Tokens;
            Console.WriteLine(string.Join(" ",tokens.Select(x => x.ToString())));
        }
        
    }
}