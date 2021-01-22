using System;
using System.Collections.Generic;

namespace sly.lexer.fsm
{
    public class FSMMatch<N>
    {
        public Dictionary<string, object> Properties { get; }

        public bool IsString { get; set; }
        
        public char StringDelimiterChar { get; set; }
        
        public bool IsSuccess { get; set; }

        public bool IsEOS { get; }
        
        public bool IsIndent { get; set; }
        
        public bool IsUnIndent { get; set; }

        public Token<N> Result { get; }

        public int NodeId { get; }

        public LexerPosition NewPosition { get; }
        
        public bool IsLineEnding { get; set; }

        
        
        public FSMMatch(bool success)
        {
            IsSuccess = success;
            IsEOS = !success;
        }

        protected FSMMatch()
        {
            
        }

        public static FSMMatch<N> Indent()
        {
            return new FSMMatch<N>()
            {
                IsIndent = true,
                IsSuccess = true
            };
        }
        
        public static FSMMatch<N> UIndent()
        {
            return new FSMMatch<N>()
            {
                IsIndent = true,
                IsSuccess = true
            };
        }
        

        public FSMMatch(bool success, N result, string value, LexerPosition position, int nodeId, LexerPosition newPosition, bool isLineEnding)
            : this(success, result, new ReadOnlyMemory<char>(value.ToCharArray()), position, nodeId,newPosition,isLineEnding)
        { }

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
        }

        
    }
}