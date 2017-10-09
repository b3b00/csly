using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.model
{
    public class Neg : Expression
    {
        Expression Value { get; set; }

        public Neg(Expression value)
        {
            Value = value;
        }
    }
}
