using sly.parser.generator;

namespace ParserExample;

public class RecursiveGrammar
{
    [Production("first : second COMMA[d]")]
    public object FirstRecurse(object o)
    {
        return o;
    } 
        
    [Production("second : third COMMA[d]")]
    public object SecondRecurse(object o)
    {
        return o;
    }
        
    [Production("third : first COMMA[d]")]
    public object ThirdRecurse(object o)
    {
        return o;
    }
}