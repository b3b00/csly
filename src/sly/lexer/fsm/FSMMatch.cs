using System;
using System.Collections.Generic;

namespace sly.lexer.fsm
{
    public class FSMMatch<N>  where N : struct
    {
        public Dictionary<string, object> Properties { get; }

        public char StringDelimiterChar { get; set; }
        
        public string HexaPrefix { get; set; }
        
        public char DecimalSeparator { get; set; }
        public bool IsSuccess { get; set; }

        public bool IsEOS { get; }
        
        public bool IsIndent { get; set; }
        
        public bool IsUnIndent { get; set; }

        public bool IsNoIndent { get; set; } = false;
        
        public int UnIndentCount { get; set; }
        
        public int IndentationLevel { get; set; }

        public Token<N> Result { get; set; }

        public int NodeId { get; }

        public LexerPosition NewPosition { get; set; }
        
        public bool IsPush { get; set; }
        
        public bool IsPop { get; set; }
        
        
        
        public bool IsLineEnding { get; set; }

        public bool IsIndentationError { get; set; }
        
        public List<Token<N>> IgnoredTokens { get; set; }
        
        public DateTime DateTimeValue { get; set; }
        
        public FSMMatch(bool success)
        {
            IgnoredTokens = new List<Token<N>>();
            IsSuccess = success;
            IsEOS = !success;
        }

        protected FSMMatch()
        {
            Properties = new Dictionary<string, object>();
            IgnoredTokens = new List<Token<N>>();
        }

        public static FSMMatch<N> Indent(int level)
        {
            return new FSMMatch<N>
            {
                IsIndent = true,
                IsSuccess = true,
                IndentationLevel = level,
                IsPop = false,
                IsPush = false,
                Result = new Token<N> {IsIndent = true, IsEOS = false}
            };
        }
        
        public static FSMMatch<N> UIndent(int level, int count = 1)
        {
            return new FSMMatch<N>
            {
                IsUnIndent = true,
                IsSuccess = true,
                IsPop = false,
                IsPush = false,
                IndentationLevel = level,
                Result = new Token<N> {IsUnIndent = true, IsEOS = false},
                UnIndentCount = count
            };
        }
        

        public FSMMatch(bool success, N result, string value, LexerPosition position, int nodeId, LexerPosition newPosition, bool isLineEnding, bool isPop = false, bool isPush = false, string mode = null, char decimalSeparator = '.')
            : this(success, result, new ReadOnlyMemory<char>(value.ToCharArray()), position, nodeId, newPosition, isLineEnding, isPop, isPush, mode, decimalSeparator)
        { }

        public FSMMatch(bool success, N result, ReadOnlyMemory<char> value, LexerPosition position, int nodeId,
            LexerPosition newPosition, bool isLineEnding, bool isPop = false, bool isPush = false, string mode = null, char decimalSeparator = '.')
        {
            Properties = new Dictionary<string, object>();
            IsSuccess = success;
            NodeId = nodeId;
            IsEOS = false;
            Result = new Token<N>(result, value, position, decimalSeparator:decimalSeparator);
            NewPosition = newPosition;
            DecimalSeparator = decimalSeparator;
            if (isPush)
            {
                NewPosition.Mode = mode;
            }
            IsLineEnding = isLineEnding;
            IsPop = isPop;
            NewPosition.IsPop = isPop;
            IsPush = isPush;
            NewPosition.IsPush = isPush;
            IgnoredTokens = new List<Token<N>>();
        }
        
        

        
    }
}