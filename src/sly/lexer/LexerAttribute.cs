using System;

namespace sly.lexer
{

    [AttributeUsage(AttributeTargets.Enum)]
    public class LexerAttribute : Attribute
    {
        private enum DefaultEnum : int
        {
        }
        private static readonly GenericLexer<DefaultEnum>.Config Defaults = new GenericLexer<DefaultEnum>.Config();

        private bool? ignoreWS;

        private bool? ignoreEOL;

        private char[] whiteSpace;

        private bool? keyWordIgnoreCase;

        
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

        public bool KeyWordIgnoreCase
        {
            get => keyWordIgnoreCase ?? Defaults.KeyWordIgnoreCase;
            set => keyWordIgnoreCase = value;
        }

        public bool IndentationAWare { get; set; } = false;
    
        public string Indentation { get; set; } = "";

    }
}