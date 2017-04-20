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

        public object VisitSyntaxTree(IConcreteSyntaxNode<T> root)
        {
            return Visit(root);
        }

        private object Visit(IConcreteSyntaxNode<T> n)
        {
            IConcreteSyntaxNode<T> visited = null;
            if (n is ConcreteSyntaxLeaf<T>)
            {
                return Visit(n as ConcreteSyntaxLeaf<T>);
            }
            else if (n is ConcreteSyntaxEpsilon<T>)
            {
                return Visit(n as ConcreteSyntaxEpsilon<T>);
            }
            else if (n is ConcreteSyntaxNode<T>)
            {
                return Visit(n as ConcreteSyntaxNode<T>);
            }
            else
            {
                return null;
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
                    else
                    {
                        ;
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