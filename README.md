# C# Lex Yacc #

[![Build status](https://ci.appveyor.com/api/projects/status/n9uffgkqn2qet7k9?svg=true)](https://ci.appveyor.com/project/OlivierDuhart/sly)
![AppVeyor tests (compact)](https://img.shields.io/appveyor/tests/OlivierDuhart/sly.svg?compact_message)
[![coveralls](https://coveralls.io/repos/github/b3b00/csly/badge.svg?branch=dev)](https://coveralls.io/github/b3b00/csly?branch=dev)
![.NET Core](https://github.com/b3b00/csly/workflows/.NET%20Core/badge.svg)
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fb3b00%2Fcsly.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2Fb3b00%2Fcsly?ref=badge_shield)


[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/b3b00/sly/blob/dev/LICENSE)
[![NuGet](https://img.shields.io/nuget/v/sly.svg)](https://www.nuget.org/packages/sly)


:warning: This readme is a bit out of date. Go to to the [wiki](https://github.com/b3b00/csly/wiki) for a more up to date documentation.

 CSLY is a parser generator halfway between parser combinators and parser generator like Yacc or ANTLR 


## Why? ##

I needed a solution for building  parsers and found all existing solution either 
 - too complicated to integrate with an additional build step as with [ANTLR](http://www.antlr.org/) )  
 - or too different from the classical BNF notation (as parser combinators like [sprache](https://github.com/sprache/Sprache) or [Eto.Parse](https://github.com/picoe/Eto.Parse)). These tools are great, but I don't feel comfortable with them.

## General presentation ##

SLY is highly inspired by the python lex yacc library ([PLY](http://www.dabeaz.com/ply/))

A lexer - parser chain is fully described in only 2 C# files :
 * [Lexer](#lexers) : an enum that lists all the lexems used (plus metadata to describe their patterns) ;
 * [Parser](#parsers) : a class that lists production rules and their associated actions.
 
CSLY also has an additional feature that allow to write expression parsers(boolean or numeric expressions for instance) in a very compact and efficient way. (see [expression parser](#expressions-parser))
 

## Installation ##

Install from the NuGet gallery GUI or with the Package Manager Console using the following command:

```Install-Package sly```

or with dotnet core 

```dotnet add package sly```


## Lexers ##

Yes 2 Lexers and not just Lexer. CSLY comes with 2 lexers :
 * a [regex based lexer](https://github.com/b3b00/csly/wiki/RegexLexer) very flexible but with some performance issues;
 * a "[generic lexer](https://github.com/b3b00/csly/wiki/GenericLexer)" based on a Finite State Machine that adresses the performance issue at the cost of some lesser flexibility.This lexer is inspired by this [post](https://blogs.msdn.microsoft.com/drew/2009/12/31/a-simple-lexer-in-c-that-uses-regular-expressions/) 

The full lexers documentation can be found in the  
[lexer wiki](https://github.com/b3b00/csly/wiki/Lexer)


### full example, for a arithmetic expression parser using the regex lexer

Here is a lexer definition for a arithmetic expression parser using the generic lexer.

```c#
using sly.lexer;

namespace simpleExpressionParser
{
    public enum SimpleExpressionToken
    {
        // float number 
        [Lexeme(GenericToken.Double)]
        DOUBLE = 1,

        // integer        
        [Lexeme(GenericToken.Int)]
        INT = 3,
        
        [Lexeme(GenericToken.Identifier)]
        IDENTIFIER = 4,

        // the + operator
        [Lexeme(GenericToken.SugarToken,"+")]
        PLUS = 5,

        // the - operator
        [Lexeme(GenericToken.SugarToken,"-")]
        MINUS = 6,

        // the * operator
        [Lexeme(GenericToken.SugarToken,"*")]
        TIMES = 7,

        //  the  / operator
        [Lexeme(GenericToken.SugarToken,"/")]
        DIVIDE = 8,

        // a left paranthesis (
        [Lexeme(GenericToken.SugarToken,"(")]
        LPAREN = 9,

        // a right paranthesis )
        [Lexeme(GenericToken.SugarToken,")")]
        RPAREN = 10,

    }
}
```



## Parsers ##

### Typed Parser ###

A parser is  of type Parser &lt;TIn,TOut&gt; where :
* TIn is the enum token type as seen before 
* TOut is the type of object produced bye the parser. Classicaly it will be an Asbtract Syntax Tree (AST) or it may be an int for an expression parser.



### Grammar definition ###

The grammar defining the parser is defined using C# attribute ```[Production("some grammar rule")]``` mapped to methods ( in the same class used for the lexer)

Production rules can used :
 * [BNF notation](https://github.com/b3b00/csly/wiki/BNF-Parser)
 * [EBNF notation](https://github.com/b3b00/csly/wiki/EBNF-Parser) : using multiplier operator 
 	* zero or more : \*
    * one or more : \+
    * zero or more (optional) : \?


A terminal notation must exactly matche (case sensitive) an enum value.
Once the syntaxic tree build, the methods of each rule will be used as a syntaxic tree visitor.
Each production rule is associated to a method that acts as a visitor for the syntaxic tree.

  
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

as we 've seen above a parser is declared on only 2 files :
	* the lexer enum 
    * the parser class

Once the class with all its methods has been written, it can be used to build the effective parser instance calling ParserBuilder.BuildParser. the builder methods takes 3 parameters :


1. an instance of the class containing the lexer and parser definition
2. the kind of parser. Currently only a recursive descent parsers are available. this implementation is limited to LL grammar by construction (no left recursion).There are 2 possible types :
	- ```ParserType.LL_RECURSIVE_DESCENT``` : a [BNF](https://fr.wikipedia.org/wiki/Forme_de_Backus-Naur) notation grammar parser
    - ```ParserType.EBNF_LL_RECURSIVE_DESCENT``` : a [EBNF](https://fr.wikipedia.org/wiki/Extended_Backus-Naur_Form) notation grammar parser. 
3. the root rule for the parser (grammar entrypoint).   

the parser is typed according to the token type and output (int for our expression parser).

```c#
ExpressionParser expressionParserDefinition = new ExpressionParser()
// here see the typing :
//  ExpressionToken is the token enum type
//  int is the type of a parse evaluation
Parser<ExpressionToken,int> Parser = ParserBuilder.BuildParser<ExpressionToken,int>(expressionParserDefinition,
                                                                            ParserType.LL_RECURSIVE_DESCENT,
                                                                            "expression");


then calling 
```var result = C#parser.Parse("2 + 2")``` 
will return the evaluation of the syntax tree.
the parser returns a ParseResult instance containing the evaluation value or a list of errors.

```c#

	string expression = "2 + 2";

    ParseResult<ExpressionToken> r = Parser.Parse(expression);


    if (!r.IsError && r.Result != null && r.Result is int)
    {
        Console.WriteLine($"result of <{expression}>  is {(int)r.Result}");
        // outputs : result of <2 + 2>  is 4"
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





 
 
 
## expressions parser ##
 
 
 Many language needs parsing expressions (boolean or numeric).
 A recursive descent parser is hard to maintain when parsing expressions with multiple precedence levels.
 So CSLY offers a way to express expression parsing using only operator tokens and precedence level.
 CSLY will then generates production rules to parse expressions. It also manages precedence and left or right associativity.
 
 here is a parser for a classical numeric expression parser using classical precedence and associativity.

Full expression parser generator documentation can be found on the (https://github.com/b3b00/csly/wiki/expression-parsing)[expression parsing wiki]

```c#

using sly.lexer;
using sly.parser.generator;

namespace simpleExpressionParser
{
    public class SimpleExpressionParser
    {        
      
        [Operation((int)ExpressionToken.PLUS, 2, Associativity.Right, 10)]
        [Operation((int)ExpressionToken.MINUS, 2, Associativity.Left, 10)]
        public int binaryTermExpression(int left, Token<ExpressionToken> operation, int right)
        {
            int result = 0;
            switch (operation.TokenID)
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

        
        [Operation((int)ExpressionToken.TIMES, 2, Associativity.Right, 50)]
        [Operation((int)ExpressionToken.DIVIDE, 2, Associativity.Left, 50)]
        public int binaryFactorExpression(int left, Token<ExpressionToken> operation, int right)
        {
            int result = 0;
            switch (operation.TokenID)
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
            }
            return result;
        }


        [Operation((int)ExpressionToken.MINUS, 1, Associativity.Right, 100)]
        public  int unaryExpression(Token<ExpressionToken> operation, int value)
        {
            return -value;
        }

        [Operand]
        [Production("operand : primary_value")]        
        public int operand(int value)
        {
            return value;
        }


        [Production("primary_value : INT")]
        public int operand1(Token<ExpressionToken> value)
        {
            return value.IntValue;
        }

        [Production("primary_value : LPAREN SimpleExpressionParser_expressions RPAREN")]
        public int operand2(Token<ExpressionToken> lparen, int value, Token<ExpressionToken> rparen)
        {
            return value;
        }
    }
}
   
```




## Full examples ##

Full examples are available under :
- [jsonparser](https://github.com/b3b00/sly/blob/master/jsonparser/JSONParser.cs) : a json parser
- [expressionParser](https://github.com/b3b00/sly/blob/master/expressionParser/ExpressionParser.cs) : a mathematical expression parser
- [While Language](https://github.com/b3b00/csly/tree/master/samples/while) : a dummy language inspired by this [parsec sample](https://wiki.haskell.org/Parsing_a_simple_imperative_language)
- You can also look at Tests that presents a simple EBNF grammar in [EBNFTests](https://github.com/b3b00/sly/blob/master/ParserTests/EBNFTests.cs)


## License
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fb3b00%2Fcsly.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2Fb3b00%2Fcsly?ref=badge_large)
