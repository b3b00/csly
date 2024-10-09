
using sly.sourceGenerator;
using sly.sourceGenerator.generated;

namespace expressionparser;

[ParserGenerator]
[CslyParser]
public partial class ExpressionParserGenerator : AbstractCslyParser<ExpressionToken,ExpressionParser,int>
{
    public ExpressionParserGenerator(ExpressionParser instance) : base(instance)
    {
    }
}