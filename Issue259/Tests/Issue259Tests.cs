using CslyNullIssue;
using System;
using Xunit;

namespace Tests
{
    public class Issue259Tests
    {
        [Theory]
        [InlineData("ON", "ON")]
        [InlineData("ON AND OFF", "(ON AND OFF)")]
        [InlineData("ON OR OFF", "(ON OR OFF)")]
        [InlineData("OFF", "OFF")]
        [InlineData("A:FOO, bool < 42", "(A:FOO, bool < 42)")]
        [InlineData("L:BAR == 1", "(L:BAR == 1)")]
        [InlineData("3 < 4", "(3 < 4)")]
        [InlineData("(1 + 3) < (3 + 4)", "((1 + 3) < (3 + 4))")]
        [InlineData("3 < 4 OR 4 < 5", "((3 < 4) OR (4 < 5))")]
        [InlineData("(2 + 3) < (A:FOO,bool + 12)", "((2 + 3) < (A:FOO, bool + 12))")]
        [InlineData("(2 + 3) <= (A:FOO,bool + 12)", "((2 + 3) <= (A:FOO, bool + 12))")]
        [InlineData("(2 + 3) >= (A:FOO,bool + 12)", "((2 + 3) >= (A:FOO, bool + 12))")]
        [InlineData("(2 + 3) > (A:FOO,bool + 12)", "((2 + 3) > (A:FOO, bool + 12))")]
        [InlineData("3 == 4", "(3 == 4)")]
        [InlineData("3 != 4", "(3 != 4)")]
        [InlineData("1 + 2 < 3 + 4", "((1 + 2) < (3 + 4))")]
        [InlineData("1 + 2 * 3 < 3 * 4 - 5", "((1 + (2 * 3)) < ((3 * 4) - 5))")]
        [InlineData("1<2 AND 2>3", "((1 < 2) AND (2 > 3))")]

        // Comparison operators
        [InlineData("1 < 2 && 1 <= 2 && 1 == 2 && 1 >= 2 && 1 > 2 && 1 != 2 && 1 <> 2", "(((((((1 < 2) AND (1 <= 2)) AND (1 == 2)) AND (1 >= 2)) AND (1 > 2)) AND (1 != 2)) AND (1 != 2))")]

        // AND/OR precedence. (AND binds tighter)
        [InlineData("ON AND OFF OR ON AND OFF", "((ON AND OFF) OR (ON AND OFF))")]
        [InlineData("ON AND (OFF OR ON) AND OFF", "((ON AND (OFF OR ON)) AND OFF)")]
        [InlineData("1==1 || 2==2 && 3==3 || 4==4", "(((1 == 1) OR ((2 == 2) AND (3 == 3))) OR (4 == 4))")]

        // Alternate AND/OR
        [InlineData("1==1 AND 2==2 && 3==3 OR 4==4 || 5==5", "(((((1 == 1) AND (2 == 2)) AND (3 == 3)) OR (4 == 4)) OR (5 == 5))")]

        // NOT checks. (NOT binds tightly to logicals and loosely to numerics
        [InlineData("NOT ON", "(NOT ON)")]
        [InlineData("NOT ON AND OFF", "((NOT ON) AND OFF)")]
        [InlineData("NOT ON OR OFF", "((NOT ON) OR OFF)")]
        [InlineData("NOT 1 == 2", "(NOT (1 == 2))")]
        [InlineData("NOT ON AND NOT OFF OR NOT ON OR NOT OFF", "((((NOT ON) AND (NOT OFF)) OR (NOT ON)) OR (NOT OFF))")]

        // Unary minus checks
        [InlineData("- A:FOO, bool > 3", "(-A:FOO, bool > 3)")]
        [InlineData("-(A:FOO, bool) > 3", "(-A:FOO, bool > 3)")]
        public void BooleanParser(string expression, string expected)
        {
            var result = ExpressionParser.Parse<ParserSubclass>(expression);
            Assert.Equal(expected, result);
        }
    }
}
