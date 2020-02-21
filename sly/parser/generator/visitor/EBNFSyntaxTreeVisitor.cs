using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sly.lexer;
using sly.parser.parser;
using sly.parser.syntax.tree;

namespace sly.parser.generator.visitor
{
    public class EBNFSyntaxTreeVisitor<IN, OUT> : SyntaxTreeVisitor<IN, OUT> where IN : struct
    {
        public EBNFSyntaxTreeVisitor(ParserConfiguration<IN, OUT> conf, object parserInstance) : base(conf,
            parserInstance)
        {
        }


        protected override SyntaxVisitorResult<IN, OUT> Visit(ISyntaxNode<IN> n, object context = null)
        {
            if (n is SyntaxLeaf<IN>)
                return Visit(n as SyntaxLeaf<IN>);
            if (n is GroupSyntaxNode<IN>)
                return Visit(n as GroupSyntaxNode<IN>, context);
            if (n is ManySyntaxNode<IN>)
                return Visit(n as ManySyntaxNode<IN>, context);
            if (n is OptionSyntaxNode<IN>)
                return Visit(n as OptionSyntaxNode<IN>, context);
            if (n is SyntaxNode<IN>)
                return Visit(n as SyntaxNode<IN>, context);

            return null;
        }

        private SyntaxVisitorResult<IN, OUT> Visit(GroupSyntaxNode<IN> node, object context = null)
        {
            var group = new Group<IN, OUT>();
            var values = new List<SyntaxVisitorResult<IN, OUT>>();
            foreach (var n in node.Children)
            {
                var v = Visit(n, context);

                if (v.IsValue) group.Add(n.Name, v.ValueResult);
                if (v.IsToken)
                    if (!v.Discarded)
                        group.Add(n.Name, v.TokenResult);
            }


            var res = SyntaxVisitorResult<IN, OUT>.NewGroup(group);
            return res;
        }

        private SyntaxVisitorResult<IN, OUT> Visit(OptionSyntaxNode<IN> node, object context = null)
        {
            var child = node.Children != null && node.Children.Any() ? node.Children[0] : null;
            if (child == null || node.IsEmpty)
            {
                if (node.IsGroupOption)
                {
                 return SyntaxVisitorResult<IN, OUT>.NewOptionGroupNone();   
                }
                else
                {
                    return SyntaxVisitorResult<IN, OUT>.NewOptionNone();
                }
            }

            var innerResult = Visit(child, context);
            if (child is SyntaxLeaf<IN> leaf)
                return SyntaxVisitorResult<IN, OUT>.NewToken(leaf.Token);
            if (child is GroupSyntaxNode<IN> group)
                return SyntaxVisitorResult<IN, OUT>.NewOptionGroupSome(innerResult.GroupResult);
            return SyntaxVisitorResult<IN, OUT>.NewOptionSome(innerResult.ValueResult);
        }


        private SyntaxVisitorResult<IN, OUT> Visit(SyntaxNode<IN> node, object context = null)
        {
            var result = SyntaxVisitorResult<IN, OUT>.NoneResult();
            if (node.Visitor != null || node.IsByPassNode)
            {
                
                
                var args = new List<object>();

                foreach (var n in node.Children)
                {
                    var v = Visit(n, context);
                    
                    if (v.IsToken)
                    {
                        if (!n.Discarded) args.Add(v.TokenResult);
                    }
                    else if (v.IsValue)
                    {
                        args.Add(v.ValueResult);
                    }
                    else if (v.IsOption)
                    {
                        args.Add(v.OptionResult);
                    }
                    else if (v.IsOptionGroup)
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
                    result = SyntaxVisitorResult<IN, OUT>.NewValue((OUT) args[0]);
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

                        if (method == null) method = node.Visitor;
                        var t = method.Invoke(ParserVsisitorInstance, args.ToArray());
                        var res = (OUT) t;
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

        private SyntaxVisitorResult<IN, OUT> Visit(ManySyntaxNode<IN> node, object context = null)
        {
            SyntaxVisitorResult<IN, OUT> result = null;

            var values = new List<SyntaxVisitorResult<IN, OUT>>();
            foreach (var n in node.Children)
            {
                var v = Visit(n, context);
                values.Add(v);
            }

            if (node.IsManyTokens)
            {
                var tokens = new List<Token<IN>>();
                values.ForEach(v => tokens.Add(v.TokenResult));
                result = SyntaxVisitorResult<IN, OUT>.NewTokenList(tokens);
            }
            else if (node.IsManyValues)
            {
                var vals = new List<OUT>();
                values.ForEach(v => vals.Add(v.ValueResult));
                result = SyntaxVisitorResult<IN, OUT>.NewValueList(vals);
            }
            else if (node.IsManyGroups)
            {
                var vals = new List<Group<IN, OUT>>();
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