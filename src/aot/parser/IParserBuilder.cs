using sly.parser;

namespace aot.parser;

public interface IProductionBuilder<T,O> where T : struct
{
    IProductionBuilder<T,O> Production(string rule, Action<object[], O> visitor);
    
    IProductionBuilder<T,O> Right(T operation, Action<object[], O> visitor);
    IProductionBuilder<T,O> Left(T operation, Action<object[], O> visitor);
    IProductionBuilder<T,O> Prefix(T operation, Action<object[], O> visitor);
    IProductionBuilder<T,O> postfix(T operation, Action<object[], O> visitor);
    public ISyntaxParser<T, O> Build();
}

public class ParserBuilder<T, O> : IProductionBuilder<T,O> where T : struct
{
    public static IProductionBuilder<T,O> NewBuilder<A>() where A : struct
    {
        return new ParserBuilder<T,O>();
    }

    private ParserBuilder()
    {
        
    }

    public IProductionBuilder<T, O> Production(string rule, Action<object[], O> visitor)
    {
        throw new NotImplementedException();
    }

    public IProductionBuilder<T, O> Right(T operation, Action<object[], O> visitor)
    {
        throw new NotImplementedException();
    }

    public IProductionBuilder<T, O> Left(T operation, Action<object[], O> visitor)
    {
        throw new NotImplementedException();
    }

    public IProductionBuilder<T, O> Prefix(T operation, Action<object[], O> visitor)
    {
        throw new NotImplementedException();
    }

    public IProductionBuilder<T, O> postfix(T operation, Action<object[], O> visitor)
    {
        throw new NotImplementedException();
    }

    public ISyntaxParser<T, O> Build()
    {
        throw new NotImplementedException();
    }
}