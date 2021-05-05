# C# Lex Yacc #


![Test status](http://teststatusbadge.azurewebsites.net/api/status/mmaitre314/securestringcodegen)
[![coveralls](https://coveralls.io/repos/github/b3b00/csly/badge.svg?branch=dev)](https://coveralls.io/github/b3b00/csly?branch=dev)
![.NET Core](https://github.com/b3b00/csly/workflows/.NET%20Core/badge.svg)
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fb3b00%2Fcsly.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2Fb3b00%2Fcsly?ref=badge_shield)


[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/b3b00/sly/blob/dev/LICENSE)
[![NuGet](https://img.shields.io/nuget/v/sly.svg)](https://www.nuget.org/packages/sly)


CSLY is highly inspired by the python lex yacc library ([PLY](http://www.dabeaz.com/ply/)) and aims at easen the writing of lexer and parser with C#.

## Getting started ##

If you're too impatient to further read this readme here is 
a [quick getting started](https://github.com/b3b00/csly/wiki/getting-started) that will guide you through the implementation of a dumb parser.

## full documentation ## 

Complete documentation can be found in the [wiki](https://github.com/b3b00/csly/wiki)


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


>###  Define languages in a very compact and isolated way. 

See [Lexer](https://github.com/b3b00/csly/wiki/Lexer) for lexers definition.
And [BNF](https://github.com/b3b00/csly/wiki/BNF-Parser) or  [EBNF](https://github.com/b3b00/csly/wiki/EBNF-Parser) for parser definitions.

### Fully and Strictly typed ### 

 CSLY is strictly typed, every parser is defines according to its input and output types. For further reading about parser typing, head to [typing section](typing) to correctly type your parser.

>### Be more confident in parser inputs and outputs validity.

### expression parsing ### 

Many language needs parsing expressions (boolean or numeric).
 A recursive descent parser is hard to maintain when parsing expressions with multiple precedence levels.
 So CSLY offers a way to express expression parsing using only operator tokens and precedence level.
 CSLY will then generates production rules to parse expressions. It also manages precedence and left or right associativity.

>### Get rid of the burden of writing an expression parser.

see [expression parsing](https://github.com/b3b00/csly/wiki/expression-parsing)


### generic lexer ### 

Lexemes are often similar from one language to another. So CSLY introduces a Generic Lexer that defines common lexemes that can be reused across languages. furthermore the generic has better performance than a regex based lexer.

>### Reuse common token definition and take avantage of a better lexer performance.


See [Generic lexer](https://github.com/b3b00/csly/wiki/GenericLexer) for generic lexer and [Lexer](https://github.com/b3b00/wiki/Lexer) for general presentation.


### What is and what is not CSLY ###

#### CSLY is not #### 

CSLY is not a full featured parser generator like [ANTLR](http://www.antlr.org/).
Hence you should not use it to define a full featured language (say C# or Java).

#### CSLY is #### 

CSLY is dedicated to small [Domain-Specific Languages](https://en.wikipedia.org/wiki/Domain-specific_language) that can easily be embedded in a C# code base. 

## Installation ##

Install from the NuGet gallery GUI or with the Package Manager Console using the following command:

```Install-Package sly```

or with dotnet core 

```dotnet add package sly```




## License
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fb3b00%2Fcsly.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2Fb3b00%2Fcsly?ref=badge_large)
