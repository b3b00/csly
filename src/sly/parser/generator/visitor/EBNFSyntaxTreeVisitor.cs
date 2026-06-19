using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sly.lexer;
using sly.parser.parser;
using sly.parser.syntax.tree;
using System;

namespace sly.parser.generator.visitor
{
    // A lightweight state enum to keep track of our position in the tree traversal
    internal enum VisitorStep
    {
        Pending,   // Node encountered for the first time, needs children queued
        Evaluate   // Children have been processed, parent can now execute its visitor logic
    }

    // A simple structural container to act as our manual stack frame
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

        // The new, completely flat, stack-safe entry point
        protected override SyntaxVisitorResult<IN, OUT> Visit(ISyntaxNode<IN, OUT> root, object context = null)
        {
            if (root == null) return null;

            // Tracks the final evaluations of completed subtrees
            var results = new Dictionary<ISyntaxNode<IN, OUT>, SyntaxVisitorResult<IN, OUT>>();
            
            // Our heap-allocated call stack. Can grow to gigabytes, completely bypassing the 1MB thread limit!
            var stack = new Stack<VisitorFrame<IN, OUT>>();
            
            stack.Push(new VisitorFrame<IN, OUT>(root, VisitorStep.Pending));

            while (stack.Count > 0)
            {
                var frame = stack.Pop();
                var node = frame.Node;

                if (frame.Step == VisitorStep.Pending)
                {
                    // Leaves have no children, we can evaluate them instantly
                    if (node is SyntaxLeaf<IN, OUT> leaf)
                    {
                        results[node] = VisitLeaf(leaf);
                        continue;
                    }

                    // For container nodes, push them back as "Evaluate", then push children as "Pending"
                    stack.Push(new VisitorFrame<IN, OUT>(node, VisitorStep.Evaluate));

                    // Push children in REVERSE order so they are popped and processed in original left-to-right order
                    if (node.Children != null)
                    {
                        for (int i = node.Children.Count - 1; i >= 0; i--)
                        {
                            if (node.Children[i] != null)
                            {
                                stack.Push(new VisitorFrame<IN, OUT>(node.Children[i], VisitorStep.Pending));
                            }
                        }
                    }
                }
                else // VisitorStep.Evaluate
                {
                    // All children are guaranteed to be evaluated and waiting in the results dictionary!
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

        private SyntaxVisitorResult<IN, OUT> VisitLeaf(SyntaxLeaf<IN, OUT> leaf)
        {
            return SyntaxVisitorResult<IN, OUT>.NewToken(leaf.Token);
        }

        private SyntaxVisitorResult<IN, OUT> EvaluateGroup(GroupSyntaxNode<IN, OUT> node, Dictionary<ISyntaxNode<IN, OUT>, SyntaxVisitorResult<IN, OUT>> results, object context)
        {
            var group = new Group<IN, OUT>();
            foreach (var n in node.Children)
            {
                var v = results[n];
                if (v.IsValue) group.Add(n.Name, v.ValueResult);
                if (v.IsToken && !v.Discarded)
                {
                    group.Add(n.Name, v.TokenResult);
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
            foreach (var n in node.Children)
            {
                values.Add(results[n]);
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
                int parametersArrayLength = node.Children.Count + (context is NoContext ? 0 : 1);
                var parameters = new object[parametersArrayLength];
                int parametersCount = 0;

                foreach (var n in node.Children)
                {
                    var v = results[n];
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
                    try
                    {
                        if (!(context is NoContext))
                        {
                            parameters[parametersCount] = context;
                            parametersCount++;
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
                            // If you want to further optimize allocations later, you can replace parameters.ToArray()
                            // with an ArrayPool lease, since the array reference stays local to this block!
                            var t = node.LambdaVisitor(parameters.ToArray());
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
