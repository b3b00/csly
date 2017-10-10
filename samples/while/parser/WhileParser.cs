using System;
using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.generator;
using csly.whileLang.model;

namespace csly.whileLang.parser
{
    public class WhileParser
    {

        #region statements

        [Production("statement :  LPAREN statement RPAREN ")]
        public WhileAST block(Token<WhileToken> discardLpar, Statement statement ,Token<WhileToken> discardRpar)
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
            seq.AddRange(next.Cast<Statement>().ToList<Statement>()) ;
            return seq;
        }

        [Production("additionalStatements : SEMICOLON statementPrim")]
        public WhileAST additional(Token<WhileToken> semi, WhileAST statement)
        {
            return statement;
        }

        [Production("statementPrim: IF expression THEN statement ELSE statement")]
        public WhileAST  ifStmt(Token<WhileToken> discardIf, WhileAST cond, Token<WhileToken> dicardThen, WhileAST thenStmt, Token<WhileToken> dicardElse, Statement elseStmt)
        {
            IfStatement stmt = new IfStatement(cond as Expression, thenStmt as Statement, elseStmt);
            return stmt;
        }
        
        [Production("statementPrim: WHILE expression DO statement")]
        public WhileAST whileStmt(Token<WhileToken> discardWhile, WhileAST cond, Token<WhileToken> dicardDo, WhileAST blockStmt)
        {
            WhileStatement stmt = new WhileStatement(cond as Expression, blockStmt as Statement);
            return stmt;
        }

        [Production("statementPrim: IDENTIFIER ASSIGN expression")]
        public WhileAST assignStmt(Token<WhileToken> variable, Token<WhileToken> discardAssign, Expression value)
        {
            AssignStatement assign = new AssignStatement(variable.StringWithoutQuotes, value);
            return assign;
        }

        [Production("statementPrim: SKIP")]
        public WhileAST skipStmt(Token<WhileToken> discard)
        {
            return new SkipStatement();
        }

        [Production("statementPrim: PRINT expression")]
        public WhileAST skipStmt(Token<WhileToken> discard, WhileAST expression)
        {
            return new PrintStatement(expression as Expression);
        }





        #endregion

        #region expression



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
            return new StringConstant(stringToken.StringWithoutQuotes);
        }

        [Production("primary: IDENTIFIER")]
        public WhileAST PrimaryId(Token<WhileToken> varToken)
        {
            return new Variable(varToken.StringWithoutQuotes);
        }




        [Production("expression : term PLUS expression")]
        [Production("expression : term MINUS expression")]
        [Production("expression : term CONCAT expression")]

        public WhileAST Expression(WhileAST left, Token<WhileToken> operatorToken, WhileAST right)
        {
            BinaryOperator oper = BinaryOperator.ADD;

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
                case WhileToken.CONCAT:
                    {
                        oper = BinaryOperator.CONCAT;
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

        [Production("expression : term")]
        public WhileAST Expression_Term(WhileAST termValue)
        {
            return termValue;
        }

        [Production("term : factor TIMES term")]
        [Production("term : factor DIVIDE term")]
        [Production("term : factor AND term")]
        [Production("term : factor LESSER term")]
        [Production("term : factor GREATER term")]
        [Production("term : factor EQUALS term")]
        [Production("term : factor DIFFERENT term")]
        public WhileAST Term(WhileAST left, Token<WhileToken> operatorToken, WhileAST right)
        {
            BinaryOperator oper = BinaryOperator.MULTIPLY;

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
                case WhileToken.AND:
                    {
                        oper = BinaryOperator.AND;
                        break;
                    }
                case WhileToken.GREATER:
                    {
                        oper = BinaryOperator.GREATER;
                        break;
                    }
                case WhileToken.LESSER:
                    {
                        oper = BinaryOperator.LESSER;
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
                default:
                    {
                        break;
                    }
            }
            BinaryOperation operation = new BinaryOperation(left as Expression, oper, right as Expression);
            return operation;
        }

        [Production("term : factor")]
        public WhileAST Term_Factor(WhileAST factorValue)
        {
            return factorValue;
        }

        [Production("factor : primary")]
        public WhileAST primaryFactor(WhileAST primValue)
        {
            return primValue;
        }
        [Production("factor : MINUS factor")]
        public WhileAST negFactor(Token<WhileToken> discardedMinus, WhileAST factorValue)
        {
            return new Neg(factorValue as Expression);
        }

        [Production("factor : NOT factor")]
        public WhileAST notFactor(Token<WhileToken> discardedMinus, WhileAST factorValue)
        {
            return new Not(factorValue as Expression);
        }



        #endregion





        #region string expression


        //[Production("string_expression : expression CONCAT expression")]
        //public WhileAST String_Term(Expression left, Token<WhileToken> discard, Expression right)
        //{
        //    WhileAST result = new BinaryOperation(left, BinaryOperator.CONCAT, right);
        //    return result;
        //}

        [Production("string_expression : STRING")]
        public WhileAST String_constantFactor(Token<WhileToken> value)
        {
            return new StringConstant(value.StringWithoutQuotes);
        }

        #endregion


    }
}


