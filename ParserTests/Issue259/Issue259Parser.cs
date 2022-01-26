using System.Globalization;
using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue259
{
    public class Issue259Parser 
    {
        [Operand]
        [Production("logical_literal: OFF")]
        [Production("logical_literal: ON")]
        public string LiteralBool(Token<Issue259ExpressionToken> token)
        {
            return token.Value;
        }

        [Operand]
        [Production("primary: HEX_NUMBER")]
        public string NumericExpressionFromLiteralNumber(Token<Issue259ExpressionToken> offsetToken)
        {
            return offsetToken.Value;
        }

        [Operand]
        [Production("primary: DECIMAL_NUMBER")]
        public string NumericExpressionFromDecimalNumber(Token<Issue259ExpressionToken> offsetToken)
        {
            var text = offsetToken.Value;
            var value = double.Parse(text, CultureInfo.InvariantCulture);
            return value.ToString(CultureInfo.InvariantCulture);
        }

        [Infix((int)Issue259ExpressionToken.PLUS, Associativity.Left, 14)]
        [Infix((int)Issue259ExpressionToken.MINUS, Associativity.Left, 14)]
        [Infix((int)Issue259ExpressionToken.TIMES, Associativity.Left, 15)]
        [Infix((int)Issue259ExpressionToken.DIVIDE, Associativity.Left, 15)]
        [Infix((int)Issue259ExpressionToken.BITWISE_AND, Associativity.Left, 10)]
        [Infix((int)Issue259ExpressionToken.BITWISE_OR, Associativity.Left, 8)]
        public string NumberExpression(string lhs, Token<Issue259ExpressionToken> token, string rhs)
        {
            return $"({lhs} {token.Value} {rhs})";
        }

        [Infix((int)Issue259ExpressionToken.LOGICAL_AND, Associativity.Left, 7)]
        [Infix((int)Issue259ExpressionToken.LOGICAL_OR, Associativity.Left, 6)]
        public string LogicalExpression(string lhs, Token<Issue259ExpressionToken> token, string rhs)
        {
            return $"({lhs} {token.Value} {rhs})";
        }

        [Prefix((int)Issue259ExpressionToken.MINUS, Associativity.Right, 17)]
        public string NumericExpression(Token<Issue259ExpressionToken> _, string child)
        {
            return $"-{child}";
        }

        // We want NOT to to bind tighter than AND/OR but looser than numeric comparison operations
        [Prefix((int)Issue259ExpressionToken.NOT, Associativity.Right, 11)]
        public string LogicalExpression(Token<Issue259ExpressionToken> _, string child)
        {
            return $"(NOT {child})";
        }

        [Infix((int)Issue259ExpressionToken.COMPARISON, Associativity.Left, 12)]
        public string Comparison(string lhs, Token<Issue259ExpressionToken> token, string rhs)
        {
            return $"({lhs} {token.Value} {rhs})";
        }
        

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
        [Production("group : LPAREN Issue259Parser_expressions RPAREN")]
        public string Group(Token<Issue259ExpressionToken> _1, string child, Token<Issue259ExpressionToken> _2)
        {
            return child;
        }

    }
}
