using System;
using System.Collections.Generic;
using System.Linq;
using ParserTests.Issue251;
using ParserTests.Issue260;
using sly.buildresult;
using sly.lexer;
using sly.parser.generator;
using Xunit;

namespace ParserTests
{
    
    public enum Token218
    {
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


    public enum Issue219Lexer
    {
        [Lexeme(GenericToken.Identifier, IdentifierType.Alpha)]
        ID = 1,

        [Lexeme(GenericToken.Int)] 
        INT = 2,

        [Lexeme(GenericToken.SugarToken, "=")] 
        EQ = 3 
    }
    public interface I219Ast {
    
    }
    
    public class Root219 : I219Ast{
        public List<I219Ast> Sets { get; set; }
    }

    public class Set219 : I219Ast
    {
        public string Id { get; set; }
        public int Value { get; set; }

        public Set219(string id, int value)
        {
            Id = id;
            Value = value;
        }
    }

    public class Exception219 : Exception
    {
        public Exception219(string error) : base(error)
        {
            
        }
    }
        
    public class Issue219ParserEBNF
    {
        [Production("root: set*")]
        public I219Ast root(List<I219Ast> sets)
        {
            return new Root219() {Sets = sets};
        }

        [Production("set : ID EQ[d] INT")]
        public Set219 set(Token<Issue219Lexer> id, Token<Issue219Lexer> value)
        {
            throw new Exception219("visitor error");
        }
    }
    
    public class Issue219ParserBNF
    {
        [Production("root: set")]
        public I219Ast root(I219Ast set)
        {
            return new Root219() {Sets = new List<I219Ast>(){set}};
        }

        [Production("set : ID EQ INT")]
        public Set219 set(Token<Issue219Lexer> id, Token<Issue219Lexer> eq, Token<Issue219Lexer> value)
        {
            throw new Exception219("visitor error");
        }
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
            var dump = string.Join(" ", tokens.Select(x => x.ToString()));
            int n = 0;
            int count = 0;
            while ((n = dump.IndexOf("<<EOS>>", n)) != -1)
            {
                n++;
                count++;
            }
            Assert.Equal(1, count);
            var eoss = tokens.Where(x => x.IsEOS);
            Assert.Single(eoss);
        }


        [Fact]
        public static void Issue219EBNF()
        {
            ParserBuilder<Issue219Lexer, I219Ast> builder = new ParserBuilder<Issue219Lexer, I219Ast>();
            Issue219ParserEBNF instance = new Issue219ParserEBNF();
            var bres = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            Assert.True(bres.IsOk);
            var parser = bres.Result;
            Assert.NotNull(parser);
            var exception = Assert.Throws<Exception219>(() => parser.Parse("a = 1 b = 2 c = 3"));
            Assert.Equal("visitor error",exception.Message);                
        }
      
        [Fact]
        public static void Issue219BNF()
        {
            ParserBuilder<Issue219Lexer, I219Ast> builder = new ParserBuilder<Issue219Lexer, I219Ast>();
            Issue219ParserBNF instance = new Issue219ParserBNF();
            var bres = builder.BuildParser(instance, ParserType.LL_RECURSIVE_DESCENT, "root");
            Assert.True(bres.IsOk);
            var parser = bres.Result;
            Assert.NotNull(parser);
            var exception = Assert.Throws<Exception219>(() => parser.Parse("a = 1"));
            Assert.Equal("visitor error",exception.Message);                
        }
        
        [Fact]
        public static void Issue251LeftrecForBNF() {
            ParserBuilder<Issue251Parser.Issue251Tokens,Issue251Parser.ExprClosure> builder = new ParserBuilder<Issue251Parser.Issue251Tokens, Issue251Parser.ExprClosure>();
            Issue251Parser instance = new Issue251Parser();
            var bres = builder.BuildParser(instance,ParserType.LL_RECURSIVE_DESCENT, "expr");
            Assert.False(bres.IsOk);
            Assert.Single(bres.Errors);
            var error = bres.Errors.First();
            Assert.Equal(ErrorCodes.PARSER_LEFT_RECURSIVE, error.Code);
        }

        [Fact]
        public static void Issue261Test()
        {
            var buildResult = LexerBuilder.BuildLexer<Issue261Lexer>();
            Assert.True(buildResult.IsOk);
            Assert.NotNull(buildResult.Result);
            var lexer = buildResult.Result;
            var lex = lexer.Tokenize(@"""test""");
            Assert.True(lex.IsOk);
            var tokens = lex.Tokens;
            Assert.NotNull(tokens);
            Assert.Equal(2,tokens.Count);
            Assert.Equal("test",tokens[0].StringWithoutQuotes);


        }
    }
}