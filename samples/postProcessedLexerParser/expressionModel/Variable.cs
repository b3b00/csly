namespace postProcessedLexerParser.expressionModel
{
    public sealed class Variable : Expression
    {
        private readonly string VariableName;

        public Variable(string varName)
        {
            VariableName = varName;
        }


        public double? Evaluate(ExpressionContext context)
        {
            return context.GetValue(VariableName);
        }
    }
}