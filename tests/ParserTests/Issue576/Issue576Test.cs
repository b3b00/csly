
using NFluent;
using ParserTests;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace issue576;

public class Issue576Test
{
    [Theory]
    [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
    // [InlineData(ParserType.EBNF_LL_STACK)]
    public void TestIssue576(ParserType parserType)
    {
        ParserBuilder<Issue576Lexer, int> builder = new ParserBuilder<Issue576Lexer, int>("en");
        var buildResult = builder.BuildParser(new Issue576Parser(),parserType,"NTSection");
        Check.That(buildResult).IsOk();
        var parser = buildResult.Result;
        var source = @"immut int i = 12;
        
        i as index;
        
        ((((i!)!)!)!)!;
        
        frozen int j = (i as number) + 1;
        j as rational rationalJ;
        
        dict<[int, string]> converter = new dict<[int, string]>(1, ""1"", 2, ""2"");
        
        SOut(converter[j]);";
        var parsed = parser.Parse(source);
        Check.That(parsed).IsOkParsing();

    }
    
   
}