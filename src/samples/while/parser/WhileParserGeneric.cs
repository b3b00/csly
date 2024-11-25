using System.Collections.Generic;
using System.Linq;
using csly.whileLang.model;
using sly.lexer;
using sly.parser.generator;

namespace csly.whileLang.parser
{
    [ParserRoot("statement")]
    public class WhileParserGeneric
    {
        #region COMPARISON OPERATIONS

        [Operation((int) WhileTokenGeneric.LESSER, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) WhileTokenGeneric.GREATER, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) WhileTokenGeneric.EQUALS, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) WhileTokenGeneric.DIFFERENT, Affix.InFix, Associativity.Right, 50)]
        public WhileAST binaryComparisonExpression(WhileAST left, Token<WhileTokenGeneric> operatorToken,
            WhileAST right)
        {
            var oper = BinaryOperator.ADD;

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
            }

            var operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        #endregion

        #region STRING OPERATIONS

        [Operation((int) WhileTokenGeneric.CONCAT, Affix.InFix, Associativity.Right, 10)]
        public WhileAST binaryStringExpression(WhileAST left, Token<WhileTokenGeneric> operatorToken, WhileAST right)
        {
            var oper = BinaryOperator.CONCAT;
            var operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        #endregion

        #region statements

        [Production("statement :  LPAREN statement RPAREN ")]
        public WhileAST block(Token<WhileTokenGeneric> discardLpar, Statement statement,
            Token<WhileTokenGeneric> discardRpar)
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

        [Production("additionalStatements : SEMICOLON statementPrim")]
        public WhileAST additional(Token<WhileTokenGeneric> semi, WhileAST statement)
        {
            return statement;
        }

        [Production("statementPrim: IF WhileParserGeneric_expressions THEN statement ELSE statement")]
        public WhileAST ifStmt(Token<WhileTokenGeneric> discardIf, WhileAST cond, Token<WhileTokenGeneric> dicardThen,
            WhileAST thenStmt, Token<WhileTokenGeneric> dicardElse, Statement elseStmt)
        {
            var stmt = new IfStatement(cond as Expression, thenStmt as Statement, elseStmt);
            return stmt;
        }

        [Production("statementPrim: WHILE WhileParserGeneric_expressions DO statement")]
        public WhileAST whileStmt(Token<WhileTokenGeneric> discardWhile, WhileAST cond,
            Token<WhileTokenGeneric> dicardDo, WhileAST blockStmt)
        {
            var stmt = new WhileStatement(cond as Expression, blockStmt as Statement);
            return stmt;
        }

        [Production("statementPrim: IDENTIFIER ASSIGN WhileParserGeneric_expressions")]
        public WhileAST assignStmt(Token<WhileTokenGeneric> variable, Token<WhileTokenGeneric> discardAssign,
            Expression value)
        {
            var assign = new AssignStatement(variable.StringWithoutQuotes, value);
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
            return new StringConstant(stringToken.StringWithoutQuotes,stringToken.Position);
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

        [Operation((int) WhileTokenGeneric.PLUS, Affix.InFix, Associativity.Right, 10)]
        [Operation((int) WhileTokenGeneric.MINUS, Affix.InFix, Associativity.Right, 10)]
        public WhileAST binaryTermNumericExpression(WhileAST left, Token<WhileTokenGeneric> operatorToken,
            WhileAST right)
        {
            var oper = BinaryOperator.ADD;

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
            }

            var operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        [Operation((int) WhileTokenGeneric.TIMES, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) WhileTokenGeneric.DIVIDE, Affix.InFix, Associativity.Right, 50)]
        public WhileAST binaryFactorNumericExpression(WhileAST left, Token<WhileTokenGeneric> operatorToken,
            WhileAST right)
        {
            var oper = BinaryOperator.MULTIPLY;

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
            }

            var operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        [Operation((int) WhileTokenGeneric.MINUS, Affix.PreFix, Associativity.Right, 100)]
        public WhileAST unaryNumericExpression(Token<WhileTokenGeneric> operation, WhileAST value)
        {
            return new Neg(value as Expression);
        }

        #endregion


        #region BOOLEAN OPERATIONS

        [Operation((int) WhileTokenGeneric.OR, Affix.InFix, Associativity.Right, 10)]
        public WhileAST binaryOrExpression(WhileAST left, Token<WhileTokenGeneric> operatorToken, WhileAST right)
        {
            var oper = BinaryOperator.OR;


            var operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        [Operation((int) WhileTokenGeneric.AND, Affix.InFix, Associativity.Right, 50)]
        public WhileAST binaryAndExpression(WhileAST left, Token<WhileTokenGeneric> operatorToken, WhileAST right)
        {
            var oper = BinaryOperator.AND;


            var operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        [Operation((int) WhileTokenGeneric.NOT, Affix.PreFix, Associativity.Right, 100)]
        public WhileAST binaryOrExpression(Token<WhileTokenGeneric> operatorToken, WhileAST value)
        {
            return new Not(value as Expression);
        }

        #endregion
    }
}