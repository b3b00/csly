using System.Collections.Generic;
using sly.parser.syntax.tree;

namespace sly.parser.parser
{
    public class SyntaxTreeCleaner<IN, OUT> where IN : struct
    {
        public SyntaxParseResult<IN, OUT> CleanSyntaxTree(SyntaxParseResult<IN, OUT> result)
        {
            var tree = result.Root;
            if (tree != null)
            {
                if (result.UsesOperations) tree = SetAssociativity(tree);
                result.Root = tree;
            }

            return result;
        }

        private ISyntaxNode<IN, OUT> SetAssociativity(ISyntaxNode<IN, OUT> tree)
        {
            ISyntaxNode<IN, OUT> result = null;


            switch (tree)
            {
                case ManySyntaxNode<IN, OUT> many:
                {
                    var newChildren = new List<ISyntaxNode<IN, OUT>>();
                    foreach (var child in many.Children) newChildren.Add(SetAssociativity(child));
                    many.Children.Clear();
                    many.Children.AddRange(newChildren);
                    result = many;
                    break;
                }
                case SyntaxLeaf<IN, OUT> leaf:
                    result = leaf;
                    break;
                case SyntaxNode<IN, OUT> node:
                {
                    if (NeedLeftAssociativity(node)) node = ProcessLeftAssociativity(node);
                    var newChildren = new List<ISyntaxNode<IN, OUT>>();
                    foreach (var child in node.Children) newChildren.Add(SetAssociativity(child));
                    node.Children.Clear();
                    node.Children.AddRange(newChildren);
                    result = node;
                    break;
                }
            }

            return result;
        }


        private bool NeedLeftAssociativity(SyntaxNode<IN, OUT> node)
        {
            return node.IsBinaryOperationNode && node.IsLeftAssociative
                                              && node.Right is SyntaxNode<IN, OUT> right && right.IsExpressionNode
                                              && right.Precedence == node.Precedence;
        }

        private SyntaxNode<IN, OUT> ProcessLeftAssociativity(SyntaxNode<IN, OUT> node)
        {
            var result = node;
            while (NeedLeftAssociativity(result))
            {
                var newLeft = result;
                var newTop = (SyntaxNode<IN, OUT>) result.Right;
                newLeft.Right = newTop.Left;
                newTop.Left = newLeft;
                result = newTop;
            }

            return result;
        }
    }
}