using jsonparser.JsonModel;
using sly.sourceGenerator;

namespace jsonparser;

[ParserGenerator(typeof(JsonTokenGeneric), typeof(EbnfJsonGenericParser), typeof(JSon))]
public partial class EbnfJsonGenericParserGenerator
{
        
}