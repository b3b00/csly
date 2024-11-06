using NFluent;
using sly.lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ParserTests.Issue499;
public class Issue499Test
{
    [Fact]
    public void LexemeChannelNullable_Test()
    {
        var lexerBuildResult = LexerBuilder.BuildLexer<Issue499Token>();
        Check.That(lexerBuildResult).IsOk();
        var lexer = lexerBuildResult.Result;
        var lexResult = lexer.Tokenize("test\"");
        Check.That(lexResult).IsOkLexing();
        var tokenChannels = lexResult.Tokens;
        Check.That(tokenChannels.GetChannel(1).Count).IsGreaterOrEqualThan(1);
    }
}
