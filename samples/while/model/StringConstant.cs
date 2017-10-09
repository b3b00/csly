using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.model
{
    class StringConstant : Expression
    {

        string Value { get; set; }

        public StringConstant(string value)
        {
            Value = value;
        }
    }
}
