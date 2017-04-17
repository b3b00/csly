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


        static ISyntaxParser<T> BuildSyntaxParser<T>(Type parserClass, ParserType parserType, string rootRule)
        {
            return null;
        }

        static object BuildParser<T>(Type parserClass, ParserType parserType, string rootRule) {
            ISyntaxParser<T> syntaxParser = BuildSyntaxParser(parserClass,parserType,rootRule);                        
            // todo build visitor
            // todo build wrapper arround visitor and syntaxparser
            return null;
        }

    }
}