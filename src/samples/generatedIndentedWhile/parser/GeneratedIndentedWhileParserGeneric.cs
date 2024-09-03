using System.Collections.Generic;
using System.Linq;
using csly.whileLang.model;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace csly.generatedIndentedWhileLang.parser
{
    [UseMemoization]
    [AutoCloseIndentations]
    [ParserRoot("program")]
    public class GeneratedIndentedWhileParserGeneric
    {
        #region COMPARISON OPERATIONS

        [Operation((int) GeneratedIndentedWhileTokenGeneric.LESSER, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) GeneratedIndentedWhileTokenGeneric.GREATER, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) GeneratedIndentedWhileTokenGeneric.EQUALS, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) GeneratedIndentedWhileTokenGeneric.DIFFERENT, Affix.InFix, Associativity.Right, 50)]
        public WhileAST binaryComparisonExpression(WhileAST left, Token<GeneratedIndentedWhileTokenGeneric> operatorToken,
            WhileAST right)
        {
            var oper = BinaryOperator.ADD;

            switch (operatorToken.TokenID)
            {
                case GeneratedIndentedWhileTokenGeneric.LESSER:
                {
                    oper = BinaryOperator.LESSER;
                    break;
                }
                case GeneratedIndentedWhileTokenGeneric.GREATER:
                {
                    oper = BinaryOperator.GREATER;
                    break;
                }
                case GeneratedIndentedWhileTokenGeneric.EQUALS:
                {
                    oper = BinaryOperator.EQUALS;
                    break;
                }
                case GeneratedIndentedWhileTokenGeneric.DIFFERENT:
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

        [Operation((int) GeneratedIndentedWhileTokenGeneric.CONCAT, Affix.InFix, Associativity.Right, 10)]
        public WhileAST binaryStringExpression(WhileAST left, Token<GeneratedIndentedWhileTokenGeneric> operatorToken, WhileAST right)
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
        
        [Production("block : INDENT[d] sequence UINDENT[d]")]
        public WhileAST sequenceStatements(SequenceStatement seq)
        {
            return seq;
        }
        
        #region statements
        
        

        [Production("statement : block")]
        public WhileAST blockStatement(WhileAST block)
        {
            return block;
        }
       
        [Production("sequence: statement*")]
        public WhileAST sequence(List<WhileAST> list)
        {
            var statements = list.Cast<Statement>().ToList();
            var seq = new SequenceStatement(statements); 
            return seq;
        }

        [Production("statement: IF[d] IndentedWhileParserGeneric_expressions THEN[d] block (ELSE[d] block)?")]
        public WhileAST ifStmt( WhileAST cond, WhileAST thenStmt, ValueOption<Group<GeneratedIndentedWhileTokenGeneric, WhileAST>> optionalElseStmt)
        {
            var elseGroup = optionalElseStmt.Match(
                x => x, () => null)?.Value(0);
            
            var stmt = new IfStatement(cond as Expression, thenStmt as Statement, elseGroup as Statement);
            return stmt;
        }

        [Production("statement: WHILE[d] IndentedWhileParserGeneric_expressions DO[d] block")]
        public WhileAST whileStmt(WhileAST cond, WhileAST blockStmt)
        {
            var stmt = new WhileStatement(cond as Expression, blockStmt as Statement);
            return stmt;
        }

        [Production("statement: IDENTIFIER ASSIGN[d] IndentedWhileParserGeneric_expressions")]
        public WhileAST assignStmt(Token<GeneratedIndentedWhileTokenGeneric> variable, Expression value)
        {
            var assign = new AssignStatement(variable.StringWithoutQuotes, value);
            return assign;
        }

        [Production("statement: SKIP[d]")]
        public WhileAST skipStmt()
        {
            return new SkipStatement();
        }
        
        [Production("statement: RETURN[d] IndentedWhileParserGeneric_expressions")]
        public WhileAST ReturnStmt(Expression expression)
        {
            return new ReturnStatement(expression);
        }

        [Production("statement: PRINT[d] IndentedWhileParserGeneric_expressions")]
        public WhileAST printStmt(WhileAST expression)
        {
            return new PrintStatement(expression as Expression);
        }

        #endregion


        #region OPERANDS

        [Production("primary: INT")]
        public WhileAST PrimaryInt(Token<GeneratedIndentedWhileTokenGeneric> intToken)
        {
            return new IntegerConstant(intToken.IntValue);
        }

        [Production("primary: TRUE")]
        [Production("primary: FALSE")]
        public WhileAST PrimaryBool(Token<GeneratedIndentedWhileTokenGeneric> boolToken)
        {
            return new BoolConstant(bool.Parse(boolToken.StringWithoutQuotes));
        }

        [Production("primary: STRING")]
        public WhileAST PrimaryString(Token<GeneratedIndentedWhileTokenGeneric> stringToken)
        {
            return new StringConstant(stringToken.StringWithoutQuotes);
        }

        [Production("primary: IDENTIFIER")]
        public WhileAST PrimaryId(Token<GeneratedIndentedWhileTokenGeneric> varToken)
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

        [Operation((int) GeneratedIndentedWhileTokenGeneric.PLUS, Affix.InFix, Associativity.Right, 10)]
        [Operation((int) GeneratedIndentedWhileTokenGeneric.MINUS, Affix.InFix, Associativity.Right, 10)]
        public WhileAST binaryTermNumericExpression(WhileAST left, Token<GeneratedIndentedWhileTokenGeneric> operatorToken,
            WhileAST right)
        {
            var oper = BinaryOperator.ADD;

            switch (operatorToken.TokenID)
            {
                case GeneratedIndentedWhileTokenGeneric.PLUS:
                {
                    oper = BinaryOperator.ADD;
                    break;
                }
                case GeneratedIndentedWhileTokenGeneric.MINUS:
                {
                    oper = BinaryOperator.SUB;
                    break;
                }
            }

            var operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        [Operation((int) GeneratedIndentedWhileTokenGeneric.TIMES, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) GeneratedIndentedWhileTokenGeneric.DIVIDE, Affix.InFix, Associativity.Right, 50)]
        public WhileAST binaryFactorNumericExpression(WhileAST left, Token<GeneratedIndentedWhileTokenGeneric> operatorToken,
            WhileAST right)
        {
            var oper = BinaryOperator.MULTIPLY;

            switch (operatorToken.TokenID)
            {
                case GeneratedIndentedWhileTokenGeneric.TIMES:
                {
                    oper = BinaryOperator.MULTIPLY;
                    break;
                }
                case GeneratedIndentedWhileTokenGeneric.DIVIDE:
                {
                    oper = BinaryOperator.DIVIDE;
                    break;
                }
            }

            var operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        [Operation((int) GeneratedIndentedWhileTokenGeneric.MINUS, Affix.PreFix, Associativity.Right, 100)]
        public WhileAST unaryNumericExpression(Token<GeneratedIndentedWhileTokenGeneric> operation, WhileAST value)
        {
            return new Neg(value as Expression);
        }

        #endregion


        #region BOOLEAN OPERATIONS

        [Operation((int) GeneratedIndentedWhileTokenGeneric.OR, Affix.InFix, Associativity.Right, 10)]
        public WhileAST binaryOrExpression(WhileAST left, Token<GeneratedIndentedWhileTokenGeneric> operatorToken, WhileAST right)
        {
            var oper = BinaryOperator.OR;


            var operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        [Operation((int) GeneratedIndentedWhileTokenGeneric.AND, Affix.InFix, Associativity.Right, 50)]
        public WhileAST binaryAndExpression(WhileAST left, Token<GeneratedIndentedWhileTokenGeneric> operatorToken, WhileAST right)
        {
            var oper = BinaryOperator.AND;


            var operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        [Operation((int) GeneratedIndentedWhileTokenGeneric.NOT, Affix.PreFix, Associativity.Right, 100)]
        public WhileAST unaryNotExpression(Token<GeneratedIndentedWhileTokenGeneric> operatorToken, WhileAST value)
        {
            return new Not(value as Expression);
        }

        #endregion
    }
}