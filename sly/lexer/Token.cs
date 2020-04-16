using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace sly.lexer
{
    public enum CommentType
    {
        Single,
        Multi,
        No
    }

    public class Token<T>
    {
        public char StringDelimiter = '"';
        
        public char CharDelimiter ='\'';


        public Token(T token, string value, LexerPosition position, bool isCommentStart = false,
            CommentType commentType = CommentType.Single) : this(token,new ReadOnlyMemory<char>(value.ToCharArray()),position,isCommentStart,commentType )
        {
            
        }
        
        public Token(T token, ReadOnlyMemory<char> value, LexerPosition position, bool isCommentStart = false,
            CommentType commentType = CommentType.Single)
        {
            IsEOS = false;
            TokenID = token;
            SpanValue = value;
            Position = position;
            CommentType = commentType;
        }


        public Token()
        {
            IsEOS = true;
            End = true;
            TokenID = DefaultToken;
            Position = new LexerPosition(0, 0, 0);
        }


        public  ReadOnlyMemory<char> SpanValue { get; set; }
        
        public LexerPosition Position { get; set; }

        public int PositionInTokenFlow { get; set; }
        public T TokenID { get; set; }
        public bool IsComment { get; set; }

        public bool Discarded { get; set; } = false;

        public bool IsEOS { get; set; }

        public CommentType CommentType { get; set; } = CommentType.No;

        public bool IsEmpty { get; set; }

        public bool IsMultiLineComment => CommentType == CommentType.Multi;

        public bool IsSingleLineComment => CommentType == CommentType.Single;

        public string Value => SpanValue.ToString();

        public static T DefaultToken
        {
            get => DefTok;
            set => DefTok = value;
        }


        public string StringWithoutQuotes
        {
            get
            {
                var result = Value;
                if (StringDelimiter != (char) 0)
                {
                    if (result.StartsWith(StringDelimiter.ToString())) result = result.Substring(1);
                    if (result.EndsWith(StringDelimiter.ToString())) result = result.Substring(0, result.Length - 1);
                }

                return result;
            }
        }



        public int IntValue => int.Parse(Value);

        public  double DoubleValue
        {
            get
            {
                // Try parsing in the current culture
                if (!double.TryParse(Value, NumberStyles.Any, CultureInfo.CurrentCulture,
                        out var result) &&
                    // Then try in US english
                    !double.TryParse(Value, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"),
                        out result) &&
                    // Then in neutral language
                    !double.TryParse(Value, NumberStyles.Any, CultureInfo.InvariantCulture,
                        out result))
                {
                    result = 0.0;
                }

                return result;
            }
            set { }
        }

        public char CharValue  {
            get
            {
                var result = Value;
                if (CharDelimiter != (char) 0)
                {
                    if (result.StartsWith(CharDelimiter.ToString()))  {
                        result = result.Substring(1);
                    }
                    if (result.EndsWith(CharDelimiter.ToString())) {
                        result = result.Substring(0, result.Length - 1);
                    }
                }
                return result[0];
            }
        } 


        public bool End { get; set; }
        public static T DefTok { get; set; }

        public static Token<T> Empty()
        {
            var empty = new Token<T>();
            empty.IsEmpty = true;
            return empty;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (!TokenID.Equals(DefaultToken))
            {
                return $"{TokenID} [{Value}] @{Position}";
            }

            return "<<EOS>>";
        }
    }
}