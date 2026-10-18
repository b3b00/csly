# Zero-Allocation Benchmark Analysis & Remediation Plan

**Source:** `src/benchZeroAllocation/BenchmarkDotNet.Artifacts/results/benchZeroAllocation.JsonZeroAllocationBench-report-github.md`
**Generated:** 2026-10-18
**Status:** Analysis complete — no allocation gain observed; remediation plan required.

---

## 1. Executive Summary

The `[ZeroAllocation]` attribute optimisation for the JSON parser **does not currently deliver any allocation reduction**.

| Scenario | Metric | Standard | Zero-Allocation | Delta | Verdict |
|---|---|---|---|---|---|
| Memoize = False | Mean | 47.43 ms | 52.46 ms | +11% slower | ❌ Regression |
| Memoize = False | Allocated | 58.73 MB | 58.73 MB | 0 B | ❌ No change |
| Memoize = True | Mean | 74.99 ms | 73.15 ms | −2% faster | ⚠ Within noise |
| Memoize = True | Allocated | 71.34 MB | 71.34 MB | 0 B | ❌ No change |

**Bottom line:** The zero-allocation path allocates the **exact same amount** as the standard path in both scenarios, and it is measurably slower when memoisation is disabled. The optimisation is not working as intended.

---

## 2. Benchmark Environment

| Property | Value |
|---|---|
| BenchmarkDotNet | v0.13.12 |
| OS | Windows 11 (10.0.26100.9448) |
| Runtime | .NET 8.0.31 (X64 RyuJIT AVX2) |
| Target framework | net8.0 |
| Job | MediumRun, IterationCount=15, LaunchCount=2, WarmupCount=10 |
| Diagnoser | MemoryDiagnoser |
| Input | `test.json` (read via `File.ReadAllText`) |

---

## 3. Detailed Results

### 3.1 Memoize = False

| Method | Mean | Error | StdDev | Ratio | RatioSD | Gen0 | Gen1 | Gen2 | Allocated | Alloc Ratio |
|---|---|---|---|---|---|---|---|---|---|---|
| TestJsonParseStandard (baseline) | 47.43 ms | 0.850 ms | 1.272 ms | 1.00 | 0.00 | 5818.18 | 3090.91 | 1363.64 | 58.73 MB | 1.00 |
| TestJsonParseZeroAllocation | 52.46 ms | 4.001 ms | 5.989 ms | 1.11 | 0.13 | 5600.00 | 3000.00 | 1200.00 | 58.73 MB | 1.00 |

- Zero-allocation is **11% slower** than the baseline.
- Allocation is **identical** (58.73 MB). Gen0/Gen1/Gen2 collection counts are essentially the same.

### 3.2 Memoize = True

| Method | Mean | Error | StdDev | Ratio | RatioSD | Gen0 | Gen1 | Gen2 | Allocated | Alloc Ratio |
|---|---|---|---|---|---|---|---|---|---|---|
| TestJsonParseStandard (baseline) | 74.99 ms | 2.503 ms | 3.669 ms | 1.00 | 0.00 | 6714.29 | 3285.71 | 1571.43 | 71.34 MB | 1.00 |
| TestJsonParseZeroAllocation | 73.15 ms | 1.610 ms | 2.310 ms | 0.98 | 0.05 | 6714.29 | 3285.71 | 1571.42 | 71.34 MB | 1.00 |

- Zero-allocation is ~2% faster but **within the error band** (StdDev 2.50 ms vs 1.61 ms); not statistically significant.
- Allocation is **identical** (71.34 MB).

---

## 4. Root-Cause Hypotheses

Based on the benchmark source (`JsonZeroAllocationBench.cs`), both parsers derive from `EbnfJsonGenericParser`. The `[ZeroAllocation]` attribute is detected by `EBNFParserBuilder` and sets `ParserConfiguration{IN, OUT}.UseZeroAllocation = true`.

Why allocations are identical:

1. **`UseZeroAllocation` flag may not be wired into the actual parse path.** The flag is set on configuration, but if the parser's recursive-descent implementation still boxes tokens, allocates temporary strings/lists, or builds intermediate AST nodes, the flag has no effect on allocations.
2. **Shared base-class implementation.** `StandardJsonParser` and `ZeroAllocationJsonParser` share `EbnfJsonGenericParser`. If the zero-allocation logic lives only in configuration and not in overridden/conditional code paths, both parsers execute identical allocation-producing code.
3. **Memoisation dominates allocation.** With `Memoize = True`, both parsers allocate 71.34 MB — the memoisation cache itself likely accounts for most allocations, masking any zero-allocation benefit.
4. **String/substring allocations.** JSON parsing typically creates many substring/token allocations. Unless the zero-allocation path uses spans/read-only segments and avoids `string.Substring`, allocations will not drop.

Why zero-allocation is slower without memoisation:

- Extra reflection/config checks or indirection introduced by the zero-allocation code path without a corresponding allocation saving to offset the cost.

---

## 5. Zero-Allocation Remediation Plan

The plan below moves from **measurement → root cause → fix → verification**. Each phase has exit criteria.

### Phase 1 — Instrumentation & Root Cause Confirmation (Days 1–2)

**Goal:** Confirm exactly *where* allocations come from in the zero-allocation path.

Actions:
1. Add `[ThreadingDiagnoser]` and/or run with `dotnet-trace` / `dotnet-gcdump` to capture allocation stacks for both parsers.
2. Add a temporary `[Allocations]` breakdown by method if possible, or instrument `EbnfJsonGenericParser` with `GC.GetTotalMemory(false)` checkpoints around key parsing steps (tokenise, consume, reduce, build node).
3. Verify that `ParserConfiguration.UseZeroAllocation` is actually `true` on the `ZeroAllocationJsonParser` instance (add a Debug.Assert or console print in Setup).
4. Check whether `UseMemoization` interacts with `UseZeroAllocation` — memoisation may allocate a cache that dwarfs any parser-level saving.

Exit criteria:
- Allocation hot spots identified (e.g. `Substring`, `new List<>`, boxing of `JsonTokenGeneric`, AST node allocation).
- Confirmation that the zero-allocation flag reaches the parser runtime.

### Phase 2 — Implement True Zero-Allocation Path (Days 3–5)

**Goal:** Make `UseZeroAllocation = true` avoid heap allocations during the parse of a single JSON document.

Proposed changes (adapt to `sly` parser internals):

| Area | Current (hypothesised) | Zero-allocation target |
|---|---|---|
| Token representation | `string` / boxed `JsonTokenGeneric` | `ReadOnlySpan<char>` / `JsonTokenGeneric` struct passed by ref |
| Token list / lookahead | `List<Token>` or array allocations | Span-based cursor over input; no new collections |
| Substring extraction | `input.Substring(start, length)` | Slice the original `ReadOnlySpan<char>`; no new string |
| AST / model building | `new JSon(...)`, `new JArray(...)`, etc. | Reuse a pre-allocated model or write into a pooled builder |
| String interning / memoisation cache | Dictionary allocations (Memoize = True) | Optional; disable cache when `UseZeroAllocation` is true, or use a struct-based cache |

Implementation notes:
- Keep `StandardJsonParser` unchanged as the baseline.
- Gate zero-allocation behaviour behind `Configuration.UseZeroAllocation` so the standard path is unaffected.
- Avoid `params` arrays, `yield return`, LINQ, and `async` in the hot parse path — all allocate.

Exit criteria:
- Code compiles and both parsers still produce correct JSON parse results (round-trip equality vs `System.Text.Json` or existing tests).

### Phase 3 — Re-benchmark & Validate (Days 6–7)

**Goal:** Prove allocation reduction and no performance regression.

Actions:
1. Re-run `JsonZeroAllocationBench` with the same configuration (15 iterations, 2 launches, 10 warmups).
2. Compare against the baseline numbers in Section 3.
3. Add a regression test in CI that fails if `Alloc Ratio` > 1.0 or `Allocated` does not decrease by a target percentage.

Exit criteria (success):
| Metric | Target |
|---|---|
| Allocated (Memoize = False) | ≤ 50% of standard (target < 30 MB) |
| Allocated (Memoize = True) | ≤ 50% of standard (target < 36 MB) |
| Alloc Ratio | < 1.0 with RatioSD low |
| Mean time | Not more than 5% slower than standard |

If targets are not met, return to Phase 2.

---

## 6. Immediate Next Steps

1. **Create** `src/benchZeroAllocation/doc/` (this document lives there).
2. **Add a debug check** in `Setup()` to assert `ZeroAllocationParser.Configuration.UseZeroAllocation == true`.
3. **Profile** the current zero-allocation parse with `dotnet-trace --providers Microsoft-Windows-DotNETRuntime` to capture GC allocation stacks.
4. **Decide** whether the `sly` parser generator (`EBNFParserBuilder`, `EbnfJsonGenericParser`) supports span-based tokenisation, or whether the zero-allocation work must happen in a custom parser override.

---

## 7. Appendix: Raw Benchmark Data

From `benchZeroAllocation.JsonZeroAllocationBench-report.csv`:

```
Method;Memoize;Mean;Error;StdDev;Ratio;Alloc Ratio;Allocated;Gen0;Gen1;Gen2
TestJsonParseStandard;False;47.43 ms;0.850 ms;1.272 ms;1.00;1.00;58.73 MB;5818.18;3090.91;1363.64
TestJsonParseZeroAllocation;False;52.46 ms;4.001 ms;5.989 ms;1.11;1.00;58.73 MB;5600.00;3000.00;1200.00
TestJsonParseStandard;True;74.99 ms;2.503 ms;3.669 ms;1.00;1.00;71.34 MB;6714.29;3285.71;1571.43
TestJsonParseZeroAllocation;True;73.15 ms;1.610 ms;2.310 ms;0.98;1.00;71.34 MB;6714.29;3285.71;1571.42
```
