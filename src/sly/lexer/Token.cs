using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace sly.lexer
{
    public enum CommentType
    {
        Single,
        Multi,
        No
    }

    [DebuggerDisplay("{TokenID} : {Value} - {IsExplicit}")]
    public class Token<T> where T:struct
    {
        
        
        [JsonIgnore]
        public char StringDelimiter = '"';
        [JsonIgnore]
        public char DecimalSeparator = '.';
        [JsonIgnore]
        public char CharDelimiter ='\'';
        [JsonIgnore]
        public bool Notignored;


        public Token(T token, string value, LexerPosition position, 
            CommentType commentType = CommentType.Single, int? channel = null, char decimalSeparator = '.' ) : this(token,new ReadOnlyMemory<char>(value.ToCharArray()),position,commentType,channel, decimalSeparator:decimalSeparator)

{
        }
        
        public Token(T token, ReadOnlyMemory<char> value, LexerPosition position, 
            CommentType commentType = CommentType.Single, int? channel = null, bool isWhiteSpace = false, char decimalSeparator = '.' )
        {
            IsWhiteSpace = isWhiteSpace;
            IsEOS = false;
            TokenID = token;
            SpanValue = value;
            Position = position;
            CommentType = commentType;
            DecimalSeparator = decimalSeparator;
            if (CommentType != CommentType.No)
            {
                if (channel == null)
                {
                    channel = Channels.Main;
                }
            }
            else
            {
                channel = channel ?? Channels.Main;
            }

            Channel = channel.Value;
        }


        public Token()
        {
            IsEOS = true;
            End = true;
            Position = new LexerPosition(0, 0, 0);
            DecimalSeparator = '.';
        }
        
        
        public List<Token<T>> NextTokens(int channelId)
        {
            TokenChannel<T> channel = null;
            if (TokenChannels.TryGet(channelId, out channel))
            {
                var list = new List<Token<T>>();
                int position = PositionInTokenFlow + 1;
                if (position >= 0 && position <= channel.Count - 1)
                {
                    
                    for (int i = position; position < channel.Count; position++)
                    {
                        var token = channel[position];
                        if (token != null)
                        {
                            list.Add(token);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                return list;
            }
            return new List<Token<T>>();
        }
        
        public Token<T> Next(int channelId)
        {
            TokenChannel<T> channel = null;
             
            if (TokenChannels.TryGet(channelId, out channel))
            {
                int position = PositionInTokenFlow + 1;
                if (position < channel.Count)
                {
                    return channel[position];
                }
            }
            return null;
        }

        public List<Token<T>> PreviousTokens(int channelId)
        {
            TokenChannel<T> channel = null;
            if (TokenChannels.TryGet(channelId, out channel))
            {
                var list = new List<Token<T>>();
                int position = PositionInTokenFlow - 1;
                if (position >= 0 && position <= channel.Count - 1)
                {
                    
                    for (int i = position; position > 0; position--)
                    {
                        var token = channel[position];
                        if (token != null)
                        {
                            list.Add(token);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                return list;
            }
            return new List<Token<T>>();
        }
        
        public Token<T> Previous(int channelId)
        {
            TokenChannel<T> channel = null;
             
            if (TokenChannels.TryGet(channelId, out channel))
            {
                int position = PositionInTokenFlow - 1;
                if (position >= 0 && position <= channel.Count - 1) 
                {
                    return channel[position];
                }
            }
            return null;
        }

        public int Channel {get; set;} = 0;

        [JsonIgnore]
        public  ReadOnlyMemory<char> SpanValue { get; set; }
        
        public LexerPosition Position { get; set; }

        public int PositionInTokenVisibleFlow { get; set; }
        
        public int PositionInTokenFlow { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public T TokenID { get; set; }

        public string Label { get; set; }
        
        public bool IsComment { get; set; }

        public bool Discarded { get; set; } = false;

        public bool IsEOS { get; set; }
        
        public bool IsIndent { get; set; }
        
        public bool IsUnIndent { get; set; }
        
        public int IndentationLevel { get; set; }
        
        public bool IsWhiteSpace { get; set; }
        
        public bool IsEOL { get; set; }
        
        public bool IsExplicit { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public CommentType CommentType { get; set; } = CommentType.No;

        public bool IsEmpty { get; set; }

        [JsonIgnore]
        public bool IsMultiLineComment => CommentType == CommentType.Multi;

        [JsonIgnore]
        public bool IsSingleLineComment => CommentType == CommentType.Single;

        public string Value => SpanValue.ToString();

        


        [JsonIgnore]
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



        [JsonIgnore]
        public int IntValue => int.Parse(Value);

        [JsonIgnore]
        public DateTime DateTimeValue { get; set; }
        
        [JsonIgnore]
        public  double DoubleValue
        {
            get
            {
                var val = Value.Replace(DecimalSeparator, '.');
                var culture = CultureInfo.InvariantCulture;
                double result = 0.0;
                double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture,
                    out result);
                
                return result;
            }
        }

        [JsonIgnore]
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
        
        public bool IsLineEnding { get; set; }
        
        [JsonIgnore]
        public TokenChannels<T> TokenChannels { get; set; }         

        public static Token<T> Empty()
        {
            var empty = new Token<T>();
            empty.IsEmpty = true;
            return empty;
        }

        [ExcludeFromCodeCoverage]

        public string GetDebug()
        {
            if (IsEOS)
            {
                return "<<EOS>>";
            }

            if (IsWhiteSpace)
            {
                return $"<<WS>>[{Value}]";
            }

            if (IsEOL)
            {
                return "<<EOL>>";
            }

            string value = $"{TokenID} [{Value.Replace("\r","").Replace("\n","")}]";
                
            if (IsIndent)
            {
                value = $"<<INDENT({IndentationLevel})>>";
            }

            if (IsUnIndent)
            {
                value = $"<<UINDENT({IndentationLevel})>>";
            }

            if (IsExplicit)
            {
                value = $"[{Value.Replace("\r", "").Replace("\n", "")}]";
            }

            return value;
        }
        
        
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (IsEOS)
            {
                return "<<EOS>>";
            }

            if (IsWhiteSpace)
            {
                return $"<<WS>>[{Value}]";
            }

            string value = $"{TokenID} [{Value.Replace("\r","").Replace("\n","")}]";
                
            if (IsIndent)
            {
                value = $"<<INDENT({IndentationLevel})>>";
            }

            if (IsUnIndent)
            {
                value = $"<<UINDENT({IndentationLevel})>>";
            }

            if (IsExplicit)
            {
                value = $"[{Value.Replace("\r", "").Replace("\n", "")}]";
            }
            return $"{value} @{Position} on channel {Channel}";
        }
    }
}