using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace ParserTests.ambiguous;

[ParserRoot("programZero")]
public class DanglingElseParserWithRules
{
    [Production("programOne : statement +")]
    public string program_statement_1(List<string> statements)
    {
        return string.Join(";", statements);
    }
        
    [Production("programZero : statement +")]
    public string program_statement_0(List<string> statements)
    {
        return string.Join(";", statements);
    }
        
    [Production("programOneOrTwo : statement statement?")]
    public string program_statement_1_2(string statement, ValueOption<string> statement2)
    {
        return statement2.Match(
            (x) => $"{statement},{x}",
            () => statement);
    }
        
    [Production("statement : if_then")]
    [Production("statement : if_then_else")] 
    [Production("statement : assign")]
    public string statement_if_assign_(string statement) => statement;

    [Production("if_then : IF[d] cond THEN[d] statement")]
    public string if_then(string cond, string thenStatement)
    {
        return $"if({cond},{thenStatement})";
    }

    [Production("if_then_else : IF[d] cond THEN[d] statement ELSE[d] statement")]
    public string if_then_else(string cond, string thenStatement, string elseStatement)
    {
        return $"if({cond},{thenStatement},{elseStatement})";
    }

    [Production("assign : ID ASSIGN[d] INT")]
    public string assign_ID_ASSIGN_INT(Token<DanglingElseToken> id, Token<DanglingElseToken> value)
    {
        return $"{id.Value}:={value.Value}";
    }

    [Production("cond : ID EQUALS[d] INT")]
    public string cond_ID_EQUALS_INT(Token<DanglingElseToken> id, Token<DanglingElseToken> value)
    {
        return $"{id.Value}=={value.Value}";
    }
}