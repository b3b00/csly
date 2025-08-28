using NFluent;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue574;

public class Issue574Tests
{
    
    
    
    [Fact]
    public void TestIssue574()
    {
        ParserBuilder<Issue574Token,object> builder = new ParserBuilder<Issue574Token,object>();
        var build = builder.BuildParser(new Issue574Parser(), ParserType.EBNF_LL_RECURSIVE_DESCENT, "NTSection");
        Check.That(build).IsOk();
        string source = @"
        collection<[list<[int]>, number]> Transform(copy ref immut readonly frozen number x, 
        int y, frozen readonly 
        immut int z)
        {
            return new collection<[list<[int]>, number]>(x, y: y, z);
            return new int(1);
        }";
        var parsed =  build.Result.Parse(source);
        Check.That(parsed).IsOkParsing();
    }
}