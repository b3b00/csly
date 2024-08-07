using sly.parser;

namespace aot.parser;

public interface IProductionBuilder<T,O> where T : struct
{
    IProductionBuilder<T,O> Production(string ruleString, Func<object[], O> visitor);
    
    IProductionBuilder<T,O> Operand(string rule, Func<object[], O> visitor);
    
    IProductionBuilder<T,O> Right(int precedence, T operation, Func<object[], O> visitor);
    
    IProductionBuilder<T,O> Right(int precedence, string explicitOperation, Func<object[], O> visitor);
    IProductionBuilder<T,O> Left(int precedence, T operation, Func<object[], O> visitor);
    
    IProductionBuilder<T,O> Left(int precedence, string explicitOperation, Func<object[], O> visitor);
    IProductionBuilder<T,O> Prefix(int precedence, T operation, Func<object[], O> visitor);
    
    IProductionBuilder<T,O> Prefix(int precedence, string explicitOperation, Func<object[], O> visitor);
    IProductionBuilder<T,O> Postfix(int precedence, T operation, Func<object[], O> visitor);
    
    IProductionBuilder<T,O> Postfix(int precedence, string explicitOperation, Func<object[], O> visitor);
    public ISyntaxParser<T, O> Build();
}