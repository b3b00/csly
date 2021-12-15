using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using expressionparser;
using simpleExpressionParser;
using sly.buildresult;
using sly.lexer;
using sly.parser;
using sly.parser.generator;

namespace lambdagenerationtests
{
    
    
    class Program
    {
    
        
        public static (BuildResult<Parser<ExpressionToken, int>>,SimpleExpressionParserWithContext instance) buildSimpleExpressionParserWithContext()
        {
            var StartingRule = $"{typeof(SimpleExpressionParserWithContext).Name}_expressions";
            var parserInstance = new SimpleExpressionParserWithContext();
            var builder = new ParserBuilder<ExpressionToken, int>();
            var Parser = builder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, StartingRule);
            return (Parser,parserInstance);
        }
        
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var (result,instance) = buildSimpleExpressionParserWithContext();

            BUildMethods(instance);
        }

        private static void BUildMethods(SimpleExpressionParserWithContext instance)
        {
            var methods = instance.GetType().GetMethods();

            List<ParameterExpression> parameterExpressions = new List<ParameterExpression>();

            foreach (var method in methods)
            {
                BuildLambda<int>(instance, method);
            }
        }

        private static void BuildLambda<OUT>(object instance, MethodInfo method)
        {
            var parameters = BuildParameters(instance.GetType(),method.GetParameters());

            var instanceParameter = Expression.Parameter(instance.GetType(), "instance");
            var argsArray = Expression.Parameter(typeof(object[]), "args");
            
            var parametersValues =   new List<ParameterExpression>(){instanceParameter, argsArray};
            var lambdaParameters =
                new List<ParameterExpression>(){Expression.Parameter(typeof(object),"instance"), argsArray}; 
            
            var body = BuildBody(method.Name, instance, method.GetParameters(), lambdaParameters);
            var bodyString = body.ToString();

            
            
            
            var l = Expression.Lambda<Func<object,object[],OUT>>(body, $"call_{method.Name}", lambdaParameters);
            
            var callIt = l.Compile();
            
        }

        static List<ParameterExpression> BuildParameters(Type instanceType, ParameterInfo[] parameters)
        {
            var parametertypes = parameters.Select(x => x.ParameterType).ToArray();
            var parameterValues = parameters.Select((item, index) => Expression.Parameter(item.ParameterType, $"p{index}")).ToList();
            parameterValues.Insert(0,Expression.Parameter(instanceType,"instance"));
            return parameterValues.ToList();
        }
        
        static Expression BuildBody(string methodName, object instance, ParameterInfo[] parameters,
            List<ParameterExpression> parameterExpressions)
        {
            var instanceParam = parameterExpressions[0];
            List<Expression> parametersValues = new List<Expression>();

            ParameterExpression args = parameterExpressions[1];
            
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var value = Expression.ArrayIndex(args, Expression.Constant(i));
                var p = BuildCast(parameter,value);
                parametersValues.Add(p);
            }

            LabelTarget returnTarget = Expression.Label(typeof(object));
            
            ParameterExpression castVariable = Expression.Variable(instance.GetType(), "parser");
            
            Expression cast = Expression.Assign(
                castVariable,
                Expression.Convert(instanceParam,instance.GetType())
            );
            
            
            var caller = Expression.Call(Expression.Convert(instanceParam,instance.GetType()),
                methodName,
                null,
                parametersValues.ToArray());

            var block  = Expression.Block(new[]{castVariable},cast,caller);
            
            
            return caller;
        }

        static Expression BuildCast(ParameterInfo parameterInfo, Expression expression)
        {
            //var variable = Expression.Variable(typeof(object), $"p{index}");
            var cast = Expression.Convert(expression, parameterInfo.ParameterType);
            return cast;
        }
    }
}