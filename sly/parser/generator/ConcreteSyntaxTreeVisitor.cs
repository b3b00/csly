using System;
using System.Collections.Generic;
using sly.parser.syntax;

namespace sly.parser.generator
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
                int i = 0;
                foreach (IConcreteSyntaxNode<T> n in node.Children)
                {
                    object v = Visit(n);

                    args.Add(v);

                    i++;

                }
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