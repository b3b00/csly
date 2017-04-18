using System;
using System.Collections.Generic;

namespace parser.parsergenerator.syntax
{

    public class TerminalClause<T> : Clause<T>
    {
        public T ExpectedToken {get; set;}
        public TerminalClause(T token) {
            ExpectedToken = token;
        }
        public object Check(T nextToken) {
            return null;
        }

    }
}