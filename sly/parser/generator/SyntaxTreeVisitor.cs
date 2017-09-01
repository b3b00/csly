using System;
using System.Collections.Generic;
using sly.parser.syntax;
using sly.lexer;

namespace sly.parser.generator
{

     class SyntaxVisitorResult<IN,OUT>
    {
        
        public Token<IN> tokenResult = null;

        public OUT valueResult = default(OUT);

        private bool isTok = false;

        public bool isToken {  get { return isTok; }}

        private bool isVal = false;

        public bool isValue { get { return isVal; } }

        public bool isNone { get { return !isToken && !isValue; } }

        public static SyntaxVisitorResult<IN,OUT> token(Token<IN> tok)
        {
            SyntaxVisitorResult<IN, OUT> res = new SyntaxVisitorResult<IN, OUT>();
            res.tokenResult = tok;
            res.isTok = true;
            return res;
        }

        public static SyntaxVisitorResult<IN, OUT> value(OUT val)
        {
            SyntaxVisitorResult<IN, OUT> res = new SyntaxVisitorResult<IN, OUT>();
            res.valueResult = val;
            res.isVal = true;
            return res;
        }

        public static SyntaxVisitorResult<IN, OUT> none()
        {
            SyntaxVisitorResult<IN, OUT> res = new SyntaxVisitorResult<IN, OUT>();            
            return res;
        }


    }

    public class SyntaxTreeVisitor<IN,OUT>
    {
        public Type ParserClass { get; set; }

        public object ParserVsisitorInstance { get; set; }

        public ParserConfiguration<IN,OUT> Configuration { get; set; }

        public SyntaxTreeVisitor(ParserConfiguration<IN,OUT> conf)
        {
            this.ParserClass = ParserClass;
            this.Configuration = conf;
            this.ParserVsisitorInstance = null;
        }

        public SyntaxTreeVisitor(ParserConfiguration<IN,OUT> conf, object parserInstance)
        {
            this.ParserClass = ParserClass;
            this.Configuration = conf;
            this.ParserVsisitorInstance = parserInstance;
        }

        public OUT VisitSyntaxTree(ISyntaxNode<IN> root)
        {
            SyntaxVisitorResult<IN, OUT> result = Visit(root);
            
        }

        private virtual SyntaxVisitorResult<IN,OUT> Visit(ISyntaxNode<IN> n)
        {
            if (n is SyntaxLeaf<IN>)
            {
                return Visit(n as SyntaxLeaf<IN>);
            }
            else if (n is SyntaxEpsilon<IN>)
            {
                return Visit(n as SyntaxEpsilon<IN>);
            }
            else if (n is SyntaxNode<IN>)
            {
                return Visit(n as SyntaxNode<IN>);
            }
            else
            {
                return null;
            }
        }

        private SyntaxVisitorResult<IN, OUT> Visit(SyntaxNode<IN> node)
        {
            SyntaxVisitorResult<IN, OUT> result = SyntaxVisitorResult<IN, OUT>.none();
            if (Configuration.Functions.ContainsKey(node.Name))
            {
                List<object> args = new List<object>();
                int i = 0;
                foreach (ISyntaxNode<IN> n in node.Children)
                {
                    object v = Visit(n);

                    args.Add(v);

                    i++;
                }
                OUT res =  (OUT)(Configuration.Functions[node.Name].Invoke(ParserVsisitorInstance, args.ToArray()));
                result = SyntaxVisitorResult<IN, OUT>.value(res);

            }
            return result;
        }

        private SyntaxVisitorResult<IN, OUT> Visit(SyntaxLeaf<IN> leaf)
        {
            return SyntaxVisitorResult<IN, OUT>.token(leaf.Token);
            //return leaf.Token;
        }
    }
}