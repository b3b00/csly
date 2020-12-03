using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class SingleLineIslandAttribute : IslandAttribute
    {
        public SingleLineIslandAttribute(string start,  int channel = Channels.Comments) : base(start, null, null, channel)
        { }
    }
}