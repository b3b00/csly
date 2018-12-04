using System.Collections.Generic;

namespace expressionparser.model
{
    public class ExpressionContext
    {
        private readonly Dictionary<string, int> Variables;

        public ExpressionContext()
        {
            Variables = new Dictionary<string, int>();
        }

        public ExpressionContext(Dictionary<string, int> variables)
        {
            Variables = variables;
        }

        public int? GetValue(string variable)
        {
            if (Variables.ContainsKey(variable)) return Variables[variable];
            return null;
        }
    }
}