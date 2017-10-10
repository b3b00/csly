using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.model
{
    public class StringConstant : Expression
    {

        public string Value { get; set; }

        public StringConstant(string value)
        {
            Value = value;
        }

        public string Dump(string tab)
        {
            return $"{tab}(STRING {Value})";
        }
    }
}
