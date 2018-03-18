using sly.parser.generator;
using sly.parser.syntax;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;


namespace sly.parser.syntax
{

    public class SyntaxNode<IN> : ISyntaxNode<IN> where IN : struct
    {



        public string Name { get; set; }

        public List<ISyntaxNode<IN>> Children { get; }

        public MethodInfo Visitor { get; set; }

        public bool IsByPassNode { get; set; } = false;

        #region expression syntax nodes

        public OperationMetaData<IN> Operation { get; set; } = null;

        public bool IsExpressionNode => Operation != null;

        public bool IsBinaryOperationNode => IsExpressionNode ? Operation.Arity == 2 : false;
        public bool IsUnaryOperationNode => IsExpressionNode ? Operation.Arity == 1 : false;
        public int Precedence => IsExpressionNode ? Operation.Precedence : -1;

        public Associativity Associativity => IsExpressionNode && IsBinaryOperationNode ? Operation.Associativity : Associativity.None;

        public bool IsLeftAssociative => Associativity == Associativity.Left;

        public bool IsRightAssociative => Associativity == Associativity.Right;

        public ISyntaxNode<IN> Left
        {
            get  {
                ISyntaxNode<IN> l = null;
                if (IsExpressionNode)
                {
                    int leftindex = -1;
                    if (IsBinaryOperationNode)
                    {
                        leftindex = 0;
                    }                    
                    if (leftindex >= 0)
                    {
                        l = Children[leftindex];
                    }
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
                    int rightIndex = -1;
                    if (IsBinaryOperationNode)
                    {
                        rightIndex = 2;
                    }
                    else if (IsUnaryOperationNode)
                    {
                        rightIndex = 1;
                    }
                    if (rightIndex > 0)
                    {
                        r = Children[rightIndex];
                    }
                }
                return r;
            }
        }


        #endregion

        public SyntaxNode(string name, List<ISyntaxNode<IN>> children = null, MethodInfo visitor = null)
        {
            this.Name = name;            
            this.Children = children == null ? new List<ISyntaxNode<IN>>() : children;
            this.Visitor = visitor;
        }
        
      

        public void AddChildren(List<ISyntaxNode<IN>> children)
        {
            this.Children.AddRange(children);
        }

        public void AddChild(ISyntaxNode<IN> child)
        {
            this.Children.Add(child);
        }

        public bool IsTerminal() {
            return false;
        }

        

       

    }
}