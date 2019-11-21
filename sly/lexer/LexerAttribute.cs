using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Enum)]
    public class LexerAttribute : Attribute
    {
        private static readonly GenericLexer<int>.Config Defaults = new GenericLexer<int>.Config();

        private bool? ignoreWS;

        private bool? ignoreEOL;

        private char[] whiteSpace;

        public bool IgnoreWS
        {
            get => ignoreWS ?? Defaults.IgnoreWS;
            set => ignoreWS = value;
        }

        public bool IgnoreEOL
        {
            get => ignoreEOL ?? Defaults.IgnoreEOL;
            set => ignoreEOL = value;
        }

        public char[] WhiteSpace
        {
            get => whiteSpace ?? Defaults.WhiteSpace;
            set => whiteSpace = value;
        }
    }
}