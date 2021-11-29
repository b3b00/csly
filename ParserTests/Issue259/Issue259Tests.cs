using System;
using Xunit;

namespace CslyNullIssue
{
    public class Issue259Tests
    {
        [Fact]
        public static void Issue259Test()
        {
            var expression = "1 < 2 && 1 <= 2 && 1 == 2 && 1 >= 2 && 1 > 2 && 1 != 2 && 1 <> 2";
            var exception = Assert.Throws<Exception>(() =>  Issue259ExpressionParser.Parse<Issue259ParserSubclass>(expression));
            Assert.True(exception.Message.Contains(@"COMPARISON [<] @line 1, column 79"));
        }
    }
}