using System.Collections.Generic;
using System.Linq;
using csly.whileLang.model;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace csly.indentedWhileLang.parser
{
    public class IndentedWhileParserGeneric
    {
        #region COMPARISON OPERATIONS

        [Operation((int) IndentedWhileTokenGeneric.LESSER, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) IndentedWhileTokenGeneric.GREATER, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) IndentedWhileTokenGeneric.EQUALS, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) IndentedWhileTokenGeneric.DIFFERENT, Affix.InFix, Associativity.Right, 50)]
        public WhileAST binaryComparisonExpression(WhileAST left, Token<IndentedWhileTokenGeneric> operatorToken,
            WhileAST right)
        {
            var oper = BinaryOperator.ADD;

            switch (operatorToken.TokenID)
            {
                case IndentedWhileTokenGeneric.LESSER:
                {
                    oper = BinaryOperator.LESSER;
                    break;
                }
                case IndentedWhileTokenGeneric.GREATER:
                {
                    oper = BinaryOperator.GREATER;
                    break;
                }
                case IndentedWhileTokenGeneric.EQUALS:
                {
                    oper = BinaryOperator.EQUALS;
                    break;
                }
                case IndentedWhileTokenGeneric.DIFFERENT:
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

        [Operation((int) IndentedWhileTokenGeneric.CONCAT, Affix.InFix, Associativity.Right, 10)]
        public WhileAST binaryStringExpression(WhileAST left, Token<IndentedWhileTokenGeneric> operatorToken, WhileAST right)
        {
            var oper = BinaryOperator.CONCAT;
            var operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        #endregion

        
        [Production("program: sequence")]
        public WhileAST program(WhileAST prog)
        {
            return prog;
        }
        
        #region statements
       

        [Production("block : INDENT[d] sequence UINDENT[d]")]
        public WhileAST sequenceStatements(SequenceStatement seq)
        {
            return seq;
        }

        [Production("sequence: statementInBlock*")]
        public WhileAST sequence(List<WhileAST> list)
        {
            var statements = list.Cast<Statement>().ToList();
            var seq = new SequenceStatement(statements); 
            return seq;
        }

        [Production("statementInBlock : basicStatement SEMICOLON[d]")]
        public WhileAST basicStatementInBlock(WhileAST basic)
        {
            return basic;
        }
        
        [Production("statementInBlock : [ifthenelse | while | block]")]
        public WhileAST flowControlStatementInBlock(WhileAST flowControl)
        {
            return flowControl;
        }


        [Production("ifthenelse: IF[d] IndentedWhileParserGeneric_expressions THEN[d] block ELSE[d] block")]
        public WhileAST ifStmt( WhileAST cond, WhileAST thenStmt, Statement elseStmt)
        {
            var stmt = new IfStatement(cond as Expression, thenStmt as Statement, elseStmt);
            return stmt;
        }

        [Production("while: WHILE[d] IndentedWhileParserGeneric_expressions DO[d] block")]
        public WhileAST whileStmt(WhileAST cond, WhileAST blockStmt)
        {
            var stmt = new WhileStatement(cond as Expression, blockStmt as Statement);
            return stmt;
        }

        [Production("basicStatement: IDENTIFIER ASSIGN[d] IndentedWhileParserGeneric_expressions")]
        public WhileAST assignStmt(Token<IndentedWhileTokenGeneric> variable, Expression value)
        {
            var assign = new AssignStatement(variable.StringWithoutQuotes, value);
            return assign;
        }

        [Production("basicStatement: SKIP[d]")]
        public WhileAST skipStmt()
        {
            return new SkipStatement();
        }
        
        [Production("basicStatement: RETURN[d] IndentedWhileParserGeneric_expressions")]
        public WhileAST ReturnStmt(Expression expression)
        {
            return new ReturnStatement(expression);
        }
        
        

        [Production("basicStatement: PRINT[d] IndentedWhileParserGeneric_expressions")]
        public WhileAST skipStmt(WhileAST expression)
        {
            return new PrintStatement(expression as Expression);
        }

        #endregion


        #region OPERANDS

        [Production("primary: INT")]
        public WhileAST PrimaryInt(Token<IndentedWhileTokenGeneric> intToken)
        {
            return new IntegerConstant(intToken.IntValue);
        }

        [Production("primary: TRUE")]
        [Production("primary: FALSE")]
        public WhileAST PrimaryBool(Token<IndentedWhileTokenGeneric> boolToken)
        {
            return new BoolConstant(bool.Parse(boolToken.StringWithoutQuotes));
        }

        [Production("primary: STRING")]
        public WhileAST PrimaryString(Token<IndentedWhileTokenGeneric> stringToken)
        {
            return new StringConstant(stringToken.StringWithoutQuotes);
        }

        [Production("primary: IDENTIFIER")]
        public WhileAST PrimaryId(Token<IndentedWhileTokenGeneric> varToken)
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

        [Operation((int) IndentedWhileTokenGeneric.PLUS, Affix.InFix, Associativity.Right, 10)]
        [Operation((int) IndentedWhileTokenGeneric.MINUS, Affix.InFix, Associativity.Right, 10)]
        public WhileAST binaryTermNumericExpression(WhileAST left, Token<IndentedWhileTokenGeneric> operatorToken,
            WhileAST right)
        {
            var oper = BinaryOperator.ADD;

            switch (operatorToken.TokenID)
            {
                case IndentedWhileTokenGeneric.PLUS:
                {
                    oper = BinaryOperator.ADD;
                    break;
                }
                case IndentedWhileTokenGeneric.MINUS:
                {
                    oper = BinaryOperator.SUB;
                    break;
                }
            }

            var operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        [Operation((int) IndentedWhileTokenGeneric.TIMES, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) IndentedWhileTokenGeneric.DIVIDE, Affix.InFix, Associativity.Right, 50)]
        public WhileAST binaryFactorNumericExpression(WhileAST left, Token<IndentedWhileTokenGeneric> operatorToken,
            WhileAST right)
        {
            var oper = BinaryOperator.MULTIPLY;

            switch (operatorToken.TokenID)
            {
                case IndentedWhileTokenGeneric.TIMES:
                {
                    oper = BinaryOperator.MULTIPLY;
                    break;
                }
                case IndentedWhileTokenGeneric.DIVIDE:
                {
                    oper = BinaryOperator.DIVIDE;
                    break;
                }
            }

            var operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        [Operation((int) IndentedWhileTokenGeneric.MINUS, Affix.PreFix, Associativity.Right, 100)]
        public WhileAST unaryNumericExpression(Token<IndentedWhileTokenGeneric> operation, WhileAST value)
        {
            return new Neg(value as Expression);
        }

        #endregion


        #region BOOLEAN OPERATIONS

        [Operation((int) IndentedWhileTokenGeneric.OR, Affix.InFix, Associativity.Right, 10)]
        public WhileAST binaryOrExpression(WhileAST left, Token<IndentedWhileTokenGeneric> operatorToken, WhileAST right)
        {
            var oper = BinaryOperator.OR;


            var operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        [Operation((int) IndentedWhileTokenGeneric.AND, Affix.InFix, Associativity.Right, 50)]
        public WhileAST binaryAndExpression(WhileAST left, Token<IndentedWhileTokenGeneric> operatorToken, WhileAST right)
        {
            var oper = BinaryOperator.AND;


            var operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        [Operation((int) IndentedWhileTokenGeneric.NOT, Affix.PreFix, Associativity.Right, 100)]
        public WhileAST binaryOrExpression(Token<IndentedWhileTokenGeneric> operatorToken, WhileAST value)
        {
            return new Not(value as Expression);
        }

        #endregion
    }
}