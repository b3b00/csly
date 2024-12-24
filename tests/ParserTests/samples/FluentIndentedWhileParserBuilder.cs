using System;
using System.Collections.Generic;
using csly.indentedWhileLang.parser;
using csly.whileLang.model;
using sly.buildresult;
using sly.lexer;
using sly.lexer.fluent;
using sly.parser;
using sly.parser.generator;
using sly.parser.parser;

namespace ParserTests.samples;

public class FluentIndentedWhileParserBuilder {

    public IFluentLexemeBuilder<IndentedWhileTokenGeneric> GetLexer()
    {
        var lexer = FluentLexerBuilder<IndentedWhileTokenGeneric>.NewBuilder()
            .IgnoreEol(true)
            .IgnoreWhiteSpace(true)
            .IsIndentationAware(true)
            .IgnoreKeywordCase(true)
            .AlphaNumDashId(IndentedWhileTokenGeneric.IDENTIFIER)
            .WithModes(ModeAttribute.DefaultLexerMode, "fstringExpression")
            .Keyword(IndentedWhileTokenGeneric.IF, "if")
            .Keyword(IndentedWhileTokenGeneric.THEN, "then")
            .Keyword(IndentedWhileTokenGeneric.ELSE, "else")
            .Keyword(IndentedWhileTokenGeneric.WHILE, "while")
            .Keyword(IndentedWhileTokenGeneric.DO, "do")
            .Keyword(IndentedWhileTokenGeneric.TRUE, "true")
            .Keyword(IndentedWhileTokenGeneric.FALSE, "false")
            .Keyword(IndentedWhileTokenGeneric.NOT, "not")
            .Keyword(IndentedWhileTokenGeneric.AND, "and")
            .Keyword(IndentedWhileTokenGeneric.OR, "or")
            .Keyword(IndentedWhileTokenGeneric.PRINT, "print")
            .Keyword(IndentedWhileTokenGeneric.RETURN, "return")
            .Keyword(IndentedWhileTokenGeneric.SKIP, "skip")
            .Int(IndentedWhileTokenGeneric.INT)
            .Sugar(IndentedWhileTokenGeneric.GREATER, ">")
            .WithModes(ModeAttribute.DefaultLexerMode, "fstringExpression")
            .Sugar(IndentedWhileTokenGeneric.LESSER, "<").WithModes(ModeAttribute.DefaultLexerMode, "fstringExpression")
            .Sugar(IndentedWhileTokenGeneric.EQUALS, "==")
            .WithModes(ModeAttribute.DefaultLexerMode, "fstringExpression")
            .Sugar(IndentedWhileTokenGeneric.DIFFERENT, "!=")
            .WithModes(ModeAttribute.DefaultLexerMode, "fstringExpression")
            .Sugar(IndentedWhileTokenGeneric.CONCAT, ".").WithModes(ModeAttribute.DefaultLexerMode, "fstringExpression")
            .Sugar(IndentedWhileTokenGeneric.PLUS, "+").WithModes(ModeAttribute.DefaultLexerMode, "fstringExpression")
            .Sugar(IndentedWhileTokenGeneric.MINUS, "-").WithModes(ModeAttribute.DefaultLexerMode, "fstringExpression")
            .Sugar(IndentedWhileTokenGeneric.TIMES, "*").WithModes(ModeAttribute.DefaultLexerMode, "fstringExpression")
            .Sugar(IndentedWhileTokenGeneric.DIVIDE, "/").WithModes(ModeAttribute.DefaultLexerMode, "fstringExpression")
            .Sugar(IndentedWhileTokenGeneric.QUESTION, "?").WithModes("fstringExpression")
            .Sugar(IndentedWhileTokenGeneric.ARROW, "->").WithModes(ModeAttribute.DefaultLexerMode, "fstringExpression")
            .Sugar(IndentedWhileTokenGeneric.OPEN_PAREN, "(")
            .WithModes(ModeAttribute.DefaultLexerMode, "fstringExpression")
            .Sugar(IndentedWhileTokenGeneric.CLOSE_PAREN, ")")
            .WithModes(ModeAttribute.DefaultLexerMode, "fstringExpression")
            .Sugar(IndentedWhileTokenGeneric.SEMICOLON, ";")
            .Sugar(IndentedWhileTokenGeneric.COLON, "|").WithModes(ModeAttribute.DefaultLexerMode, "fstringExpression")
            .SingleLineComment(IndentedWhileTokenGeneric.COMMENT, "#")
            
            .Sugar(IndentedWhileTokenGeneric.OPEN_FSTRING_EXPPRESSION, "{").WithModes("fstring").PushToMode("fstringExpression")
            .Sugar(IndentedWhileTokenGeneric.CLOSE_FSTRING_EXPPRESSION, "}").WithModes("fstringExpression").PopMode()
            .Sugar(IndentedWhileTokenGeneric.OPEN_FSTRING, "$\"").WithModes(ModeAttribute.DefaultLexerMode, "fstringExpression").PushToMode("fstring")
            .Sugar(IndentedWhileTokenGeneric.CLOSE_FSTRING, "\"").WithModes("fstring").PopMode()
            .UpTo(IndentedWhileTokenGeneric.FSTRING_CONTENT, "{","\"").WithModes("fstring")
            .Sugar(IndentedWhileTokenGeneric.ASSIGN, ":=");
        return lexer;
    }

    public BuildResult<Parser<IndentedWhileTokenGeneric, WhileAST>> GetParser()
    {
        var instance = new IndentedWhileParserGeneric();
        var builder = FluentEBNFParserBuilder<IndentedWhileTokenGeneric, WhileAST>.NewBuilder(instance,"program", "en");

        var binary = (Func<WhileAST, Token<IndentedWhileTokenGeneric>, WhileAST, WhileAST> instanceCallback) =>
        {
            Func<object[], WhileAST> callback = (object[] args) =>
            {
                WhileAST left = (WhileAST)args[0];
                Token<IndentedWhileTokenGeneric> op = (Token<IndentedWhileTokenGeneric>)args[1];
                WhileAST right = (WhileAST)args[2];
                return instanceCallback(left, op, right);
            };
            return callback;
        };
        
        var prefix = (Func<Token<IndentedWhileTokenGeneric>, WhileAST, WhileAST> instanceCallback) =>
        {
            Func<object[], WhileAST> callback = (object[] args) =>
            {
                Token<IndentedWhileTokenGeneric> op = (Token<IndentedWhileTokenGeneric>)args[0];
                WhileAST right = (WhileAST)args[1];
                return instanceCallback(op, right);
            };
            return callback;
        };
        
        var postfix = (Func<WhileAST, Token<IndentedWhileTokenGeneric>, WhileAST> instanceCallback) =>
        {
            Func<object[], WhileAST> callback = (object[] args) =>
            {
                Token<IndentedWhileTokenGeneric> op = (Token<IndentedWhileTokenGeneric>)args[0];
                WhileAST right = (WhileAST)args[1];
                return instanceCallback(right, op);
            };
            return callback;
        };
        
        var comparisonCallback = binary((left, op, right) =>
        {
            return instance.binaryComparisonExpression(left, op, right);
        });
        
        var stringCallback = binary((left, op, right) =>
        {
            return instance.binaryStringExpression(left, op, right);
        });
        var factorCallback = binary((left, op, right) =>
        {
            return instance.binaryFactorNumericExpression(left, op, right);
        });
        var termCallback = binary((left, op, right) =>
        {
            return instance.binaryTermNumericExpression(left, op, right);
        });


        var parser = builder
            .UseAutoCloseIndentations(true)
            .UseMemoization(true)
            // expressions
            .Right(IndentedWhileTokenGeneric.LESSER, 50, comparisonCallback)
            .Right(IndentedWhileTokenGeneric.GREATER, 50, comparisonCallback)
            .Right(IndentedWhileTokenGeneric.EQUALS, 50, comparisonCallback)
            .Right(IndentedWhileTokenGeneric.DIFFERENT, 50, comparisonCallback)
            .Right(IndentedWhileTokenGeneric.CONCAT, 50, stringCallback)
            .Right(IndentedWhileTokenGeneric.PLUS, 10, termCallback)
            .Right(IndentedWhileTokenGeneric.MINUS, 10, termCallback)
            .Right(IndentedWhileTokenGeneric.TIMES, 50, factorCallback)
            .Right(IndentedWhileTokenGeneric.DIVIDE, 50, factorCallback)
            .Prefix(IndentedWhileTokenGeneric.MINUS, 100,
                prefix((op, value) => instance.unaryNumericExpression(op, value)))
            .Right(IndentedWhileTokenGeneric.OR, 10,
                binary((left, op, right) => instance.binaryOrExpression(left, op, right)))
            .Right(IndentedWhileTokenGeneric.AND, 50,
                binary((left, op, right) => instance.binaryAndExpression(left, op, right)))
            .Prefix(IndentedWhileTokenGeneric.NOT, 100, prefix((op, value) => instance.unaryNotExpression(op, value)))
            // operands
            .Production("primary : INT", (args) =>
            {
                return instance.PrimaryInt((Token<IndentedWhileTokenGeneric>)args[0]);
            })
            .Production("primary : IDENTIFIER", (args) =>
            {
                return instance.PrimaryId((Token<IndentedWhileTokenGeneric>)args[0]);
            })
            .Production("primary : [TRUE|FALSE]", (args) =>
            {
                return instance.PrimaryBool((Token<IndentedWhileTokenGeneric>)args[0]);
            })
            .Production("primary : OPEN_PAREN[d] IndentedWhileParserGeneric_expressions CLOSE_PAREN[d]", args =>
            {
                return (WhileAST)args[0];
            })
            .Production(
                "primary : QUESTION[d] IndentedWhileParserGeneric_expressions ARROW[d] IndentedWhileParserGeneric_expressions COLON[d] IndentedWhileParserGeneric_expressions",
                args =>
                {
                    var condition = (WhileAST)args[0];
                    var ifTrue = (WhileAST)args[1];
                    var ifFalse = (WhileAST)args[2];
                    return instance.TernaryQuestion(condition, ifTrue, ifFalse);
                })
            .Operand("operand: primary", (args) =>
            {
                return (WhileAST)args[0];
            })
            // fstrings
            .Production("primary : OPEN_FSTRING[d] fstring_element* CLOSE_FSTRING[d]", args =>
            {
                var elements = (List<WhileAST>)args[0];
                return instance.fstring(elements);
                ;
            })
            .Production("fstring_element : FSTRING_CONTENT", args =>
            {
                return instance.FStringContent((Token<IndentedWhileTokenGeneric>)args[0]);
            })
            .Production(
                "fstring_element : OPEN_FSTRING_EXPPRESSION[d] IndentedWhileParserGeneric_expressions CLOSE_FSTRING_EXPPRESSION[d]",
                args =>
                {
                    return (WhileAST)args[0];
                })
            // main
            .Production("program: sequence", args =>
            {
                return (WhileAST)args[0];
            })
            .Production("block : INDENT[d] sequence UINDENT[d]", args =>
            {
                return (WhileAST)args[0];
            })
            // statements
            .Production("statement : block", args =>
            {
                return (WhileAST)args[0];
            })
            .Production("sequence: statement*", args =>
            {
                return instance.sequence((List<WhileAST>)args[0]);
            })
            .Production("statement: IF[d] IndentedWhileParserGeneric_expressions THEN[d] block (ELSE[d] block)?",
                args =>
                {
                    var condition = (WhileAST)args[0];
                    var ifTrue = (WhileAST)args[1];
                    var ifFalse = (ValueOption<Group<IndentedWhileTokenGeneric, WhileAST>>)args[2];
                    return instance.ifStmt(condition, ifTrue, ifFalse);
                })
            .Production("statement: WHILE[d] IndentedWhileParserGeneric_expressions DO[d] block", args =>
            {
                var condition = (WhileAST)args[0];
                var block = (WhileAST)args[1];
                return instance.whileStmt(condition, block);
            })
            .Production("statement: IDENTIFIER ASSIGN[d] IndentedWhileParserGeneric_expressions", args =>
            {
                var id = (Token<IndentedWhileTokenGeneric>)args[0];
                var value = (Expression)args[1];
                return instance.assignStmt(id, value);
            })
            .Production("statement: SKIP[d]", args =>
            {
                return new SkipStatement();
            })
            .Production("statement: RETURN[d] IndentedWhileParserGeneric_expressions", args =>
            {
                var value = (Expression)args[0];
                return new ReturnStatement(value);
            })
            .Production("statement: PRINT[d] IndentedWhileParserGeneric_expressions", args =>
            {
                var value = (Expression)args[0];
                return new PrintStatement(value);
            })
            .WithLexerbuilder(GetLexer())
            .BuildParser();


        return parser;



    }
    
    
}