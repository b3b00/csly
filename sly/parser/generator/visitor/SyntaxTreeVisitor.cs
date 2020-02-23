using System;
using System.Collections.Generic;
using System.Reflection;
using sly.lexer;
using sly.parser.parser;
using sly.parser.syntax.tree;
using static sly.parser.parser.ValueOptionConstructors;

namespace sly.parser.generator.visitor
{
    public class SyntaxVisitorResult<IN, OUT> where IN : struct
    {
        public List<Group<IN, OUT>> GroupListResult;

        public Group<IN, OUT> GroupResult;

        public ValueOption<Group<IN, OUT>> OptionGroupResult;

        public ValueOption<OUT> OptionResult;

        public List<Token<IN>> TokenListResult;

        public Token<IN> TokenResult;

        public List<OUT> ValueListResult;

        public OUT ValueResult;

        public bool IsOption => OptionResult != null;
        public bool IsOptionGroup => OptionGroupResult != null;

        public bool IsToken { get; private set; }

        public bool Discarded => IsToken && TokenResult != null && TokenResult.Discarded;
        public bool IsValue { get; private set; }
        public bool IsValueList { get; private set; }

        public bool IsGroupList { get; private set; }

        public bool IsTokenList { get; private set; }

        public bool IsGroup { get; private set; }

        public bool IsNone => !IsToken && !IsValue && !IsTokenList && !IsValueList && !IsGroup && !IsGroupList;

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

        public static SyntaxVisitorResult<IN, OUT> NewValueList(List<OUT> values)
        {
            var res = new SyntaxVisitorResult<IN, OUT>();
            res.ValueListResult = values;
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
            res.OptionResult = Some(value);
            return res;
        }

        public static SyntaxVisitorResult<IN, OUT> NewOptionGroupSome(Group<IN, OUT> group)
        {
            var res = new SyntaxVisitorResult<IN, OUT>();
            res.OptionGroupResult = Some(group);
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

        public static SyntaxVisitorResult<IN, OUT> NewGroup(Group<IN, OUT> group)
        {
            var res = new SyntaxVisitorResult<IN, OUT>();
            res.GroupResult = group;
            res.IsGroup = true;
            return res;
        }

        public static SyntaxVisitorResult<IN, OUT> NoneResult()
        {
            var res = new SyntaxVisitorResult<IN, OUT>();
            return res;
        }
    }

    public class SyntaxTreeVisitor<IN, OUT> where IN : struct
    {
        public SyntaxTreeVisitor(ParserConfiguration<IN, OUT> conf, object parserInstance)
        {
            ParserClass = ParserClass;
            Configuration = conf;
            ParserVsisitorInstance = parserInstance;
        }

        public Type ParserClass { get; set; }

        public object ParserVsisitorInstance { get; set; }

        public ParserConfiguration<IN, OUT> Configuration { get; set; }

        public OUT VisitSyntaxTree(ISyntaxNode<IN> root, object context = null)
        {
            var result = Visit(root, context);
            return result.ValueResult;
        }

        protected virtual SyntaxVisitorResult<IN, OUT> Visit(ISyntaxNode<IN> n, object context = null)
        {
            if (n is SyntaxLeaf<IN>)
                return Visit(n as SyntaxLeaf<IN>);
            if (n is SyntaxNode<IN>)
                return Visit(n as SyntaxNode<IN>, context);
            return null;
        }

        private SyntaxVisitorResult<IN, OUT> Visit(SyntaxNode<IN> node, object context = null)
        {
            var result = SyntaxVisitorResult<IN, OUT>.NoneResult();
            if (node.Visitor != null || node.IsByPassNode)
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
                        args.Add(v.ValueResult);
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
                        method = node.Visitor;
                        var t = method?.Invoke(ParserVsisitorInstance, args.ToArray());
                        var res = (OUT) t;
                        result = SyntaxVisitorResult<IN, OUT>.NewValue(res);
                    }
                    catch (Exception e)
                    {  
                        throw new ParserConfigurationException($"ERROR : calling visitor method {method?.Name} with   {node.Name}");                     
                    }
                }
            }

            return result;
        }

        private SyntaxVisitorResult<IN, OUT> Visit(SyntaxLeaf<IN> leaf)
        {
            return SyntaxVisitorResult<IN, OUT>.NewToken(leaf.Token);
        }
    }
}