using System;
using sly.parser;

namespace aot.parser;

public interface IAotProductionBuilder<T,O> where T : struct
{
    IAotProductionBuilder<T,O> Production(string ruleString, Func<object[], O> visitor);
    
    IAotProductionBuilder<T,O> Operand(string rule, Func<object[], O> visitor);
    
    IAotProductionBuilder<T,O> Right(int precedence, T operation, Func<object[], O> visitor);
    
    IAotProductionBuilder<T,O> Right(int precedence, string explicitOperation, Func<object[], O> visitor);
    IAotProductionBuilder<T,O> Left(int precedence, T operation, Func<object[], O> visitor);
    
    IAotProductionBuilder<T,O> Left(int precedence, string explicitOperation, Func<object[], O> visitor);
    IAotProductionBuilder<T,O> Prefix(int precedence, T operation, Func<object[], O> visitor);
    
    IAotProductionBuilder<T,O> Prefix(int precedence, string explicitOperation, Func<object[], O> visitor);
    IAotProductionBuilder<T,O> Postfix(int precedence, T operation, Func<object[], O> visitor);
    
    IAotProductionBuilder<T,O> Postfix(int precedence, string explicitOperation, Func<object[], O> visitor);
    public ISyntaxParser<T, O> Build();
}