using Common.Tokens;
using NFluent;
using ParserTests;
using sly.parser.generator;
using Xunit;

namespace issue576;

public class Issue576Test
{
    [Fact]
    public void TestIssue576()
    {
        ParserBuilder<SmallLangToken, int> builder = new ParserBuilder<SmallLangToken, int>("en");
        var buildResult = builder.BuildParser(new Issue576Parser(),ParserType.EBNF_LL_RECURSIVE_DESCENT,"NTSection");
        Check.That(buildResult).IsOk();
        var parser = buildResult.Result;
        var parsed = parser.Parse(@"immut int i = 12;
        
        i as index;
        
        ((((i!)!)!)!)!;
        
        frozen int j = (i as number) + 1;
        j as rational rationalJ;
        
        dict<[int, string]> converter = new dict<[int, string]>(1, ""1"", 2, ""2"");
        
        SOut(converter[j]);");
        Check.That(parsed).IsOkParsing();
    }
}