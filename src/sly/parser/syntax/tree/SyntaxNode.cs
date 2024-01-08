using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
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

        [JsonIgnore]
        public MethodInfo Visitor { get; set; }

        public bool IsByPassNode { get; set; } = false;

        public bool IsEmpty => Children == null || !Children.Any();

        public Affix ExpressionAffix { get; set; }


        public bool Discarded => false;
        public string Name { get; set; }

        public bool HasByPassNodes { get; set; } = false;

        #region expression syntax nodes

        [JsonIgnore]
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
        
        public string Dump(string tab)
        {
            StringBuilder builder = new StringBuilder();
            string expressionSuffix = "";
            if (Operation != null && Operation.IsBinary)
            {
                if (Operation.IsExplicitOperatorToken)
                {
                    expressionSuffix = Operation.ExplicitOperatorToken;
                }
                else
                {
                    expressionSuffix = Operation.OperatorToken.ToString();
                }

                expressionSuffix = $">{expressionSuffix}<";
            }
            
            builder.AppendLine($"{tab}+ {Name} {(IsByPassNode ? "===":"")}");
            
            foreach (var child in Children)
            {
                builder.AppendLine($"{child.Dump(tab + "\t")}");
            }

            return builder.ToString();
        }
        
        public string ToJson(int index = 0)
        {
            StringBuilder builder = new StringBuilder();


            builder.Append($@"""{index}.{Name}");
            if (IsByPassNode)
            {
                builder.Append("--");
            }

            builder.AppendLine(@""" : {");

            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                builder.Append(child.ToJson(i));
                if (i < Children.Count - 1)
                {
                    builder.Append(",");
                }

                builder.AppendLine();
            }

            builder.Append("}");

            return builder.ToString();
        }


        #endregion
    }
}