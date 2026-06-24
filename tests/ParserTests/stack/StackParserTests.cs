using System;
using expressionparser;
using NFluent;
using ParserTests.Issue239;
using sly.lexer;
using sly.lexer.fluent;
using sly.parser;
using sly.parser.fluent;
using sly.parser.generator;
using sly.parser.parser;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;
using Xunit;


namespace ParserTests.stack;

public class Dumb
{
    
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
        
        var grammarParser = builder.BuildParser(instance, ParserType.EBNF_LL_STACK, start);
        Check.That(grammarParser).IsOk();
        var parser = grammarParser.Result;
       
        var r = parser.Parse(source,start);
        Check.That(r).IsOkParsing();
        string expected = r.Result.ToString();

        RuleParserType.ParserType = ParserType.LL_STACK;
        
        grammarParser = builder.BuildParser(instance, ParserType.EBNF_LL_STACK, start);
        Check.That(grammarParser).IsOk();
        parser = grammarParser.Result;
        
        r = parser.Parse(source, start);
        RuleParserType.ParserType = ParserType.LL_RECURSIVE_DESCENT;
        Check.That(r).IsOkParsing();
        var actual = r.Result.ToString();
        Check.That(actual).IsEqualTo(expected);
        
        
    }

    [Fact]
    public void TestIssue239()
    {
        string source = "INT[d] ID SEMI[d]";
        string start = "clauses";
        var ruleparser = new RuleParser<Issue239Lexer, object>();
        var parser = GetParser<EbnfTokenGeneric, GrammarNode<Issue239Lexer, object>>(ruleparser, ParserType.LL_RECURSIVE_DESCENT, "clauses");

        var r = parser.Parse(source, start);
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsInstanceOf < ClauseSequence<Issue239Lexer, object>>();
        var rule = r.Result as ClauseSequence<Issue239Lexer, object>;
        var expected = rule.Dump();
        
        parser = GetParser<EbnfTokenGeneric, GrammarNode<Issue239Lexer, object>>(ruleparser, ParserType.LL_STACK, "rule");

        r = parser.Parse(source, start);
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsInstanceOf < ClauseSequence<Issue239Lexer, object>>();
        rule = r.Result as ClauseSequence<Issue239Lexer, object>;
        var actual = rule.Dump();
        Check.That(actual).IsEqualTo(expected);
    }

    [Fact]
    public void TestEbnfZeroOrMore()
    {
        var ebnf = new SimpleEBNFMany();
        RuleParserType.ParserType = ParserType.LL_RECURSIVE_DESCENT;
        var parser = GetParser<L, string>(ebnf, ParserType.EBNF_LL_STACK, "root");

        Check.That(parser).IsNotNull();

        var parseResult = parser.Parse(" A  A  A A");
        Check.That(parseResult).IsOkParsing();
        var result = parseResult.Result;
        Check.That(result).IsEqualTo("A,A,A,A");
        
        parseResult = parser.Parse(" A  ");
        Check.That(parseResult).IsOkParsing();
        result = parseResult.Result;
        Check.That(result).IsEqualTo("A");
        
        parseResult = parser.Parse("   ");
        Check.That(parseResult).IsOkParsing();
        result = parseResult.Result;
        Check.That(result).IsEqualTo("");
    } 
    
    [Fact]
    public void TestEbnfOneOrMore()
    {
        var ebnf = new SimpleEBNFMany();
        RuleParserType.ParserType = ParserType.LL_RECURSIVE_DESCENT;
        var parser = GetParser<L, string>(ebnf, ParserType.EBNF_LL_STACK, "rootplus");

        Check.That(parser).IsNotNull();

        var parseResult = parser.Parse(" A  A  A A");
        Check.That(parseResult).IsOkParsing();
        var result = parseResult.Result;
        Check.That(result).IsEqualTo("A,A,A,A");
        
        parseResult = parser.Parse(" A  ");
        Check.That(parseResult).IsOkParsing();
        result = parseResult.Result;
        Check.That(result).IsEqualTo("A");
        
        parseResult = parser.Parse("   ");
        Check.That(parseResult).Not.IsOkParsing();
        Check.That(parseResult.Errors).CountIs(1);
        var error = parseResult.Errors[0];
        Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedEOS);
    }

    [Fact]
    void TestOptionTerminal()
    {
        var lexer = FluentLexerBuilder<L>.NewBuilder()
            .Keyword(L.A, "A")
            .Keyword(L.B, "B");
        
        var buildResult = FluentEBNFParserBuilder<L, string>.NewBuilder(new FluentTests(), "root", "en")
            .WithLexerbuilder(lexer)
            .Production("root : o", (object[] args) =>
            {
                return (string)args[0];
            })
            .Production("o : A B? A", (args) =>
            {
                var a1 = (Token<L>)args[0];
                var b = (Token<L>)args[1];
                var a2 = (Token<L>)args[0];
                if (b.IsEmpty)
                {
                    return "ah ah !";
                }
                else
                {
                    return "ABBA";
                }
            })           
            .BuildParser(ParserType.EBNF_LL_STACK);

        Check.That(buildResult).IsOk();
        var parser = buildResult.Result;
        var result = parser.Parse("A A");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("ah ah !");
        result = parser.Parse("A B A");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("ABBA");
    }
    
    [Fact]
    void TestOptionNonTerminal()
    {
        var lexer = FluentLexerBuilder<L>.NewBuilder()
            .Keyword(L.A, "A")
            .Keyword(L.B, "B");
        
        var buildResult = FluentEBNFParserBuilder<L, string>.NewBuilder(new FluentTests(), "root", "en")
            .WithLexerbuilder(lexer)
            .Production("root : o", (object[] args) =>
            {
                return (string)args[0];
            })
            .Production("o : a b? a", (args) =>
            {
                var a1 = (string)args[0];
                var b = (ValueOption<string>)args[1];
                var a2 = (string)args[0];
                if (b.IsNone)
                {
                    return "ah ah !";
                }
                else
                {
                    return "ABBA";
                }
            })
            .Production("a : A", (args =>
            {
                return "a";
            }))
            .Production("b : B", (args =>
            {
                return "b";
            }))
            .BuildParser(ParserType.EBNF_LL_STACK);

        Check.That(buildResult).IsOk();
        var parser = buildResult.Result;
        var result = parser.Parse("A A");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("ah ah !");
        result = parser.Parse("A B A");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("ABBA");
    }
    
    public static Parser<IN, OUT> GetParser<IN, OUT>(object instance, ParserType type, string root) where IN : struct , Enum
    {
        ParserBuilder<IN,OUT> builder =  new ParserBuilder<IN, OUT>();
        var built = builder.BuildParser(instance, type, root);
        Check.That(built).IsOk();
        Check.That(built.Result).IsNotNull();
        return built.Result;
    }
    
    
    [Fact]
    void TestChoiceTerminal()
    {
        RuleParserType.ParserType = ParserType.LL_RECURSIVE_DESCENT;
        var parser = GetParser<L, String>(new TerminalChoice(), ParserType.EBNF_LL_STACK, "root");

        Check.That(parser).IsNotNull();
        var result = parser.Parse("A");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("A");
        result = parser.Parse("B");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("B");
    }
    
    [Fact]
    void TestManyChoiceTerminal()
    {
        RuleParserType.ParserType = ParserType.LL_RECURSIVE_DESCENT;
        var parser = GetParser<L, String>(new ManyTerminalChoice(), ParserType.EBNF_LL_STACK, "root");

        Check.That(parser).IsNotNull();
        var result = parser.Parse("A");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("A");
        result = parser.Parse("A B B A");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("A,B,B,A");
        result = parser.Parse("");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("");
    }
    
    [Fact]
    void TestOptionChoiceTerminal()
    {
        RuleParserType.ParserType = ParserType.LL_RECURSIVE_DESCENT;
        var parser = GetParser<L, String>(new OptionTerminalChoice(), ParserType.EBNF_LL_STACK, "root");

        Check.That(parser).IsNotNull();
        // var result = parser.Parse("A");
        // Check.That(result).IsOkParsing();
        // Check.That(result.Result).IsEqualTo("A");
        // result = parser.Parse("B");
        // Check.That(result).IsOkParsing();
        // Check.That(result.Result).IsEqualTo("B");
        var result = parser.Parse("");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("nothing");
    }
    
    [Fact]
    void TestChoiceNonTerminal()
    {
        RuleParserType.ParserType = ParserType.LL_RECURSIVE_DESCENT;
        var parser = GetParser<L, String>(new NonTerminalChoice(), ParserType.EBNF_LL_STACK, "root");

        Check.That(parser).IsNotNull();
        var result = parser.Parse("A");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("A");
        result = parser.Parse("B");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("B");
    }

    [Fact]
    void TestBasicGroup()
    {
        RuleParserType.ParserType = ParserType.LL_RECURSIVE_DESCENT;
        var parser = GetParser<L, String>(new BasicGroup(), ParserType.EBNF_LL_STACK, "root");
        
        Check.That(parser).IsNotNull();
        var result = parser.Parse("A B");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("A B");
        result = parser.Parse("A A");
        Check.That(result).Not.IsOkParsing();
    }
    
    [Fact]
    void TestManyGroup()
    {
        RuleParserType.ParserType = ParserType.LL_RECURSIVE_DESCENT;
        var parser = GetParser<L, String>(new ManyGroup(), ParserType.EBNF_LL_STACK, "root");
        
        Check.That(parser).IsNotNull();
        var result = parser.Parse("A B");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("A B");
        result = parser.Parse("A B A B");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("A B,A B");
    }
    
    [Fact]
    void TestOptionGroup()
    {
        RuleParserType.ParserType = ParserType.LL_RECURSIVE_DESCENT;
        var parser = GetParser<L, String>(new OptionGroup(), ParserType.EBNF_LL_STACK, "root");
        
        Check.That(parser).IsNotNull();
        var result = parser.Parse("A B");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("A B");
        result = parser.Parse("");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("nothing");
    }
    
}