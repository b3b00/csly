using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ModeAttribute : Attribute
    {
     
        public const string DefaultLexerMode = "default";
        
        public string Mode { get; }

        public ModeAttribute(string mode = DefaultLexerMode)
        {
            Mode = mode;
        }
    }
}