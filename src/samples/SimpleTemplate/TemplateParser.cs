using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SimpleTemplate.model;
using SimpleTemplate.model.expressions;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace SimpleTemplate
{
    [ParserRoot("template")]
    public class TemplateParser
    {
        
        #region structure

        [Production("template: item*")]
        public ITemplate Template(List<ITemplate> items)
        {
            return new Template(items);
        }

        [Production("item : TEXT")]
        public ITemplate Text(Token<TemplateLexer> text)
        {
         return new Text(text.Value);
        }
        
        [Production("item :OPEN_VALUE[d] ID CLOSE_VALUE[d]")]
        public ITemplate Value(Token<TemplateLexer> value)
        {
            return new Value(value.Value);
        }

        [Production(@"item : OPEN_CODE[d] IF[d] OPEN_PAREN[d] TemplateParser_expressions CLOSE_PAREN[d] CLOSE_CODE[d]
                                     item* 
                                  elseBlock? 
                                  OPEN_CODE[d] ENDIF[d] CLOSE_CODE[d] ")]
        public ITemplate Conditional(Expression cond, List<ITemplate> thenBlock, ValueOption<ITemplate> elseBlock)
        {
            var ifthenelse = new IfThenElse(cond, new Block(thenBlock), elseBlock.Match(x => x, () => new Block()) as Block);
            return ifthenelse;
        }

        [Production("if : OPEN_CODE[d] IF[d] OPEN_PAREN[d] TemplateParser_expressions CLOSE_PAREN[d] CLOSE_CODE[d]")]
        public ITemplate If(ITemplate condition)
        {
            return condition;
        }

        [Production("elseBlock : OPEN_CODE[d] ELSE[d] CLOSE_CODE[d] item*")]
        public ITemplate elseBlock(List<ITemplate> items)
        {
            return new Block(items);
        }
        

        [Production("item : OPEN_CODE[d] FOR[d] INT RANGE[d] INT AS[d] ID CLOSE_CODE[d] item* OPEN_CODE[d] END[d] CLOSE_CODE[d]")]
        public ITemplate fori(Token<TemplateLexer> start, Token<TemplateLexer> end, Token<TemplateLexer> iterator, List<ITemplate> items)
        {
            return new ForI(start.IntValue, end.IntValue, iterator.Value, items);
        }
        
        [Production("item : OPEN_CODE[d] FOR[d] ID AS[d] ID CLOSE_CODE[d] item* OPEN_CODE[d] END[d] CLOSE_CODE[d]")]
        public ITemplate _foreach(Token<TemplateLexer> listName, Token<TemplateLexer> iterator, List<ITemplate> items)
        {
            return new ForEach(listName.Value, iterator.Value, items);
        }
       
        #endregion
        
        #region COMPARISON OPERATIONS

        [Infix("LESSER", Associativity.Right, 50)]
        [Infix("GREATER", Associativity.Right, 50)]
        [Infix("EQUALS", Associativity.Right, 50)]
        [Infix("DIFFERENT", Associativity.Right, 50)]
        public Expression binaryComparisonExpression(Expression left, Token<TemplateLexer> operatorToken,
            Expression right)
        {
            
            var oper = BinaryOperator.EQUALS;

            oper = operatorToken.TokenID switch
            {
                TemplateLexer.LESSER => BinaryOperator.LESSER,
                TemplateLexer.GREATER => BinaryOperator.GREATER,
                TemplateLexer.EQUALS => BinaryOperator.EQUALS,
                TemplateLexer.DIFFERENT => BinaryOperator.DIFFERENT,
                _ => BinaryOperator.EQUALS
            };

            return new BinaryOperation(left, oper, right);
        }

        #endregion

        #region STRING OPERATIONS

        // [Operation((int) TemplateLexer.CONCAT, Affix.InFix, Associativity.Right, 10)]
        // public Expression binaryStringExpression(Expression left, Token<TemplateLexer> operatorToken, Expression right)
        // {
        //     return new BinaryOperation(left, BinaryOperator.CONCAT, right);
        // }

        #endregion
        
          #region OPERANDS

          
        [Production("primary: INT")]
        public Expression PrimaryInt(Token<TemplateLexer> intToken)
        {
            return new IntegerConstant(intToken.IntValue, intToken.Position);
        }

        
        [Production("primary: TRUE")]
        [Production("primary: FALSE")]
        public ITemplate PrimaryBool(Token<TemplateLexer> boolToken)
        {
            return new BoolConstant(bool.Parse(boolToken.StringWithoutQuotes) ? true : false);
        }

        
        [Production("primary: STRING")]
        public ITemplate PrimaryString(Token<TemplateLexer> stringToken)
        {
            return new StringConstant(stringToken.StringWithoutQuotes, stringToken.Position);
        }

        
        [Production("primary: ID")]
        public Expression PrimaryId(Token<TemplateLexer> varToken)
        {
            return new Variable(varToken.Value);
        }

        [Operand]
        [Production("operand: primary")]
        public Expression Operand(Expression prim)
        {
            return prim;
        }

        #endregion

        #region NUMERIC OPERATIONS

        [Operation((int) TemplateLexer.PLUS, Affix.InFix, Associativity.Right, 10)]
        [Operation((int) TemplateLexer.MINUS, Affix.InFix, Associativity.Right, 10)]
        public Expression binaryTermNumericExpression(Expression left, Token<TemplateLexer> operatorToken,
            Expression right)
        {
            var oper = BinaryOperator.ADD;

            oper = operatorToken.TokenID switch
            {
                TemplateLexer.PLUS => BinaryOperator.ADD,
                TemplateLexer.MINUS => BinaryOperator.SUB,
                _ => BinaryOperator.ADD
            };

            return new BinaryOperation(left, oper, right);
        }

        [Operation((int) TemplateLexer.TIMES, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) TemplateLexer.DIVIDE, Affix.InFix, Associativity.Right, 50)]
        public Expression binaryFactorNumericExpression(Expression left, Token<TemplateLexer> operatorToken,
            Expression right)
        {
            var oper = BinaryOperator.ADD;

            oper = operatorToken.TokenID switch
            {
                TemplateLexer.TIMES => BinaryOperator.MULTIPLY,
                TemplateLexer.DIVIDE => BinaryOperator.DIVIDE,
                _ => BinaryOperator.MULTIPLY
            };

            return new BinaryOperation(left, oper, right);
        }

        [Prefix((int) TemplateLexer.MINUS, Associativity.Right, 100)]
        public Expression unaryNumericExpression(Token<TemplateLexer> operation, Expression value)
        {
            return new Neg(value, operation.Position);
        }

        #endregion


        #region BOOLEAN OPERATIONS

        [Operation((int) TemplateLexer.OR, Affix.InFix, Associativity.Right, 10)]
        public Expression binaryOrExpression(Expression left, Token<TemplateLexer> operatorToken, Expression right)
        {
            return new BinaryOperation(left, BinaryOperator.OR, right);
        }

        [Operation((int) TemplateLexer.AND, Affix.InFix, Associativity.Right, 50)]
        public Expression binaryAndExpression(Expression left, Token<TemplateLexer> operatorToken, Expression right)
        {
            return new BinaryOperation(left, BinaryOperator.AND, right);
        }

        [Operation((int) TemplateLexer.NOT, Affix.PreFix, Associativity.Right, 100)]
        public Expression binaryOrExpression(Token<TemplateLexer> operatorToken, Expression value)
        {
            return new Not(value,operatorToken.Position);
        }

        #endregion
    }
}
