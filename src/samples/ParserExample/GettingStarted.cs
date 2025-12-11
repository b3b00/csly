using sly.lexer;
using sly.parser.generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserExample
{
    internal enum GettingStartedLexer
    {
        [Lexeme("[0-9]+")]
        INT = 1,

        [Lexeme("\\+")]
        PLUS = 2,

        [Lexeme("[ \\t]+", isSkippable: true)] // the lexeme is marked isSkippable : it will not be sent to the parser and simply discarded.
        WS = 3
    }

    internal class GettingStartedParser
    {

        [Production("expression: INT")]
        public int intExpr(Token<GettingStartedLexer> intToken)
        {
            return intToken.IntValue;
        }

        [Production("expression: term PLUS expression")]
        public int Expression(int left, Token<GettingStartedLexer> operatorToken, int right)
        {
            return left + right;
        }

        [Production("term: INT")]
        public int Expression(Token<GettingStartedLexer> intToken)
        {
            return intToken.IntValue;
        }
    }
}
