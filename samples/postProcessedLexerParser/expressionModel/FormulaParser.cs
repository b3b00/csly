using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace postProcessedLexerParser.expressionModel
{
    public class FormulaParser
    {
        [Operation((int) FormulaToken.PLUS, Affix.InFix, Associativity.Right, 10)]
        [Operation((int) FormulaToken.MINUS, Affix.InFix, Associativity.Left, 10)]
        public Expression BinaryTermExpression(Expression left, Token<FormulaToken> operation, Expression right)
        {
            return new BinaryOperation(left, operation.TokenID, right);
        }


        [Operation((int) FormulaToken.TIMES, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) FormulaToken.DIVIDE, Affix.InFix, Associativity.Left, 50)]
        public Expression BinaryFactorExpression(Expression left, Token<FormulaToken> operation, Expression right)
        {
            return new BinaryOperation(left, operation.TokenID, right);
        }


        [Operation((int) FormulaToken.MINUS, Affix.PreFix, Associativity.Right, 100)]
        public Expression PreFixExpression(Token<FormulaToken> operation, Expression value)
        {
            return new UnaryOperation(FormulaToken.MINUS,value);
        }
        
        [Operation((int) FormulaToken.EXP, Affix.InFix, Associativity.Left, 90)]
        public Expression ExpExpression(Expression left, Token<FormulaToken> operation, Expression right)
        {
            return new BinaryOperation(left, operation.TokenID, right);
        }

        [Operand]
        [Production("operand : primary_value")]
        public Expression OperandValue(Expression value)
        {
            return value;
        }

        
        [Production("primary_value : IDENTIFIER")]
        public Expression OperandVariable(Token<FormulaToken> identifier)
        {
            return new Variable(identifier.Value);
        }

        [Production("primary_value : DOUBLE")]
        [Production("primary_value : INT")]
        public Expression OperandInt(Token<FormulaToken> value)
        {
            return new Number(value.DoubleValue);
        }

        [Production("primary_value : LPAREN FormulaParser_expressions RPAREN")]
        public Expression OperandParens(Token<FormulaToken> lparen, Expression value, Token<FormulaToken> rparen)
        {
            return value;
        }

        [Production(
            "primary_value : IDENTIFIER LPAREN[d] FormulaParser_expressions (COMMA FormulaParser_expressions)* RPAREN[d]")]
        public Expression FunctionCall(Token<FormulaToken> funcName, Expression first,
            List<Group<FormulaToken, Expression>> others)
        {
            List<Expression> parameters = new List<Expression>();
            parameters.Add(first);
            parameters.AddRange(others.Select(x => x.Value(0)));
            return new FunctionCall(funcName.Value, parameters);
        }
    }
}