using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.model
{
    public class IntegerConstant : Expression
    {
        public int Value { get; set; } 

        public IntegerConstant(int value)
        {
            Value = value;
        }

    }
}
