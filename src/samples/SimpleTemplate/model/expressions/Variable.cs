using System;
using System.Collections.Generic;
using sly.lexer;

namespace SimpleTemplate.model.expressions
{
    public class Variable : Expression
    {
        public Variable(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public LexerPosition Position { get; set; }


        public string GetValue(Dictionary<string, object> context) => Evaluate(context).ToString();

        public object Evaluate(Dictionary<string, object> context)
        {
            object value = "";
            if (context.TryGetValue(Name, out value))
            {
                return value;
            }
            throw new SystemException($"variable {Name} not found @ {Position}");
        }
    }
}