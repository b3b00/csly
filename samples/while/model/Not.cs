using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.model
{
    class Not : Expression
    {
        Expression Value { get; set; }

        public Not(Expression value)
        {
            Value = value;
        }

    }
}
