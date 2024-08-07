using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.syntax.tree;

namespace sly.parser.generator.visitor
{
    public class ConcreteSyntaxTreeWalker<IN, OUT, OUTPUT> where IN : struct
    {
        
        public IConcreteSyntaxTreeVisitor<IN,OUT, OUTPUT > Visitor { get; set; }

        public ConcreteSyntaxTreeWalker(IConcreteSyntaxTreeVisitor<IN, OUT, OUTPUT> visitor)
        {
            Visitor = visitor;
        } 
        
        private OUTPUT VisitLeaf(SyntaxLeaf<IN, OUT> leaf)
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
        
        public OUTPUT Visit(ISyntaxNode<IN, OUT> n)
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
                case SyntaxEpsilon<IN, OUT> epsilon:
                {
                    return Visitor.VisitEpsilon();
                }
                default:
                    return Visitor.VisitLeaf(new Token<IN>() {TokenID = default(IN),SpanValue="NULL".ToCharArray()});
            }
        }

        private OUTPUT Visit(GroupSyntaxNode<IN, OUT> node)
        {
            return Visit(node as SyntaxNode<IN, OUT>);
        }

        private OUTPUT Visit(OptionSyntaxNode<IN, OUT> node)
        {
            var child = node.Children != null && node.Children.Any<ISyntaxNode<IN, OUT>>() ? node.Children[0] : null;
            if (child == null || node.IsEmpty)
            {
                Visitor.VisitOptionNode(false, default(OUTPUT));
            }
            var r = Visit(child);
            return r;
        }


        private OUTPUT Visit(SyntaxNode<IN, OUT> node)
        {
            
            var children = new List<OUTPUT>();

            foreach (var n in node.Children)
            {
                var v = Visit(n);

                children.Add(v);
            }

           
            //return callback(node, children);
            return Visitor.VisitNode(node,children);
        }
        
        private OUTPUT Visit(ManySyntaxNode<IN, OUT> manyNode)
        {
            
            var children = new List<OUTPUT>();

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