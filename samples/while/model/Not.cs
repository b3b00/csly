using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.model
{
    public class Not : Expression
    {
        public Expression Value { get; set; }

        public Not(Expression value)
        {
            Value = value;
        }

        public string Dump(string tab)
        {
            StringBuilder dmp = new StringBuilder();
            dmp.AppendLine($"{tab}(NOT");
            dmp.AppendLine(Value.Dump(tab + "\t"));
            dmp.AppendLine($"{tab})");
            return dmp.ToString();
        }

    }
}
