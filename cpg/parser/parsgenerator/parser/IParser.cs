using System;
using System.Collections.Generic;
using System.Text;

namespace cpg.parser.parsgenerator.parser
{
    interface IParser<T>
    {
        object Parse(IList<T> tokens);
    }
}
