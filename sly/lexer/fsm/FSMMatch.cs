using System;
using System.Collections.Generic;

namespace sly.lexer.fsm
{
    public class FSMMatch<N>
    {
        public Dictionary<string, object> Properties { get; }

        public bool IsString { get; set; }
        
        public char StringDelimiterChar { get; set; }
        
        public bool IsSuccess { get; }

        public bool IsEOS { get; }

        public Token<N> Result { get; }

        public int NodeId { get; }

        public LexerPosition NewPosition { get; }
        
        public bool IsLineEnding { get; set; }

        public List<Token<N>> IgnoredTokens { get; set; }
        
        public FSMMatch(bool success)
        {
            IgnoredTokens = new List<Token<N>>();
            IsSuccess = success;
            IsEOS = !success;
        }



        public FSMMatch(bool success, N result, string value, LexerPosition position, int nodeId,
            LexerPosition newPosition, bool isLineEnding)
            : this(success, result, new ReadOnlyMemory<char>(value.ToCharArray()), position, nodeId, newPosition,
                isLineEnding)
        {
            IgnoredTokens = new List<Token<N>>();
        }

        public FSMMatch(bool success, N result, ReadOnlyMemory<char> value, LexerPosition position, int nodeId,
            LexerPosition newPosition, bool isLineEnding)
        {
            Properties = new Dictionary<string, object>();
            IsSuccess = success;
            NodeId = nodeId;
            IsEOS = false;
            Result = new Token<N>(result, value, position);
            NewPosition = newPosition;
            IsLineEnding = isLineEnding;
            IgnoredTokens = new List<Token<N>>();
        }

        
    }
}