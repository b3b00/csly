using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class MultiLineIslandAttribute : IslandAttribute
    {
        public MultiLineIslandAttribute(string start, string end, Type lexerType = null, Type parserType = null, int channel = Channels.Islands) : base(null, start, end,lexerType,parserType, channel)
        { }
    }
}