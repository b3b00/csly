using System;
using System.Collections.Generic;
using System.Linq;
using NFluent;
using ParserTests.Issue251;
using ParserTests.Issue260;
using SlowEOS;
using sly.buildresult;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using sly.parser.parser;
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
    
    public enum Issue277Tokens
    {
        [Lexeme(GenericToken.Identifier, IdentifierType.AlphaNumericDash)]
        IDENTIFIER,

        [Lexeme(GenericToken.KeyWord, "or")]
        OR
    }
    
    public class Issue277Parser
    {
        [Production("widget: IDENTIFIER")]
        public string Widget(Token<Issue277Tokens> widget)
        {
            return widget.Value;
        }

        [Production("expression: widget (OR [d] widget)+")]
        public string Expression(string widget, List<Group<Issue277Tokens, string>> ors)
        {
            return ors.Aggregate($"{widget}", (acc, a) => $"{acc} | {a.Value("widget")}");
        }
    }
    
    public class IssuesTests
    {

        [Fact]
        public static void Issue218()
        {
            var lexerResult = LexerBuilder.BuildLexer<Token218>();
            Check.That(lexerResult).IsOk();
            var lexer = lexerResult.Result;
            var result = lexer.Tokenize("a = 0.0;");
            Check.That(result).IsOkLexing();
            var tokens = result.Tokens;
            var dump = string.Join(" ", tokens.Select(x => x.ToString()));

            Check.That(tokens.Where(x => x.IsEOS)).IsSingle();
            Check.That(tokens.Where(x => x.ToString().Contains("<<EOS>>"))).IsSingle();
        }


        [Fact]
        public static void Issue219EBNF()
        {
            ParserBuilder<Issue219Lexer, I219Ast> builder = new ParserBuilder<Issue219Lexer, I219Ast>();
            Issue219ParserEBNF instance = new Issue219ParserEBNF();
            var bres = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            Check.That(bres).IsOk();
            var parser = bres.Result;
            var exception = Check.ThatCode(() => { parser.Parse("a = 1 b = 2 c = 3"); }).Throws<Exception219>().Value;
            Check.That(exception.Message).IsEqualTo("visitor error");
        }
      
        [Fact]
        public static void Issue219BNF()
        {
            ParserBuilder<Issue219Lexer, I219Ast> builder = new ParserBuilder<Issue219Lexer, I219Ast>();
            Issue219ParserBNF instance = new Issue219ParserBNF();
            var bres = builder.BuildParser(instance, ParserType.LL_RECURSIVE_DESCENT, "root");
            Check.That(bres).IsOk();
            var parser = bres.Result;
            var exception = Check.ThatCode(() => { parser.Parse("a = 1"); }).Throws<Exception219>().Value;
            Check.That(exception.Message).IsEqualTo("visitor error");
        }
        
        [Fact]
        public static void Issue251LeftrecForBNF() {
            ParserBuilder<Issue251Parser.Issue251Tokens,Issue251Parser.ExprClosure> builder = new ParserBuilder<Issue251Parser.Issue251Tokens, Issue251Parser.ExprClosure>();
            Issue251Parser instance = new Issue251Parser();
            var bres = builder.BuildParser(instance,ParserType.LL_RECURSIVE_DESCENT, "expr");
            Check.That(bres).Not.IsOk();
            Check.That(bres).HasError(ErrorCodes.PARSER_LEFT_RECURSIVE, "expr > expr");
        }

        [Fact]
        public static void Issue261Test()
        {
            var buildResult = LexerBuilder.BuildLexer<Issue261Lexer>();
            Check.That(buildResult).IsOk();
            var lexer = buildResult.Result;
            var lex = lexer.Tokenize(@"""test""");
            Check.That(lex).IsOkLexing();
            var tokens = lex.Tokens;
            Check.That(tokens).CountIs(2);
            Check.That(tokens[0]).IsEqualTo(Issue261Lexer.test, @"""test""");
        }

        [Fact]
        public static void Issue277Test()
        {
            var parserInstance = new Issue277Parser();
            var builder = new ParserBuilder<Issue277Tokens, string>();

            var result = builder
                .BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "expression");

            Check.That(result).IsOk();
            
            var parser = result.Result;
            
            var expression = "foo or bar or baz";
            
            var res = parser.Parse(expression);
            Check.That(res).IsOkParsing();
            var resAsString = res.Result;
            
            Check.That(resAsString).IsEqualTo("foo | bar | baz");
        }
        
        [Fact]
        public static void Issue493Test()
        { 
            ParserBuilder<SlowOnBadParseEosToken, object> builder = new ParserBuilder<SlowOnBadParseEosToken, object>();
            var buildParser = builder.BuildParser(new SlowOnBadParseEos(), ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            Check.That(buildParser).IsOk();
            var parser = buildParser.Result; 
            string source = "FUNCTIONCALL([Identifier]";
            Check.ThatCode(() =>
            {
                var result = parser.Parse(source);
                Check.That(result).IsOkParsing();
            }).LastsLessThan(10_000, TimeUnit.Milliseconds);

        }
    }

   
}