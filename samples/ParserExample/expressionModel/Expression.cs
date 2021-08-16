using expressionparser.model;

namespace ParserExample.expressionModel
{
    public interface Expression
    {
        double? Evaluate(ExpressionContext context);
    }
}