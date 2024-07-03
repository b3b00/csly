using System;
using System.IO;
using expressionparser;
using NFluent;
using sly.parser.generator;
using sly.parser.generator.visitor;
using sly.parser.syntax.tree;
using Xunit;

namespace ParserTests;

public class NodeNamesTests
{
    [Fact]
    public void TestBNFNodeNames()
    {
        var parserInstance = new ExpressionParser();
        var builder = new ParserBuilder<ExpressionToken, int>();
        var parser = builder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, "expression").Result;
        var r = parser.Parse("1+1");
        Check.That(r).IsOkParsing();
        Check.That(r.SyntaxTree).IsNotNull();
        Check.That(r.SyntaxTree.Name).IsEqualTo("addOrSubstract");
        Check.That(r.SyntaxTree).IsInstanceOf<SyntaxNode<ExpressionToken>>();
        var root = r.SyntaxTree as SyntaxNode<ExpressionToken>;
        Check.That(r).IsNotNull();
        Check.That(root.Children).CountIs(3);
        Check.That(root.Children[0].Name).IsEqualTo("term");
        Check.That(root.Children[2].Name).IsEqualTo("expression");
    }
    
    [Fact]
    public void TestEBNFNodeNames()
    {
        var parserInstance = new ExpressionParser();
        var builder = new ParserBuilder<ExpressionToken, int>();
        var parser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "expression").Result;
        var r = parser.Parse("1+1");
        Check.That(r).IsOkParsing();
        Check.That(r.SyntaxTree).IsNotNull();
        Check.That(r.SyntaxTree.Name).IsEqualTo("addOrSubstract");
        Check.That(r.SyntaxTree).IsInstanceOf<SyntaxNode<ExpressionToken>>();
        var root = r.SyntaxTree as SyntaxNode<ExpressionToken>;
        Check.That(r).IsNotNull();
        Check.That(root.Children).CountIs(3);
        Check.That(root.Children[0].Name).IsEqualTo("term");
        Check.That(root.Children[2].Name).IsEqualTo("expression");
    }
    
    
}