using System;
using sly.lexer;
using sly.lexer.fsm;

namespace sly.sourceGenerator
{
    public abstract class AbstractParserGenerator<IN> where IN : struct
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
    
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class ParserGeneratorAttribute : System.Attribute
    {
        private readonly Type _lexerType;

        public Type LexerType => _lexerType;

        private readonly Type _parserType;

        public Type ParserType => _parserType;

        private readonly Type _outputType;

        public Type OutpuType => _outputType;

        public ParserGeneratorAttribute(Type lexerType, Type parserType, Type outputType) {
            _lexerType = lexerType;
            _parserType = parserType;
            _outputType = outputType;
        }

    }
}