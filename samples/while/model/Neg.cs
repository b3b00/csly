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


        public string Dump(string tab)
        {
            StringBuilder dmp = new StringBuilder();
            dmp.AppendLine($"{tab}(NEG");
            dmp.AppendLine(Value.Dump(tab + "\t"));
            dmp.AppendLine($"{tab})");
            return dmp.ToString();
        }
        
    }
}
