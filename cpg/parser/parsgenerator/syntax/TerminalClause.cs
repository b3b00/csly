using System;
using System.Collections.Generic;

namespace parser.parsergenerator.syntax
{

    public class TerminalClause<T> : Clause<T>
    {
        private T ExpectedToken {get; set;}
        public TerminalClause(T token) {
            ExpectedToken = token;
        }
        public object Check(T nextToken) {
            return null;
        }

    }
}