using System.Collections.Generic;

namespace SimpleTemplate.model.expressions
{
    public class BoolConstant : Expression
    {
        public BoolConstant(bool value)
        {
            Value = value;
        }

        public bool Value { get; set; }


        public string Dump(string tab)
        {
            return $"{tab}(BOOL {Value})";
        }

        public string GetValue(Dictionary<string, object> context)
        {
            return Value ? "1" : "0";
        }

        public object Evaluate(Dictionary<string, object> context)
        {
            return Value;
        }
    }
}