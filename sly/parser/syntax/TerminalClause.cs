using System;
using System.Collections.Generic;

namespace sly.parser.syntax
{

    public class TerminalClause<T> : IClause<T>
    {
        public T ExpectedToken {get; set;}
        public TerminalClause(T token) {
            ExpectedToken = token;
        }
        public bool Check(T nextToken) {
            return nextToken.Equals(ExpectedToken);
        }

        public override string ToString()
        {
            return ExpectedToken.ToString();
        }

        public bool MayBeEmpty()
        {
            return true;
        }

    }
}