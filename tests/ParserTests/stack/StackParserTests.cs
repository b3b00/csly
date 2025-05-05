using System;
using expressionparser;
using NFluent;
using sly.lexer;
using sly.lexer.fluent;
using sly.parser;
using sly.parser.fluent;
using sly.parser.generator;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;
using Xunit;


namespace ParserTests.stack;

public class Dumb
{
    
}

public enum P
{
    [Sugar("+")] e,
    [Keyword("true")] True,
    [Keyword("false")] False,
}

public enum L
{
    [Keyword("A")] A,
    [Keyword("B")] B,
    [Sugar("+")] PLUS,
    [Sugar("-")] MINUS,
}

public class Visitor : IDisposable {

    public void Dispose()
    {
        
        RuleParserType.ParserType = ParserType.LL_RECURSIVE_DESCENT;
    }
    
    [Production("root  : a [PLUS|MINUS] b")]
    public string Root(string a, Token<L> op, string b)
    {
        return "a <" + op.Value + "> b";
    }

    [Production("a : A")]
    public string A(Token<L> a)
    {
        return a.Value;
    }
    
    [Production("b : B")]
    public string B(Token<L> b)
    {
        return b.Value;
    }
}

[ParserRoot("root")]
public class SimplerStackParser
{
    [Production("root : expr")]
    public string root(string e) => e;

    [Production("expr : INT expr")]
    public string expr(Token<ExpressionToken> i, string e) => i.Value + "," + e;
    
    [Production("expr : INT")]
    public string expr2(Token<ExpressionToken> i) => i.Value;
    
}

public class StackParserTests
{

    [Fact]
    public void basic()
    {
        var instance = new SimplerStackParser();
        ParserBuilder<ExpressionToken, string> builder = new ParserBuilder<ExpressionToken, string>();
        var parser = builder.BuildParser(instance, ParserType.LL_STACK, "root");
        Check.That(parser).IsOk();
        var r = parser.Result.Parse("1");
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsEqualTo("1");
        r = parser.Result.Parse("1 2");
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsEqualTo("1,2");
        r = parser.Result.Parse("1 2 3 4 5");
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsEqualTo("1,2,3,4,5");
    }
    
    [Fact]
    public void expression()
    {
        var instance = new ExpressionParser();
        ParserBuilder<ExpressionToken, int> builder = new ParserBuilder<ExpressionToken, int>();
        var parser = builder.BuildParser(instance, ParserType.LL_STACK, "expression");
        Check.That(parser).IsOk();
        var r = parser.Result.Parse("1");
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsEqualTo(1);
        r = parser.Result.Parse("2 + 2");
        Check.That(r).IsOkParsing();
        //Check.That(r.Result).IsEqualTo(4);
        r = parser.Result.Parse("1 + 2 + 3 + 4 * 5");
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsEqualTo(1+2+3+4*5);
    }

    [Fact]
    public void basicFluent()
    {
        var lexer = FluentLexerBuilder<ExpressionToken>.NewBuilder()
            .Int(ExpressionToken.INT);
        var parser = FluentParserBuilder<ExpressionToken, string>.NewBuilder(new SimplerStackParser(), "root", "en")
            .WithLexerbuilder(lexer)
            .Production("root : expr", (object[] args) =>
            {
                return (string)args[0];
            })
            .Production("expr : INT", (args) =>
            {
                return ((Token<ExpressionToken>)args[0]).Value;
            })
            .Production("expr : INT expr", (args) =>
            {
                return ((Token<ExpressionToken>)args[0]).Value + "," + (string)args[1];
            })
            .BuildParser(ParserType.LL_RECURSIVE_DESCENT);
        Check.That(parser).IsOk();
        var result = parser.Result.Parse("1");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("1");
        result = parser.Result.Parse("1 2 3 4 5");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("1,2,3,4,5");
        


    }

    [Fact]
    public void RuleChoices()
    {
        var ruleparser = new RuleParser<P, object>();
        var builder = new ParserBuilder<EbnfTokenGeneric, GrammarNode<P, object>>("en");
        
        var grammarParser = builder.BuildParser(ruleparser, ParserType.LL_STACK, "rule");
        string source = "True|False";
        string start = "choices";
        Check.That(grammarParser).IsOk();
        var parser = grammarParser.Result;
        var r = parser.Parse(source, start);
        Check.That(r).IsOkParsing();
        var actual = r.Result.ToString();
        Check.That(actual).IsEqualTo("[ True(T) | False(T) ]");
        
    }
    
    [Fact]
    public void RuleChoices2()
    {
        var ruleparser = new RuleParser<L, string>();
        
        string source = "[ PLUS | MINUS ]";
        string start = "choiceclause";
        
        //
        // LL_RECURSIVE => OK => get expected output
        //
        
        var parser = GetParser<EbnfTokenGeneric, GrammarNode<L, string>>(ruleparser, ParserType.LL_RECURSIVE_DESCENT, "rule");
        // string source = "root  : A [PLUS|MINUS] B";
        // string start = "rule";
        
        var r = parser.Parse(source, start);
        Check.That(r).IsOkParsing();
        
        var tree = r.SyntaxTree;
        Check.That(r.SyntaxTree).IsInstanceOf<SyntaxNode<EbnfTokenGeneric, GrammarNode<L, string>>>();
        var root = r.SyntaxTree as SyntaxNode<EbnfTokenGeneric, GrammarNode<L, string>>;
        Check.That(root).IsNotNull();
        var expected = (r.Result as IClause<L, string>).Dump();
        
        
        //
        // LL_STACK => get output and compare to expected (if parse succeeded at all)
        //
        
        
        parser = GetParser<EbnfTokenGeneric, GrammarNode<L, string>>(ruleparser, ParserType.LL_STACK, "rule");
        
        r = parser.Parse(source, start);
        Check.That(r).IsOkParsing();
       
        var actualTreeDump = root.Dump("  ");
        var actual = (r.Result as IClause<L, string>).Dump();
        Check.That(actual).IsEqualTo(expected);
        
    }
    
    [Fact]
    public void RuleChoicesRule()
    {
        var ruleparser = new RuleParser<L, string>();
        
        string source = "rule : A  [ PLUS | MINUS ] B";
        string start = "rule";
        
        var parser = GetParser<EbnfTokenGeneric, GrammarNode<L, string>>(ruleparser, ParserType.LL_RECURSIVE_DESCENT, "rule");
        
        var r = parser.Parse(source, start);
        Check.That(r).IsOkParsing();
        
        var tree = r.SyntaxTree;
        Check.That(r.SyntaxTree).IsInstanceOf<SyntaxNode<EbnfTokenGeneric, GrammarNode<L, string>>>();
        var root = r.SyntaxTree as SyntaxNode<EbnfTokenGeneric, GrammarNode<L, string>>;
        Check.That(root).IsNotNull();
        var expected = (r.Result as Rule<L, string>).Dump();
        
        // LL_STACK => get output and compare to expected (if parse succeeded at all)

        parser = GetParser<EbnfTokenGeneric, GrammarNode<L, string>>(ruleparser, ParserType.LL_STACK, "rule");
        
        r = parser.Parse(source, start);
        Check.That(r).IsOkParsing();
       
        var actualTreeDump = root.Dump("  ");
        var actual = (r.Result as Rule<L, string>).Dump();
        Check.That(actual).IsEqualTo(expected);
        
    }

    [Fact]
    public void ChoicesVisitorTest()
    {
        var builder = new ParserBuilder<L, string>("en");
        var instance = new Visitor();
        
        string source = "A + B";
        string start = "root";
        RuleParserType.ParserType = ParserType.LL_RECURSIVE_DESCENT;
        
        var grammarParser = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, start);
        Check.That(grammarParser).IsOk();
        var parser = grammarParser.Result;
       
        var r = parser.Parse(source,start);
        Check.That(r).IsOkParsing();
        string expected = r.Result.ToString();

        RuleParserType.ParserType = ParserType.LL_STACK;
        
        grammarParser = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, start);
        Check.That(grammarParser).IsOk();
        parser = grammarParser.Result;
        
        r = parser.Parse(source, start);
        RuleParserType.ParserType = ParserType.LL_RECURSIVE_DESCENT;
        Check.That(r).IsOkParsing();
        var actual = r.Result.ToString();
        Check.That(actual).IsEqualTo(expected);
        
        
    }
    
    public static Parser<IN, OUT> GetParser<IN, OUT>(object instance, ParserType type, string root) where IN : struct , Enum
    {
        ParserBuilder<IN,OUT> builder =  new ParserBuilder<IN, OUT>();
        var built = builder.BuildParser(instance, type, root);
        Check.That(built).IsOk();
        Check.That(built.Result).IsNotNull();
        return built.Result;
    }
}