using sly.sourceGenerator;

namespace ExplicitTokens;

[ParserGenerator(typeof(ExplicitTokensTokens), typeof(ExplicitTokensParser), typeof(double))]
public partial class ExplicitTokensParserGenerator : AbstractParserGenerator<ExplicitTokensTokens,ExplicitTokensParser,double>
{
    
}