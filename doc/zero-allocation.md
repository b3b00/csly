Goal is to limit object allocation.
For now many objects are allocated during lexing/parsing process :
 - during lexing phase, some are abandonned after backtracking :
    - Token<T> 
    - FSMMatch<N> 
 - during parsing phase, many are bandonned due to backtracing : 
    - ISyntaxNode<IN, OUT> class hierarchy

General idea is to limit allocation using some kind of object pooling.
Analyse how to do it. 
Evaluate if this really a good choice.
Initial pool size may be an issue, propose.
feel free to propose other allocation optim. 
