using System.Collections.Generic;
using sly.parser.syntax.tree;

namespace sly.parser.parser
{
    public class SyntaxTreeCleaner<IN> where IN : struct
    {
        public SyntaxParseResult<IN> CleanSyntaxTree(SyntaxParseResult<IN> result)
        {
            var tree = result.Root;
            if (tree != null)
            {
                if (result.HasByPassNodes)
                {
                    tree = RemoveByPassNodes(tree);
                }
                    
                
                if (result.UsesOperations) tree = SetAssociativity(tree);
                result.Root = tree;
            }

            return result;
        }

        private ISyntaxNode<IN> RemoveByPassNodes(ISyntaxNode<IN> tree)
        {
            ISyntaxNode<IN> result = null;


            if (tree is SyntaxNode<IN> node && node.IsByPassNode)
            {
                result = RemoveByPassNodes(node.Children[0]);
            }
            else
            {
                switch (tree)
                {
                    case SyntaxLeaf<IN> leaf:
                        result = leaf;
                        break;
                    case SyntaxNode<IN> innernode:
                    {
                        var newChildren = new List<ISyntaxNode<IN>>();
                        foreach (var child in innernode.Children) newChildren.Add(RemoveByPassNodes(child));
                        innernode.Children.Clear();
                        innernode.Children.AddRange(newChildren);
                        result = innernode;
                        break;
                    }
                }

                if (tree is ManySyntaxNode<IN> many)
                {
                    var newChildren = new List<ISyntaxNode<IN>>();
                    foreach (var child in many.Children) newChildren.Add(RemoveByPassNodes(child));
                    many.Children.Clear();
                    many.Children.AddRange(newChildren);
                    result = many;
                }
            }

            return result;
        }


        private ISyntaxNode<IN> SetAssociativity(ISyntaxNode<IN> tree)
        {
            ISyntaxNode<IN> result = null;


            switch (tree)
            {
                case ManySyntaxNode<IN> many:
                {
                    var newChildren = new List<ISyntaxNode<IN>>();
                    foreach (var child in many.Children) newChildren.Add(SetAssociativity(child));
                    many.Children.Clear();
                    many.Children.AddRange(newChildren);
                    result = many;
                    break;
                }
                case SyntaxLeaf<IN> leaf:
                    result = leaf;
                    break;
                case SyntaxNode<IN> node:
                {
                    if (NeedLeftAssociativity(node)) node = ProcessLeftAssociativity(node);
                    var newChildren = new List<ISyntaxNode<IN>>();
                    foreach (var child in node.Children) newChildren.Add(SetAssociativity(child));
                    node.Children.Clear();
                    node.Children.AddRange(newChildren);
                    result = node;
                    break;
                }
            }

            return result;
        }


        private bool NeedLeftAssociativity(SyntaxNode<IN> node)
        {
            return node.IsBinaryOperationNode && node.IsLeftAssociative
                                              && node.Right is SyntaxNode<IN> right && right.IsExpressionNode
                                              && right.Precedence == node.Precedence;
        }

        private SyntaxNode<IN> ProcessLeftAssociativity(SyntaxNode<IN> node)
        {
            var result = node;
            while (NeedLeftAssociativity(result))
            {
                var newLeft = result;
                var newTop = (SyntaxNode<IN>) result.Right;
                newLeft.Children[2] = newTop.Left;
                newTop.Children[0] = newLeft;
                result = newTop;
            }

            return result;
        }
    }
}