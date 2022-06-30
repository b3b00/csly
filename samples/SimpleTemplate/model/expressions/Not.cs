using System;
using System.Collections.Generic;
using sly.lexer;

namespace SimpleTemplate.model.expressions
{
    public class Not : Expression
    {
        public Not(Expression value, LexerPosition position)
        {
            Value = value;
            Position = position;
        }

        public Expression Value { get; set; }

        public LexerPosition Position { get; set; }
        public string GetValue(Dictionary<string, object> context)
        {
            return (bool)(Evaluate(context)) ? "1" : "0";
        }

        public object Evaluate(Dictionary<string, object> context)
        {
            object value = Value.GetValue(context);
            if (value is bool b)
            {
                return !b;
            }

            return false;
        }
    }
}