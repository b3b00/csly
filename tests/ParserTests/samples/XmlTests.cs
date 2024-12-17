using NFluent;
using sly.parser.generator;
using XML;
using Xunit;

namespace ParserTests.samples;

public class XmlTests
{

    [Fact]
    public void TestXmlParserWithLexerModes()
    {
        ParserBuilder<MinimalXmlLexer, string> builder = new ParserBuilder<MinimalXmlLexer, string>();
        var xmlparser = new MinimalXmlParser();
        var r = builder.BuildParser(xmlparser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "document");
        Check.That(r.IsError).IsFalse();
         var parser = r.Result;
         var parsed = parser.Parse(@"
<?xml version=""1.0""?>
<!-- starting doc -->
<root name=""root"">
    <autoInner name=""autoinner1""/>
    <inner name=""inner"">
         <?PI name=""pi""?> 
        <innerinner name=""innerinner"">
            inner inner content        
");
         Check.That(parsed).Not.IsOkParsing();
         var error = parsed.Errors[0];
         Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedEOS);
    }
}