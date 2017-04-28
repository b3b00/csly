# sharp Lex Yacc #

 #LY is a parser generator halfway between parser combinators and parser generator like [ANTLR](http://www.antlr.org/) 

It provides a way to build a full lexer and parser using only C# with no extra build step. The goal of this parser generator is not to build highly efficient parser but rather rapid prototyping or small DSL embedded in .Net solutions.   

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
``` 


## Parser ##

The grammar defining the parser is defined using C# attribute [Reduction("some grammar rule")] mapped to static methods ( in the same class used for the lexer)
The rules follow the classical BNF notation.
A terminal notation must exactly matches (case sensitive) an enum value.
Once build the methods of each rule will be used as a syntaxic tree visitor.
Each methods takes a List<object> as a parameters. This list contains the right part

  
### full example, for a mathematical parser ###



