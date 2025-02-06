# C# lex and yacc #      

 [![Coverage Status](https://coveralls.io/repos/github/b3b00/csly/badge.svg?branch=dev&kill_cache=1)](https://coveralls.io/github/b3b00/csly?branch=dev)
[![Build](https://github.com/b3b00/csly/actions/workflows/dotnetcore.yml/badge.svg)](https://github.com/b3b00/csly/actions/workflows/dotnetcore.yml)
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fb3b00%2Fcsly.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2Fb3b00%2Fcsly?ref=badge_shield)
[![Quality gate](https://sonarcloud.io/api/project_badges/quality_gate?project=b3b00_csly)](https://sonarcloud.io/summary/new_code?id=b3b00_csly)


[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://github.com/codespaces/new?hide_repo_select=true&ref=dev&repo=89719340)

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/b3b00/sly/blob/dev/LICENSE)
[![NuGet](https://img.shields.io/nuget/v/sly.svg?kill_cache=1)](https://www.nuget.org/packages/sly)


Csly is inspired by the Python lex yacc library ([PLY](http://www.dabeaz.com/ply/)) and aims
to simplify generating lexer/parsers in C#.

## Getting started ##

If you'd like to get coding right away, read 
the  [quick getting-started guide](https://github.com/b3b00/csly/wiki/getting-started), which will guide you through the implementation of a basic parser.

## Documentation and examples ## 

Complete documentation can be found in the [wiki](https://github.com/b3b00/csly/wiki). For a list of more advanced 
samples check out the samples folder in the repo.


## Csly special features ##

Csly is packed with special features that make it simpler to use, maintainable, and type-safe.

### Fully embeddable ###

Csly has been designed to avoid extra build steps. Parser generators often need a build-time step 
 to generate target language source code. That is not the case with csly.
 A simple Nuget command will configure csly for use in a 100% .NET implementation.

>### Csly does not need a build-time step, simplifying the build/CI process

### Compact lexer/parser definition ### 

The csly lexer/parser is defined with only 2 types: 
 - a C# ```enum``` for the lexer,
 - a C# ```class``` for the parser.

Lexeme and parser production rules are defined using C# custom attributes making your code compact and readable.
Although these features already exist with parser combinators (like [Sprache](https://github.com/sprache/Sprache) 
or [Eto.Parse](https://github.com/picoe/Eto.Parse)), 
csly can use productions rules defined using either [BNF](https://github.com/b3b00/csly/wiki/BNF-Parser) or [EBNF](https://github.com/b3b00/csly/wiki/EBNF-Parser)  notation, which I think is more natural and easier to understand, assuring maintainability.

>###  Define languages in a very compact and dependency-free way

See [Lexer](https://github.com/b3b00/csly/wiki/Lexer) for lexers definition and [BNF](https://github.com/b3b00/csly/wiki/BNF-Parser) or  [EBNF](https://github.com/b3b00/csly/wiki/EBNF-Parser) for parser definitions.

### Fully and Strictly typed ### 

 Csly is strictly typed, so every parser you define renders according to its input and output types. 
 For additional details on parser typing, head to the [parser definition section](https://github.com/b3b00/csly/wiki/defining-your-parser).
>### Be more confident that your parser will generate valid inputs and outputs.

### Expression parsing ### 

Many domain-specific languages need parsing expressions (boolean or numeric).
A recursive-descent parser is hard to maintain when parsing expressions have multiple precedence levels. For that reason, csly offers a way to generate expression-parsing rules using only operator tokens and a simple-to-understand precedence scheme. Csly will then generate production rules to parse expressions, managing precedence and either left-or-right associativity.

>### Avoid burdensome hand-made expression parser implementations.

see [expression parsing](https://github.com/b3b00/csly/wiki/expression-parsing)


### Indentable languages support ###

Some languages use indentation to denote functional blocks, like Python or Yaml.
Csly provides native support for indentation. Head to [Indented Languages](https://github.com/b3b00/csly/wiki/Indented-languages)

>### Easily use indentation to make your language more readable.


### Preserve comments 

Comments or whitespace are almost every time discardable. But sometimes it makes sens to preserve them : 

- use comments to generate auto doc (think javadoc, c#'s xmldoc or python's docstring )
- introduce meta data without cluttering your grammar

So CSLY borrowed the (antlr channel concept)[https://datacadamia.com/antlr/channel]. 
Every lexeme can be redirected to a specific channel. By default comments go to channel 2.

This feature is already available on branch dev but not deployed in any nuget package.
Documentation is also a work in progress event though you can have it a glance looking at (EBNF unit tests)[https://github.com/b3b00/csly/blob/dev/ParserTests/EBNFTests.cs] :
  - [TestIndentedParserNestedBlocks](https://github.com/b3b00/csly/blob/dev/ParserTests/EBNFTests.cs#L1357)
  - [TestIndentedParserWithEolAwareness](https://github.com/b3b00/csly/blob/dev/ParserTests/EBNFTests.cs#L1409)
  - [TestIndentedParserWithEolAwareness2](https://github.com/b3b00/csly/blob/dev/ParserTests/EBNFTests.cs#L1445)
  - [TestIssue213WithChannels](https://github.com/b3b00/csly/blob/dev/ParserTests/EBNFTests.cs#L1481)
    


### Generic lexer ### 

Lexemes are often similar from one language to another. Csly introduces a generic lexer that defines common lexemes and which can be reused across languages. 
The built-in generic lexer has better performance than a regex-based lexer.

>### Reuse common token definition and take advantage of better lexer performance.

See [Generic lexer](https://github.com/b3b00/csly/wiki/GenericLexer) for the generic lexer implementation and [Lexer](https://github.com/b3b00/wiki/Lexer) for a general presentation on rolling your own.


### What Csly is and isn't ###

#### Csly is not #### 

Csly is not a fully-featured parser generator like [ANTLR](http://www.antlr.org/).
You should therefore not use it to define strong-typed languages like  C# or Java.

#### Csly is #### 

Csly is perfect for small [domain-specific languages (DSLs)](https://en.wikipedia.org/wiki/Domain-specific_language) that can be bundled in C# applications for end-users to interact with your application using natural language, for example. 

## Installation ##

Install from the NuGet gallery GUI or with the Package Manager Console using the following command:

```Install-Package sly```

or with dotnet core 

```dotnet add package sly```

# Buy me a coffee

If you found CSLY usefull consider supporting with : 

[!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/b3b00)

# Special thanks to

<a href="https://jb.gg/OpenSource"><img height="200" src="src/logos/jetbrains-variant-2.svg"><a>
