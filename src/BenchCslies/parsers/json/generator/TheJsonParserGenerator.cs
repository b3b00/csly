using BenchCslies.parsers.json.JsonModel;
using csly.ebnf.models;

namespace BenchCslies.parsers.json.generator
{
    
    [ParserGenerator]
    public partial class TheJsonParserGenerator : AbstractParserGenerator<GeneratedCslyJsonTokenGeneric, GeneratedEbnfJsonGenericParser, JSon>{
    
    }
}