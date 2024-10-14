using System;
using System.Collections.Generic;
using System.Linq;
using NFluent;
using sly.lexer;
using Xunit;

namespace ParserTests.Issue487;

public class Issue487Tests
        {

            [Fact]
            public void TestIssue487()
            {
                string source = "@dmode1 >> convert: @dmode2";
                var lexBuild = LexerBuilder.BuildLexer<Issue487Token>();
                Check.That(lexBuild).IsOk();
                var lexer = lexBuild.Result;
                var lexed = lexer.Tokenize(source);
                Check.That(lexed).IsOkLexing();
                Check.That(lexed.Tokens.Take(lexed.Tokens.Count-1).Extracting(x => (x.TokenID, x.Value)).Equals(new List<(Issue487Token, string)>()
                {
                    (Issue487Token.START_D, "@"),
                    (Issue487Token.OPS, "dmode1"),
                    (Issue487Token.INSTALL, ">>"),
                    (Issue487Token.CONVERT, "convert"),
                    (Issue487Token.COLON, ":"),
                    (Issue487Token.START_D, "@"),
                    (Issue487Token.OPS, "dmode2")
                }));
            }
}