using Microsoft.CodeAnalysis.CSharp.Syntax;
using sly.lexer;
using sly.parser.generator;

namespace sly.sourceGenerator.generated.ebnfparser.model;

public interface IOperationClause
{
    bool IsOperand { get;  }
}

public class OperandClause : IOperationClause
{
    public bool IsOperand => true;
}

public class OperationClause : IOperationClause
{
    public bool IsOperand => false;
    public int Precedence { get; set; }

    public Associativity Associativity { get; set; }

    public MethodDeclarationSyntax Method { get; set; }
    
    public string OperatorToken { get; set; }

    public string Operatorkey => NodeName ?? (IsExplicitOperatorToken ? ExplicitOperatorToken : OperatorToken);

    public Affix Affix { get; set; }
        
    public string NodeName { get; set; }

    public bool IsBinary => Affix == Affix.InFix;

    public bool IsUnary => Affix != Affix.InFix;

    public bool IsExplicitOperatorToken => !string.IsNullOrEmpty(ExplicitOperatorToken);

    public string ExplicitOperatorToken { get; set; }
    
    public OperationClause(int precedence, Associativity assoc, MethodDeclarationSyntax method, Affix affix, string oper, string nodeName = null)
    {
        Precedence = precedence;
        Associativity = assoc;
        Method = method;
        if (oper.StartsWith("'") && oper.EndsWith("'"))
        {
            ExplicitOperatorToken = oper.Substring(1, oper.Length - 2);
        }
        else
        {
            OperatorToken = oper;
        }
        Affix = affix;
        NodeName = nodeName;
    }

}