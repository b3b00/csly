using System;


namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class IslandAttribute : Attribute
    {
        public string MultiLineIslandEnd;

        public string MultiLineIslandStart;

        public string SingleLineIslandStart;

        public int Channel = 1;

        public Type LexerType;
        
        public Type ParserType;

        public IslandType IslandType =>
            string.IsNullOrEmpty(SingleLineIslandStart) ? IslandType.Multi : IslandType.Single;
        
        public IslandAttribute(string singleLineStart, string multiLineStart, string multiLineEnd, Type lexerType = null, Type parserType = null, int channel = 1)
        {
            SingleLineIslandStart = singleLineStart;
            MultiLineIslandStart = multiLineStart;
            MultiLineIslandEnd = multiLineEnd;
            Channel = channel;
            LexerType = lexerType;
            ParserType = parserType;
        }
    }
}