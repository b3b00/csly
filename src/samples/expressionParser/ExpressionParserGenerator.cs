
using sly.sourceGenerator;

namespace expressionparser;

[ParserGenerator(typeof(ExpressionToken), typeof(ExpressionParser), typeof(int))]
public partial class ExpressionParserGenerator : AbstractParserGenerator<ExpressionToken,ExpressionParser,int>
{
    
}