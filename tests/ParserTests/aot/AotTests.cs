using System;
using System.Collections.Generic;
using System.Linq;
using aot.parser;
using csly.indentedWhileLang.parser;
using csly.whileLang.model;
using NFluent;
using ParserTests.aot.expressions;
using Sigil;
using sly.buildresult;
using sly.lexer;
using sly.parser.parser;
using Xunit;

namespace ParserTests.aot;

public enum RegexLexer
{
    Eos,
    Value1,
    Value2,
    Ws,
    Eol,
}

public class AotTests
{

    [Fact]
    public void CannotMixRegexAndGGenericTest()
    {
        var builder = AotLexerBuilder<RegexLexer>.NewBuilder<RegexLexer>();
        var mixedLexer = builder.Double(RegexLexer.Value1)
            .Regex(RegexLexer.Value2, "a*")
            .Build();
        Check.That(mixedLexer).Not.IsOk();
        Check.That(mixedLexer.Errors.Exists(x => x.Code == ErrorCodes.LEXER_CANNOT_MIX_GENERIC_AND_REGEX));
    }
    
    [Fact]
    public void ValueRegexLexerTest()
    {
        var builder = AotLexerBuilder<RegexLexer>.NewBuilder<RegexLexer>();
        var validLexerResult = builder.Regex(RegexLexer.Value1,"[0-9]+")
            .Regex(RegexLexer.Value2, "([a-z]|[A-Z])+")
            .Regex(RegexLexer.Eol,"[\\r|\\n]+",true,true)
            .Regex(RegexLexer.Ws,"[ |\\t]+",true)
            .Build();
        Check.That(validLexerResult).IsOk();
        var lexer = validLexerResult.Result;
        var result = lexer.Tokenize("42 abc");
        Check.That(result).IsOkLexing();
        var tokens = result.Tokens.MainTokens();
        Check.That(tokens).CountIs(3);
        Check.That(tokens[2].IsEOS).IsTrue();
        Check.That(tokens.Take(2).Extracting(x => x.TokenID))
            .IsEqualTo(new [] { RegexLexer.Value1, RegexLexer.Value2});
        Check.That(tokens.Take(2).Extracting(x => x.Value))
            .IsEqualTo(new [] { "42", "abc" });

    }
    
    private IAotLexerBuilder<AotExpressionsLexer> BuildAotexpressionLexer()
    {
        var builder = AotLexerBuilder<AotExpressionsLexer>.NewBuilder<AotExpressionsLexer>();
        var lexerBuilder = builder.Double(AotExpressionsLexer.DOUBLE)
            .Sugar(AotExpressionsLexer.PLUS, "+")
            .Keyword(AotExpressionsLexer.PLUS, "PLUS")
            .Labeled("en", "sum")
            .Labeled("fr", "somme")
            .Sugar(AotExpressionsLexer.MINUS, "-")
            .Sugar(AotExpressionsLexer.TIMES, "*")
            .Sugar(AotExpressionsLexer.DIVIDE, "/")
            .Sugar(AotExpressionsLexer.LPAREN, "(")
            .Sugar(AotExpressionsLexer.RPAREN, ")")
            .Sugar(AotExpressionsLexer.FACTORIAL, "!")
            .Sugar(AotExpressionsLexer.INCREMENT, "++")
            .AlphaNumId(AotExpressionsLexer.IDENTIFIER);
            
        return lexerBuilder;
    }
    
    [Fact]
    public void AotExpressionLexerTest()
    {
        var lexerBuilder = BuildAotexpressionLexer();
        var lexer = lexerBuilder.Build();
        Check.That(lexer).IsNotNull();
        var lexed = lexer.Result.Tokenize("2+2");
        Check.That(lexed).IsOkLexing();
        Check.That(lexed.Tokens.MainTokens()).CountIs(4);
        Check.That(lexed.Tokens.MainTokens().Take(3).Extracting(x => x.TokenID)).IsEqualTo(new[]
        {
            AotExpressionsLexer.DOUBLE,
            AotExpressionsLexer.PLUS,
            AotExpressionsLexer.DOUBLE
        });
    }

    [Fact]
    public void AotExpressionParserTest()
    {
         AotExpressionsParser expressionsParserInstance = new AotExpressionsParser();
        
        var builder = AotEBNFParserBuilder<AotExpressionsLexer,double>.NewBuilder<AotExpressionsLexer,double > (expressionsParserInstance,"root");

        var lexerBuilder = BuildAotexpressionLexer();

        var p = builder.UseMemoization()
            .WithLexerbuilder(lexerBuilder)
            .Production("root : AotExpressionsParser_expressions", (args) =>
            {
                var result = expressionsParserInstance.Root((double)args[0]);
                return result;
            })
            .Right(10, AotExpressionsLexer.PLUS, (args =>
            {
                double result = expressionsParserInstance.BinaryTermExpression((double)args[0], (Token<AotExpressionsLexer>)args[1], (double)args[2]);
                return result;
            }))
            .Right(10, AotExpressionsLexer.MINUS, (args =>
            {
                double result = expressionsParserInstance.BinaryTermExpression((double)args[1], (Token<AotExpressionsLexer>)args[2], (double)args[3]);
                return result;
            }))
            .Right(50, AotExpressionsLexer.TIMES, (args =>
            {
                double result =
                    expressionsParserInstance.BinaryFactorExpression((double)args[0], (Token<AotExpressionsLexer>)args[1], (double)args[2]);
                return result;
            }))
            .Right(50, AotExpressionsLexer.DIVIDE, (args =>
            {
                double result =
                    expressionsParserInstance.BinaryFactorExpression((double)args[1], (Token<AotExpressionsLexer>)args[2], (double)args[3]);
                return result;
            }))
            .Prefix(100, AotExpressionsLexer.MINUS, (object[] args) =>
            {
                return expressionsParserInstance.PreFixExpression((Token<AotExpressionsLexer>)args[0], (double)args[1]);
            })
            .Postfix(100, "'!'", (object[] args) =>
            {
                return expressionsParserInstance.PostFixExpression((double)args[0], (Token<AotExpressionsLexer>)args[1]);
            })
            // .Operand("operand : primary_value", args =>
            // {
            //     return parserInstance.OperandValue((double)args[0]);
            // })
            .Operand("primary_value : DOUBLE", args =>
            {
                return expressionsParserInstance.OperandDouble((Token<AotExpressionsLexer>)args[0]);
            })
            .Operand("primary_value : INT", args =>
            {
                return expressionsParserInstance.OperandInt((Token<AotExpressionsLexer>)args[0]);
            })
            .Operand("primary_value : LPAREN SimpleExpressionParser_expressions RPAREN", args =>
            {
                return expressionsParserInstance.OperandGroup((Token<AotExpressionsLexer>)args[0], (double)args[1], (Token<AotExpressionsLexer>)args[2]);
            });

        var parser = p.BuildParser();
        Check.That(parser).IsOk();
        var r = parser.Result.Parse(" 2 + 2");
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsEqualTo(4);
        r = parser.Result.Parse(" 2 + 2 * 2");
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsEqualTo(6);
    }


    private IAotLexerBuilder<IndentedWhileTokenGeneric> BuildAotWhileLexer()
    {
        var builder = AotLexerBuilder<IndentedWhileTokenGeneric>.NewBuilder<IndentedWhileTokenGeneric>();
        builder.IsIndentationAware()
            .UseIndentations("\t")
            // keywords
            .Keyword(IndentedWhileTokenGeneric.IF, "if")
            .Keyword(IndentedWhileTokenGeneric.THEN, "then")
            .Keyword(IndentedWhileTokenGeneric.ELSE, "else")
            .Keyword(IndentedWhileTokenGeneric.WHILE, "while")
            .Keyword(IndentedWhileTokenGeneric.DO, "do")
            .Keyword(IndentedWhileTokenGeneric.SKIP, "skip")
            .Keyword(IndentedWhileTokenGeneric.PRINT, "print")
            .Keyword(IndentedWhileTokenGeneric.TRUE, "true")
            .Keyword(IndentedWhileTokenGeneric.FALSE, "false")
            .Keyword(IndentedWhileTokenGeneric.NOT, "not")
            .Keyword(IndentedWhileTokenGeneric.AND, "and")
            .Keyword(IndentedWhileTokenGeneric.OR, "or")
            .Keyword(IndentedWhileTokenGeneric.RETURN, "return")
            // literals
            .AlphaNumDashId(IndentedWhileTokenGeneric.IDENTIFIER)
            .String(IndentedWhileTokenGeneric.STRING)
            .Integer(IndentedWhileTokenGeneric.INT)
            // operators
            .Sugar(IndentedWhileTokenGeneric.GREATER, ">")
            .Sugar(IndentedWhileTokenGeneric.LESSER, "<")
            .Sugar(IndentedWhileTokenGeneric.EQUALS, "==")
            .Sugar(IndentedWhileTokenGeneric.DIFFERENT, "!==")
            .Sugar(IndentedWhileTokenGeneric.CONCAT, ".")
            .Sugar(IndentedWhileTokenGeneric.ASSIGN, ":=")
            .Sugar(IndentedWhileTokenGeneric.PLUS, "+")
            .Sugar(IndentedWhileTokenGeneric.MINUS, "-")
            .Sugar(IndentedWhileTokenGeneric.TIMES, "*")
            .Sugar(IndentedWhileTokenGeneric.DIVIDE, "/")
            .Sugar(IndentedWhileTokenGeneric.SEMICOLON, ";")
            .SingleLineComment(IndentedWhileTokenGeneric.COMMENT, "#");
        return builder;
            
            
        return builder;
    }

    private IAotEBNFParserBuilder<IndentedWhileTokenGeneric, WhileAST> BuildAotWhileParser()
    {
        IndentedWhileParserGeneric instance = new IndentedWhileParserGeneric();
        var builder = AotEBNFParserBuilder<IndentedWhileTokenGeneric, WhileAST>
            .NewBuilder<IndentedWhileTokenGeneric, WhileAST>(instance, "program", "en");
        Func<object[], WhileAST> comparisons = (object[] args) =>
        {
            return instance.binaryComparisonExpression((WhileAST)args[0], (Token<IndentedWhileTokenGeneric>)args[1],
                (WhileAST)args[2]);
        };

        Func<object[], WhileAST> numericTerm = args => instance.binaryTermNumericExpression((WhileAST)args[0],
            (Token<IndentedWhileTokenGeneric>)args[1],
            (WhileAST)args[2]);
        Func<object[], WhileAST> numericFactor = args => instance.binaryFactorNumericExpression((WhileAST)args[0],
            (Token<IndentedWhileTokenGeneric>)args[1],
            (WhileAST)args[2]);

        builder.Right(50, IndentedWhileTokenGeneric.LESSER, comparisons)
            .Right(50, IndentedWhileTokenGeneric.GREATER, comparisons)
            .Right(50, IndentedWhileTokenGeneric.EQUALS, comparisons)
            .Right(50, IndentedWhileTokenGeneric.DIFFERENT, comparisons)
            .Right(10, IndentedWhileTokenGeneric.CONCAT, (args) =>
            {
                return instance.binaryStringExpression((WhileAST)args[0], (Token<IndentedWhileTokenGeneric>)args[1],
                    (WhileAST)args[2]);
            })
            .Production("program : sequence", args => { return instance.program((WhileAST)args[0]); })
            .Production("block : INDENT[d] sequence UINDENT[d]",
                args => { return instance.sequenceStatements((SequenceStatement)args[0]); })
            .Production("statement : block", args => { return instance.blockStatement((WhileAST)args[0]); })
            .Production("sequence : statement", args => { return instance.sequence((List<WhileAST>)args[0]); })
            .Production("statement: IF[d] IndentedWhileParserGeneric_expressions THEN[d] block (ELSE[d] block)?",
                args =>
                {
                    return instance.ifStmt((WhileAST)args[0], (WhileAST)args[1],
                        (ValueOption<Group<IndentedWhileTokenGeneric, WhileAST>>)args[2]);
                })
            .Production("statement: WHILE[d] IndentedWhileParserGeneric_expressions DO[d] block",
                args => { return instance.whileStmt((WhileAST)args[0], (WhileAST)args[1]); })
            .Production("statement: IDENTIFIER ASSIGN[d] IndentedWhileParserGeneric_expressions",
                args => { return instance.assignStmt((Token<IndentedWhileTokenGeneric>)args[0], (Expression)args[1]); })
            .Production("statement: SKIP[d]", args => { return instance.skipStmt(); })
            .Production("statement: RETURN[d] IndentedWhileParserGeneric_expressions",
                args => { return instance.ReturnStmt((Expression)args[0]); })
            .Production("statement: PRINT[d] IndentedWhileParserGeneric_expressions",
                args => { return instance.printStmt((Expression)args[0]); })
            .Production("primary : INT",
                args => { return instance.PrimaryInt(((Token<IndentedWhileTokenGeneric>)args[0])); })
            .Production("primary : [TRUE|FALSE]",
                args => { return instance.PrimaryBool(((Token<IndentedWhileTokenGeneric>)args[0])); })
            .Production("primary : STRING",
                args => { return instance.PrimaryString(((Token<IndentedWhileTokenGeneric>)args[0])); })
            .Production("primary : IDENTIFIER",
                args => { return instance.PrimaryId(((Token<IndentedWhileTokenGeneric>)args[0])); })
            .Operand("operand : primary", args => instance.Operand((WhileAST)args[0]))
            .Right(10, IndentedWhileTokenGeneric.MINUS, numericTerm)
            .Right(10, IndentedWhileTokenGeneric.PLUS, numericTerm)
            .Right(50, IndentedWhileTokenGeneric.TIMES, numericFactor)
            .Right(50, IndentedWhileTokenGeneric.DIVIDE, numericFactor)
            .Prefix(100, IndentedWhileTokenGeneric.MINUS,
                args => instance.unaryNumericExpression((Token<IndentedWhileTokenGeneric>)args[0], (WhileAST)args[1]))
            .Right(10, IndentedWhileTokenGeneric.OR,
                args => instance.binaryOrExpression((WhileAST)args[0], (Token<IndentedWhileTokenGeneric>)args[1],
                    (WhileAST)args[2]))
            .Right(50, IndentedWhileTokenGeneric.AND,
                args => instance.binaryAndExpression((WhileAST)args[0], (Token<IndentedWhileTokenGeneric>)args[1],
                    (WhileAST)args[2]))
            .Prefix(100, IndentedWhileTokenGeneric.NOT,
                args => instance.unaryNotExpression((Token<IndentedWhileTokenGeneric>)args[0], (WhileAST)args[1]))
            .UseMemoization()
            .UseAutoCloseIndentations()
            .WithLexerbuilder(BuildAotWhileLexer())
            ;
        
        return builder;
    }

    [Fact]
    public void AotWhileLexerTest()
    {
        var lexerBuilder = BuildAotWhileLexer();
        var lexer = lexerBuilder.Build();
        Check.That(lexer).IsNotNull();
        var commentResult = lexer.Result.Tokenize("# comment");
        Check.That(commentResult).IsOkLexing();
        var commentChannel = commentResult.Tokens.GetChannel(Channels.Comments);
        Check.That(commentChannel.Tokens).CountIs(1);
        Check.That(commentChannel.Tokens[0].TokenID).IsEqualTo(IndentedWhileTokenGeneric.COMMENT);
        Check.That(commentChannel.Tokens[0].Value).IsEqualTo(" comment");
        // TODO AOT : check has COMMENT
        string program = @"
a:=0 
while a < 10 do 
	print a
	a := a +1
";
        var programResult = lexer.Result.Tokenize(program);
        Check.That(programResult).IsOkLexing();
        Check.That(programResult.Tokens.MainTokens().Exists(x => x.IsIndent)).IsTrue();
        Check.That(programResult.Tokens.MainTokens().Exists(x => x.IsUnIndent)).IsTrue();
        // TODO AOT : check has indents
    }

    [Fact]
    public void AotWhileParserTest()
    {
        var builder = BuildAotWhileParser();
        var buildResult = builder.BuildParser();
        Check.That(buildResult).IsOk();
        var parser = buildResult.Result;
        Check.That(parser).IsNotNull();
        string program = @"
a:=0 
while a < 10 do 
    print a
    a := a +1
";
        var result = parser.Parse(program);
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsInstanceOf<SequenceStatement>();
        var seq = result.Result as SequenceStatement;
        Check.That(seq.Get(0)).IsInstanceOf<WhileStatement>();
        var whil = seq.Get(0) as WhileStatement;
        var cond = whil.Condition;
        Check.That(cond).IsInstanceOf<BoolConstant>();
        Check.That((cond as BoolConstant).Value).IsTrue();
        var s = whil.BlockStmt;
        Check.That(whil.BlockStmt).IsInstanceOf<SequenceStatement>();
        var seqBlock = whil.BlockStmt as SequenceStatement;
        Check.That(seqBlock).CountIs(1);
        Check.That(seqBlock.Get(0)).IsInstanceOf<SkipStatement>();
    }
    
    
}