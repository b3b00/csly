using System;

namespace lexer
{
    public class Token<T>
    {

        public static T DefaultToken;
        public Token(T token,  string value, TokenPosition position)
        {
            TokenID = token;
            Value = value;
            Position = position;
        }

        public Token()
        {   
            End = true;
            TokenID = DefaultToken;
        }

        public TokenPosition Position { get; set; }
        public  T TokenID { get; set; }
        public string Value { get; set; }

        public bool End { get; set; }

        public override string ToString()
        {
            return string.Format($" {Position.Index}, {Position.Line}, {Position.Column}");
        }
    }
}
