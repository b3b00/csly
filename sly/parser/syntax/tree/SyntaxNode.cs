using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using sly.parser.generator;

namespace sly.parser.syntax.tree
{
    public class SyntaxNode<IN> : ISyntaxNode<IN> where IN : struct
    {
        public SyntaxNode(string name, List<ISyntaxNode<IN>> children = null, MethodInfo visitor = null)
        {
            Name = name;
            Children = children == null ? new List<ISyntaxNode<IN>>() : children;
            Visitor = visitor;
        }

        public List<ISyntaxNode<IN>> Children { get; }

        public MethodInfo Visitor { get; set; }

        public bool IsByPassNode { get; set; } = false;

        public bool IsEmpty => Children == null || !Children.Any();

        public Affix ExpressionAffix { get; set; }


        public bool Discarded => false;
        public string Name { get; set; }


        [ExcludeFromCodeCoverage]
        public virtual string Dump(string tab)
        {
            var dump = new StringBuilder();

            dump.AppendLine($"{tab}Node {Name} {{");
            foreach (var c in Children) dump.AppendLine(c.Dump(tab + "\t"));

            dump.AppendLine($"{tab}}}");

            return dump.ToString();
        }

        #region expression syntax nodes

        public OperationMetaData<IN> Operation { get; set; } = null;

        public bool IsExpressionNode => Operation != null;

        public bool IsBinaryOperationNode => IsExpressionNode ? Operation.Affix == Affix.InFix : false;
        public bool IsUnaryOperationNode => IsExpressionNode ? Operation.Affix != Affix.InFix : false;
        public int Precedence => IsExpressionNode ? Operation.Precedence : -1;

        public Associativity Associativity =>
            IsExpressionNode && IsBinaryOperationNode ? Operation.Associativity : Associativity.None;

        public bool IsLeftAssociative => Associativity == Associativity.Left;

        public ISyntaxNode<IN> Left
        {
            get
            {
                ISyntaxNode<IN> l = null;
                if (IsExpressionNode)
                {
                    var leftindex = -1;
                    if (IsBinaryOperationNode) leftindex = 0;
                    if (leftindex >= 0) l = Children[leftindex];
                }

                return l;
            }
        }

        public ISyntaxNode<IN> Right
        {
            get
            {
                ISyntaxNode<IN> r = null;
                if (IsExpressionNode)
                {
                    var rightIndex = -1;
                    if (IsBinaryOperationNode)
                        rightIndex = 2;
                    else if (IsUnaryOperationNode) rightIndex = 1;
                    if (rightIndex > 0) r = Children[rightIndex];
                }

                return r;
            }
        }

        #endregion
    }
}