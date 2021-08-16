namespace ParserExample.expressionModel
{
    public sealed class Variable : ParserExample.expressionModel.Expression
    {
        private readonly string VariableName;

        public Variable(string varName)
        {
            VariableName = varName;
        }


        public double? Evaluate(ParserExample.expressionModel.ExpressionContext context)
        {
            return context.GetValue(VariableName);
        }
    }
}