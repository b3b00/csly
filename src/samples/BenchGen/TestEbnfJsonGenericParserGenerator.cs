using benchgen.jsonparser;
using benchgen.jsonparser.JsonModel;
using sly.sourceGenerator.generated;


namespace benchgen;

[CslyParser]
public partial class EbnfJsonGenericParserGenerator : AbstractCslyParser<JsonTokenGeneric,EbnfJsonGenericParser, JSon> {
    public EbnfJsonGenericParserGenerator(EbnfJsonGenericParser instance) : base(instance)
    {
    }

}