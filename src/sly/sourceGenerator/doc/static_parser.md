
# Goal


The parser **MUST NOT** depend on the legacy CSLY runtime. 
So it must simply be a set of mutually recursive methods.

This analysis relies on the following grammar. Tokens are T1 to Tn

```
root : items T1
items : item items
items : item

item : T2 T3 [d] 'explicit'

```


## Teerminals

Given a terminal named `HELLO`

Without parsing context (for memoization)
```csharp
public SyntaxParseResult<IN,OUT> parse_HELLO(List<Token<IN>> tokens ,int position, LeadingToken<IN> expected, bool discarded = false)
{
    var result = new SyntaxParseResult<IN, OUT>();
    result.IsError = !expected.Match(tokens[position]);
    
    result.EndingPosition = !result.IsError ? position + 1 : position;
    var token = tokens[position];
    token.Discarded = discarded;
    token.IsExplicit = expected.IsExplicitToken;
    result.Root = new SyntaxLeaf<IN, OUT>(token, discarded);
    result.HasByPassNodes = false;
    if (result.IsError)
    {
        result.AddError(new UnexpectedTokenSyntaxError<IN>(token, LexemeLabels, I18n, expected));
    }

    return result;
}
```

## Rules

A rule is a sequence of clauses. 
Each clause is a dedicated method. 
So a rule method is simply a sequence of calls to the clauses methods

for a rule `rule: TERMINAL non_terminal DISCARDED_TERMINAL[d]`

```csharp
public SyntaxParseResult<IN, OUT> parse_my_rule(List<Token<IN>> tokens, int position)
{
    var result = new SyntaxParseResult<IN, OUT>();
    
    var r1 = parse_TERMINAL(tokens, position);
    if (r1.IsError)
    {
        return r1;
    }
    
    var r2 = parse_NON_TERMINAL(tokens,position);
    if (r2.IsError)
    {
        return r2;
    }
    
    var r3 = parse_DISCARDED_TERMINAL(tokens, position, discarded: true);
    if (r3.IsError)
    {
        return r3;
    }

    var tree = new SyntaxNode<IN, OUT>("my_rule", new List<ISyntaxNode<IN, OUT>>() { r1.Root, r2.Root, r3.Root },
        theMagicalVisitor);
    result.Root = tree;
    result.IsError = false;
    result.AddErrors(r1.Errors);
    result.AddErrors(r2.Errors);
    result.AddErrors(r3.Errors);
    return result;
}
```
  

## Non Terminals

A non terminal parser must iterate on non terminal rules and returns as soon as it find one matching rule.



## EBNF Manies

## EBNF choices

## EBNF options

