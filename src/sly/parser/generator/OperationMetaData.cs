using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace sly.parser.generator
{
    public class OperationMetaData<IN, OUT> where IN : struct
    {
        public OperationMetaData(int precedence, Associativity assoc, MethodInfo method, Affix affix, IN oper, string nodeName = null)
        {
            Precedence = precedence;
            Associativity = assoc;
            VisitorMethod = method;
            OperatorToken = oper;
            Affix = affix;
            NodeName = nodeName;
        }
        
        public OperationMetaData(int precedence, Associativity assoc, MethodInfo method, Affix affix, string oper, string nodeName = null)
        {
            Precedence = precedence;
            Associativity = assoc;
            VisitorMethod = method;
            ExplicitOperatorToken = oper;
            Affix = affix;
            NodeName = nodeName;
        }
        
        public OperationMetaData(int precedence, Associativity assoc, Func<object[],OUT> lambda, Affix affix, IN oper, string nodeName = null)
        {
            Precedence = precedence;
            Associativity = assoc;
            VisitorLambda = lambda;
            OperatorToken = oper;
            Affix = affix;
            NodeName = nodeName;
        }
        
        public OperationMetaData(int precedence, Associativity assoc, Func<object[],OUT> lambda, Affix affix, string oper, string nodeName = null)
        {
            Precedence = precedence;
            Associativity = assoc;
            VisitorLambda = lambda;
            ExplicitOperatorToken = oper;
            Affix = affix;
            NodeName = nodeName;
        }

        public int Precedence { get; set; }

        public Associativity Associativity { get; set; }

        public MethodInfo VisitorMethod { get; set; }
        
        public Func<object[],OUT> VisitorLambda { get; set; }  

        public IN OperatorToken { get; set; }

        public string Operatorkey => NodeName ?? (IsExplicitOperatorToken ? ExplicitOperatorToken : OperatorToken.ToString());

        public Affix Affix { get; set; }
        
        public string NodeName { get; set; }

        public bool IsBinary => Affix == Affix.InFix;

        public bool IsUnary => Affix != Affix.InFix;

        public bool IsExplicitOperatorToken => !string.IsNullOrEmpty(ExplicitOperatorToken);

        public string ExplicitOperatorToken { get; set; }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"{OperatorToken} / {Affix} : {Precedence} / {Associativity}";
        }
    }
}