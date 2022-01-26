
namespace postProcessedLexerParser.expressionModel
{
    public interface Expression
    {
        double? Evaluate(ExpressionContext context);
    }
}