namespace expressionparser.model
{
    public sealed class Variable : Expression
    {
        private readonly string VariableName;

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