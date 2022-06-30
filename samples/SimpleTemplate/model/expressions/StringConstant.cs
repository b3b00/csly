using System;
using System.Collections.Generic;
using sly.lexer;

namespace SimpleTemplate.model.expressions
{
    public class StringConstant : Expression
    {
        public StringConstant(string value, LexerPosition position)
        {
            Value = value;
            Position = position;
        }

        public string Value { get; set; }


        public LexerPosition Position { get; set; }


        public string GetValue(Dictionary<string, object> context)
        {
            return Value;
        }

        public object Evaluate(Dictionary<string, object> context)
        {
            return Value;
        }
    }
}