### presentation ###

CSLY comes with  two kinds of lexer :
 * a regex based lexer inpired by this  [post](https://blogs.msdn.microsoft.com/drew/2009/12/31/a-simple-lexer-in-c-that-uses-regular-expressions/) 
So it's not a very efficient lexer. Indeed this lexer is slow and is the bottleneck of the whole lexer/parser.  
 * a "generic" lexer that restrict the lexer possibilities but offers way better performance. 


### General configuration ###

The full lexer configuration is done in a C# ```enum```:

The ```enum``` is listing all the possible tokens (no special constraint here except public visibility)

Each ```enum``` value has a ```[Lexeme]``` attribute to mark it has a lexeme. 



**Regex lexer**

The idea of a regex lexer is to associate to every lexeme a matching regex. So a lexeme needs 3  parameters :
 -  ```string regex``` : a regular expression that captures the lexeme
 - ```boolean isSkippable``` (optional, default is ```false```): a boolean ,  true if the lexeme must be ignored ( whitespace for example)
 - ```boolean isLineending``` (optionanl, default is ```false```) : true if the lexeme matches a line end (to allow line counting while lexing).
    
**generic lexer** : 
the idea of the generic lexer is to start from a limited set of classical lexemes and to refine this set to fit your needs.
The basic lexemes are :
 - ```GenericToken.Identifier```: an identifier : only alpha caharacters are accepted (A-Z and a-z). Here may relies the biggest limitation of the generic lexer.
 - ```GenericToken.String``` : a classical string delimited by double quotes "
 - ```GenericToken.Int``` : an int (i.e. a serie of one or more digit)
 - ```GenericToken.Double``` : a float number (decimal separator is dot '.' )
 - ```GenericToken.keyWord``` : a keyword is an identifier with a special meaning (it comes with the same constraint as the ```GenericToken.Identifier```. here again performance comes at the price of less flexibility. This lexeme is configurable.
 - ```GenericToken.SugarToken``` : a general purpose lexeme with no special constraint except the use of a leading alpha char. this lexer is configurable.
 
 To build a generic lexer Lexeme attribute we have 2 different constructors:
  - static generic lexeme. this constructor allows to do a 1 to 1 mapping between a generic token and your lexer token. It uses only one parameter that is the mapped generic token :			```[Lexeme(GenericToken.String)]``` (static lexemes are String, Int , Double  and Identifier)  
  - configurable lexemes (KeyWord and SugarToken). It takes 2 parameters :
  	* the mapped GenericToken
    * the value of the keyword or sugar token.
            	


### How to use ###

The lexer can be used apart from the parser. It provides a method that returns an ```IEnumerable<Token<T>>``` (where T is the tokens ```enum```) from a ```string```



```c#
 IList<Token<T>> tokens = Lexer.Tokenize(source).ToList<Token<T>>();
```


You can also build only a lexer using :

```c#
ILexer<ExpressionToken> lexer = LexerBuilder.BuildLexer<ExpressionToken>();
var tokens = lexer.Tokenize(source).ToList();
```

### full example, for a mathematical parser (regex based) ###

```c#
public enum ExpressionToken
    {
        // float number 
        [Lexeme("[0-9]+\\.[0-9]+")]
        DOUBLE = 1,

        // integer        
        [Lexeme("[0-9]+")]
        INT = 3,

        // the + operator
        [Lexeme("\\+")]
        PLUS = 4,

        // the - operator
        [Lexeme("\\-")]
        MINUS = 5,

        // the * operator
        [Lexeme("\\*")]
        TIMES = 6,

        //  the  / operator
        [Lexeme("\\/")]
        DIVIDE = 7,

        // a left paranthesis (
        [Lexeme("\\(")]
        LPAREN = 8,

        // a right paranthesis )
        [Lexeme("\\)")]
        RPAREN = 9,

        // a whitespace
        [Lexeme("[ \\t]+",true)]
        WS = 12, 

        [Lexeme("[\\n\\r]+", true, true)]
        EOL = 14
    }
```

### full example, for a dumb language (generic token based) ###
```c#
  public enum WhileTokenGeneric
    {

        #region keywords 0 -> 19

        [Lexeme(GenericToken.KeyWord,"if")]
        IF = 1,

        [Lexeme(GenericToken.KeyWord, "then")]
        THEN = 2,

        [Lexeme(GenericToken.KeyWord, "else")]
        ELSE = 3,

        [Lexeme(GenericToken.KeyWord, "while")]
        WHILE = 4,

        [Lexeme(GenericToken.KeyWord, "do")]
        DO = 5,

        [Lexeme(GenericToken.KeyWord, "skip")]
        SKIP = 6,

        [Lexeme(GenericToken.KeyWord, "true")]
        TRUE = 7,

        [Lexeme(GenericToken.KeyWord, "false")]
        FALSE = 8,
        [Lexeme(GenericToken.KeyWord, "not")]
        NOT = 9,

        [Lexeme(GenericToken.KeyWord, "and")]
        AND = 10,

        [Lexeme(GenericToken.KeyWord, "or")]
        OR = 11,

        [Lexeme(GenericToken.KeyWord, "(print)")]
        PRINT = 12,

        #endregion

        #region literals 20 -> 29

        [Lexeme(GenericToken.Identifier)]
        IDENTIFIER = 20,

        [Lexeme(GenericToken.String)]
        STRING = 21,

        [Lexeme(GenericToken.Int)]
        INT = 22,

        #endregion

        #region operators 30 -> 49

        [Lexeme(GenericToken.SugarToken,">")]
        GREATER = 30,

        [Lexeme(GenericToken.SugarToken, "<")]
        LESSER = 31,

        [Lexeme(GenericToken.SugarToken, "==")]
        EQUALS = 32,

        [Lexeme(GenericToken.SugarToken, "!=")]
        DIFFERENT = 33,

        [Lexeme(GenericToken.SugarToken, ".")]
        CONCAT = 34,

        [Lexeme(GenericToken.SugarToken, ":=")]
        ASSIGN = 35,

        [Lexeme(GenericToken.SugarToken, "+")]
        PLUS = 36,

        [Lexeme(GenericToken.SugarToken, "-")]
        MINUS = 37,


        [Lexeme(GenericToken.SugarToken, "*")]
        TIMES = 38,

        [Lexeme(GenericToken.SugarToken, "/")]
        DIVIDE = 39,

        #endregion 

        #region sugar 50 ->

        [Lexeme(GenericToken.SugarToken, "(")]
        LPAREN = 50,

        [Lexeme(GenericToken.SugarToken, ")")]
        RPAREN = 51,

        [Lexeme(GenericToken.SugarToken, ";")]
        SEMICOLON = 52,

    

        EOF = 0

        #endregion

    }
```
