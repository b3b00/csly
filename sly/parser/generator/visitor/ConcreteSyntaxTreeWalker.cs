using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.syntax.tree;

namespace sly.parser.generator.visitor
{
    public class ConcreteSyntaxTreeWalker<IN, OUT> where IN : struct
    {
        
        public IConcreteSyntaxTreeVisitor<IN,OUT> Visitor { get; set; }

        public ConcreteSyntaxTreeWalker(IConcreteSyntaxTreeVisitor<IN, OUT> visitor)
        {
            Visitor = visitor;
        } 
        
        private OUT VisitLeaf(SyntaxLeaf<IN> leaf)
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
        
        public OUT Visit(ISyntaxNode<IN> n)
        {
            switch (n)
            {
                case SyntaxLeaf<IN> leaf:
                    return VisitLeaf(leaf);
                case GroupSyntaxNode<IN> node:
                    return Visit(node);
                case ManySyntaxNode<IN> node:
                    return Visit(node);
                case OptionSyntaxNode<IN> node:
                    return Visit(node);
                case SyntaxNode<IN> node:
                    return Visit(node);
                case SyntaxEpsilon<IN> epsilon:
                {
                    return Visitor.VisitEpsilon();
                }
                default:
                    return Visitor.VisitLeaf(new Token<IN>() {TokenID = default(IN),SpanValue="NULL".ToCharArray()});
            }
        }

        private OUT Visit(GroupSyntaxNode<IN> node)
        {
            return Visit(node as SyntaxNode<IN>);
        }

        private OUT Visit(OptionSyntaxNode<IN> node)
        {
            var child = node.Children != null && node.Children.Any<ISyntaxNode<IN>>() ? node.Children[0] : null;
            if (child == null || node.IsEmpty)
            {
                Visitor.VisitOptionNode(false, default(OUT));
            }
            var r = Visit(child);
            return r;
        }


        private OUT Visit(SyntaxNode<IN> node)
        {
            
            var children = new List<OUT>();

            foreach (var n in node.Children)
            {
                var v = Visit(n);

                children.Add(v);
            }

           
            //return callback(node, children);
            return Visitor.VisitNode(node,children);
        }
        
        private OUT Visit(ManySyntaxNode<IN> manyNode)
        {
            
            var children = new List<OUT>();

            foreach (var n in manyNode.Children)
            {
                var v = Visit(n);

                children.Add(v);
            }

           
            //return callback(node, children);
            return Visitor.VisitManyNode(manyNode,children);
        }

        
    }
}