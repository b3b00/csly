# optimisation appels visitor



```csharp
OUT visit<OUT>(object instance, params object[] args);


public OUT visitXXXX(object instance, params object[] args) {
    if (args.length != 3) {
        throw new VisitorException();
    }
    OUT result = instance.ruleVisitorMethod((Token<IN>) x, ()OUT)head, (List<OUT>) trail );
    return result;    
} 
```




