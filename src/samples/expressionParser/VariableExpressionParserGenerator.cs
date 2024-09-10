using expressionparser.model;
using sly.sourceGenerator;

namespace expressionparser;

[ParserGenerator(typeof(ExpressionToken), typeof(VariableExpressionParser), typeof(Expression))]
public partial class VariableExpressionParserGenerator
{
    
}