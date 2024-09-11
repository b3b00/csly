using jsonparser.JsonModel;
using sly.sourceGenerator;

namespace jsonparser;

[ParserGenerator(typeof(JsonToken), typeof(EbnfJsonParser), typeof(JSon))]
public partial class EbnfJsonParserGenerator : AbstractParserGenerator<JsonToken>
{
        
}