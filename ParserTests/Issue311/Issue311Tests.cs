using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using NFluent;
using sly.lexer;
using sly.parser.generator.visitor;
using Xunit;

namespace ParserTests.Issue311;

public class Issue311Tests
{
    [Fact]
    public void TestIssue311()
    {
        var parser = Parser311.GetParser();
        var constantResult = parser.Parse("123.456");
// result1.Result is a ConstantExpression with Value=123456D
// -> the decimal separator was ignored?
        Check.That(constantResult).IsOkParsing();
        Check.That(constantResult.Result).IsInstanceOf<ConstantExpression>();
        var constant = constantResult.Result as ConstantExpression;
        Check.That(constant.Value).IsInstanceOf<double>();
        Check.That((double)(constant.Value)).IsEqualTo(123.456);
        var equalResult = parser.Parse("'str1' eq 'str2'");
        Check.That(equalResult).IsOkParsing();
        
        var binary = equalResult.Result as BinaryExpression;
        Check.That(binary).IsNotNull();
        Check.That(binary.Left).IsInstanceOf<ConstantExpression>();
        Check.That((binary.Left as ConstantExpression).Value.ToString()).IsEqualTo("str1");
        Check.That(binary.Right).IsInstanceOf<ConstantExpression>();
        Check.That((binary.Right as ConstantExpression).Value.ToString()).IsEqualTo("str2");
        Check.That(binary.Method).IsNotNull();
        Check.That(binary.Method.Name).IsEqualTo("op_Equality");
    }

    [Fact]
    public void TestIssue311StringToken()
    {
        var lexRes = LexerBuilder.BuildLexer<Token311>();
        Check.That(lexRes).IsOk();
        var lexer = lexRes.Result;
        var fsm = (lexer as GenericLexer<Token311>).ToGraphViz();
        
        var expectations = new List<(Token311, string)>()
        {
            (Token311.STRING, "str1"),
            (Token311.EQ, "eq"),
            (Token311.STRING, "str2"),
        };
        
        var lexed = lexer.Tokenize("'str1' eq 'str2'");
        Check.That(lexed).IsOkLexing();
        var tokens = lexed.Tokens;
        Check.That(tokens.Tokens).Not.IsNullOrEmpty();
        Check.That(tokens.Tokens).CountIs(4);

        var z = tokens.Tokens.Take(3).Extracting(x => (x.TokenID, x.StringWithoutQuotes)).ToList();
        
        Check.That(tokens.Tokens.Take(3).Extracting(x => (x.TokenID, x.StringWithoutQuotes))).ContainsExactly(expectations);
        
        expectations = new List<(Token311, string)>()
        {
            (Token311.STRING, "str'1"),
            (Token311.EQ, "eq"),
            (Token311.STRING, "str2"),
        };
        lexed = lexer.Tokenize("'str''1' eq 'str2'");
        Check.That(lexed).IsOkLexing();
        tokens = lexed.Tokens;
        Check.That(tokens.Tokens).Not.IsNullOrEmpty();
        Check.That(tokens.Tokens).CountIs(4);
        Check.That(tokens.Tokens.Take(3).Extracting(x => (x.TokenID, x.StringWithoutQuotes))).ContainsExactly(expectations);
    }
}