using System;
using System.Collections.Generic;
using System.Reflection;
using sly.lexer;
using sly.parser.parser;
using sly.parser.syntax.tree;
using static sly.parser.parser.ValueOptionConstructors;

namespace sly.parser.generator.visitor
{
    public class SyntaxVisitorResult<IN, OUT> where IN : struct, Enum
    {
        public List<Group<IN, OUT>> GroupListResult;

        public Group<IN, OUT> GroupResult;
        
        public Group<IN, object> RelaxedGroupResult;

        public ValueOption<Group<IN, OUT>> OptionGroupResult;

        public object RelaxedOptionGroupResult;

        public ValueOption<OUT> OptionResult;

        public ValueOption<object> RelaxedOptionResult;

        public List<Token<IN>> TokenListResult;

        public Token<IN> TokenResult;

        public List<OUT> ValueListResult;
        
        public object  RelaxedValueListResult;

        public OUT ValueResult;

        public object RelaxedValueResult;

        public bool IsOption => OptionResult != null || RelaxedOptionResult != null;
        public bool IsOptionGroup => OptionGroupResult != null  || RelaxedOptionGroupResult != null;

        public bool IsToken { get; private set; }

        public bool Discarded => IsToken && TokenResult != null && TokenResult.Discarded;
        public bool IsValue { get; private set; }
        public bool IsValueList { get; private set; }

        public bool IsGroupList { get; private set; }

        public bool IsTokenList { get; private set; }

        public bool IsGroup { get; private set; }

        public static SyntaxVisitorResult<IN, OUT> NewToken(Token<IN> tok)
        {
            var res = new SyntaxVisitorResult<IN, OUT>();
            res.TokenResult = tok;
            res.IsToken = true;
            return res;
        }

        public static SyntaxVisitorResult<IN, OUT> NewValue(OUT val)
        {
            var res = new SyntaxVisitorResult<IN, OUT>();
            res.ValueResult = val;
            res.IsValue = true;
            return res;
        }
        
        public static SyntaxVisitorResult<IN, OUT> NewRelaxedValue(object val)
        {
            var res = new SyntaxVisitorResult<IN, OUT>();
            res.RelaxedValueResult = val;
            res.IsValue = true;
            return res;
        }

        public static SyntaxVisitorResult<IN, OUT> NewValueList(List<OUT> values)
        {
            var res = new SyntaxVisitorResult<IN, OUT>();
            res.ValueListResult = values;
            res.IsValueList = true;
            return res;
        }
        
        public static SyntaxVisitorResult<IN, OUT> NewRelaxedValueList(List<object> values)
        {
            var res = new SyntaxVisitorResult<IN, OUT>();
            res.RelaxedValueListResult = values;
            res.IsValueList = true;
            return res;
        }
        
       

        public static SyntaxVisitorResult<IN, OUT> NewGroupList(List<Group<IN, OUT>> values)
        {
            var res = new SyntaxVisitorResult<IN, OUT>();
            res.GroupListResult = values;
            res.IsGroupList = true;
            return res;
        }

        public static SyntaxVisitorResult<IN, OUT> NewTokenList(List<Token<IN>> tokens)
        {
            var res = new SyntaxVisitorResult<IN, OUT>();
            res.TokenListResult = tokens;
            res.IsTokenList = true;
            return res;
        }

        public static SyntaxVisitorResult<IN, OUT> NewOptionSome(OUT value)
        {
            var res = new SyntaxVisitorResult<IN, OUT>();
            res.OptionResult = Some<OUT>(value);
            return res;
        }
        
        public static SyntaxVisitorResult<IN, OUT> NewOptionSomeRelaxed(object value)
        {
            var res = new SyntaxVisitorResult<IN, OUT>();
            res.RelaxedOptionResult = Some<object>(value);
            return res;
        }
        

        public static SyntaxVisitorResult<IN, OUT> NewOptionGroupSome(Group<IN, OUT> group)
        {
            var res = new SyntaxVisitorResult<IN, OUT>();
            res.OptionGroupResult = Some<Group<IN, OUT>>(group);
            return res;
        }
        
        public static SyntaxVisitorResult<IN, OUT> NewOptionGroupNone()
        {
            var res = new SyntaxVisitorResult<IN, OUT>();
            res.OptionGroupResult = NoneGroup<IN,OUT>();
            return res;
        }


        public static SyntaxVisitorResult<IN, OUT> NewOptionNone()
        {
            var res = new SyntaxVisitorResult<IN, OUT>();
            res.OptionResult = None<OUT>();
            return res;
        }

        public static SyntaxVisitorResult<IN, OUT> NewOptionNoneRelaxed()
        {
            var res = new SyntaxVisitorResult<IN, OUT>();
            res.RelaxedOptionResult = None<object>();
            return res;
        }

        public static SyntaxVisitorResult<IN, OUT> NewGroup(Group<IN, OUT> group)
        {
            var res = new SyntaxVisitorResult<IN, OUT>();
            res.GroupResult = group;
            res.IsGroup = true;
            return res;
        }
        
        public static SyntaxVisitorResult<IN, OUT> NewRelaxedGroup(Group<IN, object> group)
        {
            var res = new SyntaxVisitorResult<IN, OUT>();
            res.RelaxedGroupResult = group;
            res.IsGroup = true;
            return res;
        }

        public static SyntaxVisitorResult<IN, OUT> NoneResult()
        {
            var res = new SyntaxVisitorResult<IN, OUT>();
            return res;
        }

    }

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