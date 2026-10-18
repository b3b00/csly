# Zero-allocation plan

**Goal:** limit object allocation during the lexing/parsing pipeline of **csly**, focusing on objects that are allocated and then abandoned because of backtracking.

**Source doc:** `doc/zero-allocation.md`

**Target frameworks:** `net8.0; net7.0; netstandard2.0; netstandard2.1; net6.0` (see `src/sly/sly.csproj`).  
This matters for pooling APIs: `ArrayPool<T>.Shared` is **not** available on `netstandard2.0` (only `netstandard2.1`+), and there is no `ObjectPool<T>` in the BCL. Any pooling proposal must either be conditional or hand-rolled.

---

## 1. Objects currently allocated (and where)

### Lexing phase

| Type | File | Where it is allocated |
|------|------|-----------------------|
| `FSMMatch<N>` | `src/sly/lexer/fsm/FSMMatch.cs` | `FSMLexer.Run()` – one per successful end-node match (line ~243), one `ko` match on failure (line ~285 / line ~356), plus `Indent`/`UIndent` factories (lines ~62–87) |
| `Token<N>` | `src/sly/lexer/Token.cs` | inside every `FSMMatch` (line ~101), `Indent`/`UIndent` results (lines ~71, ~84), `ConsumeIgnored` per whitespace char (`FSMLexer.cs` line ~412), and copies in `GenericLexer.Tokenize()` (lines ~166, ~224–226) |

Details observed in `FSMLexer.Run()` (`src/sly/lexer/fsm/FSMLexer.cs`):
- The FSM walks the input char by char and keeps **only the last** end-node match in the local `result` variable. Intermediate matches are overwritten, not separately allocated — so the walk itself does not allocate a `FSMMatch` per character.
- On total failure a `ko = new FSMMatch<N>(false, ...)` is allocated and returned.
- Every `FSMMatch` eagerly allocates `Properties = new Dictionary<string, object>()` and `IgnoredTokens = new List<Token<N>>()` even when neither is used.
- `ConsumeIgnored` allocates one `Token<N>` per ignored (whitespace) character, unconditionally.

In `GenericLexer.Tokenize()` (`src/sly/lexer/GenericLexer.cs`):
- Ignored tokens are re-wrapped with `new Token<IN>(...)` (lines ~166 and ~224–226), i.e. a second set of token allocations for whitespace that the parser may never see.
- `Transcode` allocates a fresh `Token<IN>` per match (line ~1212).
- One final EOS `Token<IN>` is appended (line ~262).

### Parsing phase

| Type | File | Where it is allocated |
|------|------|-----------------------|
| `ISyntaxNode<IN, OUT>` hierarchy | `src/sly/parser/syntax/tree/` | `SyntaxNode`, `SyntaxLeaf`, `OptionSyntaxNode`, `ManySyntaxNode`, `GroupSyntaxNode` |

Concrete node allocations:
- `SyntaxLeaf<IN, OUT>(token, discarded)` — `ParseTerminal` (`.../Terminal.cs` line ~27), `ParseOption` / stack parser (empty leaf on failed option).
- `SyntaxNode<IN, OUT>(name, children)` / `GroupSyntaxNode` — `Parse` / `ParseNonTerminal` / expression rules, one per matched rule, with a `new List<ISyntaxNode<IN, OUT>>()` for children.
- `OptionSyntaxNode<IN, OUT>` — `ParseOption` (always, even when the inner clause fails).
- `ManySyntaxNode<IN, OUT>` — `ParseZeroOrMore` / `ParseOneOrMore` / `ParseRepeat`, one per repetition clause.

---

## 2. Where backtracking abandons objects

### Lexer
- `FSMLexer.Run()` backtracks to the longest match by keeping only the final `result`. The abandoned part is not a separately allocated object per step, but a **failed lex** still allocates a `ko` match plus all `ConsumeIgnored` tokens produced before the failure.
- In `GenericLexer.Tokenize()`, ignored whitespace tokens are allocated and added to the stream even when they are ultimately skipped/discarded.

### Parser (recursive descent, `src/sly/parser/parser/llparser/`)
The parser is an **LL recursive descent with backtracking** (it tries alternatives and keeps the best/longest). Objects are abandoned at:

1. **`ParseChoice`** (`EBNFRecursiveDescentSyntaxParser.Choice.cs`, line ~15): every alternate is parsed at the same position. All failed alternates produce full `SyntaxParseResult` trees (`ISyntaxNode` graphs) that are thrown away; only the winning alternate's tree survives.
2. **`ParseNonTerminal`** (`RecursiveDescentSyntaxParser.NonTerminal.cs`, line ~20): **all** rules of a non-terminal are tried (`rulesResults` list), and only the best one is kept. Every other rule's syntax tree is abandoned.
3. **`ParseOption`** (`EBNFRecursiveDescentSyntaxParser.Option.cs`): an `OptionSyntaxNode` is allocated even when the inner clause fails (the "default" branch at line ~77).
4. **Expression rules** (`EBNFRecursiveDescentSyntaxParser.Expressions.cs`): intermediate `firstResult`/`secondResult`/`thirdResult` trees are discarded when a later clause fails.

Note: memoization (`SyntaxParsingContext`, `src/sly/parser/parser/SyntaxParsingContext.cs`) already avoids re-parsing the same clause at the same position when enabled, but it does **not** prevent the initial allocation of abandoned trees.

---

## 3. Evaluation: is object pooling a good choice?

### Short answer
Pooling `FSMMatch<N>` is **plausible and low-risk**. Pooling `Token<T>` is **risky** because tokens outlive the lexer and are stored in `TokenChannels` / referenced by `SyntaxLeaf` nodes during parsing — returning them to a pool while still referenced would corrupt the parse tree. Pooling the `ISyntaxNode` hierarchy is **complex and likely counter-productive** because nodes form arbitrary graphs with children lists; resetting them safely is error-prone, and the win is uncertain.

### .NET GC reality check
The .NET GC is generational and heavily optimized for short-lived allocations:
- Gen-0 allocation is essentially a pointer bump (very cheap).
- Short-lived objects that die in Gen-0 are collected almost for free.
- Object pooling adds: a pool data structure, synchronization (if the lexer/parser is ever used concurrently — `GenericLexer` / parsers appear to be mutable instance objects, so concurrent use is not obviously safe anyway), reset/clear logic, and the risk of "object resurrection" (a pooled object handed out twice while still referenced elsewhere).

**Pooling wins only when:**
1. The object is expensive to construct (e.g. allocates inner collections), **and**
2. The object is short-lived (dies in Gen-0 anyway), **and**
3. Resetting it is cheaper than constructing a new one, **and**
4. The pool does not become a contention point.

Given that, here is the per-type assessment:

| Type | Pool? | Rationale |
|------|-------|-----------|
| `FSMMatch<N>` | **Yes (candidate)** | Short-lived (one lex call), eagerly allocates `Dictionary` + `List`, constructed per token. Reset cost is low (clear collections, reset fields). |
| `Token<T>` | **No / only internal** | Tokens are handed to `TokenChannels` and kept for the whole parse. Pooling them is unsafe unless the pool tracks outstanding references. Better to avoid allocating the copies in `ConsumeIgnored` / `Transcode` instead. |
| `ISyntaxNode` hierarchy | **No** | Node graphs are deep and shared; resetting children lists is fragile. Allocation is usually not the hot path compared to dictionary lookups and memoization keys. |
| `SyntaxParseResult<IN, OUT>` | **Maybe** | Allocated per clause/rule attempt, but holds a root node; pooling it means also pooling the node graph. Low priority. |

### Verdict
Object pooling is **not a silver bullet** here. The highest-ROI zero-allocation work is likely **not** pooling but:

1. **Avoiding `value.ToCharArray()` / `new ReadOnlyMemory<char>(value.ToCharArray())`** in `Token` constructors — copies the entire source substring per token.
2. **Avoiding eager `Dictionary<string, object> Properties` and `List<Token<N>> IgnoredTokens`** on every `FSMMatch` — allocate lazily on first use.
3. **Avoiding per-character `Token<N>` allocations** in `ConsumeIgnored`.
4. **Reusing `List<ISyntaxNode<IN, OUT>>` children lists** (e.g. via `ArrayPool` or list pooling) instead of `new List<>()` per node.
5. **Memoization** (already present) to avoid re-parsing.

Pooling should be treated as a **secondary, opt-in optimization** applied only to `FSMMatch<N>` (and maybe internal transient tokens), and only after benchmarking shows GC pressure.

---

## 4. How pooling would work (design sketch)

If pursued, a simple, non-generic, per-type pool:

```csharp
public sealed class FsmMatchPool<N> where N : struct, Enum
{
    private readonly Stack<FSMMatch<N>> _stack = new();
    // netstandard2.0 has no ArrayPool<T>.Shared; a Stack-based pool works on all targets.

    public FSMMatch<N> Rent() =>
        _stack.Count > 0 ? _stack.Pop() : new FSMMatch<N>(false);

    public void Return(FSMMatch<N> match)
    {
        match.Properties?.Clear();
        match.IgnoredTokens?.Clear();
        match.Result = null;
        match.IsSuccess = false;
        match.IsEOS = true;
        match.IsIndent = match.IsUnIndent = match.IsNoIndent = false;
        match.IsLineEnding = match.IsIndentationError = false;
        match.IsPush = match.IsPop = false;
        _stack.Push(match);
    }
}
```

Key correctness rules:
- **Never pool objects that escape the caller.** `FSMMatch` is returned by `Run()` and consumed synchronously by `GenericLexer.Tokenize()`, so it can be returned to the pool immediately after `Transcode` finishes reading it.
- **Reset every observable field**, including nested collections, to avoid leaking state between uses.
- **No pre-sizing by default** — see §5.

For `Token<T>` (internal only, e.g. whitespace tokens inside `ConsumeIgnored`), a similar pattern but the tokens must **not** be added to the public token stream; they must be copied out before returning to the pool.

---

## 5. Initial pool size — proposal

An initial fixed pool size is hard to choose because it depends on:
- input length,
- number of tokens (grammar-dependent),
- nesting / indentation (indent tokens),
- number of parser backtracking attempts (grammar-dependent).

**Proposal:** do **not** pre-size a fixed number. Instead:

1. **Start empty (capacity 0) and grow organically.** `Rent()` creates a new object when the pool is empty. This adapts to any input size with no configuration.
2. **Cap the pool** to a soft maximum (e.g. `Math.Max(64, 2 * estimatedTokenCount)` or a user-configurable `MaxPooledItems`, default ~256–512) so a pathological input does not retain unbounded memory in Gen-2. `Return()` drops the object if the pool is at capacity (let the GC collect it).
3. **Expose optional knobs** for advanced users:
   - `InitialCapacity` (default 0) — pre-warm if the caller knows the typical input size.
   - `MaxCapacity` (default e.g. 1024) — upper bound on retained objects.
   - These can be passed via `LexerConfig` / `ParserConfiguration`.

**Rule of thumb to propose in the doc:** measure the average token count per lex call on representative inputs; set any pre-warm size to roughly that average, but never more than a few hundred objects unless profiling proves otherwise. A pool larger than the live set wastes memory (idle objects promoted to Gen-2).

---

## 6. Recommended action plan (priority order)

### P0 — Measure first (no code change yet)
- Add a simple allocation benchmark (e.g. BenchmarkDotNet in `src/bench` or `src/benchCurrent`) for lexing + parsing a representative input, capturing Gen-0/Gen-1/Gen-2 collections and allocated bytes.
- Identify the actual hot allocation path before optimizing. The doc's claim ("many objects abandoned after backtracking") should be verified with data.

### P1 — Low-risk allocation reductions (higher ROI than pooling)
1. `FSMMatch<N>`: make `Properties` and `IgnoredTokens` **lazy** (allocate on first use).
2. `Token<T>` constructors: avoid `value.ToCharArray()`; keep the `ReadOnlyMemory<char>` slice of the source instead of copying.
3. `ConsumeIgnored`: avoid allocating a `Token<N>` per whitespace character when ignored tokens are not needed; or reuse a single buffer.
4. `Lexer<T>.Tokenize()` (regex-based, `src/sly/lexer/Lexer.cs`): `source.Substring(currentIndex)` per rule — avoid substring copies.

### P2 — Opt-in pooling for `FSMMatch<N>` only
- Implement `FsmMatchPool<N>` as described in §4.
- Plug it into `FSMLexer.Run()`: rent at start, return after `GenericLexer.Tokenize()` is done with the match.
- Guard with `#if` or interface so `netstandard2.0` (no `ArrayPool.Shared`) still works.

### P3 — Re-evaluate `Token<T>` and `ISyntaxNode` pooling
- Only if P0–P2 show GC remains a problem. Likely answer: do not pool; instead reduce allocations at the source (see P1).

### P4 — Documentation
- Record benchmark results (before/after) in this plan or a `profiling/` note.
- Update `doc/zero-allocation.md` with the decisions taken.

---

## 7. Risks / open questions

1. **Thread safety:** `GenericLexer` and the parsers are mutable instance classes with instance state (`Nodes`, `Transitions`, `CallBacks`, `parsingContext`, …). Pooling does not make them thread-safe; document that a lexer/parser instance is single-use per parse (or not thread-safe).
2. **Genericity:** pools are per closed generic type (`Pool<Token<GenericToken>>`, `Pool<FSMMatch<GenericToken>>`, …). A pool keyed by `Type` or a generic static class is needed to avoid cross-contamination between different `N`/`IN` enums.
3. **`netstandard2.0` compatibility:** no `ArrayPool<T>.Shared`; any pool implementation must work on all five TFMs or use `#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER`.
4. **Backtracking cost vs allocation cost:** the parser re-parses entire rules on backtracking. Eliminating the *allocation* of abandoned trees does not eliminate the *CPU* cost of re-parsing. Memoization is the bigger lever there.
5. **Correctness of `Token<T>` reuse:** `Token.Value` calls `SpanValue.ToString()`; if a pooled token's `SpanValue` is a slice of a source buffer that gets mutated, reused tokens could observe stale data. Only pool tokens whose `SpanValue` is detached or immutable.

---

## 8. Suggested definition of done

- [ ] Benchmark baseline allocations for lex + parse of representative inputs.
- [ ] `FSMMatch` lazy `Properties` / `IgnoredTokens`.
- [ ] `FSMMatch` pool opt-in (rent/return in `FSMLexer.Run()` → `GenericLexer.Tokenize()`), all TFMs.
- [ ] Confirm no pooled token leaks into `TokenChannels` / parse tree.
- [ ] Compare benchmarks before/after; decide whether to keep, extend, or drop pooling.
