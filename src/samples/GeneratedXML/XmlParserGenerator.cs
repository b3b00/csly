using sly.sourceGenerator;

namespace GeneratedXML;

[ParserGenerator(typeof(GeneratedMinimalXmlLexer), typeof(GeneratedMinimalXmlParser), typeof(string))]
public partial class GeneratedXmlParserGenerator : AbstractParserGenerator<GeneratedMinimalXmlLexer,GeneratedMinimalXmlParser, string>
{
    
}