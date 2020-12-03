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

        public IslandType IslandType =>
            string.IsNullOrEmpty(SingleLineIslandStart) ? IslandType.Multi : IslandType.Single;
        
        public IslandAttribute(string singleLineStart, string multiLineStart, string multiLineEnd,  int channel = 1)
        {
            SingleLineIslandStart = singleLineStart;
            MultiLineIslandStart = multiLineStart;
            MultiLineIslandEnd = multiLineEnd;
            Channel = channel;
        }
    }
}