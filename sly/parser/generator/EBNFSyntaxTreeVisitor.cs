using System;
using System.Linq;
using System.Collections.Generic;
using sly.parser.syntax;
using sly.lexer;
using System.Reflection;
using sly.parser.parser;

namespace sly.parser.generator
{
    public class EBNFSyntaxTreeVisitor<IN, OUT> : SyntaxTreeVisitor<IN, OUT> where IN : struct
    {



        public EBNFSyntaxTreeVisitor(ParserConfiguration<IN, OUT> conf, object parserInstance) : base(conf, parserInstance)
        {
        }



        protected override SyntaxVisitorResult<IN, OUT> Visit(ISyntaxNode<IN> n)
        {
            if (n is SyntaxLeaf<IN>)
            {
                return Visit(n as SyntaxLeaf<IN>);
            }
            else if (n is GroupSyntaxNode<IN>)
            {
                return Visit(n as GroupSyntaxNode<IN>);
            }
            else if (n is ManySyntaxNode<IN>)
            {
                return Visit(n as ManySyntaxNode<IN>);
            }
            else if (n is OptionSyntaxNode<IN>)
            {
                return Visit(n as OptionSyntaxNode<IN>);
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

        private SyntaxVisitorResult<IN, OUT> Visit(GroupSyntaxNode<IN> node)
        {

            Group<IN, OUT> group = new Group<IN, OUT>();
            List<SyntaxVisitorResult<IN, OUT>> values = new List<SyntaxVisitorResult<IN, OUT>>();
            foreach (ISyntaxNode<IN> n in node.Children)
            {
                SyntaxVisitorResult<IN, OUT> v = Visit(n);

                if (v.IsValue)
                {
                    group.Add(n.Name, v.ValueResult);
                }
                if (v.IsToken)
                {
                    if (!v.Discarded)
                    {
                        group.Add(n.Name, v.TokenResult);
                    }
                }
            }


            var res = SyntaxVisitorResult<IN, OUT>.NewGroup(group);
            return res;
        }

        private SyntaxVisitorResult<IN, OUT> Visit(OptionSyntaxNode<IN> node)
        {
            var child = node.Children != null && node.Children.Any() ? node.Children[0] : null;
            if (child == null || node.IsEmpty)
            {
                return SyntaxVisitorResult<IN, OUT>.NewOptionNone();
            }
            else
            {
                SyntaxVisitorResult<IN, OUT> innerResult = Visit(child);
                if (child is SyntaxLeaf<IN> leaf)
                {
                    return SyntaxVisitorResult<IN, OUT>.NewToken(leaf.Token);
                }
                else if (child is GroupSyntaxNode<IN> group) {
                    return SyntaxVisitorResult<IN, OUT>.NewOptionGroupSome(innerResult.GroupResult);
                }
                else
                {
                    return SyntaxVisitorResult<IN, OUT>.NewOptionSome(innerResult.ValueResult);
                }
            }
        }


        private SyntaxVisitorResult<IN, OUT> Visit(SyntaxNode<IN> node)
        {

            SyntaxVisitorResult<IN, OUT> result = SyntaxVisitorResult<IN, OUT>.NoneResult();
            if (node.Visitor != null || node.IsByPassNode)
            {               
                List<object> args = new List<object>();

                foreach (ISyntaxNode<IN> n in node.Children)
                {
                    SyntaxVisitorResult<IN, OUT> v = Visit(n);
if (node.Name == "root__a_B") {
    ;
}

                    if (v.IsToken)
                    {
                        if (!v.Discarded)
                        {
                            args.Add(v.TokenResult);
                        }
                    }
                    else if (v.IsValue)
                    {
                        args.Add(v.ValueResult);
                    }
                    else if (v.IsOption)
                    {
                        args.Add(v.OptionResult);
                    }
                    else if (v.IsOPtionGroup)
                    {
                        args.Add(v.OptionGroupResult);
                    }
                    else if (v.IsGroup)
                    {
                        args.Add(v.GroupResult);
                    }
                    else if (v.IsTokenList)
                    {
                        args.Add(v.TokenListResult);
                    }
                    else if (v.IsValueList)
                    {
                        args.Add(v.ValueListResult);
                    }
                    else if (v.IsGroupList)
                    {
                        args.Add(v.GroupListResult);
                    }
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
                        if (method == null)
                        {
                            method = node.Visitor;
                        }
                        object t = (method.Invoke(ParserVsisitorInstance, args.ToArray()));
                        OUT res = (OUT)t;
                        result = SyntaxVisitorResult<IN, OUT>.NewValue(res);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OUTCH {e.Message} calling {node.Name} =>  {method.Name}");
                    }
                }
            }
            return result;
        }

        private SyntaxVisitorResult<IN, OUT> Visit(ManySyntaxNode<IN> node)
        {
            SyntaxVisitorResult<IN, OUT> result = null;

            List<SyntaxVisitorResult<IN, OUT>> values = new List<SyntaxVisitorResult<IN, OUT>>();
            foreach (ISyntaxNode<IN> n in node.Children)
            {
                SyntaxVisitorResult<IN, OUT> v = Visit(n);
                values.Add(v);
            }

            if (node.IsManyTokens)
            {
                List<Token<IN>> tokens = new List<Token<IN>>();
                values.ForEach(v => tokens.Add(v.TokenResult));
                result = SyntaxVisitorResult<IN, OUT>.NewTokenList(tokens);
            }
            else if (node.IsManyValues)
            {
                List<OUT> vals = new List<OUT>();
                values.ForEach(v => vals.Add(v.ValueResult));
                result = SyntaxVisitorResult<IN, OUT>.NewValueList(vals);
            }
            else if (node.IsManyGroups)
            {
                List<Group<IN,OUT>> vals = new List<Group<IN,OUT>>();
                values.ForEach(v => vals.Add(v.GroupResult));
                result = SyntaxVisitorResult<IN, OUT>.NewGroupList(vals);
            }



            return result;


        }



        private SyntaxVisitorResult<IN, OUT> Visit(SyntaxLeaf<IN> leaf)
        {
            return SyntaxVisitorResult<IN, OUT>.NewToken(leaf.Token);
        }
    }
}