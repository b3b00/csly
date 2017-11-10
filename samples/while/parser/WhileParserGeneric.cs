using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.generator;
using csly.whileLang.model;

namespace csly.whileLang.parser
{
    public class WhileParserGeneric
    {

        #region statements

        [Production("statement :  LPAREN statement RPAREN ")]
        public WhileAST block(Token<WhileTokenGeneric> discardLpar, Statement statement, Token<WhileTokenGeneric> discardRpar)
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
            SequenceStatement seq = new SequenceStatement(first as Statement);
            seq.AddRange(next.Cast<Statement>().ToList<Statement>());
            return seq;
        }

        [Production("additionalStatements : SEMICOLON statementPrim")]
        public WhileAST additional(Token<WhileTokenGeneric> semi, WhileAST statement)
        {
            return statement;
        }

        [Production("statementPrim: IF WhileParserGeneric_expressions THEN statement ELSE statement")]
        public WhileAST ifStmt(Token<WhileTokenGeneric> discardIf, WhileAST cond, Token<WhileTokenGeneric> dicardThen, WhileAST thenStmt, Token<WhileTokenGeneric> dicardElse, Statement elseStmt)
        {
            IfStatement stmt = new IfStatement(cond as Expression, thenStmt as Statement, elseStmt);
            return stmt;
        }

        [Production("statementPrim: WHILE WhileParserGeneric_expressions DO statement")]
        public WhileAST whileStmt(Token<WhileTokenGeneric> discardWhile, WhileAST cond, Token<WhileTokenGeneric> dicardDo, WhileAST blockStmt)
        {
            WhileStatement stmt = new WhileStatement(cond as Expression, blockStmt as Statement);
            return stmt;
        }

        [Production("statementPrim: IDENTIFIER ASSIGN WhileParserGeneric_expressions")]
        public WhileAST assignStmt(Token<WhileTokenGeneric> variable, Token<WhileTokenGeneric> discardAssign, Expression value)
        {
            AssignStatement assign = new AssignStatement(variable.StringWithoutQuotes, value);
            return assign;
        }

        [Production("statementPrim: SKIP")]
        public WhileAST skipStmt(Token<WhileTokenGeneric> discard)
        {
            return new SkipStatement();
        }

        [Production("statementPrim: PRINT WhileParserGeneric_expressions")]
        public WhileAST skipStmt(Token<WhileTokenGeneric> discard, WhileAST expression)
        {
            return new PrintStatement(expression as Expression);
        }





        #endregion


        #region OPERANDS

        [Production("primary: INT")]
        public WhileAST PrimaryInt(Token<WhileTokenGeneric> intToken)
        {
            return new IntegerConstant(intToken.IntValue);
        }

        [Production("primary: TRUE")]
        [Production("primary: FALSE")]
        public WhileAST PrimaryBool(Token<WhileTokenGeneric> boolToken)
        {
            return new BoolConstant(bool.Parse(boolToken.StringWithoutQuotes));
        }

        [Production("primary: STRING")]
        public WhileAST PrimaryString(Token<WhileTokenGeneric> stringToken)
        {
            return new StringConstant(stringToken.StringWithoutQuotes);
        }

        [Production("primary: IDENTIFIER")]
        public WhileAST PrimaryId(Token<WhileTokenGeneric> varToken)
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
        [Operation((int)WhileTokenGeneric.PLUS, 2, Associativity.Right, 10)]
        [Operation((int)WhileTokenGeneric.MINUS, 2, Associativity.Right, 10)]
        public WhileAST binaryTermNumericExpression(WhileAST left, Token<WhileTokenGeneric> operatorToken, WhileAST right)
        {
            BinaryOperator oper = BinaryOperator.ADD;

            switch (operatorToken.TokenID)
            {
                case WhileTokenGeneric.PLUS:
                    {
                        oper = BinaryOperator.ADD;
                        break;
                    }
                case WhileTokenGeneric.MINUS:
                    {
                        oper = BinaryOperator.SUB;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            BinaryOperation operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        [Operation((int)WhileTokenGeneric.TIMES, 2, Associativity.Right, 50)]
        [Operation((int)WhileTokenGeneric.DIVIDE, 2, Associativity.Right, 50)]
        public WhileAST binaryFactorNumericExpression(WhileAST left, Token<WhileTokenGeneric> operatorToken, WhileAST right)
        {
            BinaryOperator oper = BinaryOperator.MULTIPLY;

            switch (operatorToken.TokenID)
            {
                case WhileTokenGeneric.TIMES:
                    {
                        oper = BinaryOperator.MULTIPLY;
                        break;
                    }
                case WhileTokenGeneric.DIVIDE:
                    {
                        oper = BinaryOperator.DIVIDE;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            BinaryOperation operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        [Operation((int)WhileTokenGeneric.MINUS, 1, Associativity.Right, 100)]
        public WhileAST unaryNumericExpression(Token<WhileTokenGeneric> operation, WhileAST value)
        {
            return new Neg(value as Expression);
        }


        #endregion



        #region BOOLEAN OPERATIONS

        [Operation((int)WhileTokenGeneric.OR, 2, Associativity.Right, 10)]
        public WhileAST binaryOrExpression(WhileAST left, Token<WhileTokenGeneric> operatorToken, WhileAST right)
        {
            BinaryOperator oper = BinaryOperator.OR;


            BinaryOperation operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        [Operation((int)WhileTokenGeneric.AND, 2, Associativity.Right, 50)]
        public WhileAST binaryAndExpression(WhileAST left, Token<WhileTokenGeneric> operatorToken, WhileAST right)
        {
            BinaryOperator oper = BinaryOperator.AND;


            BinaryOperation operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        [Operation((int)WhileTokenGeneric.NOT, 1, Associativity.Right, 100)]
        public WhileAST binaryOrExpression(Token<WhileTokenGeneric> operatorToken, WhileAST value)
        {

            return new Not(value as Expression);
        }
        #endregion

        #region COMPARISON OPERATIONS

        [Operation((int)WhileTokenGeneric.LESSER, 2, Associativity.Right, 50)]
        [Operation((int)WhileTokenGeneric.GREATER, 2, Associativity.Right, 50)]
        [Operation((int)WhileTokenGeneric.EQUALS, 2, Associativity.Right, 50)]
        [Operation((int)WhileTokenGeneric.DIFFERENT, 2, Associativity.Right, 50)]
        public WhileAST binaryComparisonExpression(WhileAST left, Token<WhileTokenGeneric> operatorToken, WhileAST right)
        {
            BinaryOperator oper = BinaryOperator.ADD;

            switch (operatorToken.TokenID)
            {
                case WhileTokenGeneric.LESSER:
                    {
                        oper = BinaryOperator.LESSER;
                        break;
                    }
                case WhileTokenGeneric.GREATER:
                    {
                        oper = BinaryOperator.GREATER;
                        break;
                    }
                case WhileTokenGeneric.EQUALS:
                    {
                        oper = BinaryOperator.EQUALS;
                        break;
                    }
                case WhileTokenGeneric.DIFFERENT:
                    {
                        oper = BinaryOperator.DIFFERENT;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            BinaryOperation operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }


        #endregion

        #region STRING OPERATIONS

        [Operation((int)WhileTokenGeneric.CONCAT, 2, Associativity.Right, 10)]
        public WhileAST binaryStringExpression(WhileAST left, Token<WhileTokenGeneric> operatorToken, WhileAST right)
        {
            BinaryOperator oper = BinaryOperator.CONCAT;
            BinaryOperation operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }
        #endregion











    }
}


