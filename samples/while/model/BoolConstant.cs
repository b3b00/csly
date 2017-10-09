using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.model
{
    public class BoolConstant : Expression
    {

        public bool Value { get; set; }

        public BoolConstant(bool value)
        {
            Value = value;
        }
    }
}
