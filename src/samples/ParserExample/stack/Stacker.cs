using System;
using System.Collections.Generic;
using System.Linq;
using expressionparser;
using indented;
using NFluent;
using ParserTests;
using ParserTests.Issue239;
using ParserTests.stack;
using sly.lexer;
using sly.lexer.fluent;
using sly.parser;
using sly.parser.fluent;
using sly.parser.generator;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;
using postProcessedLexerParser.expressionModel;

namespace ParserExample;

public enum P
{
    [Sugar("+")] e,
    [Keyword("true")] True,
    [Keyword("false")] False,
}

public enum L
{
    [Keyword("A")] A = 1,
    [Keyword("B")] B = 2,
    [Sugar("+")] PLUS = 3,
    [Sugar("-")] MINUS = 4
}


public class SimpleEBNFMany
{
    [Production("root : astar")]
    public string Root(string astar)
    {
        return astar;
    }
    
    [Production("rootplus : aplus")]
    public string RootPlus(string aplus)
    {
        return aplus;
    }

    [Production("astar : A*")]
    public string Astar(List<Token<L>> all)
    {
        return string.Join(",",all.Select(x => x.Value));
    }
    
    [Production("aplus : A+")]
    public string Aplus(List<Token<L>> all)
    {
        return string.Join(",",all.Select(x => x.Value));
    }
}

public class Visitor {

    [Production("root  : a [ PLUS | MINUS ] b")]
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

public class Stacker
{
    public static void Stack()
    {
        var instance = new EvenSimplerStackParser();
        ParserBuilder<SimplerStackLexer, string> builder = new ParserBuilder<SimplerStackLexer, string>();
        var parser = builder.BuildParser(instance, ParserType.LL_STACK, "root");
        if (parser.IsOk)
        {
            var r = parser.Result.Parse("1 2");
            if (r.IsOk)
            {
                Console.WriteLine($"PARSE OK !!! >{r.Result}<");
            }
            else
            {
                foreach (var error in r.Errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }
        }
        else
        {
            foreach (var error in parser.Errors)
            {
                Console.WriteLine(error.Message);
            }
        }
    }

    public static void MoreStack()
    {
        var instance = new SimplerStackParser();
        ParserBuilder<SimplerStackLexer, string> builder = new ParserBuilder<SimplerStackLexer, string>();
        var parser = builder.BuildParser(instance, ParserType.LL_STACK, "root");
        if (parser.IsOk)
        {
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
        else
        {
            foreach (var error in parser.Errors)
            {
                Console.WriteLine(error.Message);
            }
        }
    }

    public static void FluentStack()
    {
        var lexer = FluentLexerBuilder<ExpressionToken>.NewBuilder()
            .Int(ExpressionToken.INT);
        var parser = FluentParserBuilder<ExpressionToken, string>
            .NewBuilder(new SimplerStackParser(), "root", "en")
            .WithLexerbuilder(lexer)
            .Production("root : expr", (object[] args) => { return (string)args[0]; })
            .Production("expr : INT", (args) => { return ((Token<ExpressionToken>)args[0]).Value; })
            .Production("expr : INT expr",
                (args) => { return ((Token<ExpressionToken>)args[0]).Value + "," + (string)args[1]; })
            .BuildParser(ParserType.LL_STACK);
        Check.That(parser).IsOk();
        var result = parser.Result.Parse("1");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("1");
        result = parser.Result.Parse("1 2 3 4 5");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("1,2,3,4,5");
    }

    public static void EvenMoreStack()
    {
        var instance = new SimpleStackParser();
        ParserBuilder<SimpleStackLexer, int> builder = new ParserBuilder<SimpleStackLexer, int>();
        var parser = builder.BuildParser(instance, ParserType.LL_STACK, "root");
        if (parser.IsOk)
        {
            var r = parser.Result.Parse("1+2+3+4");
            Check.That(r).IsOkParsing();
            Check.That(r.Result).IsEqualTo(10);
            r = parser.Result.Parse("1+2+3+4+5+6+7+8+9+10");
            Check.That(r).IsOkParsing();
            Check.That(r.Result).IsEqualTo(55);
        }
        else
        {
            foreach (var error in parser.Errors)
            {
                Console.WriteLine(error.Message);
            }
        }
    }

    public static void Expression()
    {
        var instance = new ExpressionParser();
        ParserBuilder<expressionparser.ExpressionToken, int> builder2 =
            new ParserBuilder<expressionparser.ExpressionToken, int>();
        var parser = builder2.BuildParser(instance, ParserType.LL_STACK, "expression");
        if (parser.IsOk)
        {
            string source = "2+2";
            ;
            Console.WriteLine($"start parsing {source}");
            var r = parser.Result.Parse(source);
            Console.WriteLine($"parsing done : {(r.IsOk ? "OK" : "KO")}");
            Check.That(r).IsOkParsing();
            Check.That(r.Result).IsEqualTo(4);
            source = "1+2+3+4+5+6+7+8+9*10";
            Console.WriteLine($"start parsing {source}");
            r = parser.Result.Parse(source);
            Check.That(r).IsOkParsing();
            Check.That(r.Result).IsEqualTo(126);
            Console.WriteLine("parsing done !!! OOH YEAH !! :: "+r.Result);
            ;
        }
        else
        {
            foreach (var error in parser.Errors)
            {
                Console.WriteLine(error.Message);
            }
        }
    }


    public static void ParseVisitorAPlusB()
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
        Check.That(r).IsOkParsing();
        var actual = r.Result.ToString();
        Check.That(actual).IsEqualTo(expected);
    }
    
    public static void Rules()
    {
        var ruleparser = new RuleParser<L, string>();
        string source = "[ PLUS | MINUS ]";
        string start = "choiceclause";
        
        //
        // LL_RECURSIVE => OK => get expected output
        //
        
        var parser = GetParser<EbnfTokenGeneric, GrammarNode<L, string>>(ruleparser, ParserType.LL_RECURSIVE_DESCENT, "rule");

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
        tree = r.SyntaxTree;
        var actual = (r.Result as IClause<L, string>).Dump();
        Check.That(actual).IsEqualTo(expected);
    }

    public static void List()
    {
        var lexer = FluentLexerBuilder<L>.NewBuilder()
            .Keyword(L.A, "A")
            .Keyword(L.B, "B");
        ;
        var p = FluentParserBuilder<L, string>.NewBuilder(new Dumb(), "r", "en")
            .Production("r : cs", (args) => { return args[0].ToString(); })
            .Production("cs : c cs", (args) =>
            {
                var head = (string)args[0];
                var tail = (string)args[1];

                return head + ".." + tail;
            })
            .Production("cs : c", (args) => { return (string)args[0]; })
            .Production("c : A", (args) => { return ((Token<L>)args[0]).Value; })
            .Production("c : B", (args) => { return ((Token<L>)args[0]).Value; })
            .WithLexerbuilder(lexer)
            .BuildParser(ParserType.LL_STACK);
        Check.That(p.IsOk);
        var t = p.Result.Parse("A  B", "cs");
        Check.That(t).IsOkParsing();
        Check.That(t.Result).IsEqualTo("A..B");
        Console.WriteLine("OK : "+t.Result);
        Console.WriteLine(t.SyntaxTree.Dump("  "));
    }

    public static Parser<IN, OUT> GetParser<IN, OUT>(object instance, ParserType type, string root) where IN : struct , Enum
    {
        ParserBuilder<IN,OUT> builder =  new ParserBuilder<IN, OUT>();
        var built = builder.BuildParser(instance, type, root);
        Check.That(built).IsOk();
        Check.That(built.Result).IsNotNull();
        return built.Result;
    }

    public static void Test239()
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

    public static void TestPostProcessedLexer()
    {
        string start = "rule";
        string source = "rule : IDENTIFIER LPAREN[d] FormulaParser_expressions (COMMA FormulaParser_expressions)* ";
        //string source = "IDENTIFIER LPAREN[d] FormulaParser_expressions (COMMA FormulaParser_expressions)* ";
//        string source = "FormulaParser_expressions (COMMA FormulaParser_expressions)* ";
        //string source = "(COMMA FormulaParser_expressions)* ";
        //string source = "( COMMA FormulaParser_expressions )";
        var ruleparser = new RuleParser<FormulaToken, Expression>();
 
        
        //
        // LL_RECURSIVE => OK => get expected output
        //
        
        var parser = GetParser<EbnfTokenGeneric, GrammarNode<FormulaToken, Expression>>(ruleparser, ParserType.LL_RECURSIVE_DESCENT, "rule");

        var r = parser.Parse(source, start);
        Check.That(r).IsOkParsing();
        
        var tree = r.SyntaxTree;
        Check.That(r.SyntaxTree).IsInstanceOf<SyntaxNode<EbnfTokenGeneric, GrammarNode<FormulaToken, Expression>>>();
        var root = r.SyntaxTree as SyntaxNode<EbnfTokenGeneric, GrammarNode<FormulaToken, Expression>>;
        Check.That(root).IsNotNull();
        Check.That(r.Result).IsInstanceOf<Rule<FormulaToken,postProcessedLexerParser.expressionModel.Expression >> ();
        var rule = r.Result as Rule<FormulaToken, Expression>;
        var expected = rule.Dump();
        
        
        //
        // LL_STACK => get output and compare to expected (if parse succeeded at all)
        //
        
        
        parser = GetParser<EbnfTokenGeneric, GrammarNode<FormulaToken, Expression>>(ruleparser, ParserType.LL_STACK, "rule");
        
        r = parser.Parse(source, start);
        Check.That(r).IsOkParsing();
        tree = r.SyntaxTree;
        Check.That(r.Result).IsInstanceOf<Rule<FormulaToken,postProcessedLexerParser.expressionModel.Expression >> ();
        rule = r.Result as Rule<FormulaToken, Expression>;
        var actual = rule.Dump();
        Check.That(actual).IsEqualTo(expected);
Console.WriteLine("***************************************");
Console.WriteLine("*** YAHOO ! WE'VE DONE A GREAT JOB");
Console.WriteLine($"*** {actual} == {expected}");
Console.WriteLine("***************************************");
    }

    public static void TestEbnfSimpleZeroOrMore()
    {
        var ebnf = new SimpleEBNFMany();
        RuleParserType.ParserType = ParserType.LL_RECURSIVE_DESCENT;
        var parser = GetParser<L, string>(ebnf, ParserType.EBNF_LL_STACK, "root");

        Check.That(parser).IsNotNull();

        var parseResult = parser.Parse(" A  A  A A");
        Check.That(parseResult).IsOkParsing();
        var result = parseResult.Result;
        Check.That(result).IsEqualTo("A,A,A,A");
        Console.WriteLine("parse A A A A : OK");
        
        parseResult = parser.Parse(" A  ");
        Check.That(parseResult).IsOkParsing();
        result = parseResult.Result;
        Check.That(result).IsEqualTo("A");
        Console.WriteLine("parse A  : OK");
        
        parseResult = parser.Parse("   ");
        Check.That(parseResult).IsOkParsing();
        result = parseResult.Result;
        Check.That(result).IsEqualTo("");
        Console.WriteLine("parse  (empty) : OK");
    }

    public static void TestIndentedNotClosing()
    {
        var source =@"
if truc == 1
    un = 1
    deux = 2";
        ParserBuilder<IndentedLangLexer, Ast> builder = new ParserBuilder<IndentedLangLexer, Ast>();
        var instance = new IndentedParser();
        var parserRes = builder.BuildParser(instance, ParserType.EBNF_LL_STACK, "root");
        Check.That(parserRes.IsOk).IsTrue();
            
        var parser = parserRes.Result;
        Check.That(parser).IsNotNull();
        var parseResult = parser.Parse(source);
        Check.That(parseResult).Not.IsOkParsing();
        Check.That(parseResult.Errors).CountIs(1);
        var error = parseResult.Errors[0];
        Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedEOS);
    }
    
    public static void TestEbnfSimpleOneOrMore()
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

    public static void TestFluentOption()
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
            .BuildParser(ParserType.EBNF_LL_RECURSIVE_DESCENT);

        Check.That(buildResult).IsOk();
        var parser = buildResult.Result;
        var result = parser.Parse("A A");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("ah ah !");
        result = parser.Parse("A B A");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("ABBA");
    }

    public static void TestIndented()
    {
        var source =@"
if truc == 1
    un = 1
    deux = 2";
        ParserBuilder<IndentedLangLexer, Ast> builder = new ParserBuilder<IndentedLangLexer, Ast>();
        var instance = new IndentedParser();
        var parserRes = builder.BuildParser(instance, ParserType.EBNF_LL_STACK, "root");
        Check.That(parserRes.IsOk).IsTrue();
            
        var parser = parserRes.Result;
        Check.That(parser).IsNotNull();
        var parseResult = parser.Parse(source);
        Check.That(parseResult).Not.IsOkParsing();
    }
}