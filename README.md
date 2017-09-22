# C# Lex Yacc #

[![Build status](https://ci.appveyor.com/api/projects/status/n9uffgkqn2qet7k9?svg=true)](https://ci.appveyor.com/project/OlivierDuhart/sly)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/b3b00/sly/blob/dev/LICENSE)
[![NuGet version](https://img.shields.io/nuget/v/sly.svg)](https://www.nuget.org/packages/sly/)


 #LY is a parser generator halfway between parser combinators and parser generator like 


## Why?

I needed a solution for building  parsers and found all existing solution either 
 - too complicated to integrate with an additional build step as with [ANTLR](http://www.antlr.org/) )  
 - or too different from the classical BNF notation (as parser combinators like [sprache](https://github.com/sprache/Sprache) or [Eto.Parse](https://github.com/picoe/Eto.Parse)). These tools are great, but I don't feel comfortable with them.

## General principle

SLY is highly inspired by the python lex yacc library ([PLY](http://www.dabeaz.com/ply/))

The parser and lexer implementations fully reside in a single class.
The class describes 
 - every token definition for the lexer
 - every grammar rule and its associated action to transform the syntaxic tree.  
 

## Installation

Install from the NuGet gallery GUI or with the Package Manager Console using the following command:

```Install-Package sly```

or with dotnet core 

```dotnet add package sly```


## Lexer ##

### presentation ###

For now the lexer is a poor man regex based lexer inspired by this [post](https://blogs.msdn.microsoft.com/drew/2009/12/31/a-simple-lexer-in-c-that-uses-regular-expressions/) 
So it's not a very efficient lexer. Indeed this lexer is slow and is the bottleneck of the whole lexer/parser.  
It could be improved in the future.

### configuration ###

The full lexer configuration is done in a C# ```enum```:

The ```enum``` is listing all the possible tokens (no special constraint here except public visibility)

Each ```enum``` value has a ```[Lexeme]``` attribute to mark it has a lexeme. The lexeme attribute takes 3 parameters:
 -  ```string regex``` : a regular expression that captures the lexeme
 - ```boolean isSkippable``` (optional, default is ```false```): a boolean ,  true if the lexeme must be ignored ( whitespace for example)
 - ```boolean isLineending``` (optionanl, default is ```false```) : true if the lexeme matches a line end (to allow line counting while lexing).


The lexer can be used apart from the parser. It provides a method that returns an ```IEnumerable<Token<T>>``` (where T is the tokens ```enum```) from a ```string```



```c#
 IList<Token<T>> tokens = Lexer.Tokenize(source).ToList<Token<T>>();
```


You can also build only a lexer using :

```c#
ILexer<ExpressionToken> lexer = LexerBuilder.BuildLexer<ExpressionToken>();
var tokens = lexer.Tokenize(source).ToList();
```

### full example, for a mathematical parser ###

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



## Parser ##

### Parser typing ###

A parser is  of type Parser<TIn,TOut> where :
* TIn is the enum token type as seen before 
* TOut is the type of object produced bye the parser. Classicaly it will be an Asbtract Syntax Tree (AST) or it may be an int for an expression parser.



### Grammar definition ###

The grammar defining the parser is defined using C# attribute ```[Production("some grammar rule")]``` mapped to methods ( in the same class used for the lexer)

The rules follow the classical BNF notation.
A terminal notation must exactly matches (case sensitive) an enum value.
Once the wytaxic tree build, the methods of each rule will be used as a syntaxic tree visitor.
Each methods takes as many parameters as rule clauses. Each parameter can be typed according to the return value of the clause :
- for a terminal : the ```Token<T>``` corresponding to the token
- for a non terminal : the result of the evaluation of the non terminal, i.e the value returned by the matching static method. As the parser output is typed (TOut as seen before) , the result of an evaluation for a non terminal is necessarily of type TOut.

  
### partial example for a mathematical expression evaluator ###

a mathematical parser calculate a mathematical expression. It takes a string as input and return a numeric value. So each method of the parser will return a numeric value (an int for simplicity concern)


```c#


         [Production("primary: INT")]
        public int Primary(Token<ExpressionToken> intToken)
        {
            return intToken.IntValue;
        }

        [Production("primary: LPAREN expression RPAREN")]
        public int Group(object discaredLParen, int groupValue ,object discardedRParen)
        {
            return groupValue;
        }



        [Production("expression : term PLUS expression")]
        [Production("expression : term MINUS expression")]

        public int Expression(int left, Token<ExpressionToken> operatorToken, int  right)
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

- lexer configuration (with the ```[LexerConfiguration]``` attribute )

- grammar rules (with the ```[Production("")]``` attribute )

Once the class with all its methods has been written, it can be used to build the effective parser instance calling ParserBuilder.BuildParser. the builder methods takes 3 parameters :


1. an instance of the class containing the lexer and parser definition
2. the kind of parser. Currently only a recursive descent parsers are available. this implementation is limited to LL grammar by construction (no left recursion).There are 2 possible types :
	- ```ParserType.LL_RECURSIVE_DESCENT``` : a [BNF](https://fr.wikipedia.org/wiki/Forme_de_Backus-Naur) notation grammar parser
    - ```ParserType.EBNF_LL_RECURSIVE_DESCENT``` : a [EBNF](https://fr.wikipedia.org/wiki/Extended_Backus-Naur_Form) notation grammar parser. EBNF notation provides additional multiplier notation (* and + for now)
3. the root rule for the parser.   

the parser is typed according to the token type.

```c#
ExpressionParser expressionParserDefinition = new ExpressionParser()
// here see the typing :
//  ExpressionToken is the token enum type
//  int is the type of a parse evaluation
Parser<ExpressionToken,int> Parser = ParserBuilder.BuildParser<ExpressionToken,int>(expressionParserDefinition,
                                                                            ParserType.LL_RECURSIVE_DESCENT,
                                                                            "expression");


then calling 
```C#parser.Parse("some source code")``` 
will return the evaluation of the syntax tree.
the parser returns a ParseResult instance containing the evaluation value or a list of errors.

```c#

	string expression = "1 + 1";

    ParseResult<ExpressionToken> r = Parser.Parse(expression);


    if (!r.IsError && r.Result != null && r.Result is int)
    {
        Console.WriteLine($"result of {expression}  is {(int)r.Result}");
    }
    else
    {
        if (r.Errors !=null && r.Errors.Any())
        {
        	// display errors
            r.Errors.ForEach(error => Console.WriteLine(error.ErrorMessage));
        }
    }
```

### access lexer and parsers
One build a parser expose :

- a main API through the ```Parse(string content)``` method (chain lexical analysis, syntax parsing and finally call your parsing methods)

- the lexer through the Lexer property

- the syntax parser  through the SyntaxParser property (which type is a ```ISyntaxParser```)

## Full examples ##

Full examples are available under :
- [jsonparser](https://github.com/b3b00/sly/blob/master/jsonparser/JSONParser.cs) : a json parser
- [expressionParser](https://github.com/b3b00/sly/blob/master/expressionParser/ExpressionParser.cs) : a mathematical expression parser
- You can also look at Tests that presents a simple EBNF grammar in [EBNFTests](https://github.com/b3b00/sly/blob/master/ParserTests/EBNFTests.cs)


## EBNF notation ##

you can now use EBNF notation :
 - '*' to repeat 0 or more the same terminal or non terminal
 - '+' to repeat once or more the same terminal or non terminal
 
 
 
 for repeated elements values passed to ```[Production]``` methods are :
 * ```List<TOut>``` for a repeated non terminal
 * ```List<Token<TIn>>``` for a repeated terminal
 
 See [EBNFJsonParser.cs](https://github.com/b3b00/csly/blob/master/jsonparser/EBNFJSONParser.cs) for a complete EBNF json parser.
 
#### under the hood meta consideration on EBNF parsers ####

The EBNF notation has been implemented in CSLY using the BNF notation. The EBNF parser builder is built using the BNF parser builder. Incidently the EBNF parser builder is a good and complete example for BNF parser : [RuleParser.cs](https://github.com/b3b00/csly/blob/master/sly/parser/generator/RuleParser.cs)
 
