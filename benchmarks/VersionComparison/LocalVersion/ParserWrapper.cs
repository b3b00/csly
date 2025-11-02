using System;
using sly.lexer;
using sly.parser.generator;

namespace LocalVersion
{
    public enum ExpressionToken
    {
        [Lexeme("[0-9]+")]
        INT = 1,

        [Lexeme("\\+")]
        PLUS = 2,

        [Lexeme("\\-")]
        MINUS = 3,

        [Lexeme("\\*")]
        TIMES = 4,

        [Lexeme("\\/")]
        DIVIDE = 5,

        [Lexeme("\\(")]
        LPAREN = 6,

        [Lexeme("\\)")]
        RPAREN = 7
    }

    public class ExpressionParser
    {
        [Production("expression : term expressionPrime")]
        public int Expression(int term, int prime)
        {
            return term + prime;
        }

        [Production("expressionPrime : PLUS term expressionPrime")]
        public int ExpressionPrimePlus(Token<ExpressionToken> plus, int term, int prime)
        {
            return term + prime;
        }

        [Production("expressionPrime : MINUS term expressionPrime")]
        public int ExpressionPrimeMinus(Token<ExpressionToken> minus, int term, int prime)
        {
            return -term + prime;
        }

        [Production("expressionPrime : ")]
        public int ExpressionPrimeEmpty()
        {
            return 0;
        }

        [Production("term : factor termPrime")]
        public int Term(int factor, int prime)
        {
            return factor * prime;
        }

        [Production("termPrime : TIMES factor termPrime")]
        public int TermPrimeTimes(Token<ExpressionToken> times, int factor, int prime)
        {
            return factor * prime;
        }

        [Production("termPrime : DIVIDE factor termPrime")]
        public int TermPrimeDivide(Token<ExpressionToken> divide, int factor, int prime)
        {
            return factor != 0 ? 1 / (factor * prime) : 0;
        }

        [Production("termPrime : ")]
        public int TermPrimeEmpty()
        {
            return 1;
        }

        [Production("factor : INT")]
        public int Factor(Token<ExpressionToken> intToken)
        {
            return int.Parse(intToken.Value);
        }

        [Production("factor : LPAREN expression RPAREN")]
        public int FactorParens(Token<ExpressionToken> lparen, int expression, Token<ExpressionToken> rparen)
        {
            return expression;
        }
    }

    public class ParserWrapper
    {
        private sly.parser.Parser<ExpressionToken, int> _parser;

        public ParserWrapper()
        {
            var builder = new ParserBuilder<ExpressionToken, int>();
            var buildResult = builder.BuildParser(
                new ExpressionParser(),
                ParserType.EBNF_LL_RECURSIVE_DESCENT,
                "expression"
            );

            if (buildResult.IsError)
            {
                throw new Exception($"Failed to build parser: {string.Join(", ", buildResult.Errors)}");
            }

            _parser = buildResult.Result;
        }

        public void Parse(string input)
        {
            var result = _parser.Parse(input);
            if (result.IsError)
            {
                throw new Exception($"Parse error: {string.Join(", ", result.Errors)}");
            }
        }
    }
}

