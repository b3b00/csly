namespace expressionparser.model
{
    public interface Expression
    {
        int? Evaluate(ExpressionContext context);
    }
}