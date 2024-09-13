using expressionparser.model;
using sly.sourceGenerator;

namespace expressionparser;

[ParserGenerator]
public partial class VariableExpressionParserGenerator : AbstractParserGenerator<ExpressionToken,VariableExpressionParser,Expression>
{
    
}