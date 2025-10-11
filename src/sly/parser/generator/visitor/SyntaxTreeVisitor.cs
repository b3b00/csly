using System;
using System.Collections.Generic;
using System.Reflection;
using sly.parser.syntax.tree;

namespace sly.parser.generator.visitor
{
    public class SyntaxTreeVisitor<IN, OUT> where IN : struct, Enum
    {
        public SyntaxTreeVisitor(ParserConfiguration<IN, OUT> conf, object parserInstance, bool relaxed = false)
        {
            Configuration = conf;
            ParserVsisitorInstance = parserInstance;
            IsRelaxed = relaxed;
        }

        public bool IsRelaxed { get; set; } = false;

        public object ParserVsisitorInstance { get; set; }

        public ParserConfiguration<IN, OUT> Configuration { get; set; }

        public OUT VisitSyntaxTree(ISyntaxNode<IN, OUT> root, object context = null)
        {
            var result = Visit(root, context);
            return IsRelaxed ? (OUT)result.RelaxedValueResult : result.ValueResult;
        }

        protected virtual SyntaxVisitorResult<IN, OUT> Visit(ISyntaxNode<IN, OUT> n, object context = null)
        {
            switch (n)
            {
                case SyntaxLeaf<IN, OUT> leaf:
                    return Visit(leaf);
                case SyntaxNode<IN, OUT> node:
                    return Visit(node, context);
                default:
                    return null;
            }
        }

        private SyntaxVisitorResult<IN, OUT> Visit(SyntaxNode<IN, OUT> node, object context = null)
        {
            var result = SyntaxVisitorResult<IN, OUT>.NoneResult();
            if (node.LambdaVisitor != null || node.Visitor != null || node.IsByPassNode)
            {
                var args = new List<object>();
                var i = 0;
                foreach (var n in node.Children)
                {
                    var v = Visit(n,context);


                    if (v.IsToken)
                    {
                        if (!v.Discarded) args.Add(v.TokenResult);
                    }
                    else if (v.IsValue)
                    {
                        args.Add(IsRelaxed ? v.RelaxedValueResult : v.ValueResult);
                    }

                    i++;
                }

                if (node.IsByPassNode)
                {
                    var v = args[0];
                    if (v == null)
                    {
                        result = SyntaxVisitorResult<IN, OUT>.NewValue(default(OUT));    
                    }
                    else
                    {
                        result = SyntaxVisitorResult<IN, OUT>.NewValue((OUT)args[0]);
                    }
                }
                else
                {
                    MethodInfo method = null;
                    try
                    {
                        if (!(context is NoContext))
                        {
                            args.Add(context);
                        }

                        if (node.Visitor != null)
                        {
                            method = node.Visitor;
                            var t = method?.Invoke(ParserVsisitorInstance, args.ToArray());
                            if (!IsRelaxed)
                            {
                                var res = (OUT)t;
                                result = SyntaxVisitorResult<IN, OUT>.NewValue(res);
                            }
                            else
                            {
                                result = SyntaxVisitorResult<IN, OUT>.NewRelaxedValue(t);
                            }
                        }
                        else if (node.LambdaVisitor != null)
                        {
                            var visitor = node.LambdaVisitor;
                            var res = visitor(args.ToArray());
                            result = SyntaxVisitorResult<IN, OUT>.NewValue(res);
                        }
                    }
                    catch (TargetInvocationException tie)
                    {
                        if (tie.InnerException != null)
                        {
                            throw tie.InnerException;
                        }
                    }
                }
            }

            return result;
        }

        private SyntaxVisitorResult<IN, OUT> Visit(SyntaxLeaf<IN, OUT> leaf)
        {
            return SyntaxVisitorResult<IN, OUT>.NewToken(leaf.Token);
        }
    }
}