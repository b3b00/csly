using System;
using System.Linq;
using NFluent;
using sly.buildresult;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue414;

public class Issue414Tests
{
    [Fact]
    public void Issue414Test()
    {
        var parserInstance = new Issue414Parser();
        var builder = new ParserBuilder<Issue414Token, string>();
        var buildResult = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "block");//line-based, 1 statement per line.
        Check.That(buildResult).IsOk();
        var parser = buildResult.Result;
        string source = "funcA(funcC(B==2));";
        ParseResult<Issue414Token, string> result = null;
        Check.ThatCode(() =>
        {
            result = parser.Parse(source);
        }).LastsLessThan(10_000, TimeUnit.Milliseconds);
        Check.That(result).IsOkParsing();
    }
    
    [Fact]
    public void Issue414ExpressionTest()
    {
        var parserInstance = new Issue414ExpressionParser();
        var builder = new ParserBuilder<Issue414Token, string>();

        BuildResult<Parser<Issue414Token, string>> buildResult = null;
        buildResult = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "block");//line-based, 1 statement per line.
        
        Check.That(buildResult).IsOk();
        var parser = buildResult.Result;
        string source = "funcA(funcC(B==2));";
        ParseResult<Issue414Token, string> result = null;
        Check.ThatCode(() =>
        {
            result = parser.Parse(source);
        }).LastsLessThan(10_000, TimeUnit.Milliseconds);
        Check.That(result).IsOkParsing();
    }
    
    [Fact]
    public void Issue414AltTest()
    {
        var parserInstance = new Issue414AltParser();
        var builder = new ParserBuilder<Issue414Token, string>();

        BuildResult<Parser<Issue414Token, string>> buildResult = null;
        buildResult = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "block");//line-based, 1 statement per line.
        
        Check.That(buildResult).IsOk();
        var parser = buildResult.Result;
        string source = "funcA(funcC(B==2));";
        ParseResult<Issue414Token, string> result = null;
        Check.ThatCode(() =>
        {
            result = parser.Parse(source);
        }).LastsLessThan(10_000, TimeUnit.Milliseconds);
        Check.That(result).IsOkParsing();
    }
}