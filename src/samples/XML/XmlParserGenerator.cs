using sly.sourceGenerator;

namespace XML;

[ParserGenerator(typeof(MinimalXmlLexer), typeof(MinimalXmlParser), typeof(string))]
public partial class XmlParserGenerator : AbstractParserGenerator<MinimalXmlLexer>
{
    
}