using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class SubParserAttribute : Attribute
     {
        public Type VisitorType { get; set; }
        public Type LexerType { get; set; }
        public Type OutputType { get; set; }
        public string StartingRule { get; set; }
        
        public SubParserAttribute(Type visitorType, Type lexerType, Type outputType, string startingRule)
        {
            VisitorType = visitorType;
            LexerType = lexerType;
            OutputType = outputType;
            StartingRule = startingRule;
        }
    }
}