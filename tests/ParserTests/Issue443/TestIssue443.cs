using System;
using System.Collections.Generic;
using indented;
using NFluent;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue443;

public interface INode
{
}

public class Item : INode
{
    public string NewLine { get; set; }
    public Item(string newLine)
    {
        NewLine = newLine;
    }
}

public class TemplateNode : INode
{
    public List<INode> Items { get; set; }
    public TemplateNode(List<INode> items)
    {
        Items = items;
    }
}

public class TestIssue443
{

    [Fact]
    public void Issue443Test()
    {
        var issue443Parser = new Issue443Parser();
        var builder = new ParserBuilder<Issue443Lexer, INode>();

        var parser = builder.BuildParser(issue443Parser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "template");

        Check.That(parser).IsOk();
        var result = parser.Result.Parse(@"
");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsInstanceOf<TemplateNode>();
        var template = result.Result as TemplateNode;
        Check.That(template).IsNotNull();
        Check.That(template.Items).CountIs(1);
        
        result = parser.Result.Parse("");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsInstanceOf<TemplateNode>();
        template = result.Result as TemplateNode;
        Check.That(template).IsNotNull();
        Check.That(template.Items).IsEmpty();
    }
}