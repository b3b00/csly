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
        
        public int UnIndentCount { get; set; }
        
        public int IndentationLevel { get; set; }

        public Token<N> Result { get; set; }

        public int NodeId { get; }

        public LexerPosition NewPosition { get; set; }
        
        public bool IsLineEnding { get; set; }

        public bool IsIndentationError { get; set; }
        
        
        public FSMMatch(bool success)
        {
            IsSuccess = success;
            IsEOS = !success;
        }

        protected FSMMatch()
        {
            Properties = new Dictionary<string, object>();
        }

        public static FSMMatch<N> Indent(int level)
        {
            return new FSMMatch<N>()
            {
                IsIndent = true,
                IsSuccess = true,
                IndentationLevel = level,
                Result = new Token<N>(){IsIndent = true, IsEOS = false}
            };
        }
        
        public static FSMMatch<N> UIndent(int level, int count = 1)
        {
            return new FSMMatch<N>()
            {
                IsUnIndent = true,
                IsSuccess = true,
                IndentationLevel = level,
                Result = new Token<N>(){IsUnIndent = true, IsEOS = false},
                UnIndentCount = count
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