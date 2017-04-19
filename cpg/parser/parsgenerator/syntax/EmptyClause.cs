using System;
using System.Collections.Generic;

namespace parser.parsergenerator.syntax
{

    public class EmptyClause<T> : Clause<T>
    {
        
        public bool Check(T nextToken) {
            return true;
        }

    }
}