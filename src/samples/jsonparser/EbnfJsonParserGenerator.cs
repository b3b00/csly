using jsonparser.JsonModel;
using sly.sourceGenerator;

namespace jsonparser;

[ParserGenerator]
public partial class EbnfJsonParserGenerator : AbstractParserGenerator<JsonToken, EbnfJsonParser, JSon>
{
        
}