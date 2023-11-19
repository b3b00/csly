using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
 public class ModeAttribute : Attribute
    {
     
        public const string DefaultLexerMode = "default";
        
        public string[] Modes { get; }

        public ModeAttribute()
        {
            Modes = new[] { DefaultLexerMode };
        }
        
        public ModeAttribute(params string[] modes)
        {
            Modes = modes;
        }
    }
}