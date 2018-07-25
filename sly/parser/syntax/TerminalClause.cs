using System;
using System.Collections.Generic;
using System.Text;

namespace sly.parser.syntax
{

    public class TerminalClause<T> : IClause<T>
    {
        public T ExpectedToken {get; set;}

        public bool Discarded { get; set; }

        public TerminalClause(T token) {
            ExpectedToken = token;
        }

        public TerminalClause(T token, bool discard) : this(token)
        {            
            Discarded = discard;
        }
        public bool Check(T nextToken) {
            return nextToken.Equals(ExpectedToken);
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.Append(ExpectedToken.ToString());
            if (Discarded)
            {
                b.Append("[d]");
            }
            return b.ToString();
            
        }

        public bool MayBeEmpty()
        {
            return false;
        }

    }
}