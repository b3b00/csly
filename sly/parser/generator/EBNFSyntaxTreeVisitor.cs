using System;
using System.Collections.Generic;
using sly.parser.syntax;
using sly.lexer;

namespace sly.parser.generator
{
    public class EBNFSyntaxTreeVisitor<T> : SyntaxTreeVisitor<T>
    {

        public EBNFSyntaxTreeVisitor(ParserConfiguration<T> conf) : base(conf)
        {
        }

        public EBNFSyntaxTreeVisitor(ParserConfiguration<T> conf, object parserInstance) : base(conf, parserInstance)
        {
        }



        protected override object Visit(ISyntaxNode<T> n)
        {
            if (n is SyntaxLeaf<T>)
            {
                return Visit(n as SyntaxLeaf<T>);
            }
            else if (n is SyntaxEpsilon<T>)
            {
                return Visit(n as SyntaxEpsilon<T>);
            }
            else if (n is ManySyntaxNode<T>)
            {
                return Visit(n as ManySyntaxNode<T>);
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

        private object Visit(ManySyntaxNode<T> node)
        {

            List<object> values = new List<object>();
            List<Token<T>> tokenValues = new List<Token<T>>();
            bool areTokens = false;
            int i = 0;
            foreach (ISyntaxNode<T> n in node.Children)
            {
                object v = Visit(n);

                values.Add(v);

                i++;
            }
            areTokens = values.Count > 0 && values[0].GetType() == typeof(Token<T>);
            if (areTokens)
            {
                foreach (object v in values)
                {
                    Token<T> t = (Token<T>)v;
                    tokenValues.Add(t);

                }
            }
            if (values.Count == 0)
            {
                return new List<Token<T>>();
            }
            if (areTokens)
            {
                return tokenValues;
            }
            else
            {
                return values;
            }

        }



        private object Visit(SyntaxLeaf<T> leaf)
        {
            return leaf.Token;
        }
    }
}