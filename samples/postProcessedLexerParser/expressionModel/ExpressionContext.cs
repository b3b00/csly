using System.Collections.Generic;

namespace postProcessedLexerParser.expressionModel
{
    public class ExpressionContext
    {
        private readonly Dictionary<string, double> Variables;

        public ExpressionContext()
        {
            Variables = new Dictionary<string, double>();
        }

        public ExpressionContext(Dictionary<string, double> variables)
        {
            Variables = variables;
        }

        public double? GetValue(string variable)
        {
            if (Variables.ContainsKey(variable)) return Variables[variable];
            return null;
        }
    }
}