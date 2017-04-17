using System;
using System.Collections.Generic;

namespace parser.parsergenerator.syntax
{

    public interface Clause<T>
    {
        object Check(T nextToken);

    }
}