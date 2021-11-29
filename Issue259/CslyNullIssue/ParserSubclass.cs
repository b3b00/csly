using sly.lexer;
using sly.parser.generator;

namespace CslyNullIssue
{
    public class ParserSubclass : ExpressionParser
    {

        [Operand]
        [Production("numeric_literal: LVAR")]
        public string Lvar(Token<ExpressionToken> token)
        {
            return token.Value;
        }

        [Operand]
        [Production("numeric_literal: SIMVAR")]
        public string SimVarExpression(Token<ExpressionToken> simvarToken)
        {
            var text = simvarToken.Value[2..];
            var bits = text.Split(",");
            var varName = bits[0];
            var type = bits[1].Trim();
            return $"A:{varName}, {type}";
        }

        [Operand]
        [Production("group : LPAREN ParserSubclass_expressions RPAREN")]
        public string Group(Token<ExpressionToken> _1, string child, Token<ExpressionToken> _2)
        {
            return child;
        }

    }
}
