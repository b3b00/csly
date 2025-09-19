using System;
using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.syntax.tree;

namespace sly.parser.generator.visitor
{
    public class ConcreteSyntaxTreeWalker<IN, OUT, TREE_OUT> where IN : struct, Enum
    {
        
        public IConcreteSyntaxTreeVisitor<IN,OUT, TREE_OUT> Visitor { get; set; }

        public ConcreteSyntaxTreeWalker(IConcreteSyntaxTreeVisitor<IN, OUT, TREE_OUT> visitor)
        {
            Visitor = visitor;
        } 
        
        private TREE_OUT VisitLeaf(SyntaxLeaf<IN, OUT> leaf)
        {
            if (leaf.Token.IsIndent)
            {
                return Visitor.VisitLeaf(leaf.Token);
            }
            else if (leaf.Token.IsUnIndent)
            {
                return Visitor.VisitLeaf(leaf.Token);
            }
            else if (leaf.Token.IsExplicit)
            {
                return Visitor.VisitLeaf(leaf.Token);
            }
            return Visitor.VisitLeaf(leaf.Token);
        }
        
        public TREE_OUT Visit(ISyntaxNode<IN, OUT> n)
        {
            switch (n)
            {
                case SyntaxLeaf<IN, OUT> leaf:
                    return VisitLeaf(leaf);
                case GroupSyntaxNode<IN, OUT> node:
                    return Visit(node);
                case ManySyntaxNode<IN, OUT> node:
                    return Visit(node);
                case OptionSyntaxNode<IN, OUT> node:
                    return Visit(node);
                case SyntaxNode<IN, OUT> node:
                    return Visit(node);
                default:
                    return Visitor.VisitLeaf(new Token<IN>() {TokenID = default(IN),SpanValue="NULL".ToCharArray()});
            }
        }

        private TREE_OUT Visit(GroupSyntaxNode<IN, OUT> node)
        {
            return Visit(node as SyntaxNode<IN, OUT>);
        }

        private TREE_OUT Visit(OptionSyntaxNode<IN, OUT> node)
        {
            var child = node.Children != null && node.Children.Any<ISyntaxNode<IN, OUT>>() ? node.Children[0] : null;
            if (child == null || node.IsEmpty)
            {
                Visitor.VisitOptionNode(false, default(TREE_OUT));
            }
            var r = Visit(child);
            return r;
        }


        private TREE_OUT Visit(SyntaxNode<IN, OUT> node)
        {
            
            var children = new List<TREE_OUT>();

            foreach (var n in node.Children)
            {
                var v = Visit(n);

                children.Add(v);
            }
           
            return Visitor.VisitNode(node,children);
        }
        
        private TREE_OUT Visit(ManySyntaxNode<IN, OUT> manyNode)
        {

            var children = new List<TREE_OUT>();

            foreach (var n in manyNode.Children)
            {
                var v = Visit(n);

                children.Add(v);
            }

            return Visitor.VisitManyNode(manyNode,children);
        }

        
    }
}