using System;

namespace sly.lexer
{

    public enum CommentType {
        Single,
        Multi
    }
    public class Token<T>
    {


        private static T defTok;


        public char StringDelimiter = '"';

        public TokenPosition Position { get; set; }
        public T TokenID { get; set; }
        public bool IsCommentStart { get; set; }

        public CommentType CommentType {get; set;} = CommentType.Single;

        public bool IsMultiLineComment => CommentType == CommentType.Multi;

        public bool IsSingleLineComment => CommentType == CommentType.Single;

        public string Value { get; set; }

        public static T DefaultToken
        {
            get { return defTok; }
            set { defTok = value; }
        }


        public Token(T token, string value, TokenPosition position, bool isCommentStart = false, CommentType commentType =  CommentType.Single) 
        {
            TokenID = token;
            Value = value;
            Position = position;
            CommentType = commentType;
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
        }

       

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
        }

        public int IntValue
        {
            get
            {
                return int.Parse(Value);
            }
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
