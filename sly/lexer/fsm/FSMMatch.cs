using System;
using System.Collections.Generic;

namespace sly.lexer.fsm
{
    public class FSMMatch<N>
    {
        public Dictionary<string, object> Properties { get; }

        public bool IsSuccess { get; }

        public bool IsEOS { get; }

        public Token<N> Result { get; }

        public int NodeId { get; }

        public FSMMatch(bool success)
        {
            IsSuccess = success;
            IsEOS = !success;
        }

        public FSMMatch(bool success, N result, string value, TokenPosition position, int nodeId)
            : this(success, result, new ReadOnlyMemory<char>(value.ToCharArray()), position, nodeId)
        { }

        public FSMMatch(bool success, N result, ReadOnlyMemory<char> value, TokenPosition position, int nodeId)
        {
            Properties = new Dictionary<string, object>();
            IsSuccess = success;
            NodeId = nodeId;
            IsEOS = false;
            Result = new Token<N>(result, value, position);
        }
    }
}