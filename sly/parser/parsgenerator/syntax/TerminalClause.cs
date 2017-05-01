using System;
using System.Collections.Generic;

namespace sly.parser.syntax
{

    public class TerminalClause<T> : Clause<T>
    {
        public T ExpectedToken {get; set;}
        public TerminalClause(T token) {
            ExpectedToken = token;
        }
        public bool Check(T nextToken) {
            return nextToken.Equals(ExpectedToken);
        }

    }
}