using sly.sourceGenerator;

namespace XML;

[ParserGenerator(typeof(GeneratedMinimalXmlLexer), typeof(GeneratedMinimalXmlParser), typeof(string))]
public partial class XmlParserGenerator : AbstractParserGenerator<GeneratedMinimalXmlLexer>
{
    
}