using System;
using parser.parsergenerator.syntax;
using System.Collections.Generic;
using cpg.parser.parsgenerator.syntax;

namespace parser.parsergenerator.generator
{

    public class ConcreteSyntaxTreeVisitor<T>
    {

        public Type ParserClass { get; set; }

        public ParserConfiguration<T> Configuration { get; set; }

        public ConcreteSyntaxTreeVisitor(ParserConfiguration<T> conf)
        {
            this.ParserClass = ParserClass;
            this.Configuration = conf;
        }

        public object VisitSyntaxTree(ConcreteSyntaxNode<T> root)
        {
            return Visit(root);
        }

        private object Visit(IConcreteSyntaxNode<T> n)
        {
            if (n.IsTerminal())
            {
                return Visit(n as ConcreteSyntaxLeaf<T>);
            }
            else
            {
                return Visit(n as ConcreteSyntaxNode<T>);
            }
        }

        private object Visit(ConcreteSyntaxNode<T> node)
        {
            object result = null;
            if (Configuration.Functions.ContainsKey(node.Name))
            {
                List<object> args = new List<object>();
                node.Children.ForEach(n =>
                {
                    object v = Visit(n);
                    if (v != null)
                    {
                        args.Add(v);
                    }
                });
                result = Configuration.Functions[node.Name].Invoke(args);
            }
            return result;
        }

        private object Visit(ConcreteSyntaxLeaf<T> leaf)
        {
            return leaf.Token;
        }
    }
}