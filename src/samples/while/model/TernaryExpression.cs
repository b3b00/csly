using System;
using csly.whileLang.compiler;
using Sigil;
using sly.lexer;

namespace csly.whileLang.model;

public class TernaryExpression : Expression
{
    public LexerPosition Position { get; set; }
    public Scope CompilerScope { get; set; }
    
    public Expression Condition { get; set; }
    
    public Expression TrueExpression { get; set; }
    
    public Expression FalseExpression { get; set; }

    public TernaryExpression(Expression condition, Expression trueExpression, Expression falseExpression)
    {
        Condition = condition;
        TrueExpression = trueExpression;
        FalseExpression = falseExpression;
    }
    
    public string Dump(string tab)
    {
        return $"{tab}{Condition.Dump("")} ? {TrueExpression.Dump("")} :  {FalseExpression.Dump("")}";
    }

    public string Transpile(CompilerContext context)
    {
        return $"{Condition.Transpile(context)} ? {TrueExpression.Transpile(context)} :  {FalseExpression.Transpile(context)};";
    }

    public Emit<Func<int>> EmitByteCode(CompilerContext context, Emit<Func<int>> emiter)
    {
        var thenLabel = emiter.DefineLabel();
        var elseLabel = emiter.DefineLabel();
        var endLabel = emiter.DefineLabel();
        Condition.EmitByteCode(context, emiter);
        emiter.BranchIfTrue(thenLabel);
        emiter.Branch(elseLabel);
        emiter.MarkLabel(thenLabel);
        TrueExpression.EmitByteCode(context, emiter);
        emiter.Branch(endLabel);
        emiter.MarkLabel(elseLabel);
        FalseExpression.EmitByteCode(context, emiter);
        emiter.Branch(endLabel);
        emiter.MarkLabel(endLabel);
        return emiter;
    }

    public WhileType Whiletype { get; set; }
}