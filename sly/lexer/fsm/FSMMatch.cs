using System;
using System.Collections.Generic;

namespace sly.lexer.fsm
{
    public class FSMMatch<N>
    {
        public char StringDelimiter = '"';

        
        public Dictionary<string, object> Properties { get; set; }

        public bool IsSuccess { get; set; }

        public Token<N> Result { get; set; }
        
        public int NodeId { get; set; }
        public FSMMatch(bool success)
        {
            IsSuccess = success;
        }
        
        public FSMMatch(bool success, N result, string value, TokenPosition position,int nodeId) : this(success,result,new ReadOnlyMemory<char>(value.ToCharArray()),position,nodeId )
        {
           
        }

        public FSMMatch(bool success, N result, ReadOnlyMemory<char> value, TokenPosition position, int nodeId)
        {
            Properties = new Dictionary<string, object>();
            IsSuccess = success;
            NodeId = nodeId;
            Result = new Token<N>(result, value, position);
        }

        
    }
}