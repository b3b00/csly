using System;
using System.Collections.Generic;
using sly.parser.syntax;
using sly.lexer;
using System.Reflection;

namespace sly.parser.generator
{
    public class EBNFSyntaxTreeVisitor<IN,OUT> : SyntaxTreeVisitor<IN,OUT> where IN : struct
    {

        public EBNFSyntaxTreeVisitor(ParserConfiguration<IN,OUT> conf) : base(conf)
        {
        }

        public EBNFSyntaxTreeVisitor(ParserConfiguration<IN,OUT> conf, object parserInstance) : base(conf, parserInstance)
        {
        }



        protected override SyntaxVisitorResult<IN,OUT> Visit(ISyntaxNode<IN> n)
        {
            if (n is SyntaxLeaf<IN>)
            {
                return Visit(n as SyntaxLeaf<IN>);
            }
            else if (n is SyntaxEpsilon<IN>)
            {
                return Visit(n as SyntaxEpsilon<IN>);
            }
            else if (n is ManySyntaxNode<IN>)
            {
                return Visit(n as ManySyntaxNode<IN>);
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
            SyntaxVisitorResult < IN, OUT > result = SyntaxVisitorResult<IN, OUT>.NoneResult();
            if (Configuration.Functions.ContainsKey(node.Name) || node.Visitor != null || node.IsByPassNode)
            {
                List<object> args = new List<object>();
                int i = 0;

                if (node.Name == "statementPrim__IDENTIFIER_ASSIGN_WhileParser_expressions")
                {
                    ;
                }

                foreach (ISyntaxNode<IN> n in node.Children)
                {
                    SyntaxVisitorResult<IN, OUT> v = Visit(n);


                    if (v.IsToken)
                    {
                        args.Add(v.TokenResult);
                    }
                    else if (v.IsValue)
                    {
                        args.Add(v.ValueResult);
                    }
                    else if (v.IsTokenList)
                    {
                        args.Add(v.TokenListResult);
                    }
                    else if (v.IsValueList)
                    {
                        args.Add(v.ValueListResult);
                    }

                    i++;
                }

                if (node.IsByPassNode)
                {
                    result = SyntaxVisitorResult<IN, OUT>.NewValue((OUT)args[0]);
                }
                else
                {
                    MethodInfo method = null;
                    try
                    {
                        bool found = Configuration.Functions.TryGetValue(node.Name, out method);
                        if (method == null && !found)
                        {
                            method = node.Visitor;
                        }
                        object t = (method.Invoke(ParserVsisitorInstance, args.ToArray()));
                        //string typename = t.GetType().Name;
                        OUT res = (OUT)t;
                        result = SyntaxVisitorResult<IN, OUT>.NewValue(res);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OUTCH {e.Message} calling {node.Name} =>  {method.Name}");
                    }
                }


                //result = (OUT) (Configuration.Functions[node.Name].Invoke(ParserVsisitorInstance, args.ToArray()));
            }
            return result;
        }

        private SyntaxVisitorResult<IN, OUT> Visit(ManySyntaxNode<IN> node)
        {
            SyntaxVisitorResult<IN, OUT> result = null;
            
            List<SyntaxVisitorResult<IN, OUT>> values = new List<SyntaxVisitorResult<IN, OUT>>();
            int i = 0;
            foreach (ISyntaxNode<IN> n in node.Children)
            {
                SyntaxVisitorResult<IN, OUT> v = Visit(n);

                values.Add(v);

                i++;
            }

            if (node.IsManyTokens)
            {
                List<Token<IN>> tokens = new List<Token<IN>>();
                values.ForEach(v => tokens.Add(v.TokenResult));
                result = SyntaxVisitorResult<IN, OUT>.NewTokenList(tokens);
            }
            else
            {
                List<OUT> vals = new List<OUT>();
                values.ForEach(v => vals.Add(v.ValueResult));
                result = SyntaxVisitorResult<IN, OUT>.NewValueList(vals);
            }
                        
            


            return result;


        }



        private SyntaxVisitorResult<IN, OUT> Visit(SyntaxLeaf<IN> leaf)
        {
            return SyntaxVisitorResult<IN, OUT>.NewToken(leaf.Token);
        }
    }
}