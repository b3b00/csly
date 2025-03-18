using System;
using System.Collections.Generic;
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
    
    public int Number { get; set; }

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

    public Emit<Func<int>> EmitByteCodeForVariable(CompilerContext context, Emit<Func<int>> emiter)
    {
        var falseAssign = new AssignStatement($"ternary_{Number}",FalseExpression);
        falseAssign.IsVariableCreation = true;
        context.CurrentScope.SetVariableType($"ternary_{Number}", TrueExpression.Whiletype);
        falseAssign.CompilerScope = context.CurrentScope;
        var trueAssign = new AssignStatement($"ternary_{Number}",TrueExpression);
        trueAssign.CompilerScope = context.CurrentScope;
        var ifthen = new IfStatement(Condition, trueAssign,new SkipStatement());
        ifthen.CompilerScope = context.CurrentScope;
        SequenceStatement seq = new SequenceStatement(new List<Statement>() {falseAssign, ifthen});
        seq.CompilerScope = context.CurrentScope;
        seq.EmitByteCode(context, emiter);
        //
        // var type = TypeConverter.WhileToType(TrueExpression.Whiletype);
        // _local = emiter.DeclareLocal(type);
        // var thenLabel = emiter.DefineLabel();
        // var elseLabel = emiter.DefineLabel();
        // var endLabel = emiter.DefineLabel();
        // Condition.EmitByteCode(context, emiter);
        // emiter.BranchIfTrue(thenLabel);
        // emiter.Branch(elseLabel);
        // emiter.MarkLabel(thenLabel);
        // TrueExpression.EmitByteCode(context, emiter);
        // emiter.StoreLocal(_local);
        // emiter.Branch(endLabel);
        // emiter.MarkLabel(elseLabel);
        // FalseExpression.EmitByteCode(context, emiter);
        // emiter.StoreLocal(_local);
        // emiter.MarkLabel(endLabel);
        return emiter;
    }

    public Emit<Func<int>> EmitByteCode(CompilerContext context, Emit<Func<int>> emiter)
    {
        var local = emiter.Locals[$"ternary_{Number}"];
        emiter.LoadLocal(local);
        return emiter;
    }

    public WhileType Whiletype { get; set; }
    public void AppendTernaries(List<TernaryExpression> ternaries)
    {
        ternaries.Add(this);
        Condition.AppendTernaries(ternaries);
        TrueExpression.AppendTernaries(ternaries);
        FalseExpression.AppendTernaries(ternaries);
    }
}