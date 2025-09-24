using System.Collections.Generic;
using sly.parser.generator;
using sly.parser.parser;

namespace ParserExample;

public class RecursiveGrammar2
{
    [Production("first : second* third COMMA[d]")]
    public object FirstRecurse(List<object> seconds, object third)
    {
        return third;
    } 
        
    [Production("first : second? third COMMA[d]")]
    public object FirstRecurse2(ValueOption<object> optSecond, object third)
    {
        return null;
    }
        
    [Production("second :  COMMA[d]")]
    public object SecondRecurse()
    {
        return null;
    }
        
    [Production("third : first COMMA[d]")]
    public object ThirdRecurse(object o)
    {
        return o;
    }
}