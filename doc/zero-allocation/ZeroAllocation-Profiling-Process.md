# Zero-Allocation Profiling Process (PerfView & dotnet-trace)

**Goal:** Find where the `csly` lexer/parser allocates during lexing + parsing, confirm the allocation hot spots (e.g. `FSMMatch<N>`, `Token<T>`, `ISyntaxNode`, eager `Dictionary`/`List`, substring copies), measure before/after, and prove any change reduces GC pressure.

**Source docs:** `zero-allocation.md`, `zero-allocation-plan.md`, `ZeroAllocation-Benchmark-Analysis.md`

**Applies to:** `src/sly` (lexer + recursive-descent parser), targets `net8.0; net7.0; netstandard2.0; netstandard2.1; net6.0`.  
**OS:** Windows 11 (PowerShell). Commands are mostly cross-platform where the tool supports it.

---

## 1. Tool selection — which tool when

| Situation | Recommended tool | Why |
|-----------|------------------|-----|
| Detailed **allocation stacks** (who allocates what, how many bytes, Gen-0/1/2) | **PerfView** | Best-in-class .NET allocation profiling; GC Alloc Stacks view maps bytes → method stacks. Windows-first (there is a Linux build, but UI is Windows). |
| Quick capture on a **running process** (prod/CI, no PerfView install) | **dotnet-trace** | Cross-platform, single `dotnet tool`, attaches to a live PID, produces `.nettrace` openable in PerfView/speedscope. |
| **Heap inspection** — what is alive on the heap, retained sizes, dominator tree | **dotnet-gcdump** + PerfView / WinDbg+SOS | Snapshot of the managed heap; good for "why is this still alive?" questions (e.g. leaked pooled tokens, memoization cache growth). |
| Regression / CI gate on allocated bytes | **BenchmarkDotNet MemoryDiagnoser** | Repeatable, statistical. Use profiling tools to *find* the problem; benchmarks to *prove* the fix. |

**Recommended workflow:** PerfView for deep allocation-stack analysis on a dev machine; dotnet-trace when you cannot install PerfView or need to attach to an already-running process; dotnet-gcdump for heap retention questions.

---

## 2. Prerequisites

### 2.1 Install the tools

```powershell
# PerfView: download the single perfview.exe from
# https://github.com/microsoft/perfview/releases  (no installer; just unzip)
# Put perfview.exe on PATH, e.g. C:\tools\perfview\PerfView.exe

# dotnet-trace
dotnet tool install --global dotnet-trace

# dotnet-gcdump (heap snapshots)
dotnet tool install --global dotnet-gcdump

# dotnet-dump (SOS analysis of dumps)
dotnet tool install --global dotnet-dump
```

### 2.2 Prepare the build you profile

Allocation numbers are only meaningful if the build matches production characteristics.

| Setting | Recommendation | Reason |
|---------|----------------|--------|
| Configuration | `Release` | Debug builds allocate more (debuggable code, no inlining, JIT tracking). |
| Tiered compilation | **Disable** for profiling: set `DOTNET_TieredPGO=0` and `DOTNET_TieredCompilation=0` | Tiered JIT re-jits methods, which skews allocation stacks and timings. |
| ReadyToRun | Off (default for class libraries) | R2R can obscure JIT-generated frames. |
| Symbols | Enable PDBs (`<DebugType>full</DebugType>` or `pdbonly`) | Needed to resolve method names in PerfView. |
| Target framework | Profile `net8.0` first (best tooling), then verify on `netstandard2.0` if you use `#if` pooling code | Pooling code paths differ per TFM (see plan §7.3). |

Example PowerShell to launch the bench/app with tiering disabled:

```powershell
$env:DOTNET_TieredCompilation=0
$env:DOTNET_TieredPGO=0
dotnet run -c Release -f net8.0 --project src/benchZeroAllocation -- <args>
```

> **Note:** `netstandard2.0` lacks `ArrayPool<T>.Shared` and `ObjectPool<T>` (see `zero-allocation-plan.md` §1). If you profile pooling changes, profile under `net8.0` AND confirm the `netstandard2.0` fallback path behaves.

---

## 3. Phase 0 — Establish a reproducible baseline

Before any profiling capture:

1. Pick **one representative input** (e.g. `test.json` used by `benchZeroAllocation`, or a real grammar sample). Keep it fixed across all measurements.
2. Capture a **BenchmarkDotNet baseline** (MemoryDiagnoser) so you have Gen-0/1/2 counts and `Allocated` MB to compare against.
   - Existing baseline (from `ZeroAllocation-Benchmark-Analysis.md`): JSON parse allocates ~58.73 MB (Memoize=False) / ~71.34 MB (Memoize=True) on net8.0.
3. Record the environment (runtime version, OS, input size) in the profiling notes.

Exit criterion for Phase 0: you can run the same input twice and get allocation numbers within ~5% of each other.

---

## 4. Phase 1 — Capture allocation data

Choose **PerfView** (deep analysis) or **dotnet-trace** (quick/live attach). You can capture both from the same run if desired.

### Option A — PerfView (recommended for allocation stacks)

PerfView is a single executable (`PerfView.exe`). There are two ways to use it: the UI (easiest for allocation stacks) or the command line.

#### A1. PerfView UI (GUI) — canonical allocation workflow

1. Launch `PerfView.exe`.
2. **Collect → Collect**. In the dialog:
   - **Command** (optional): the app/bench to run, e.g. `dotnet run -c Release -f net8.0 --project src/benchZeroAllocation -- ...`
   - Or leave blank and attach to a running PID via **Collect → Collect from Running Process**.
   - **GC Collect:** checked (required for GC stats).
   - **Collect GC Alloc Stacks:** checked (this is what gives you per-method allocation bytes/stacks).
   - **Circular MB:** `1024` (buffer size; increase for long runs).
   - **Max Sec:** e.g. `300` (or let the bench finish and stop manually).
   - **Kernel events:** `Stack, Profile, File, Proc, Thread` (helps correlate with CPU/disk).
   - **Additional Providers:** `Microsoft-Windows-DotNETRuntime:0x1:5` (GC keyword, Verbose) — ensures GC/allocation events fire.
   - **Output file:** `C:\...\alloc-investigation.etl` or `.zip`.
3. Click **Start Collecting**, run your scenario, then **Stop Collecting**.
4. PerfView parses on close (or open the `.etl`/`.zip` later).

#### A2. PerfView command line

Capture to a file (runs the command under PerfView):

```powershell
PerfView.exe collect `
  -CircularMB:1024 -MaxSec:300 -NoGui `
  -Kernel:Stack,Profile,File,Proc,Thread `
  -AdditionalProviders:Microsoft-Windows-DotNETRuntime:0x1:5 `
  -Zip `
  C:\temp\csly-alloc.etl `
  dotnet run -c Release -f net8.0 --project src/benchZeroAllocation -- --input test.json
```

Capture from an already-running process (replace `<PID>`):

```powershell
PerfView.exe collect `
  -CircularMB:1024 -MaxSec:300 -NoGui `
  -Kernel:Stack,Profile,File,Proc,Thread `
  -AdditionalProviders:Microsoft-Windows-DotNETRuntime:0x1:5 `
  -Zip `
  C:\temp\csly-alloc.etl `
  <PID>
```

> If `-Zip` is used, PerfView produces a `.zip` containing the `.etl` plus metadata. Open the `.zip` directly in PerfView.

### Option B — dotnet-trace (cross-platform / live attach)

`dotnet-trace` attaches to a running process and writes a `.nettrace` file (openable in PerfView or [speedscope](https://www.speedscope.app)).

```powershell
# 1. Start your app/bench in one window
dotnet run -c Release -f net8.0 --project src/benchZeroAllocation -- --input test.json

# 2. Find the PID
Get-Process dotnet | Select-Object Id, ProcessName, CPU

# 3. Collect GC events (allocation-relevant) for 60 seconds
dotnet-trace collect `
  --process-id <PID> `
  --providers Microsoft-Windows-DotNETRuntime:0x1:4 `
  --output C:\temp\csly-alloc.nettrace `
  --duration 60
```

- Provider keyword `0x1` = GC events (GC start/end, heap size, generation counts).
- Level `4` = Informative. Use level `5` (Verbose) if you need more granular GC detail.

To also capture **sampled allocation stacks** with dotnet-trace, add the sampled-allocation keyword:

```powershell
dotnet-trace collect `
  --process-id <PID> `
  --providers Microsoft-Windows-DotNETRuntime:0x11:5 `
  --output C:\temp\csly-alloc-stacks.nettrace
```

(`0x11` = GC `0x1` + GCHeapCollect `0x10`… adjust per provider documentation; if stacks are missing, fall back to PerfView's "Collect GC Alloc Stacks".)

Open the result:

```powershell
# In PerfView UI: File → Open → csly-alloc-stacks.nettrace
# Or convert for speedscope:
dotnet-trace convert C:\temp\csly-alloc-stacks.nettrace --format speedscope
```

---

## 5. Phase 2 — Analyze (what to look for)

### 5.1 PerfView analysis steps

Open the `.etl`/`.zip`/`.nettrace` in PerfView and work top-down:

1. **GC Stats** view → confirm Gen-0/Gen-1/Gen-2 collection counts and % time in GC. High Gen-0 count + high %GC = allocation pressure.
2. **GC Alloc Stacks** view → the core view. It lists, by method, the bytes allocated and number of allocations.
   - Look for `csly` namespaces: `Sly.Lexer`, `Sly.Parser`, `FSMMatch<>`, `Token<>`, `SyntaxNode<>`, `SyntaxLeaf<>`, `List<>`, `Dictionary<>`, `String.Substring`.
   - Sort by **Bytes Allocated** (descending) and by **Alloc Count** (descending). These often differ: a few large allocations vs many small ones.
   - Expand a method to see its **callers** (who caused the allocation) and **callees** (what it allocated).
3. **CPU Sampling** view (if you collected kernel profiling) → confirm allocation hot spots are also CPU hot spots, not just noise.
4. **Events → GCHeapAlloc** → raw allocation events with stack traces, useful for drilling into a specific method.

### 5.2 dotnet-trace / speedscope analysis

Open the `.nettrace` in PerfView (same views as above) or load the speedscope JSON in speedscope.com:
- Switch to the **"allocations"** or **"sampled allocation"** profile if present.
- Otherwise inspect **GC events**: heap size over time, GC pause durations.

### 5.3 dotnet-gcdump heap snapshot (retention / "why alive?")

Use when you suspect objects are being held too long (e.g. pooled `FSMMatch` leaking into the parse tree, memoization cache growing unbounded).

```powershell
# Capture a heap dump of a running process
dotnet-gcdump collect -p <PID> --output C:\temp\csly-heap.diag

# Analyze with SOS via dotnet-dump (interactive)
dotnet-dump analyze C:\temp\csly-heap.diag
# Inside the SOS prompt:
#   !dumpheap -type FSMMatch     # count/size of FSMMatch instances
#   !dumpheap -type Token`1      # count/size of Token<T>
#   !gcroot <address>            # why is this instance still rooted?
#   !dumpheap -stat              # top types by count/size
```

Look for:
- `FSMMatch<N>` instances still rooted after the lex call completes (leak/pool bug).
- `Dictionary<string, object>` / `List<Token<N>>` counts matching `FSMMatch` count (eager allocations).
- Memoization dictionary growth (SyntaxParsingContext cache) when `Memoize=True`.

---

## 6. Phase 3 — Interpret results for csly

Map what you see to the known allocation suspects from `zero-allocation-plan.md`:

| PerfView / dotnet-trace signal | Likely csly source | Suggested fix (from plan) |
|--------------------------------|--------------------|---------------------------|
| `FSMMatch<N>` high alloc count + eager `Dictionary`/`List` per instance | `FSMLexer.Run()`, `FSMMatch` constructor | Lazy `Properties` / `IgnoredTokens`; opt-in `FsmMatchPool<N>` |
| `Token<N>` per whitespace char | `ConsumeIgnored` (`FSMLexer.cs`) | Skip token creation for ignored chars unless needed |
| `new Token<IN>(...)` copies / `ToCharArray()` per token | `GenericLexer.Tokenize()`, `Token` ctor | Keep `ReadOnlyMemory<char>` slice of source; no copy |
| `List<ISyntaxNode<IN,OUT>>()` per node | `SyntaxNode`/`GroupSyntaxNode` constructors | Reuse children list (pool) or avoid materializing |
| `SyntaxLeaf` / `OptionSyntaxNode` / `ManySyntaxNode` abandoned | Backtracking in `ParseChoice`/`ParseNonTerminal`/`ParseOption` | Memoization (already present) avoids re-parse; allocation of abandoned trees remains |
| `Substring` / string copies | Lexer rule matching, token value extraction | Slice `ReadOnlySpan<char>` over source instead |

**Decision rule:** If a type appears high in **Bytes Allocated** but its objects die in Gen-0 quickly (low Gen-1/Gen-2, low GC pause), pooling is low-value (per plan §2 "GC reality check"). Prioritize *not allocating* (P1 in plan) over pooling (P2).

---

## 7. Phase 4 — Hypothesis → change → re-measure loop

1. **Formulate hypothesis** from the capture, e.g. "Lazy `IgnoredTokens` in `FSMMatch` should cut X MB per lex of test.json."
2. **Make the code change** on a feature branch.
3. **Re-capture with the same tool, same input, same settings** (same PerfView config, same dotnet-trace providers/duration).
4. **Compare**:
   - PerfView: open both traces; compare **GC Alloc Stacks** totals for the suspect methods. PerfView supports comparing two files via **Compare** (or open both and view side by side).
   - BenchmarkDotNet: re-run `benchZeroAllocation`; `Allocated`, Gen-0/1/2 should drop toward the Phase 3 targets in `ZeroAllocation-Benchmark-Analysis.md` (e.g. ≤50% of standard).
5. **Accept/reject**: keep the change only if allocations drop *and* mean time is not >5% slower (per plan §3 exit criteria).
6. **Record** before/after numbers in `ZeroAllocation-Benchmark-Analysis.md` or a `profiling/` note (plan §4.4).

---

## 8. Example commands (copy-paste adapt)

```powershell
# ---- 0. Baseline benchmark ----
dotnet run -c Release -f net8.0 --project src/benchZeroAllocation -- --input test.json

# ---- 1. PerfView capture (run bench under PerfView) ----
$env:DOTNET_TieredCompilation=0
$env:DOTNET_TieredPGO=0
PerfView.exe collect -CircularMB:1024 -MaxSec:300 -NoGui `
  -Kernel:Stack,Profile,File,Proc,Thread `
  -AdditionalProviders:Microsoft-Windows-DotNETRuntime:0x1:5 `
  -Zip C:\temp\csly-baseline.zip `
  dotnet run -c Release -f net8.0 --project src/benchZeroAllocation -- --input test.json

# ---- 1b. PerfView capture from running process ----
PerfView.exe collect -CircularMB:1024 -MaxSec:120 -NoGui `
  -AdditionalProviders:Microsoft-Windows-DotNETRuntime:0x1:5 `
  -Zip C:\temp\csly-live.zip <PID>

# ---- 2. dotnet-trace capture (live attach, 60s) ----
dotnet-trace collect --process-id <PID> `
  --providers Microsoft-Windows-DotNETRuntime:0x1:4 `
  --output C:\temp\csly-trace.nettrace --duration 60

# ---- 3. dotnet-gcdump heap snapshot ----
dotnet-gcdump collect -p <PID> --output C:\temp\csly-heap.diag
dotnet-dump analyze C:\temp\csly-heap.diag
# (SOS) !dumpheap -stat | findstr /i "FSMMatch\|Token\|SyntaxNode\|Dictionary"
```

---

## 9. Common pitfalls & gotchas

- **Debug build numbers are meaningless.** Always profile `-c Release`. Debug allocations can be 2–5× higher and stacks include JIT debugging helpers.
- **Tiered compilation skews stacks.** A method may show allocations in its "tier 0" jitted frame; disable tiering (`DOTNET_TieredCompilation=0`) for stable attribution.
- **PerfView needs admin/elevated for kernel events** on some Windows builds. If collection fails with access errors, run the terminal as Administrator, or drop `-Kernel:...`.
- **`netstandard2.0` has no `ArrayPool<T>.Shared`.** If profiling a pooling change, verify under `net8.0` (fast path) AND `netstandard2.0` (fallback path) — they exercise different code.
- **Allocations ≠ retained memory.** A type can allocate heavily and still be cheap if it dies in Gen-0. Use Gen-1/Gen-2 counts and GC pause % to judge real pressure, not just `Allocated` MB.
- **dotnet-trace `.nettrace` opens best in PerfView.** If you only have speedscope, convert first (`dotnet-trace convert --format speedscope`).
- **Backtracking re-parses CPU, not just allocation.** Eliminating an allocation does not eliminate the cost of re-running a rule; memoization (already in `SyntaxParsingContext`) is the bigger lever for backtracking CPU.

---

## 10. Definition of done (profiling)

- [ ] Baseline captured (BenchmarkDotNet + one PerfView/dotnet-trace trace) on representative input, `net8.0`, Release, tiering off.
- [ ] Allocation hot spots identified and mapped to `csly` source (method + type + bytes/count).
- [ ] Heap retention checked with dotnet-gcdump if any pooled/cached object is suspected of leaking.
- [ ] Change implemented; re-captured with identical settings.
- [ ] Allocation reduced (bytes + count) on the hot paths; Gen-0/1/2 down; mean time within 5% of baseline.
- [ ] Before/after numbers recorded; decision (keep / extend / drop) documented per `zero-allocation-plan.md` §6.

---

## Appendix A — .NET Runtime ETW provider keywords (quick reference)

Used with `Microsoft-Windows-DotNETRuntime`:

| Keyword | Hex | Use |
|---------|-----|-----|
| GC | `0x1` | GC start/end, generation, heap size (dotnet-trace default for GC) |
| GCHeapCollect | `0x2` | Detailed GC collection |
| GCHeapCollectAndFinalize | `0x4` | Finalization during GC |
| GCHeapStats | `0x8` | Per-heap statistics |
| GCSampledAllocation / GCHeapAlloc | `0x10` | Sampled allocation stacks (PerfView "GC Alloc Stacks") |
| GCHandle | `0x20` | GC handle tracking |
| Finalizers | `0x40` | Finalizer thread events |

Level: `4` = Informative, `5` = Verbose.  
dotnet-trace syntax: `--providers Microsoft-Windows-DotNETRuntime:<keywords>:<level>`  
Example: `Microsoft-Windows-DotNETRuntime:0x1:4` → GC events at Informative level.
