using System;
using System.Collections.Generic;
using sly.parser.syntax;

namespace sly.parser.generator
{
    public class SyntaxTreeVisitor<T>
    {
        public Type ParserClass { get; set; }

        public object ParserVsisitorInstance { get; set; }

        public ParserConfiguration<T> Configuration { get; set; }

        public SyntaxTreeVisitor(ParserConfiguration<T> conf)
        {
            this.ParserClass = ParserClass;
            this.Configuration = conf;
            this.ParserVsisitorInstance = null;
        }

        public SyntaxTreeVisitor(ParserConfiguration<T> conf, object parserInstance)
        {
            this.ParserClass = ParserClass;
            this.Configuration = conf;
            this.ParserVsisitorInstance = parserInstance;
        }

        public object VisitSyntaxTree(ISyntaxNode<T> root)
        {
            return Visit(root);
        }

        protected virtual object Visit(ISyntaxNode<T> n)
        {
            if (n is SyntaxLeaf<T>)
            {
                return Visit(n as SyntaxLeaf<T>);
            }
            else if (n is SyntaxEpsilon<T>)
            {
                return Visit(n as SyntaxEpsilon<T>);
            }
            else if (n is SyntaxNode<T>)
            {
                return Visit(n as SyntaxNode<T>);
            }
            else
            {
                return null;
            }
        }

        private object Visit(SyntaxNode<T> node)
        {
            object result = null;
            if (Configuration.Functions.ContainsKey(node.Name))
            {
                List<object> args = new List<object>();
                int i = 0;
                foreach (ISyntaxNode<T> n in node.Children)
                {
                    object v = Visit(n);

                    args.Add(v);

                    i++;
                }

                result = Configuration.Functions[node.Name].Invoke(ParserVsisitorInstance, args.ToArray());

            }
            return result;
        }

        private object Visit(SyntaxLeaf<T> leaf)
        {
            return leaf.Token;
        }
    }
}