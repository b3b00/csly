using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.model
{
    public class AssignStatement : Statement
    {

        public string VariableName { get; set; }

        public Expression Value { get; set; }

        public AssignStatement(string variableName, Expression value)
        {
            VariableName = variableName;
            Value = value;
        }

        public string Dump(string tab)
        {
            StringBuilder dmp = new StringBuilder();
            dmp.AppendLine($"{tab}(ASSIGN");
            dmp.AppendLine($"{tab}\t{VariableName}");
            dmp.AppendLine(Value.Dump(tab+"\t"));
            dmp.AppendLine($"{tab})");
            return dmp.ToString();
        }

    }
}
