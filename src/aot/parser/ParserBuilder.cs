using sly.parser;

namespace aot.parser;

public class ParserBuilder<T, O> : IProductionBuilder<T,O> where T : struct
{
    public static IProductionBuilder<T,O> NewBuilder<T,O>() where T : struct
    {
        return new ParserBuilder<T,O>();
    }

    private ParserBuilder()
    {
        
    }

    public IProductionBuilder<T, O> Production(string rule, Func<object[], O> visitor)
    {
        throw new NotImplementedException();
    }

    public IProductionBuilder<T, O> Right(T operation, Func<object[], O> visitor)
    {
        throw new NotImplementedException();
    }

    public IProductionBuilder<T, O> Right(string explicitOperation, Func<object[], O> visitor)
    {
        throw new NotImplementedException();
    }

    public IProductionBuilder<T, O> Left(T operation, Func<object[], O> visitor)
    {
        throw new NotImplementedException();
    }

    public IProductionBuilder<T, O> Left(string explicitOperation, Func<object[], O> visitor)
    {
        throw new NotImplementedException();
    }

    public IProductionBuilder<T, O> Prefix(T operation, Func<object[], O> visitor)
    {
        throw new NotImplementedException();
    }

    public IProductionBuilder<T, O> Prefix(string explicitOperation, Func<object[], O> visitor)
    {
        throw new NotImplementedException();
    }

    public IProductionBuilder<T, O> postfix(T operation, Func<object[], O> visitor)
    {
        throw new NotImplementedException();
    }

    public IProductionBuilder<T, O> postfix(string explicitOperation, Func<object[], O> visitor)
    {
        throw new NotImplementedException();
    }

    public ISyntaxParser<T, O> Build()
    {
        throw new NotImplementedException();
    }
}