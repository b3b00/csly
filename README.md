# C# Lex Yacc #


![Test status](http://teststatusbadge.azurewebsites.net/api/status/mmaitre314/securestringcodegen)
[![coveralls](https://coveralls.io/repos/github/b3b00/csly/badge.svg?branch=dev)](https://coveralls.io/github/b3b00/csly?branch=dev)
![.NET Core](https://github.com/b3b00/csly/workflows/.NET%20Core/badge.svg)
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fb3b00%2Fcsly.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2Fb3b00%2Fcsly?ref=badge_shield)


[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/b3b00/sly/blob/dev/LICENSE)
[![NuGet](https://img.shields.io/nuget/v/sly.svg)](https://www.nuget.org/packages/sly)


CSLY is highly inspired by the python lex yacc library ([PLY](http://www.dabeaz.com/ply/)) that aims at easen the writing of lexer and parser with C#.

##[Getting started](https://github.com/b3b00/csly/wiki/getting-started) ##

a [quick getting started](https://github.com/b3b00/csly/wiki/getting-started) will guide you through the implementation of a dumb parser


## CSLY special features ##

CSLY provide some special features that make it easier or safer to use.

### fully embedded ###

CSLY has been thought to avoid any extra build step. Parser generators often need a build time step to generate target language source code that do the parse job.
Juste include a nuget and configure your lexer/parser in pure C# code.

>### CSLY does not need a build time step and easen your build / CI process

### compact lexer/parser definition ### 

A lexer/parser is defined with only 2 files : 
    * a C# ```enum``` for the lexer
    * a C# ```class``` for the parser

Lexeme and parser production rules are defined with c# ```attributes``` making notation even more compact.
this features already exists with parser combinators (like [sprache](https://github.com/sprache/Sprache) or [Eto.Parse](https://github.com/picoe/Eto.Parse)), but productions rules are defined using either [BNF](https://github.com/b3b00/csly/wiki/BNF-Parser) or  [EBNF](https://github.com/b3b00/csly/wiki/EBNF-Parser)  notation which I think is more natural and easier to understand for maintenance.


>###  A full language is defined in a very compact and isolated way. 

See [Lexer](https://github.com/b3b00/csly/wiki/Lexer) for lexers definition.
And [BNF](https://github.com/b3b00/csly/wiki/BNF-Parser) or  [EBNF](https://github.com/b3b00/csly/wiki/EBNF-Parser) for parser definitions.

### strict typed ### 

 CSLY is strictly typed, every parser is defines according to its input and output types. For further reading about parser typing, head to [typing section](typing) to correctly type your parser.

>### This feature allows you to be more confident in input and output validity.

### expression parsing ### 

Many language needs parsing expressions (boolean or numeric).
 A recursive descent parser is hard to maintain when parsing expressions with multiple precedence levels.
 So CSLY offers a way to express expression parsing using only operator tokens and precedence level.
 CSLY will then generates production rules to parse expressions. It also manages precedence and left or right associativity.

>### get rid of the burden of writing an expression parser.

see [expression parsing](https://github.com/b3b00/csly/wiki/expression-parsing)


### generic lexer ### 

>### reuse common token definition and take avantage of a better lexer performance.


See [Generic lexer](https://github.com/b3b00/csly/wiki/GenericLexer) for generic lexer and [Lexer](https://github.com/b3b00/wiki/Lexer) for lexer general presentation.


### What is and what is not CSLY ###

Not a fully featured parser generator like [ANTLR](http://www.antlr.org/), do not use it to define a full featured language.
Dedicated at small DSL ([Domain-Specific Language](https://en.wikipedia.org/wiki/Domain-specific_language))

## Installation ##

Install from the NuGet gallery GUI or with the Package Manager Console using the following command:

```Install-Package sly```

or with dotnet core 

```dotnet add package sly```




## License
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fb3b00%2Fcsly.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2Fb3b00%2Fcsly?ref=badge_large)
