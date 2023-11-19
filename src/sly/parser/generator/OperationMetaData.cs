using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace sly.parser.generator
{
    public class OperationMetaData<T> where T : struct
    {
        public OperationMetaData(int precedence, Associativity assoc, MethodInfo method, Affix affix, T oper)
        {
            Precedence = precedence;
            Associativity = assoc;
            VisitorMethod = method;
            OperatorToken = oper;
            Affix = affix;
        }
        
        public OperationMetaData(int precedence, Associativity assoc, MethodInfo method, Affix affix, string oper)
        {
            Precedence = precedence;
            Associativity = assoc;
            VisitorMethod = method;
            ExplicitOperatorToken = oper;
            Affix = affix;
        }

        public int Precedence { get; set; }

        public Associativity Associativity { get; set; }

        public MethodInfo VisitorMethod { get; set; }

        public T OperatorToken { get; set; }

        public string Operatorkey => IsExplicitOperatorToken ? ExplicitOperatorToken : OperatorToken.ToString();

        public Affix Affix { get; set; }

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