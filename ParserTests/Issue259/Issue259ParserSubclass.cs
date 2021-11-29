using sly.lexer;
using sly.parser.generator;

namespace CslyNullIssue
{
    public class Issue259ParserSubclass : Issue259ExpressionParser
    {

        [Operand]
        [Production("numeric_literal: LVAR")]
        public string Lvar(Token<Issue259ExpressionToken> token)
        {
            return token.Value;
        }

        [Operand]
        [Production("numeric_literal: SIMVAR")]
        public string SimVarExpression(Token<Issue259ExpressionToken> simvarToken)
        {
            var text = simvarToken.Value[2..];
            var bits = text.Split(",");
            var varName = bits[0];
            var type = bits[1].Trim();
            return $"A:{varName}, {type}";
        }

        [Operand]
        [Production("group : LPAREN Issue259ParserSubclass_expressions RPAREN")]
        public string Group(Token<Issue259ExpressionToken> _1, string child, Token<Issue259ExpressionToken> _2)
        {
            return child;
        }

    }
}
