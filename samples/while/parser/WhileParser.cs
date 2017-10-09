using System;
using System.Collections.Generic;
using System.Text;
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
        public WhileAST statementSequence(Statement sequence)
        {
            return sequence;
        }

        [Production("sequence : statementPrim additionalStatements*")]
        public WhileAST sequenceStatements(Statement first, List<Statement> next)
        {
            SequenceStatement seq = new SequenceStatement(first);
            seq.AddRange(next);
            return seq;
        }

        [Production("additionalStatements : SEMICOLON statementPrim*")]
        public WhileAST additional(Token<WhileToken> semi, Statement statement)
        {
            return statement;
        }

        [Production("statementPrim: IF expression THEN statement ELSE statement")]
        public WhileAST  ifStmt(Token<WhileToken> discardIf, Expression cond, Token<WhileToken> dicardThen, Statement thenStmt, Token<WhileToken> dicardElse, Statement elseStmt)
        {
            IfStatement stmt = new IfStatement(cond, thenStmt, elseStmt);
            return stmt;
        }
        
        [Production("statementPrim: WHILE expression DO statement")]
        public WhileAST ifStmt(Token<WhileToken> discardWhile, Expression cond, Token<WhileToken> dicardDo, Statement blockStmt)
        {
            WhileStatement stmt = new WhileStatement(cond, blockStmt);
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
        public WhileAST skipStmt(Token<WhileToken> discard, Expression expression)
        {
            return new PrintStatement(expression);
        }

      

        

        #endregion 

        #region expression

        [Production("expression : arithm_expression")]
        [Production("expression : bool_expression")]
        [Production("expression : string_expression")]
        public WhileAST expression(Expression expr)
        {
            return expr;
        }


        #endregion

        #region arithmetic expression



        [Production("arithm_expression : arithm_term PLUS arithm_expression")]
        [Production("arithm_expression : arithm_term MINUS arithm_expression")]

        public WhileAST Arithm_Expression(Expression left, Token<WhileToken> operatorToken, Expression right)
        {
            WhileAST result = null;
            switch (operatorToken.TokenID)
            {
                case WhileToken.PLUS:
                    {
                        result = new BinaryOperation(left, BinaryOperator.ADD, right);
                        break;
                    }
                case WhileToken.MINUS:
                    {
                        result = new BinaryOperation(left, BinaryOperator.SUB, right);
                        break;
                    }
                default:
                    {
                        break;

                    }
            }
            return result;
        }

    

        [Production("arithm_expression : arithm_term")]
        public WhileAST Arithm_Expression_Term(Expression termValue)
        {
            return termValue;
        }

        [Production("arithm_term : arithm_factor TIMES arithm_term")]
        [Production("arithm_term : arithm_factor DIVIDE arithm_term")]
        public WhileAST Arithm_Term(Expression left, Token<WhileToken> operatorToken, Expression right)
        {
            WhileAST result = null;

            switch (operatorToken.TokenID)
            {
                case WhileToken.TIMES:
                    {
                        result = new BinaryOperation(left, BinaryOperator.MULTIPLY, right);
                        break;
                    }
                case WhileToken.DIVIDE:
                    {
                        result = new BinaryOperation(left, BinaryOperator.DIVIDE, right);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            return result;
        }

        [Production("arithm_term : arithm_factor")]
        public WhileAST Arithm_Term_Factor(Expression factorValue)
        {
            return factorValue;
        }

        [Production("arithm_factor : INT")]
        public WhileAST Arithm_constantFactor(Token<WhileToken> primValue)
        {
            return new IntegerConstant(primValue.IntValue);
        }

        [Production("arithm_factor : MINUS arithm_factor")]
        public WhileAST Arithm_Factor(Token<WhileToken> discardedMinus, Expression factorValue)
        {
            return new Neg(factorValue);
        }


        #endregion

        #region boolean expression




        [Production("bool_expression : arithm_term OR arithm_expression")]
        
        public WhileAST Bool_Expression(Expression left, Token<WhileToken> operatorToken, Expression right)
        {
            
            WhileAST result = new BinaryOperation(left, BinaryOperator.OR, right);                        
            return result;
        }



        [Production("bool_expression : bool_term")]
        public WhileAST Bool_Expression_Term(Expression termValue)
        {
            return termValue;
        }

        [Production("bool_term : bool_factor AND bool_term")]
        public WhileAST Bool_Term(Expression left, Token<WhileToken> operatorToken, Expression right)
        {
            WhileAST result = new BinaryOperation(left, BinaryOperator.AND, right);
            return result;
        }

        [Production("bool_term : bool_factor")]
        public WhileAST Bool_Term_Factor(Expression factorValue)
        {
            return factorValue;
        }

        [Production("bool_factor : TRUE")]
        [Production("bool_factor : FALSE")]
        public WhileAST Bool_constantFactor(Token<WhileToken> boolValue)
        {
            return new BoolConstant(bool.Parse(boolValue.Value));
        }

        [Production("bool_factor : NOT bool_factor")]
        public WhileAST Bool_Factor(Token<WhileToken> discardedMinus, Expression factorValue)
        {
            return new Not(factorValue);
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
            return new StringConstant(value.Value);
        }

        #endregion


    }
}


