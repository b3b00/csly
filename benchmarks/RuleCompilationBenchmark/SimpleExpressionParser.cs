using System;
using sly.lexer;
using sly.parser.generator;
using sly.parser;

namespace RuleCompilationBenchmark
{
    /// <summary>
    /// Simple expression parser for benchmarking
    /// Supports: numbers, +, -, *, /, parentheses
    /// </summary>
    public class SimpleExpressionParser
    {
        [NodeName("integer")]
        [Production("primary: NUMBER")]
        public int Primary(Token<ExpressionToken> intToken)
        {
            return intToken.IntValue;
        }

        [NodeName("group")]
        [Production("primary: LPAREN [d] expression RPAREN [d]")]
        public int Group(int groupValue)
        {
            return groupValue;
        }


        [NodeName("addOrSubstract")]
        [Production("expression : term PLUS expression")]
        [Production("expression : term MINUS expression")]
        public int Expression(int left, Token<ExpressionToken> operatorToken, int right)
        {
            var result = 0;


            switch (operatorToken.TokenID)
            {
                case ExpressionToken.PLUS:
                {
                    result = left + right;
                    break;
                }
                case ExpressionToken.MINUS:
                {
                    result = left - right;
                    break;
                }
            }

            return result;
        }

        
        [NodeName("expression")]
        [Production("expression : term")]
        public int Expression_Term(int termValue)
        {
            return termValue;
        }

        [NodeName("multOrDivide")]
        [Production("term : factor MULTIPLY term")]
        [Production("term : factor DIVIDE term")]
        public int Term(int left, Token<ExpressionToken> operatorToken, int right)
        {
            var result = 0;


            switch (operatorToken.TokenID)
            {
                case ExpressionToken.MULTIPLY:
                {
                    result = left * right;
                    break;
                }
                case ExpressionToken.DIVIDE:
                {
                    result = left / right;
                    break;
                }
            }

            return result;
        }

        [Production("term : factor")]
        [NodeName("term")]
        public int Term_Factor(int factorValue)
        {
            return factorValue;
        }

        [Production("factor : primary")]
        [NodeName("primary")]
        public int primaryFactor(int primValue)
        {
            return primValue;
        }

        [NodeName("negate")]
        [Production("factor : MINUS factor")]
        public int Factor(Token<ExpressionToken> discardedMinus, int factorValue)
        {
            return -factorValue;
        }
    }
}

