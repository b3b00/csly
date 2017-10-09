using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.model
{
    public class Not : Expression
    {
        Expression Value { get; set; }

        public Not(Expression value)
        {
            Value = value;
        }

    }
}
