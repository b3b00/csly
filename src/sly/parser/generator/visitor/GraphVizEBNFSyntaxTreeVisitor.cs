using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using sly.lexer;
using sly.parser.generator.visitor.dotgraph;
using sly.parser.syntax.tree;

namespace sly.parser.generator.visitor
{
    [ExcludeFromCodeCoverage]
    public class GraphVizEBNFSyntaxTreeVisitor<IN>  : IConcreteSyntaxTreeVisitor<IN,DotNode> where IN : struct
    {
        public DotGraph Graph { get; private set; }

        public GraphVizEBNFSyntaxTreeVisitor()
        {
            Graph = new DotGraph("syntaxtree", true);
        }

        private int NodeCounter = 0;

        public DotNode VisitTree(ISyntaxNode<IN> root)
        {
            Graph = new DotGraph("syntaxtree", true);
            
            ConcreteSyntaxTreeWalker<IN,DotNode> walker = new ConcreteSyntaxTreeWalker<IN,DotNode>(this);
            var dot =  walker.Visit(root);
            return dot;
        }

        private DotNode Node(string label)
        {
            var node = new DotNode(NodeCounter.ToString())
            {
                // Set all available properties
                Shape = "ellipse",
                Label = label,
                FontColor = "black",
                Style = null,
                Height = 0.5f
            };
            NodeCounter++;
            Graph.Add(node);
            return node;
        }

        public DotNode VisitOptionNode(bool exists, DotNode child)
        {
            if (!exists)
            {
                return VisitLeaf(new Token<IN>() { TokenID = default(IN), SpanValue = "<NONE>".ToCharArray() });
            }
            return child;
        }

        public DotNode VisitNode(SyntaxNode<IN> node, IList<DotNode> children)
        {
            DotNode result = null;

            result = Node(GetNodeLabel(node));
            //children.ForEach(c =>
            foreach (var child in children)
            {
                if (child != null) // Prevent arrows with null destinations
                {
                    var edge = new DotArrow(result, child)
                    {
                        // Set all available properties
                        ArrowHeadShape = "none"
                    };
                    Graph.Add(edge);
                    //Graph.Add(child);
                }
            }
            return result;
        }

        public DotNode VisitManyNode(ManySyntaxNode<IN> node, IList<DotNode> children)
        {
            DotNode result = null;

            result = Node(GetNodeLabel(node));
            Graph.Add(result);
            //children.ForEach(c =>
            foreach (var child in children)
            {
                if (child != null) // Prevent arrows with null destinations
                {
                    var edge = new DotArrow(result, child)
                    {
                        // Set all available properties
                        ArrowHeadShape = "none"
                    };
                    Graph.Add(edge);
                }
            }
            return result;
        }

        public DotNode VisitEpsilon()
        {
            return VisitLeaf(new Token<IN>() { TokenID = default(IN), SpanValue = "epsilon".ToCharArray() });
        }

       

        public DotNode VisitLeaf(Token<IN> token)
        {
            if (token.IsIndent)
            {
                return Leaf(token.TokenID, "INDENT>>");
            }
            else if (token.IsUnIndent)
            {
                return Leaf(token.TokenID, "<<UNINDENT");
            }
            else if (token.IsExplicit)
            {
                return Leaf(token.Value);
            }
            return Leaf(token.TokenID, token.Value);
        }

        private DotNode Leaf(IN type, string value)
        {
            string label = type.ToString();
            if (label == "0")
            {
                label = "";
            }
            else
            {
                label += "\n";
            }
            var esc = value.Replace("\"", "\\\"");
            label += "\\\"" + esc + "\\\"";
            var node = new DotNode(NodeCounter.ToString())
            {
                // Set all available properties
                Shape = "doublecircle",
                Label = label,
                FontColor = "",
                Style = "",
                Height = 0.5f
            };
            NodeCounter++;
            Graph.Add(node);
            return node;
        }
        
        private DotNode Leaf(string value)
        {
            string label = "";
            var esc = value.Replace("\"", "\\\"");
            label += "\\\"" + esc + "\\\"";
            var node = new DotNode(NodeCounter.ToString())
            {
                // Set all available properties
                Shape = "doublecircle",
                Label = label,
                FontColor = "",
                Style = "",
                Height = 0.5f
            };
            NodeCounter++;
            Graph.Add(node);
            return node;
        }
        
        
        private string GetNodeLabel(SyntaxNode<IN> node)
        {
            string label = node.Name;
            return label;
        }


    }
}