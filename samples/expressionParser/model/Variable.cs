using System.Globalization;

namespace expressionparser.model
{
    public sealed class Variable : Expression
    {
        private string VariableName;

        public Variable(string varName)
        {
            VariableName = varName;
        }


        public int? Evaluate(ExpressionContext context)
        {
            return context.GetValue(VariableName);
        }
    }
}