# Manage ambiguous grammars

## goal

For now CSLY only manage unambiguous grammars. When parsing a n ambiguous grammar it resolves ambiguity returning the first derivation.
The goal is to return a forest instead of a tree.

## Resolving ambiguity

Resolving ambiguity (if ever possible) would be up to user. either at :
- visiting tree time (throwing an `AmbiguityExcpetion`
- some following step , like a semantic or typing pass

## grammaire ambiguë de référence

```
S> ::= 'a' <S> 'a' | 'a' 'a' <S> | 'a'
```