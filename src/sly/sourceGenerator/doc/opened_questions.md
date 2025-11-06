
# Opened questions

This page lists all opened questions regarding source generation

## models
We need a minimal set of shared model classes for :
- tokens
- syntax tree
- special EBNF types (Group, ValueOption)

How to include these types ?
- namespace :
    - static (e.g sly.model.generated)
    - by parser (e.g while.sly.model.generated)

Static namespace may produces conflicts at build if consuming assembly defines many parser. Using parser specific namespaces may produces too many classes.

Is this a real issue ? how many assembly will define many parser and such a number that it could be a real issue ?

## Memoization

Memoization will require a dedicated `ParsingContext` as we can not use the legacy one. 
The memoization cache key may be a bit harder to compute , not necessiraly ?
The memoization classes must be generated, this leads to the same question as for the model classes

