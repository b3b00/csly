using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using sly.lexer;
using sly.parser.generator.visitor.dotgraph;
using sly.parser.generator.visitor.mermaid;
using sly.parser.syntax.tree;

namespace sly.parser.generator.visitor
{
    [ExcludeFromCodeCoverage]
    public class MermaidEBNFSyntaxTreeVisitor<IN>  : IConcreteSyntaxTreeVisitor<IN,MermaidNode> where IN : struct
    {
        public MermaidGraph Graph { get; private set; }

        public MermaidEBNFSyntaxTreeVisitor()
        {
            Graph = new MermaidGraph();
        }

        private int NodeCounter = 0;

        public MermaidNode VisitTree(ISyntaxNode<IN> root)
        {
            Graph = new MermaidGraph();
            
            ConcreteSyntaxTreeWalker<IN,MermaidNode> walker = new ConcreteSyntaxTreeWalker<IN,MermaidNode>(this);
            var dot =  walker.Visit(root);
            return dot;
        }

        private MermaidNode Node(string label, bool nodeIsByPassNode)
        {
            var shape = nodeIsByPassNode ? MermaidNodeShape.Circle : MermaidNodeShape.Rhombus;
            var style = nodeIsByPassNode ? MermaidNodeStyle.Dotted : MermaidNodeStyle.Solid;
            var node = new MermaidNode(NodeCounter.ToString())
            {
                // Set all available properties
                Shape = shape,
                Style = style,
                Label = label,
                FontColor = "black",
                Height = 0.5f
            };
            NodeCounter++;
            Graph.Add(node);
            return node;
        }

        public MermaidNode VisitOptionNode(bool exists, MermaidNode child)
        {
            if (!exists)
            {
                return VisitLeaf(new Token<IN>() { TokenID = default(IN), SpanValue = "<NONE>".ToCharArray() });
            }
            return child;
        }

        public MermaidNode VisitNode(SyntaxNode<IN> node, IList<MermaidNode> children)
        {
            MermaidNode result = null;

            result = Node(GetNodeLabel(node),node.IsByPassNode);
            //children.ForEach(c =>
            foreach (var child in children)
            {
                if (child != null) // Prevent arrows with null destinations
                {
                    var edge = new MermaidArrow(result, child)
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

        public MermaidNode VisitManyNode(ManySyntaxNode<IN> node, IList<MermaidNode> children)
        {
            MermaidNode result = null;

            result = Node(GetNodeLabel(node),node.IsByPassNode);
            Graph.Add(result);
            //children.ForEach(c =>
            foreach (var child in children)
            {
                if (child != null) // Prevent arrows with null destinations
                {
                    var edge = new MermaidArrow(result, child)
                    {
                        // Set all available properties
                        ArrowHeadShape = "none"
                    };
                    Graph.Add(edge);
                }
            }
            return result;
        }

        public MermaidNode VisitEpsilon()
        {
            return VisitLeaf(new Token<IN>() { TokenID = default(IN), SpanValue = "epsilon".ToCharArray() });
        }

       

        public MermaidNode VisitLeaf(Token<IN> token)
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
                return Leaf(token.StringWithoutQuotes);
            }
            return Leaf(token.TokenID, token.StringWithoutQuotes);
        }

        private MermaidNode Leaf(IN type, string value)
        {
            if (value.Contains("pi"))
            {
                Console.WriteLine("3.14");
            }
            string label = "\""+type.ToString();
            if (label == "0")
            {
                label = "";
            }
            else
            {
                label += "\n";
            }
            label += $"'{value}'\"";
            var node = new MermaidNode(NodeCounter.ToString())
            {
                // Set all available properties
                Shape = MermaidNodeShape.DoubleCircle,
                Label = label,
                FontColor = "",
                Style = MermaidNodeStyle.Solid,
                Height = 0.5f
            };
            NodeCounter++;
            Graph.Add(node);
            return node;
        }
        
        private MermaidNode Leaf(string value)
        {
            string label = "";
            label += $@"""'{value}'""";
            var node = new MermaidNode(NodeCounter.ToString())
            {
                // Set all available properties
                Shape = MermaidNodeShape.DoubleCircle,
                Label = label,
                FontColor = "",
                Style = MermaidNodeStyle.Solid,
                Height = 0.5f
            };
            NodeCounter++;
            Graph.Add(node);
            return node;
        }
        
        
        private string GetNodeLabel(SyntaxNode<IN> node)
        {
            string label = $@"""{node.Name}""";
            return label;
        }


    }
}