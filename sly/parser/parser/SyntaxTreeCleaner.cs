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
                // todo :
                // si n isexpr 
                if (node.IsBinaryOperationNode &&  node.IsLeftAssociative)
                {
                    if (node.Right is SyntaxNode<IN> right && right.IsExpressionNode)
                    {
                        if (right.Precedence == node.Precedence)
                        {

                            // TODO first recurse down

                            SyntaxNode<IN> top = new SyntaxNode<IN>(right.Name);
                            SyntaxNode<IN> left = new SyntaxNode<IN>(node.Name);
                            left.AddChild(node.Left);
                            left.AddChild(node.Operator);
                            left.AddChild(right.Left);
                            //do permutation
                            top.AddChild(left);
                            top.AddChild(right.Operator);
                            top.AddChild(right.Right);

                            result = top;
                        }
                    }
                }                
            }
            if (tree is ManySyntaxNode<IN> many)
            {

            }
            if (tree is SyntaxEpsilon<IN> leaf)
            {

            }
            return result;

            
        }





    }
}
