using System.Collections.Generic;
using NFluent;
using RelaxedVisitorTyping;
using sly.parser.generator;
using Xunit;

namespace ParserTests.relaxed;

public class RelaxedVisitorTypingTests
{
    
    [Fact]
    public void BnfRelaxedVisitorTest()
    {
        var parserInstance = new RelaxedExpressionParser();

        var builder = new ParserBuilder<RelaxedExpressionToken, Clause>();
        var buildResult = builder.BuildRelaxedParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, "compare");
        Check.That(buildResult).IsOk();
        var parser = buildResult.Result;
        var parseResult = parser.Parse("abcd.def -eq 12");
        Check.That(parseResult).IsOkParsing();
        Check.That(parseResult.Result.ToString()).Equals("abcd.def == 12");
    }
    
    [Fact]
    public void EbnfManyRelaxedVisitorTest()
    {
        var parserInstance = new EbnfManyRelaxedExpressionParser();

        var builder = new ParserBuilder<RelaxedExpressionToken, List<int>>();
        var buildResult = builder.BuildRelaxedParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "many");
        Check.That(buildResult).IsOk();
        var parser = buildResult.Result;
        var parseResult = parser.Parse("1 2 3 4");
        Check.That(parseResult).IsOkParsing();
        Check.That(parseResult.Result).CountIs(4);
        Check.That(parseResult.Result).Contains(new List<int>() { 1, 2, 3, 4 });
    }
    
    [Fact]
    public void EbnfOptionRelaxedVisitorTest()
    {
        var parserInstance = new EbnfOptionRelaxedExpressionParser();

        var builder = new ParserBuilder<RelaxedExpressionToken, string>();
        var buildResult = builder.BuildRelaxedParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "option");
        Check.That(buildResult).IsOk();
        var parser = buildResult.Result;
        var parseResult = parser.Parse("1 2");
        Check.That(parseResult).IsOkParsing();
        Check.That(parseResult.Result).Not.IsNullOrEmpty();
        Check.That(parseResult.Result).IsEqualTo("1-2");
        parseResult = parser.Parse("1 ");
        Check.That(parseResult).IsOkParsing();
        Check.That(parseResult.Result).Not.IsNullOrEmpty();
        Check.That(parseResult.Result).IsEqualTo("1-NONE");
    }

    [Fact]
    public void EbnfGroupRelaxedVisitorTest()
    {
        var parserInstance = new EbnfGroupRelaxedExpressionParser();

        var builder = new ParserBuilder<RelaxedExpressionToken, string>();
        var buildResult = builder.BuildRelaxedParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "group");
        Check.That(buildResult).IsOk();
        var parser = buildResult.Result;
        var parseResult = parser.Parse("1 Prop 2");
        Check.That(parseResult).IsOkParsing();
        var result = parseResult.Result;
        Check.That(result).Not.IsNullOrEmpty();
        Check.That(result).IsEqualTo("1 Prop=2");
    }
}