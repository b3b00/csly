using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using sly.parser.syntax.grammar;

namespace sly.parser.parser
{
    public class VisitorCallerBuilder
    {
         public static CallVisitor BuildLambda(object instance, MethodInfo method)
        {
            var parameters = BuildParameters(instance.GetType(),method.GetParameters());

            var instanceParameter = Expression.Parameter(instance.GetType(), "instance");
            var argsArray = Expression.Parameter(typeof(object[]), "args");
            
            var parametersValues =   new List<ParameterExpression>(){instanceParameter, argsArray};
            var lambdaParameters =
                new List<ParameterExpression>(){Expression.Parameter(typeof(object),"instance"), argsArray}; 
            
            var body = BuildBody(method.Name, instance, method.GetParameters(), lambdaParameters);
            var bodyString = body.ToString();

            
            
            
            var l = Expression.Lambda<CallVisitor>(body, $"call_{method.Name}", lambdaParameters);
            
            var lambda = l.Compile();

            return lambda;

        }

         public static List<ParameterExpression> BuildParameters(Type instanceType, ParameterInfo[] parameters)
        {
            var parametertypes = parameters.Select(x => x.ParameterType).ToArray();
            var parameterValues = parameters.Select((item, index) => Expression.Parameter(item.ParameterType, $"p{index}")).ToList();
            parameterValues.Insert(0,Expression.Parameter(instanceType,"instance"));
            return parameterValues.ToList();
        }
        
         public static Expression BuildBody(string methodName, object instance, ParameterInfo[] parameters,
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

         public static Expression BuildCast(ParameterInfo parameterInfo, Expression expression)
        {
            //var variable = Expression.Variable(typeof(object), $"p{index}");
            var cast = Expression.Convert(expression, parameterInfo.ParameterType);
            return cast;
        }
    }
}