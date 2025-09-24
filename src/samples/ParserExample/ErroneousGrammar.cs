using sly.parser.generator;

namespace ParserExample;

public class ErroneousGrammar
{
    [Production("clauses : clause (COMMA [D] clause)*")]

    public object test()
    {
        return null;
    }    
}