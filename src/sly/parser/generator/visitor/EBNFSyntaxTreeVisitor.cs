using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sly.lexer;
using sly.parser.parser;
using sly.parser.syntax.tree;
using System;

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
            switch (n)
            {
                case SyntaxLeaf<IN> leaf:
                    return Visit(leaf);
                case GroupSyntaxNode<IN> node:
                    return Visit(node, context);
                case ManySyntaxNode<IN> node:
                    return Visit(node, context);
                case OptionSyntaxNode<IN> node:
                    return Visit(node, context);
                case SyntaxNode<IN> node:
                    return Visit(node, context);
                default:
                    return null;
            }
        }

        private SyntaxVisitorResult<IN, OUT> Visit(GroupSyntaxNode<IN> node, object context = null)
        {
            var group = new Group<IN, OUT>();
            var values = new List<SyntaxVisitorResult<IN, OUT>>();
            foreach (var n in node.Children)
            {
                var v = Visit(n, context);

                if (v.IsValue) group.Add(n.Name, v.ValueResult);
                if (v.IsToken && !v.Discarded)
                {
                    group.Add(n.Name, v.TokenResult);
                }
            }


            var res = SyntaxVisitorResult<IN, OUT>.NewGroup(group);
            return res;
        }

        private SyntaxVisitorResult<IN, OUT> Visit(OptionSyntaxNode<IN> node, object context = null)
        {
            var child = node.Children != null && node.Children.Any<ISyntaxNode<IN>>() ? node.Children[0] : null;
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
            switch (child)
            {
                case SyntaxLeaf<IN> leaf:
                    return SyntaxVisitorResult<IN, OUT>.NewToken(leaf.Token);
                case GroupSyntaxNode<IN> group:
                    return SyntaxVisitorResult<IN, OUT>.NewOptionGroupSome(innerResult.GroupResult);
                default:
                    return SyntaxVisitorResult<IN, OUT>.NewOptionSome(innerResult.ValueResult);
            }
        }


        private SyntaxVisitorResult<IN, OUT> Visit(SyntaxNode<IN> node, object context = null)
        {
            var result = SyntaxVisitorResult<IN, OUT>.NoneResult();
            if (node.Visitor != null || node.IsByPassNode)
            {
                int parametersArrayLength = node.Children.Count + (context is NoContext ? 0 : 1); 
                var parameters = new object[parametersArrayLength];
                
                int parametersCount = 0;

                foreach (var n in node.Children)
                {
                    var v = Visit(n, context);
                    if (v.IsToken && !n.Discarded)
                    {
                        parameters[parametersCount] = v.TokenResult;
                        parametersCount++;
                    }
                    else if (v.IsValue)
                    {
                        parameters[parametersCount] = v.ValueResult;
                        parametersCount++;
                    }
                    else if (v.IsOption)
                    {
                        parameters[parametersCount] = v.OptionResult;
                        parametersCount++;
                    }
                    else if (v.IsOptionGroup)
                    {
                        parameters[parametersCount] = v.OptionGroupResult;
                        parametersCount++;
                    }
                    else if (v.IsGroup)
                    {
                        parameters[parametersCount] = v.GroupResult;
                        parametersCount++;
                    }
                    else if (v.IsTokenList)
                    {
                        parameters[parametersCount] = v.TokenListResult;
                        parametersCount++;
                    }
                    else if (v.IsValueList)
                    {
                        parameters[parametersCount] = v.ValueListResult;
                        parametersCount++;
                    }
                    else if (v.IsGroupList)
                    {
                        parameters[parametersCount] = v.GroupListResult;
                        parametersCount++;
                    }
                }

                if (node.IsByPassNode)
                {
                    result = SyntaxVisitorResult<IN, OUT>.NewValue((OUT)parameters[0]);
                }
                else
                {
                    MethodInfo method = null;
                    try
                    {
                        if (!(context is NoContext))
                        {
                            parameters[parametersCount] = context;
                            parametersCount++;
                        }

                        method = node.Visitor;
                        Array.Resize(ref parameters, parametersCount);
                        var t = method.Invoke(ParserVsisitorInstance, parameters);
                        var res = (OUT) t;
                        result = SyntaxVisitorResult<IN, OUT>.NewValue(res);
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