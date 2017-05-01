using sly.lexer;
using sly.parser.generator;

using System.Collections.Generic;


namespace expressionparser
{
    public enum ExpressionToken
    {
        INT = 2, // integer
        DOUBLE = 3, // float number 
        PLUS = 4, // the + operator
        MINUS = 5, // the - operator
        TIMES = 6, // the * operator
        DIVIDE = 7, //  the  / operator
        LPAREN = 8, // a left paranthesis (
        RPAREN = 9,// a right paranthesis )
        WS = 12, // a whitespace
        EOL = 14
    }


    public class ExpressionParser
    {



        [LexerConfigurationAttribute]
        public static Lexer<ExpressionToken> BuildExpressionLexer(Lexer<ExpressionToken> lexer = null)
        {
            if (lexer == null)
            {
                lexer = new Lexer<ExpressionToken>();
            }
            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.DOUBLE, "[0-9]+\\.[0-9]+"));
            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.INT, "[0-9]+"));            
            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.PLUS, "\\+"));
            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.MINUS, "\\-"));
            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.TIMES, "\\*"));
            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.DIVIDE, "\\/"));

            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.LPAREN, "\\("));
            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.RPAREN, "\\)"));

            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.WS, "[ \\t]+", true));
            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.EOL, "[\\n\\r]+", true, true));
            return lexer;
        }




        [Reduction("primary: INT")]
        public static object Primary(List<object> args)
        {
            return ((Token<ExpressionToken>)args[0]).IntValue;
        }

        [Reduction("primary: LPAREN expression RPAREN")]
        public static object Group(List<object> args)
        {
            return args[1];
        }



        [Reduction("expression : term PLUS expression")]
        [Reduction("expression : term MINUS expression")]

        public static object Expression(List<object> args)
        {
            object result = 0;
            int left = (int)args[0];
            int right = (int)args[2];
            ExpressionToken token = ((Token<ExpressionToken>)args[1]).TokenID;
            switch (token)
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
                default:
                    {
                        break;
                    }
            }
            return result;
        }

        [Reduction("expression : term")]
        public static object Expression_Term(List<object> args)
        {
            object result = (int)args[0];
            return result;
        }

        [Reduction("term : factor TIMES term")]
        [Reduction("term : factor DIVIDE term")]
        public static object Term(List<object> args)
        {
            int result = 0;

            int left = (int)args[0];
            int right = (int)args[2];
            ExpressionToken token = ((Token<ExpressionToken>)args[1]).TokenID;
            switch (token)
            {
                case ExpressionToken.TIMES:
                    {
                        result = left * right;
                        break;
                    }
                case ExpressionToken.DIVIDE:
                    {
                        result = left / right;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            return result;
        }

        [Reduction("term : factor")]
        public static object Term_Factor(List<object> args)
        {
            object result = (int)args[0];
            return result;
        }

        [Reduction("factor : primary")]
        [Reduction("factor : MINUS factor")]
        public static object Factor(List<object> args)
        {
            int result = 0;
            switch (args.Count)
            {
                case 1:
                    {
                        result = (int)args[0];
                        break;
                    }
                case 2:
                    {
                        ExpressionToken token = ((Token<ExpressionToken>)args[0]).TokenID;
                        int val = (int)args[1];
                        val = token == ExpressionToken.MINUS ? -val : val;
                        result = val;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            return result;
        }




    }
}

