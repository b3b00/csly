using System;

namespace GenericLexerWithCallbacks
{
    public class TokenCallbackAttribute : Attribute
    {
        public int EnumValue { get; set;}
        
        public TokenCallbackAttribute(int enumValue)
        {
            EnumValue = enumValue;
        }
        
    }
}