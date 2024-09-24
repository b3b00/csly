using handExpressions;
using handExpressions.sourceGenerator;
using sly.lexer;
using testgen.JsonModel;

namespace testgen;

public partial class GeneratedEbnfJsonGenericParser : BaseParser<JsonTokenGeneric, JSon>
{
    
}

[CslyParser]
public partial class EbnfJsonGenericParserGenerator : AbstractCslyParser<JsonTokenGeneric,EbnfJsonGenericParser, JSon> {
    public EbnfJsonGenericParserGenerator(EbnfJsonGenericParser instance) : base(instance)
    {
    }

}