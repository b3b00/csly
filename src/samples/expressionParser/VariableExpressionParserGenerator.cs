using sly.sourceGenerator;

namespace expressionparser;

[ParserGenerator(typeof(ExpressionToken), typeof(VariableExpressionParser), typeof(int))]
public partial class VariableExpressionParserGenerator
{
    
}