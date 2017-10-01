using System.Threading;

namespace expressionparser.model
{
    public interface Expression
    {
        int? Evaluate(ExpressionContext context);
    }
}