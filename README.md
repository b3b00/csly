# sharp Lex Yacc #

 #LY is a parser generator halfway between parser combinators and parser generator like [ANTLR](http://www.antlr.org/) 

It provides a way to build a full lexer and parser using only C# with no extra build step. The goal of this parser generator is not to build highly efficient parser but rather rapid prototyping or small DSL embedded in .Net solutions.   
It is highly inspired by the python lex yacc library ([PLY](http://www.dabeaz.com/ply/))

The parser implementation fully resides in a single static class.    

## Lexer ##

### presentation ###

For now the lexer is a poor man regex based lexer inspired by this [post](https://blogs.msdn.microsoft.com/drew/2009/12/31/a-simple-lexer-in-c-that-uses-regular-expressions/) 
So it's not a very efficient lexer. Indeed this lexer is slow and is the bottleneck of the whole lexer/parser.  
It could be improved in the future.

### configuration ###

To configure a lexer 2 items has to be done :


- an <span style="color:blue">**enum**</span>  listing all the possible tokens (no special constraint here except public visibility)
- a method with special attribute [LexerConfigurationAttribute] that associates tokens from the above enum with matching regex.
 
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
        public Lexer<ExpressionToken> BuildExpressionLexer(Lexer<ExpressionToken> lexer = null)
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
``` 


## Parser ##

The grammar defining the parser is defined using C# attribute [Reduction("some grammar rule")] mapped to static methods ( in the same class used for the lexer)
The rules follow the classical BNF notation.
A terminal notation must exactly matches (case sensitive) an enum value.
Once build the methods of each rule will be used as a syntaxic tree visitor.
Each methods takes as many parameters as rule clauses. Each parameter can be typed according to the return value of the clause :
- for a terminal : the Token<T> corresponding to the token
- for a non terminal : the result of the evaluation of the non terminal, i.e the value returned by the matching static method. 

  
### partial example for a mathematical expression evaluator ###

a mathematical parser calculate a mathematical expression. It takes a string as input and return a numeric value. So each static method of the parser will return a numeric value (an int for simplicity concern)


```c#


         [Reduction("primary: INT")]
        public object Primary(Token<ExpressionToken> intToken)
        {
            return intToken.IntValue;
        }

        [Reduction("primary: LPAREN expression RPAREN")]
        public object Group(object discaredLParen, int groupValue ,object discardedRParen)
        {
            return groupValue;
        }



        [Reduction("expression : term PLUS expression")]
        [Reduction("expression : term MINUS expression")]

        public object Expression(int left, Token<ExpressionToken> operatorToken, int  right)
        {
            object result = 0;
            

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
                default:
                    {
                        break;
                    }
            }
            return result;
        }

       

``` 
## Building a parser and using it ##

as we 've seen above a parser is declared on a single class with static methods that address :

- lexer configuration (with the [LexerConfigurationAttribute] attribute )

- grammar rules (with the [Reduction("")] attribute )

Once the class with all its methods has been written, it can be used to build the effective parser instance calling ParserBuilder.BuildParser. the builder methods takes 3 parameters :


1. an instance of the class containing the lexer and parser definition
2. the kind of parser. Currently only a recursive descent parser is available. this implementation is limited to LL grammar by construction (no left recursion) : ParserType.LL_RECURSIVE_DESCENT
3. the root rule for the parser.   

the parser is typed according to the token type.

```c#
ExpressionParser expressionParserDefinition = new ExpressionParser()
Parser<ExpressionToken> Parser = ParserBuilder.BuildParser<ExpressionToken>(expressionParserDefinition,
                                                                            ParserType.LL_RECURSIVE_DESCENT,
                                                                            "expression");
```

then calling parser.Parse("some source code") will return the evaluation of the syntax tree.
the parser returns a ParseResult instance containing the evaluation value or a list of errors.

```c#

    ParseResult<ExpressionToken> r = Parser.Parse("1 + 1");


    if (!r.IsError && r.Result != null && r.Result is int)
    {
        Console.WriteLine($"result is {(int)r.Result}");
    }
    else
    {
        if (r.Errors !=null && r.Errors.Any())
        {
            r.Errors.ForEach(error => Console.WriteLine(error.ErrorMessage));
        }
    }
```

### access lexer and parsers
One build a parser expose :

- a main API through the Parse(string content) method (chain lexical analysis, syntax parsing and finally call your parsing methods)

- the lexer through the Lexer property

- the syntax parser  through the SyntaxParser property (which type is ISyntaxParser)

## Full examples ##

Full examples are available under :
- [jsonparser](https://github.com/b3b00/sly/blob/master/jsonparser/JSONParser.cs) : a json parser
- [expressionParser](https://github.com/b3b00/sly/blob/master/expressionParser/ExpressionParser.cs) : a mathematical expression parser
- You can also look at Tests


## EBNF notation ##

you can now use EBNF notation :
 - '*' to repeat 0 or more the same terminal or non terminal
 - '+' to repeat once or more the same terminal or non terminal
