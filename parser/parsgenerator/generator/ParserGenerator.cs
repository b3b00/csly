using System;
using parser.parsergenerator.parser;

namespace parser.parsergenerator.generator
{

    public enum ParserType {
        LL = 1,
        LR = 2,

        COMBINATOR = 3
    }


    public class ParserGenerator
    {


        static IParser<T> BuildParser<T>(Type parserClass, ParserType parserType, string rootRule)
        {
            return null;
        }

    }
}