using jsonparser.JsonModel;
using sly.sourceGenerator;

namespace jsonparser;

[ParserGenerator]
public partial class EbnfJsonGenericParserGenerator: AbstractParserGenerator<JsonTokenGeneric, EbnfJsonGenericParser, JSon>
{
        
}