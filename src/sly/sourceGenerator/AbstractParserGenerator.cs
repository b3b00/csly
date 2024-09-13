using System;
using sly.lexer;
using sly.lexer.fsm;

namespace sly.sourceGenerator;

public abstract class AbstractParserGenerator<IN,PARSER,OUT> where IN : struct
{
    public virtual Action<IN, LexemeAttribute, GenericLexer<IN>> UseTokenExtensions()
    {
        return null;
    }

    public virtual LexerPostProcess<IN> UseTokenPostProcessor()
    {
        return null;
    }
}