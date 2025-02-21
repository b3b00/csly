using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue538;

public class Issue538Parser
{
    [Production("NTSection :  NTStatement *")]
    public object Section(List<object> statements)
    {
        return null;
    }

    [Production(
        "NTStatement : [ NTSCExpr  |  NTLoop   ]")]
    public object Statement(object statement)
    {
        return null;
    }

    [Production("NTLoop : [ NTForLoopHeader  |  NTWhileLoopHeader ]  NTLoopLabel ?  NTStatement   NTElse ?")]
    public object Loop(object loopHeader, object loopLabel, object statement, object elseStatement)
    {
        return null;
    }

    [Production(
        "NTForLoopHeader : For [d] OpenParen [d]  NTExpression  Semicolon [d]  NTExpression  Semicolon [d]  NTExpression  CloseParen [d]")]
    public object ForLoopHeader(object expression1, object expression2, object expression3)
    {
        return null;
    }

    [Production("NTWhileLoopHeader : While [d] OpenParen [d]  NTExpression  CloseParen [d]")]
    public object WhileLoopHeader(object expression)
    {
        return null;
    }
    
    [Production("NTLoopLabel : Identifier [d] Colon [d]")]
    public object LoopLabel(object identifier)
    {
        return null;
    }
    
    [Production("NTSCExpr :'42'")]
    public object SCExpr(Token<Issue538Token> token)
    {
        return null;
    }
    
    [Production("NTExpression :'42'")]
    public object Expression(Token<Issue538Token> token)
    {
        return null;
    }
    
}