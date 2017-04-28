# sharp Lex Yacc #

 #LY is a parser generator halfway between parser combinators and parser generator like [ANTLR](http://www.antlr.org/) 

It provides a way to build a full lexer and parser using only C# with no extra build step. The goal of this parser generator is not to build highly efficient parser but rather rapid prototyping or small DSL embedded in .Net solutions.   
It is highly inspired by the python lex yacc library ([PLY](http://www.dabeaz.com/ply/))

The parser implementation fully resides in a single static class.    

## Lexer ##

###presentation###
For now the lexer is a poor man regex based lexer inspired by this [post](https://blogs.msdn.microsoft.com/drew/2009/12/31/a-simple-lexer-in-c-that-uses-regular-expressions/) 
So it's not a very efficient lexer. Indeed this lexer is slow and is the bottleneck of the whole lexer/parser.  
It could be improved in the future.

### configuration ###

To configure a lexer 2 items has to be done :


- an <span style="color:blue">**enum**</span>  listing all the possible tokens (no special constraint here except public visibility)
- a static method with special attribute [LexerConfigurationAttribute] that associates tokens from the above enum with matching regex.
 
the configuration method takes a Lexer<T> (where T is the tokens <span style="color:blue">**enum**</span>  parameters and returns the same lexer after having added (token,regex) associations.

The lexer can be used apart from the parser. It provides a method that returns an IEnumerable<Token<T>> (where T is the tokens <span style="color:blue">**enum**</span>) from a <span style="color:blue">**string**</span>

```c#
 IList<Token<T>> tokens = Lexer.Tokenize(source).ToList<Token<T>>();
```

### full example, for a mathematical parser ###

#### the tokens enum ####

```c#

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

		EOL = 14 // an end of line

    }


####regular expressions
```c#

 		[LexerConfigurationAttribute]
        public static Lexer<ExpressionToken> BuildExpressionLexer(Lexer<ExpressionToken> lexer = null)
        {
 			if (lexer == null)
            {
                lexer = new Lexer<ExpressionToken>();
            }

            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.DOUBLE, "[0-9]+\\.[0-9]+"));
            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.INT, "[0-9]+"));
            //lexer.AddDefinition(new TokenDefinition<JsonToken>(JsonToken.IDENTIFIER, "[A-Za-z0-9_àâéèêëîô][A-Za-z0-9\u0080-\u00FF_àâéèêëîô°]*"));
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
``` 


## Parser ##

The grammar defining the parser is defined using C# attribute [Reduction("some grammar rule")] mapped to static methods ( in the same class used for the lexer)
The rules follow the classical BNF notation.
A terminal notation must exactly matches (case sensitive) an enum value.
Once build the methods of each rule will be used as a syntaxic tree visitor.
Each methods takes a List<object> as a parameters. This list contains the values of the evalation of each clause of the right part of a rule.
The values of the clauses are :
- for a terminal : the Token<T> corresponding to the token
- for a non terminal : the result of the evaluation of the non terminal, i.e the value returned by the matching static method. 

  
### full example, for a mathematical parser ###


```c#


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

``` 
