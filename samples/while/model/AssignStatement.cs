using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.model
{
    class AssignStatement : Statement
    {

        public string VariableName { get; set; }

        public Expression Value { get; set; }

        public AssignStatement(string variableName, Expression value)
        {
            VariableName = variableName;
            Value = value;
        }

    }
}
