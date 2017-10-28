using sly.parser.syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace sly.parser.parser
{
    public class SyntaxTreeCleaner<IN> where IN : struct
    {

        public SyntaxParseResult<IN> CleanSyntaxTree(SyntaxParseResult<IN> result)
        {
            ISyntaxNode<IN> tree = result.Root;
            if (tree != null)
            {
                tree = RemoveByPassNodes(tree);
                tree = SetAssociativity(tree);
                result.Root = tree;
                
            }
            return result;
        }

        private ISyntaxNode<IN> RemoveByPassNodes(ISyntaxNode<IN> tree)
        {
            ISyntaxNode<IN> result = null;
            try
            {
                
                if (tree is SyntaxNode<IN> node && node.IsByPassNode)
                {
                    result = RemoveByPassNodes(node.Children[0]);
                }
                else
                {
                    if (tree is SyntaxLeaf<IN> leaf)
                    {
                        result = leaf;
                    }
                    if (tree is SyntaxNode<IN> innernode)
                    {
                        var newChildren = new List<ISyntaxNode<IN>>();
                        foreach (var child in innernode.Children)
                        {
                            newChildren.Add(RemoveByPassNodes(child));
                        }
                        innernode.Children.Clear();
                        innernode.Children.AddRange(newChildren);
                        result = innernode;
                    }
                    if (tree is ManySyntaxNode<IN> many)
                    {
                        var newChildren = new List<ISyntaxNode<IN>>();
                        foreach (var child in many.Children)
                        {
                            newChildren.Add(RemoveByPassNodes(child));
                        }
                        many.Children.Clear();
                        many.Children.AddRange(newChildren);
                        result = many;
                    }
                }
            }
            catch(Exception e)
            {
                ;
            }
            return result;
        }




        private ISyntaxNode<IN> SetAssociativity(ISyntaxNode<IN> tree)
        {
            ISyntaxNode<IN> result = null;

            if (tree is SyntaxNode<IN> node)
            {
                var newNode = (SyntaxNode<IN>)node.Clone();
               if (NeedLeftAssociativity(node))
                {
                    newNode = ApplyLeftAssociativity(node);
                }
                var newChildren = new List<ISyntaxNode<IN>>();
                foreach (var child in newNode.Children)
                {
                    newChildren.Add(SetAssociativity(child));
                }
                node.Children.Clear();
                node.Children.AddRange(newChildren);
                result = newNode;

            }
            if (tree is ManySyntaxNode<IN> many)
            {
                var newNode = (ManySyntaxNode<IN>)many.Clone();
                var newChildren = new List<ISyntaxNode<IN>>();
                foreach (var child in newNode.Children)
                {
                    newChildren.Add(SetAssociativity(child));
                }
                many.Children.Clear();
                many.Children.AddRange(newChildren);
                result = many;
            }
            if (tree is SyntaxLeaf<IN> leaf)
            {
                result = leaf;
            }
            return result;
        }


        private bool NeedLeftAssociativity(SyntaxNode<IN> node)
        {
            return node.IsBinaryOperationNode && node.IsLeftAssociative
                && node.Right is SyntaxNode<IN> right && right.IsExpressionNode
                && right.Precedence == node.Precedence;
        }

        private SyntaxNode<IN> ApplyLeftAssociativity(SyntaxNode<IN> node)
        {
            var result = node;
            while (NeedLeftAssociativity(result))
            {
                var newLeft = (SyntaxNode<IN>)node.Clone();
                var newTop = (SyntaxNode<IN>)node.Right.Clone();
                newLeft.Children[2] = newTop.Left;
                newTop.Children[0] = newLeft;
                result = newTop;
            }
            
            return result;
        }

    }
}
