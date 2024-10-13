using NFluent;
using sly.lexer;
using Xunit;

namespace ParserTests.Issue487;

public class Issue487Tests
        {

            [Fact]
            public void TestIssue487()
            {
                string source = "@g_capcalc >> EtnRef: @appcapcalc.bankcapconfigid";
                var lexBuild = LexerBuilder.BuildLexer<Issue190Token>();
                Check.That(lexBuild).IsOk();
                var lexer = lexBuild.Result;
                var lexed = lexer.Tokenize(source);
                Check.That(lexed).IsOkLexing();
            }
}