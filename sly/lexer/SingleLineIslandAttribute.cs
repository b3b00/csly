using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class SingleLineIslandAttribute : IslandAttribute
    {
        public SingleLineIslandAttribute(string start, Type lexerType = null, Type parserType = null, int channel = Channels.Comments) : base(start, null, null,lexerType, parserType, channel)
        { }
    }
}