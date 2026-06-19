using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sly.lexer;
using sly.parser.parser;
using sly.parser.syntax.tree;
using System;

namespace sly.parser.generator.visitor
{
    internal enum VisitorStep
    {
        Pending,
        Evaluate
    }

    internal struct VisitorFrame<IN, OUT> where IN : struct, Enum
    {
        public ISyntaxNode<IN, OUT> Node { get; }
        public VisitorStep Step { get; }

        public VisitorFrame(ISyntaxNode<IN, OUT> node, VisitorStep step)
        {
            Node = node;
            Step = step;
        }
    }

    public class EBNFSyntaxTreeVisitor<IN, OUT> : SyntaxTreeVisitor<IN, OUT> where IN : struct, Enum
    {
        public EBNFSyntaxTreeVisitor(ParserConfiguration<IN, OUT> conf, object parserInstance) 
            : base(conf, parserInstance)
        {
        }

        protected override SyntaxVisitorResult<IN, OUT> Visit(ISyntaxNode<IN, OUT> root, object context = null)
        {
            if (root == null) return null;

            var results = new Dictionary<ISyntaxNode<IN, OUT>, SyntaxVisitorResult<IN, OUT>>();
            var stack = new Stack<VisitorFrame<IN, OUT>>();
            
            stack.Push(new VisitorFrame<IN, OUT>(root, VisitorStep.Pending));

            while (stack.Count > 0)
            {
                var frame = stack.Pop();
                var node = frame.Node;

                if (frame.Step == VisitorStep.Pending)
                {
                    if (node is SyntaxLeaf<IN, OUT> leaf)
                    {
                        results[node] = VisitLeaf(leaf);
                        continue;
                    }

                    stack.Push(new VisitorFrame<IN, OUT>(node, VisitorStep.Evaluate));

                    // SAFE CAST FIX: Dynamically retrieve children depending on the node's true concrete type
                    var children = GetChildrenSafely(node);
                    if (children != null)
                    {
                        for (int i = children.Count - 1; i >= 0; i--)
                        {
                            if (children[i] != null)
                            {
                                stack.Push(new VisitorFrame<IN, OUT>(children[i], VisitorStep.Pending));
                            }
                        }
                    }
                }
                else // VisitorStep.Evaluate
                {
                    switch (node)
                    {
                        case GroupSyntaxNode<IN, OUT> groupNode:
                            results[node] = EvaluateGroup(groupNode, results, context);
                            break;
                        case ManySyntaxNode<IN, OUT> manyNode:
                            results[node] = EvaluateMany(manyNode, results, context);
                            break;
                        case OptionSyntaxNode<IN, OUT> optionNode:
                            results[node] = EvaluateOption(optionNode, results, context);
                            break;
                        case SyntaxNode<IN, OUT> syntaxNode:
                            results[node] = EvaluateSyntaxNode(syntaxNode, results, context);
                            break;
                    }
                }
            }

            return results[root];
        }

        // Helper method to safely pull the Children list out of any matching concrete variant
        private List<ISyntaxNode<IN, OUT>> GetChildrenSafely(ISyntaxNode<IN, OUT> node)
        {
            return node switch
            {
                SyntaxNode<IN, OUT> sn => sn.Children,
                GroupSyntaxNode<IN, OUT> gn => gn.Children,
                ManySyntaxNode<IN, OUT> mn => mn.Children,
                OptionSyntaxNode<IN, OUT> on => on.Children,
                _ => null
            };
        }

        private SyntaxVisitorResult<IN, OUT> VisitLeaf(SyntaxLeaf<IN, OUT> leaf)
        {
            return SyntaxVisitorResult<IN, OUT>.NewToken(leaf.Token);
        }

        private SyntaxVisitorResult<IN, OUT> EvaluateGroup(GroupSyntaxNode<IN, OUT> node, Dictionary<ISyntaxNode<IN, OUT>, SyntaxVisitorResult<IN, OUT>> results, object context)
        {
            var group = new Group<IN, OUT>();
            if (node.Children != null)
            {
                foreach (var n in node.Children)
                {
                    var v = results[n];
                    if (v.IsValue) group.Add(n.Name, v.ValueResult);
                    if (v.IsToken && !v.Discarded)
                    {
                        group.Add(n.Name, v.TokenResult);
                    }
                }
            }
            return SyntaxVisitorResult<IN, OUT>.NewGroup(group);
        }

        private SyntaxVisitorResult<IN, OUT> EvaluateOption(OptionSyntaxNode<IN, OUT> node, Dictionary<ISyntaxNode<IN, OUT>, SyntaxVisitorResult<IN, OUT>> results, object context)
        {
            var child = node.Children != null && node.Children.Any() ? node.Children[0] : null;
            if (child == null || node.IsEmpty)
            {
                return node.IsGroupOption 
                    ? SyntaxVisitorResult<IN, OUT>.NewOptionGroupNone() 
                    : SyntaxVisitorResult<IN, OUT>.NewOptionNone();
            }

            var innerResult = results[child];
            switch (child)
            {
                case SyntaxLeaf<IN, OUT> leaf:
                    return SyntaxVisitorResult<IN, OUT>.NewToken(leaf.Token);
                case GroupSyntaxNode<IN, OUT> _:
                    return SyntaxVisitorResult<IN, OUT>.NewOptionGroupSome(innerResult.GroupResult);
                default:
                    return SyntaxVisitorResult<IN, OUT>.NewOptionSome(innerResult.ValueResult);
            }
        }

        private SyntaxVisitorResult<IN, OUT> EvaluateMany(ManySyntaxNode<IN, OUT> node, Dictionary<ISyntaxNode<IN, OUT>, SyntaxVisitorResult<IN, OUT>> results, object context)
        {
            var values = new List<SyntaxVisitorResult<IN, OUT>>();
            if (node.Children != null)
            {
                foreach (var n in node.Children)
                {
                    values.Add(results[n]);
                }
            }

            if (node.IsManyTokens)
            {
                var tokens = values.Select(v => v.TokenResult).ToList();
                return SyntaxVisitorResult<IN, OUT>.NewTokenList(tokens);
            }
            if (node.IsManyValues)
            {
                var vals = values.Select(v => v.ValueResult).ToList();
                return SyntaxVisitorResult<IN, OUT>.NewValueList(vals);
            }
            if (node.IsManyGroups)
            {
                var vals = values.Select(v => v.GroupResult).ToList();
                return SyntaxVisitorResult<IN, OUT>.NewGroupList(vals);
            }

            return null;
        }

        private SyntaxVisitorResult<IN, OUT> EvaluateSyntaxNode(SyntaxNode<IN, OUT> node, Dictionary<ISyntaxNode<IN, OUT>, SyntaxVisitorResult<IN, OUT>> results, object context)
        {
            var result = SyntaxVisitorResult<IN, OUT>.NoneResult();

            if (!node.IsByPassNode && (node.LambdaVisitor != null || node.Visitor != null))
            {
                int childrenCount = node.Children?.Count ?? 0;
                int parametersArrayLength = childrenCount + (context is NoContext ? 0 : 1);
                var parameters = new object[parametersArrayLength];
                int parametersCount = 0;

                if (node.Children != null)
                {
                    foreach (var n in node.Children)
                    {
                        var v = results[n];
                        if (v.IsToken && !n.Discarded)
                        {
                            parameters[parametersCount++] = v.TokenResult;
                        }
                        else if (v.IsValue)
                        {
                            parameters[parametersCount++] = v.ValueResult;
                        }
                        else if (v.IsOption)
                        {
                            parameters[parametersCount++] = v.OptionResult;
                        }
                        else if (v.IsOptionGroup)
                        {
                            parameters[parametersCount++] = v.OptionGroupResult;
                        }
                        else if (v.IsGroup)
                        {
                            parameters[parametersCount++] = v.GroupResult;
                        }
                        else if (v.IsTokenList)
                        {
                            parameters[parametersCount++] = v.TokenListResult;
                        }
                        else if (v.IsValueList)
                        {
                            parameters[parametersCount++] = v.ValueListResult;
                        }
                        else if (v.IsGroupList)
                        {
                            parameters[parametersCount++] = v.GroupListResult;
                        }
                    }
                }

                if (node.IsByPassNode)
                {
                    result = SyntaxVisitorResult<IN, OUT>.NewValue((OUT)parameters[0]);
                }
                else
                {
                    try
                    {
                        if (!(context is NoContext))
                        {
                            parameters[parametersCount++] = context;
                        }

                        if (node.Visitor != null)
                        {
                            var method = node.Visitor;
                            Array.Resize(ref parameters, parametersCount);
                            var t = method.Invoke(ParserVsisitorInstance, parameters);
                            result = SyntaxVisitorResult<IN, OUT>.NewValue((OUT)t);
                        }
                        if (node.LambdaVisitor != null)
                        {
                            var exactParams = new object[parametersCount];
                            Array.Copy(parameters, exactParams, parametersCount);
                            var t = node.LambdaVisitor(exactParams);
                            result = SyntaxVisitorResult<IN, OUT>.NewValue(t);
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
            else if (node.IsByPassNode)
            {
                var child = node.Children[0];
                return results[child];
            }

            return result;
        }
    }
}
