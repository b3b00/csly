using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.model
{
    class BoolConstant : Expression
    {

        bool Value { get; set; }

        public BoolConstant(bool value)
        {
            Value = value;
        }
    }
}
