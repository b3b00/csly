using System.Collections.Generic;
using sly.lexer;

namespace SimpleTemplate.model.expressions
{
    public class Neg : Expression
    {
        public Neg(Expression value, LexerPosition position)
        {
            Value = value;
            Position = position;
        }

        public Expression Value { get; set; }

        public LexerPosition Position { get; set; }

        
        

        public string GetValue(Dictionary<string, object> context) => Evaluate(context).ToString();

        public object Evaluate(Dictionary<string, object> context)
        {
            object value = Value.GetValue(context);
            if (value is int i)
            {
                return -i;
            }

            return 0;
        }
    }
}