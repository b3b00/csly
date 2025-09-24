using System.Linq;
using NFluent;
using ParserTests.Issue218;
using ParserTests.Issue219;
using ParserTests.Issue251;
using ParserTests.Issue260;
using ParserTests.Issue277;
using ParserTests.Issue536;
using ParserTests.Issue538;
using ParserTests.Issue540;
using SlowEOS;
using sly.buildresult;
using sly.lexer;
using sly.parser.generator;
using Xunit;

namespace ParserTests
{
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


        [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public static void Issue219EBNF(ParserType parserType)
        {
            ParserBuilder<Issue219Lexer, I219Ast> builder = new ParserBuilder<Issue219Lexer, I219Ast>();
            Issue219ParserEBNF instance = new Issue219ParserEBNF();
            var bres = builder.BuildParser(instance, parserType, "root");
            Check.That(bres).IsOk();
            var parser = bres.Result;
            var exception = Check.ThatCode(() => { parser.Parse("a = 1 b = 2 c = 3"); }).Throws<Exception219>().Value;
            Check.That(exception.Message).IsEqualTo("visitor error");
        }
      
        [Theory]
        [InlineData(ParserType.LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.LL_STACK)]
        public static void Issue219BNF(ParserType parserType)
        {
            ParserBuilder<Issue219Lexer, I219Ast> builder = new ParserBuilder<Issue219Lexer, I219Ast>();
            Issue219ParserBNF instance = new Issue219ParserBNF();
            var bres = builder.BuildParser(instance, parserType, "root");
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

        [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public static void Issue277Test(ParserType parserType)
        {
            var parserInstance = new Issue277Parser();
            var builder = new ParserBuilder<Issue277Tokens, string>();

            var result = builder
                .BuildParser(parserInstance, parserType, "expression");

            Check.That(result).IsOk();
            
            var parser = result.Result;
            
            var expression = "foo or bar or baz";
            
            var res = parser.Parse(expression);
            Check.That(res).IsOkParsing();
            var resAsString = res.Result;
            
            Check.That(resAsString).IsEqualTo("foo | bar | baz");
        }
        
        [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public static void Issue493Test(ParserType parserType)
        { 
            ParserBuilder<SlowOnBadParseEosToken, object> builder = new ParserBuilder<SlowOnBadParseEosToken, object>();
            var buildParser = builder.BuildParser(new SlowOnBadParseEos(), parserType, "root");
            Check.That(buildParser).IsOk();
            Check.That(buildParser.Errors).IsEmpty();
            var parser = buildParser.Result; 
            string source = "FUNCTIONCALL([Identifier]";
            Check.ThatCode(() =>
            {
                var result = parser.Parse(source);
                Check.That(result).IsOkParsing();
            }).LastsLessThan(60_000, TimeUnit.Milliseconds);

        }

        [Fact]
        public static void Issue536Test()
        {
            var lexerBuild = LexerBuilder.BuildLexer<Token536>();
            Check.That(lexerBuild).IsOk();
            var lexer = lexerBuild.Result;
            var lexerResult = lexer.Tokenize("=");
            Check.That(lexerResult).IsOkLexing();
            var tokens = lexerResult.Tokens;
            Check.That(tokens).CountIs(2);
            var equals = tokens[0];
            Check.That(equals).IsNotNull();
            Check.That(equals.TokenID).IsEqualTo(Token536.Equals);
        }


        [Fact]
        public static void Issue538Test()
        {
            ParserBuilder<Issue538Token, object> builder = new ParserBuilder<Issue538Token, object>();
            var buildParser = builder.BuildParser(new Issue538Parser(), ParserType.EBNF_LL_RECURSIVE_DESCENT, "NTSection");
            Check.That(buildParser).IsOk();
        }

   
        [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public static void Issue540Test(ParserType parserType)
        {
            ParserBuilder<Issue540Token, object> builder = new ParserBuilder<Issue540Token, object>();
            var buildParser = builder.BuildParser(new Issue540Parser(), parserType, "NtSCExpr");
            Check.That(buildParser).IsOk();
            
            var parser = buildParser.Result;
            var parsed = parser.Parse("1;");            
            Check.That(parsed).IsOkParsing(checkIfResultisNull:false);
        }

    }

   
}