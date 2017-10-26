using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.model
{
    public sealed class SkipStatement : Statement
    {

        public string Dump(string tab)
        {
            return $"{tab}(SKIP)";
        }
    }
}
