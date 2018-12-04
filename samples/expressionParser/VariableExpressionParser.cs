using expressionparser.model;
using sly.lexer;
using sly.parser.generator;

namespace expressionparser
{
    public class VariableExpressionParser
    {
        [Production("primary: INT")]
        public Expression PrimaryNumber(Token<ExpressionToken> intToken)
        {
            return new Number(intToken.IntValue);
        }

        [Production("primary: IDENTIFIER")]
        public Expression PrimaryIdentifier(Token<ExpressionToken> idToken)
        {
            return new Variable(idToken.StringWithoutQuotes);
        }

        [Production("primary: LPAREN expression RPAREN")]
        public Expression Group(object discaredLParen, Expression groupValue, object discardedRParen)
        {
            return new Group(groupValue);
        }


        [Production("expression : term PLUS expression")]
        [Production("expression : term MINUS expression")]
        public Expression Expression(Expression left, Token<ExpressionToken> operatorToken, Expression right)
        {
            return new BinaryOperation(left, operatorToken.TokenID, right);
        }

        [Production("expression : term")]
        public Expression Expression_Term(Expression termValue)
        {
            return termValue;
        }

        [Production("term : factor TIMES term")]
        [Production("term : factor DIVIDE term")]
        public Expression Term(Expression left, Token<ExpressionToken> operatorToken, Expression right)
        {
            return new BinaryOperation(left, operatorToken.TokenID, right);
        }

        [Production("term : factor")]
        public Expression Term_Factor(Expression factorValue)
        {
            return factorValue;
        }

        [Production("factor : primary")]
        public Expression primaryFactor(Expression primValue)
        {
            return primValue;
        }

        [Production("factor : MINUS factor")]
        public Expression Factor(Token<ExpressionToken> minus, Expression factorValue)
        {
            return new UnaryOperation(minus.TokenID, factorValue);
        }
    }
}