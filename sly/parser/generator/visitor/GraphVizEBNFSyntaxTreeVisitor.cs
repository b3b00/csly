using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetGraph;
using sly.lexer;
using sly.parser.parser;
using sly.parser.syntax.tree;

namespace sly.parser.generator.visitor
{
    public class GraphVizEBNFSyntaxTreeVisitor<IN>  where IN : struct
    {
        public  DotGraph Graph { get; private set; }

        public GraphVizEBNFSyntaxTreeVisitor()
        {
         Graph = new DotGraph("syntaxtree",true);
        }

        private int NodeCounter = 0;


        private DotNode Leaf(SyntaxLeaf<IN> leaf)
        {
            return Leaf(leaf.Token.TokenID, leaf.Token.Value);
        }
        
        private DotNode Leaf(IN type, string value)
        {
            string label = type.ToString();
            label += "\n";
            var esc = value.Replace("\"", "\\\"");
            label += "\\\"" + esc + "\\\"";
            var node =  new DotNode(NodeCounter.ToString()) {
                // Set all available properties
                Shape = DotNodeShape.Doublecircle,
                Label = label,
                FontColor = DotColor.Black,
                Style = DotNodeStyle.Default,
                Height = 0.5f
            };
            NodeCounter++;
            Graph.Add(node);
            return node;
        }

        public DotNode VisitTree(ISyntaxNode<IN> root)
        {
            return Visit(root);
        }

        private DotNode Node(string label)
        {
            var node =  new DotNode(NodeCounter.ToString()) {
                // Set all available properties
                Shape = DotNodeShape.Ellipse,
                Label = label,
                FontColor = DotColor.Black,
                Style = DotNodeStyle.Default,
                Height = 0.5f
            };
            NodeCounter++;
            Graph.Add(node);
            return node;
        }

        protected DotNode Visit(ISyntaxNode<IN> n)
        {
            if (n is SyntaxLeaf<IN>)
                return Visit(n as SyntaxLeaf<IN>);
            if (n is GroupSyntaxNode<IN>)
                return Visit(n as GroupSyntaxNode<IN>);
            if (n is ManySyntaxNode<IN>)
                return Visit(n as ManySyntaxNode<IN>);
            if (n is OptionSyntaxNode<IN>)
                return Visit(n as OptionSyntaxNode<IN>);
            if (n is SyntaxNode<IN>)
                return Visit(n as SyntaxNode<IN>);

            return null;
        }

        private DotNode Visit(GroupSyntaxNode<IN> node)
        {
            return Visit(node as SyntaxNode<IN>);
//            var values = new List<SyntaxVisitorResult<IN>>();
//            foreach (var n in node.Children)
//            {
//                var v = Visit(n);
//
//                if (v.IsValue) group.Add(n.Name, v.ValueResult);
//                if (v.IsToken)
//                    if (!v.Discarded)
//                        group.Add(n.Name, v.TokenResult);
//            }
//
//
//            var res = SyntaxVisitorResult<IN>.NewGroup(group);
//            return res;
        }

        private DotNode Visit(OptionSyntaxNode<IN> node)
        {
            var child = node.Children != null && node.Children.Any() ? node.Children[0] : null;
            if (child == null || node.IsEmpty)
            {
                    return null;
            }

            return Visit(child);
        }

        private string GetNodeLabel(SyntaxNode<IN> node)
        {
            string label = node.ShortName;
            if (node.IsExpressionNode)
            {
                label =  node.Operation.OperatorToken.ToString();
            }

            return label;
        }

        private DotNode Visit(SyntaxNode<IN> node)
        {
            DotNode result = null;


            var children = new List<DotNode>();

            foreach (var n in node.Children)
            {
                var v = Visit(n);

                children.Add(v);
            }

            if (node.IsByPassNode)
            {
                result = children[0];
            }
            else
            {

                result = Node(GetNodeLabel(node));
                Graph.Add(result);
                children.ForEach(c =>
                {
                    var edge = new DotArrow(result, c) {
                        // Set all available properties
                        ArrowHeadShape = DotArrowShape.None
                    };    
                    Graph.Add(edge);
                });
                
            }


            return result;
        }

        private DotNode Visit(ManySyntaxNode<IN> node)
        {
            return Visit(node as SyntaxNode<IN>);
        }


        private DotNode Visit(SyntaxLeaf<IN> leaf)
        {
            return Leaf(leaf.Token.TokenID,leaf.Token.Value);
        }
    }
}