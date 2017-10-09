using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.model
{
    public class PrintStatement : Statement
    {
        public Expression Value { get; set; }

        public PrintStatement(Expression value)
        {
            Value = value;
        }
    }
}
