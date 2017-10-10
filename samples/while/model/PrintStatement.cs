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

        public string Dump(string tab)
        {
            StringBuilder dmp = new StringBuilder();
            dmp.AppendLine($"{tab}(PRINT ");
            dmp.AppendLine($"{Value.Dump("\t" + tab)}");            
            dmp.AppendLine($"{tab})");
            return dmp.ToString();
        }
    }
}
