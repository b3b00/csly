using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using sly;
using sly.buildresult;
using sly.lexer;
using sly.lexer.fluent;
using sly.lexer.fsm;
using sly.parser.syntax.grammar;

namespace sly.parser.generator;

internal class FluentParserBuilderFromIntrospection<IN, OUT> : EBNFParserBuilder<IN, OUT> where IN : struct, Enum
{
    public override BuildResult<Parser<IN, OUT>> BuildParser(object parserInstance, ParserType parserType,
        string rootRule = null, Action<IN, LexemeAttribute, GenericLexer<IN>> extensionBuilder = null,
        LexerPostProcess<IN> lexerPostProcess = null)
    {
        IFluentLexerBuilder<IN> lexerBuilder = FluentLexerBuilderFromIntrospection.BuildFluentLexerBuilder<IN>();
        
        if (string.IsNullOrEmpty(rootRule))
        {
            var rootAttribute = parserInstance.GetType().GetCustomAttribute<ParserRootAttribute>();
            if (rootAttribute != null)
            {
                rootRule = rootAttribute.RootRule;
            }
        }
        
        var builder = FluentEBNFParserBuilder<IN,OUT>.NewBuilder(parserInstance,rootRule, I18N);
        
        bool useMemoization = parserInstance.GetType().GetCustomAttribute<UseMemoizationAttribute>() != null;

        builder.UseMemoization(useMemoization);
            
        bool broadenTokenWindow = parserInstance.GetType().GetCustomAttribute<BroadenTokenWindowAttribute>() != null;
        builder.UseBroadenTokenWindow(broadenTokenWindow);

        bool autoCloseIndentations = parserInstance.GetType().GetCustomAttribute<AutoCloseIndentationsAttribute>() != null;
        builder.UseAutoCloseIndentations(autoCloseIndentations);
        
        var ruleparser = new RuleParser<IN, OUT>();
        var ruleBuilder = new ParserBuilder<EbnfTokenGeneric, GrammarNode<IN, OUT>>(I18N);

        var grammarParser = ruleBuilder.BuildParser(ruleparser, ParserType.LL_RECURSIVE_DESCENT, "rule").Result;
       
        var configuration = ExtractEbnfParserConfiguration(parserInstance.GetType(), grammarParser);
        
        foreach (var nonTerminal in configuration.NonTerminals)
        {
            foreach (var rule in nonTerminal.Value.Rules)
            {
                builder = BuildRule(parserInstance, rule, builder);
            }
        }

        var operationMethods = parserInstance.GetType()
            .GetMethods()
            .Where(m => m.GetCustomAttributes(typeof(OperationAttribute), true).Any());
        
        foreach (var method in operationMethods)
        {
            var attrs = method.GetCustomAttributes(typeof(OperationAttribute), true)
                .Cast<OperationAttribute>();
            foreach(var attr in attrs)
            {
                BuildOperation(parserInstance, attr, method, builder);
            }
        }
        builder.WithLexerbuilder(lexerBuilder);
        return builder.BuildParser();
    }

    private static void BuildOperation(object parserInstance, OperationAttribute attr, MethodInfo method,
        IFluentEbnfParserBuilder<IN, OUT> builder)
    {
        if (attr == null)
        {
            return;
        }            
        var argsParam = Expression.Parameter(typeof(object[]), "args");
        var methodParams = method.GetParameters();
        var castArgs = methodParams.Select((p, i) =>
            (Expression)Expression.Convert(
                Expression.ArrayIndex(argsParam, Expression.Constant(i)),
                p.ParameterType
            )).ToArray();
        var instanceExpr = Expression.Constant(parserInstance, parserInstance.GetType());
        var callExpr = Expression.Call(instanceExpr, method, castArgs);
        var body = Expression.Convert(callExpr, typeof(OUT));
        Func<object[], OUT> visitor = Expression.Lambda<Func<object[], OUT>>(body, argsParam).Compile();

        IN token = default;
        if (attr.IsIntToken)
        {
            token = EnumConverter.ConvertIntToEnum<IN>(attr.IntToken);
        }
        else if (attr.IsStringToken)
        {
            token = EnumConverter.ConvertStringToEnum<IN>(attr.StringToken);
        }
        if (attr.Affix == Affix.PreFix)
        {                
            builder.Prefix(token, attr.Precedence, visitor);
        }
        if (attr.Affix == Affix.InFix)
        {
            builder.Infix(token, attr.Assoc, attr.Precedence, visitor);
        }
        if (attr.Affix == Affix.PostFix)
        {
            builder.Postfix(token, attr.Precedence, visitor);
        }
    }

    private static IFluentEbnfParserBuilder<IN, OUT> BuildRule(object parserInstance, Rule<IN, OUT> rule, IFluentEbnfParserBuilder<IN, OUT> builder)
    {
        var ruleString = rule.RuleString;
        Func<object[], OUT> visitor = null;
        var method = rule.GetVisitorMethod();
        if (method != null)
        {
            var argsParam = Expression.Parameter(typeof(object[]), "args");
            var methodParams = method.GetParameters();
            var castArgs = methodParams.Select((p, i) =>
                (Expression)Expression.Convert(
                    Expression.ArrayIndex(argsParam, Expression.Constant(i)),
                    p.ParameterType
                )).ToArray();
            var instanceExpr = Expression.Constant(parserInstance, parserInstance.GetType());
            var callExpr = Expression.Call(instanceExpr, method, castArgs);
            var body = Expression.Convert(callExpr, typeof(OUT));
            visitor = Expression.Lambda<Func<object[], OUT>>(body, argsParam).Compile();
        }                
        builder = builder.Production(ruleString, visitor).Named(rule.NodeName);
        return builder;
    }
}