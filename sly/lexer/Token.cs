using System;

namespace sly.lexer
{
    public class Token<T>
    {


        private static T defTok;

        public static T DefaultToken
        {
            get { return defTok; }
            set { defTok = value; }
        }
        public Token(T token, string value, TokenPosition position)
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

        public bool IsEndOfStream
        {
            get
            {
                return TokenID.Equals(DefaultToken);
            }
            private set
            {
            }
        }

        public char StringDelimiter = '"';

        public TokenPosition Position { get; set; }
        public T TokenID { get; set; }
        public string Value { get; set; }

        public string StringWithoutQuotes
        {
            get
            {
                string result = Value;
                if (StringDelimiter != (char)0 )
                {
                    if (result.StartsWith(StringDelimiter.ToString()))
                    {
                        result = result.Substring(1);
                    }
                    if (result.EndsWith(StringDelimiter.ToString()))
                    {
                        result = result.Substring(0, result.Length - 1);
                    }
                }
                return result;
            }
            private set { }
        }

        public int IntValue
        {
            get
            {
                return int.Parse(Value);
            }
            set { }
        }

        public double DoubleValue
        {
            get
            {
                return double.Parse(Value);
            }
        }

        public char CharValue
        {
            get
            {
                return StringWithoutQuotes[0];
            }
        }


        public bool End { get; set; }

        public override string ToString()
        {
            return $"{TokenID} [{Value}] @{Position}";
        }
    }
}
