namespace handExpressions.sourceGenerator;

public abstract class AbstractCslyParser<I, P, O> where I: struct
{

    protected P _instance;

    public AbstractCslyParser(P instance)
    {
        _instance = instance;
    }
    //public abstract O Parse(string production, string source);
}