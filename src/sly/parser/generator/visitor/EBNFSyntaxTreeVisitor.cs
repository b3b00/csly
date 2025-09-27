using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sly.lexer;
using sly.parser.parser;
using sly.parser.syntax.tree;
using System;

namespace sly.parser.generator.visitor
{
    public class EBNFSyntaxTreeVisitor<IN, OUT> : SyntaxTreeVisitor<IN, OUT> where IN : struct, Enum
    {
        public EBNFSyntaxTreeVisitor(ParserConfiguration<IN, OUT> conf, object parserInstance, bool relaxed = false) : base(conf,
            parserInstance, relaxed)
        {
        }


        protected override SyntaxVisitorResult<IN, OUT> Visit(ISyntaxNode<IN, OUT> n, object context = null)
        {
            switch (n)
            {
                case SyntaxLeaf<IN, OUT> leaf:
                    return Visit(leaf);
                case GroupSyntaxNode<IN, OUT> node:
                    return Visit(node, context);
                case ManySyntaxNode<IN, OUT> node:
                    return Visit(node, context);
                case OptionSyntaxNode<IN, OUT> node:
                    return Visit(node, context);
                case SyntaxNode<IN, OUT> node:
                    return Visit(node, context);
                default:
                    return null;
            }
        }

        private SyntaxVisitorResult<IN, OUT> Visit(GroupSyntaxNode<IN, OUT> node, object context = null)
        {

            if (IsRelaxed)
            {
                var group = new Group<IN, object>();
                var values = new List<SyntaxVisitorResult<IN, OUT>>();
                foreach (var n in node.Children)
                {
                    var v = Visit(n, context);

                    if (v.IsValue) group.Add(n.Name, v.RelaxedValueResult);
                    if (v.IsToken && !v.Discarded)
                    {
                        group.Add(n.Name, v.TokenResult);
                    }
                }

                var res = SyntaxVisitorResult<IN, OUT>.NewRelaxedGroup(group);
                    
                return res;
            }
            else
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
        }

        private SyntaxVisitorResult<IN, OUT> Visit(OptionSyntaxNode<IN, OUT> node, object context = null)
        {
            var child = node.Children != null && node.Children.Any<ISyntaxNode<IN, OUT>>() ? node.Children[0] : null;
            if (child == null || node.IsEmpty)
            {
                if (node.IsGroupOption)
                {
                 return SyntaxVisitorResult<IN, OUT>.NewOptionGroupNone();   
                }
                else
                {
                    return IsRelaxed ? SyntaxVisitorResult<IN, OUT>.NewOptionNoneRelaxed(): SyntaxVisitorResult<IN, OUT>.NewOptionNone();
                }
            }

            var innerResult = Visit(child, context);
            switch (child)
            {
                case SyntaxLeaf<IN, OUT> leaf:
                    return SyntaxVisitorResult<IN, OUT>.NewToken(leaf.Token);
                case GroupSyntaxNode<IN, OUT> group:
                    return SyntaxVisitorResult<IN, OUT>.NewOptionGroupSome(innerResult.GroupResult);
                default:
                {
                    if (IsRelaxed)
                    {
                        return SyntaxVisitorResult<IN, OUT>.NewOptionSomeRelaxed(innerResult.RelaxedValueResult);
                    }
                    return SyntaxVisitorResult<IN, OUT>.NewOptionSome(innerResult.ValueResult);
                }
            }
        }


        private SyntaxVisitorResult<IN, OUT> Visit(SyntaxNode<IN, OUT> node, object context = null)
        {
            var result = SyntaxVisitorResult<IN, OUT>.NoneResult();
            if (!node .IsByPassNode && (node.LambdaVisitor != null || node.Visitor != null ))
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
                        parameters[parametersCount] = IsRelaxed ? v.RelaxedValueResult : v.ValueResult;
                        parametersCount++;
                    }
                    else if (v.IsOption)
                    {
                        parameters[parametersCount] = IsRelaxed ? v.RelaxedOptionResult : v.OptionResult;
                        parametersCount++;
                    }
                    else if (v.IsOptionGroup)
                    {
                        parameters[parametersCount] = v.OptionGroupResult;
                        parametersCount++;
                    }
                    else if (v.IsGroup)
                    {
                        parameters[parametersCount] = IsRelaxed ? v.RelaxedGroupResult : v.GroupResult;
                        parametersCount++;
                    }
                    else if (v.IsTokenList)
                    {
                        parameters[parametersCount] = v.TokenListResult;
                        parametersCount++;
                    }
                    else if (v.IsValueList)
                    {
                        parameters[parametersCount] = IsRelaxed ? v.RelaxedValueListResult : v.ValueListResult;
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

                        if (node.Visitor != null)
                        {
                            method = node.Visitor;
                            Array.Resize(ref parameters, parametersCount);
                            if (IsRelaxed)
                            {
                                parameters = RecastParameters(parameters, method);
                            }
                            
                            var t = method.Invoke(ParserVsisitorInstance, parameters);
                            if (IsRelaxed)
                            {
                                result = SyntaxVisitorResult<IN, OUT>.NewRelaxedValue(t);
                            }
                            else
                            {
                                result = IsRelaxed ? SyntaxVisitorResult<IN, OUT>.NewRelaxedValue(t) : SyntaxVisitorResult<IN, OUT>.NewValue((OUT)t) ;
                            }
                        }
                        if (node.LambdaVisitor != null)
                        {
                            var t = node.LambdaVisitor(parameters.ToArray());
                            result = SyntaxVisitorResult<IN,OUT>.NewValue(t);
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
                var v = Visit(child, context);
                return v;
            }

            return result;
        }

        private SyntaxVisitorResult<IN, OUT> Visit(ManySyntaxNode<IN, OUT> node, object context = null)
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
                
                if (!IsRelaxed)
                {
                    var vals = new List<OUT>();
                    values.ForEach(v => vals.Add(v.ValueResult));
                    result = SyntaxVisitorResult<IN, OUT>.NewValueList(vals);
                }
                else
                {
                    var vals = new List<object>();
                    values.ForEach(v => vals.Add(v.RelaxedValueResult));
                    result = SyntaxVisitorResult<IN, OUT>.NewRelaxedValueList(vals);
                }
            }
            else if (node.IsManyGroups)
            {
                var vals = new List<Group<IN, OUT>>();
                values.ForEach(v => vals.Add(v.GroupResult));
                result = SyntaxVisitorResult<IN, OUT>.NewGroupList(vals);
            }


            return result;
        }


        private SyntaxVisitorResult<IN, OUT> Visit(SyntaxLeaf<IN, OUT> leaf)
        {
            return SyntaxVisitorResult<IN, OUT>.NewToken(leaf.Token);
        }

        private object Recast(object value, Type type)
        {
            if (value == null)
            {
                ;
            }
            if (value.GetType() == type)
            {
                return value;
            }
            if (value is List<object> valueList)
            {
                var elementType = type.GetGenericArguments()[0];
                var castMethod = typeof(Enumerable).GetMethod("Cast")!.MakeGenericMethod(elementType);
                var toListMethod = typeof(Enumerable).GetMethod("ToList")!.MakeGenericMethod(elementType);

                var casted = castMethod.Invoke(null, new object[] { valueList });
                return toListMethod.Invoke(null, new object[] { casted });
            }

            if (value is ValueOption<object> optionValue)
            {
                var elementType = type.GetGenericArguments()[0];
                var valueOptionType = typeof(ValueOption<>).MakeGenericType(elementType);
                var option = optionValue.Match((x) =>
                {
                    var casted = System.Convert.ChangeType(x, elementType);
                    var instance = Activator.CreateInstance(valueOptionType,casted);
                    return instance;
                }, () =>
                {
                    var instance = Activator.CreateInstance(valueOptionType);
                    return instance;
                });
                return option;

            }

            if (value is Group<IN, object> groupValue)
            {
                var elementType = type.GetGenericArguments()[1];
                var valueGroupType = typeof(Group<,>).MakeGenericType(typeof(IN),elementType);
                var valueGroupItemType = typeof(GroupItem<,>).MakeGenericType(typeof(IN),elementType);
                var instance = Activator.CreateInstance(valueGroupType);
                // TODO : get the correct add method
                var addMethod = instance.GetType().GetMethod("Add", new Type[] {valueGroupItemType });
                foreach (var item in groupValue.Items)
                {
                    var casted = Recast(item, elementType);
                    addMethod.Invoke(instance, new[] {casted});
                }

                return instance;
            }

            if (value is GroupItem<IN, object> groupItemValue)
            {
                
                var valueGroupItemType = typeof(GroupItem<,>).MakeGenericType(typeof(IN), type);

                // Supposons que le constructeur prend (string name, IN key, elementType value)
                var tokenCtor = valueGroupItemType.GetConstructor(new[] { typeof(string), typeof(Token<IN>)});
                var valueCtor = valueGroupItemType.GetConstructor(new[] { typeof(string), type });
                if (groupItemValue.IsToken)
                {
                    if (tokenCtor != null)
                    {
                        var instance =
                            tokenCtor.Invoke([groupItemValue.Name, groupItemValue.Token]);
                        return instance;
                    }
                }
                else if (groupItemValue.IsValue) {
                    if (valueCtor != null)
                    {
                        var castValue = Recast(groupItemValue.Value, type);
                        var instance = valueCtor.Invoke([groupItemValue.Name, castValue]);
                        return instance;
                    }
                }

            }
            return value;
        }

        private object[] RecastParameters(object[] parameters, MethodInfo method)
        {
            List<object> retypedArgs = new List<object>();
            var types = method.GetParameters().Select(x =>  x.ParameterType).ToList();
            for (int i = 0; i < parameters.Length; i++)
            {
                var retyped = Recast(parameters[i], types[i]);
                retypedArgs.Add(retyped);
            }
            
            return retypedArgs.ToArray();
        }
    }    
}