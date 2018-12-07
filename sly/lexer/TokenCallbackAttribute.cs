using System;

namespace sly.lexer
{
    public class TokenCallbackAttribute : Attribute
    {
        public int EnumValue { get; set; }

        public TokenCallbackAttribute(int enumValue)
        {
            EnumValue = enumValue;
        }
    }
}