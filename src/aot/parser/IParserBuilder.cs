using sly.parser;

namespace aot.parser;

public interface IProductionBuilder<T,O> where T : struct
{
    IProductionBuilder<T,O> Production(string rule, Func<object[], O> visitor);
    
    IProductionBuilder<T,O> Right(T operation, Func<object[], O> visitor);
    
    IProductionBuilder<T,O> Right(string explicitOperation, Func<object[], O> visitor);
    IProductionBuilder<T,O> Left(T operation, Func<object[], O> visitor);
    
    IProductionBuilder<T,O> Left(string explicitOperation, Func<object[], O> visitor);
    IProductionBuilder<T,O> Prefix(T operation, Func<object[], O> visitor);
    
    IProductionBuilder<T,O> Prefix(string explicitOperation, Func<object[], O> visitor);
    IProductionBuilder<T,O> postfix(T operation, Func<object[], O> visitor);
    
    IProductionBuilder<T,O> postfix(string explicitOperation, Func<object[], O> visitor);
    public ISyntaxParser<T, O> Build();
}