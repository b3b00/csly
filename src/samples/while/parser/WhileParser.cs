using System.Collections.Generic;
using System.Linq;
using csly.whileLang.model;
using sly.lexer;
using sly.parser.generator;

namespace csly.whileLang.parser
{
    [ParserRoot("statement")]
    public class WhileParser
    {
        #region COMPARISON OPERATIONS

        [Operation((int) WhileToken.LESSER, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) WhileToken.GREATER, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) WhileToken.EQUALS, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) WhileToken.DIFFERENT, Affix.InFix, Associativity.Right, 50)]
        public WhileAST binaryComparisonExpression(WhileAST left, Token<WhileToken> operatorToken, WhileAST right)
        {
            var oper = BinaryOperator.ADD;

            switch (operatorToken.TokenID)
            {
                case WhileToken.LESSER:
                {
                    oper = BinaryOperator.LESSER;
                    break;
                }
                case WhileToken.GREATER:
                {
                    oper = BinaryOperator.GREATER;
                    break;
                }
                case WhileToken.EQUALS:
                {
                    oper = BinaryOperator.EQUALS;
                    break;
                }
                case WhileToken.DIFFERENT:
                {
                    oper = BinaryOperator.DIFFERENT;
                    break;
                }
            }

            var operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        #endregion

        #region STRING OPERATIONS

        [Operation((int) WhileToken.CONCAT, Affix.InFix, Associativity.Right, 10)]
        public WhileAST binaryStringExpression(WhileAST left, Token<WhileToken> operatorToken, WhileAST right)
        {
            var oper = BinaryOperator.CONCAT;
            var operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        #endregion

        #region statements

        [Production("statement :  LPAREN [d] statement RPAREN [d]")]
        public WhileAST block(Statement statement)
        {
            return statement;
        }

        [Production("statement : sequence")]
        public WhileAST statementSequence(WhileAST sequence)
        {
            return sequence;
        }

        [Production("sequence : statementPrim additionalStatements*")]
        public WhileAST sequenceStatements(WhileAST first, List<WhileAST> next)
        {
            var seq = new SequenceStatement(first as Statement);
            seq.AddRange(next.Cast<Statement>().ToList());
            return seq;
        }

        [Production("additionalStatements : SEMICOLON [d] statementPrim")]
        public WhileAST additional(WhileAST statement)
        {
            return statement;
        }

        [Production("statementPrim: IF [d] WhileParser_expressions THEN [d] statement ELSE [d] statement")]
        public WhileAST ifStmt(WhileAST cond, WhileAST thenStmt, Statement elseStmt)
        {
            var stmt = new IfStatement(cond as Expression, thenStmt as Statement, elseStmt);
            return stmt;
        }

        [Production("statementPrim: WHILE [d] WhileParser_expressions DO [d] statement")]
        public WhileAST whileStmt(WhileAST cond, WhileAST blockStmt)
        {
            var stmt = new WhileStatement(cond as Expression, blockStmt as Statement);
            return stmt;
        }

        [Production("statementPrim: IDENTIFIER ASSIGN [d] WhileParser_expressions")]
        public WhileAST assignStmt(Token<WhileToken> variable, Expression value)
        {
            var assign = new AssignStatement(variable.StringWithoutQuotes, value);
            return assign;
        }

        [Production("statementPrim: SKIP [d]")]
        public WhileAST skipStmt()
        {
            return new SkipStatement();
        }

        [Production("statementPrim: RETURN [d] WhileParser_expressions")]
        public WhileAST returnStmt(WhileAST expression)
        {
            return new ReturnStatement(expression as Expression);
        }

        [Production("statementPrim: PRINT [d] WhileParser_expressions")]
        public WhileAST skipStmt(WhileAST expression)
        {
            return new PrintStatement(expression as Expression);
        }

        #endregion


        #region OPERANDS

        [Production("primary: INT")]
        public WhileAST PrimaryInt(Token<WhileToken> intToken)
        {
            return new IntegerConstant(intToken.IntValue);
        }

        [Production("primary: TRUE")]
        [Production("primary: FALSE")]
        public WhileAST PrimaryBool(Token<WhileToken> boolToken)
        {
            return new BoolConstant(bool.Parse(boolToken.StringWithoutQuotes));
        }

        [Production("primary: STRING")]
        public WhileAST PrimaryString(Token<WhileToken> stringToken)
        {
            return new StringConstant(stringToken.StringWithoutQuotes, stringToken.Position);
        }

        [Production("primary: IDENTIFIER")]
        public WhileAST PrimaryId(Token<WhileToken> varToken)
        {
            return new Variable(varToken.StringWithoutQuotes);
        }

        [Operand]
        [Production("operand: primary")]
        public WhileAST Operand(WhileAST prim)
        {
            return prim;
        }

        #endregion

        #region NUMERIC OPERATIONS

        [Operation((int) WhileToken.PLUS, Affix.InFix, Associativity.Right, 10)]
        [Operation((int) WhileToken.MINUS, Affix.InFix, Associativity.Right, 10)]
        public WhileAST binaryTermNumericExpression(WhileAST left, Token<WhileToken> operatorToken, WhileAST right)
        {
            var oper = BinaryOperator.ADD;

            switch (operatorToken.TokenID)
            {
                case WhileToken.PLUS:
                {
                    oper = BinaryOperator.ADD;
                    break;
                }
                case WhileToken.MINUS:
                {
                    oper = BinaryOperator.SUB;
                    break;
                }
            }

            var operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        [Operation((int) WhileToken.TIMES, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) WhileToken.DIVIDE, Affix.InFix, Associativity.Right, 50)]
        public WhileAST binaryFactorNumericExpression(WhileAST left, Token<WhileToken> operatorToken, WhileAST right)
        {
            var oper = BinaryOperator.MULTIPLY;

            switch (operatorToken.TokenID)
            {
                case WhileToken.TIMES:
                {
                    oper = BinaryOperator.MULTIPLY;
                    break;
                }
                case WhileToken.DIVIDE:
                {
                    oper = BinaryOperator.DIVIDE;
                    break;
                }
            }

            var operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        [Operation((int) WhileToken.MINUS, Affix.PreFix, Associativity.Right, 100)]
        public WhileAST unaryNumericExpression(Token<WhileToken> operation, WhileAST value)
        {
            return new Neg(value as Expression);
        }

        #endregion


        #region BOOLEAN OPERATIONS

        [Operation((int) WhileToken.OR, Affix.InFix, Associativity.Right, 10)]
        public WhileAST binaryOrExpression(WhileAST left, Token<WhileToken> operatorToken, WhileAST right)
        {
            var oper = BinaryOperator.OR;


            var operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        [Operation((int) WhileToken.AND, Affix.InFix, Associativity.Right, 50)]
        public WhileAST binaryAndExpression(WhileAST left, Token<WhileToken> operatorToken, WhileAST right)
        {
            var oper = BinaryOperator.AND;


            var operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        [Operation((int) WhileToken.NOT, Affix.PreFix, Associativity.Right, 100)]
        public WhileAST binaryOrExpression(Token<WhileToken> operatorToken, WhileAST value)
        {
            return new Not(value as Expression);
        }

        #endregion
    }
}