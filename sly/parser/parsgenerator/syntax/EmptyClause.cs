using System;
using System.Collections.Generic;

namespace sly.parser.syntax
{

    public class EmptyClause<T> : Clause<T>
    {
        
        public bool Check(T nextToken) {
            return true;
        }

    }
}