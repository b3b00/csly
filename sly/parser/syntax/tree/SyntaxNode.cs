using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sly.parser.generator;

namespace sly.parser.syntax.tree
{
    public class SyntaxNode<IN> : ISyntaxNode<IN> where IN : struct
    {
        public SyntaxNode(string name, List<ISyntaxNode<IN>> children = null, MethodInfo visitor = null)
        {
            Name = name;
            Children = children ?? new List<ISyntaxNode<IN>>();
            Visitor = visitor;
        }

        public List<ISyntaxNode<IN>> Children { get; }

        public MethodInfo Visitor { get; set; }

        public bool IsByPassNode { get; set; } = false;

        public bool IsEmpty => Children == null || !Children.Any();

        public Affix ExpressionAffix { get; set; }


        public bool Discarded => false;
        public string Name { get; set; }

        #region expression syntax nodes

        public OperationMetaData<IN> Operation { get; set; } = null;

        public bool IsExpressionNode => Operation != null;

        public bool IsBinaryOperationNode => IsExpressionNode && Operation.Affix == Affix.InFix;
        public bool IsUnaryOperationNode => IsExpressionNode && Operation.Affix != Affix.InFix;
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