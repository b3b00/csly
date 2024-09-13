using sly.sourceGenerator;

namespace ExplicitTokens;


[ParserGenerator(typeof(ExplicitTokensTokens), typeof(ExplicitTokensExpressionParser),typeof(double))]
public partial class ExplicitTokensExpressionParserGenerator : AbstractParserGenerator<ExplicitTokensTokens,ExplicitTokensExpressionParser, double>
{
        
}