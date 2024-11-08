using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NFluent;
using sly.lexer;
using sly.parser;
using Xunit;

namespace ParserTests.Issue501;

public class Issue501Tests
{
    [Fact]
    public void TestIssue501()
    {
        var build = LexerBuilder.BuildLexer<Issue501Token>();
        Check.That(build).IsOk();
        Check.That(build.Result).IsNotNull();
        Check.That(build.Result).IsInstanceOf<GenericLexer<Issue501Token>>();
        var lexer = build.Result as GenericLexer<Issue501Token>;
        foreach (var subLexer in lexer.GetSubLexers())
        {
            var graph = lexer.ToGraphViz(subLexer);
            File.WriteAllText(Path.Combine("c:/tmp",$"{subLexer}.txt"),graph);
        }
        var source = @"test = 3";
        var lexed = lexer.Tokenize(source);
        Check.That(lexed).IsOkLexing();
        // Check.That(lexed.Error.ErrorType).IsEqualTo(ErrorType.UnexpectedChar);
        // Check.That(lexed.Error.UnexpectedChar).IsEqualTo('=');
        var tokens = lexed.Tokens;
        Check.That(tokens).IsNotNull();
        var mainTokens = tokens.MainTokens();
        Check.That(mainTokens).CountIs(4);
        Check.That(mainTokens.Last().IsEOS).IsTrue();
        Check.That(mainTokens.Take(3).Select(x => x.TokenID)).IsEquivalentTo(new List<Issue501Token>()
            { Issue501Token.Identifier, Issue501Token.Assign, Issue501Token.Number });
        
    }
}