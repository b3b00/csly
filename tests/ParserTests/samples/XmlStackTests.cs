using NFluent;
using sly.parser;
using sly.parser.generator;
using XML;
using Xunit;

namespace ParserTests.samples;

public class XmlStackTests
{

    [Fact]
    public void TestXmlParserWithLexerModesOk()
    {
        ParserBuilder<MinimalXmlLexer, string> builder = new ParserBuilder<MinimalXmlLexer, string>();
        var xmlparser = new MinimalXmlParser();
        var r = builder.BuildParser(xmlparser, ParserType.EBNF_LL_STACK, "document");
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
        </innerinner>
    </inner>
</root>        
");
        Check.That(parsed).IsOkParsing();
        // var tree = parsed.SyntaxTree;
        // var graphviz = new GraphVizEBNFSyntaxTreeVisitor<MinimalXmlLexer>();
        // var root = graphviz.VisitTree(tree);
        // string graph = graphviz.Graph.Compile();
        // File.Delete("c:\\tmp\\tree.dot");
        // File.AppendAllText("c:\\tmp\\tree.dot", graph);
    }


    [Fact]
    public void TestXmlParserWithLexerModesKo()
    {
        ParserBuilder<MinimalXmlLexer, string> builder = new ParserBuilder<MinimalXmlLexer, string>();
        var xmlparser = new MinimalXmlParser();
        var r = builder.BuildParser(xmlparser, ParserType.EBNF_LL_STACK, "document");
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

    [Fact]
    public void TestXmlLeadingMiscs()
    {
        ParserBuilder<MinimalXmlLexer, string> builder = new ParserBuilder<MinimalXmlLexer, string>();
        var xmlparser = new MinimalXmlParser();
        var r = builder.BuildParser(xmlparser, ParserType.EBNF_LL_STACK, "document");
        Check.That(r).IsOk();
        var parser = r.Result;
        var parsed = parser.Parse(@"
<?xml version=""1.0""?>
<!-- starting doc -->
<root name=""root"">
data
<!-- middle of the doc -->
</root>       
<!-- ending doc -->
");
        Check.That(parsed).IsOkParsing();
        var rr = parsed.Result;
        Check.That(rr).Contains("pi(xml :: version = 1.0)")
            .And.Contains("comment( starting doc )")
            .And.Contains("comment( ending doc )");
        ;
    }

    [Fact]
    public void TestXmlLeadingMiscs2()
    {
        ParserBuilder<MinimalXmlLexer2, string> builder = new ParserBuilder<MinimalXmlLexer2, string>();
        var xmlparser = new MinimalXmlParser2();
        var r = builder.BuildParser(xmlparser, ParserType.EBNF_LL_STACK, "document");
        Check.That(r).IsOk();
        var parser = r.Result;
        var parsed = parser.Parse(@"
<?xml version=""1.0""?>
<!-- starting doc -->
<root name=""root"">
data
<!-- middle of the doc -->
</root>       
<!-- ending doc -->
");
        Check.That(parsed).IsOkParsing();
        var rr = parsed.Result;
        Check.That(rr).Contains("pi(xml :: version = 1.0)")
            .And.Contains("comment( starting doc )")
            .And.Contains("comment( ending doc )");
        ;
    }
}