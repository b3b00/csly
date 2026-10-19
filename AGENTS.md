# AGENTS.md — csly development harness

This file helps AI agents and developers work effectively in the **csly** repository.

---

## Project overview

**csly** (a.k.a. `sly`) is a C# lexer/parser generator inspired by Python's PLY. It is designed to be **fully embeddable** — no build-time code generation step — and distributed as a single NuGet package (`sly`).

A language is defined with only two C# types:

1. A C# `enum` for the lexer tokens (e.g. `JsonToken`).
2. A C# `class` for the parser, with productions declared via `[Production("...")]` custom attributes (BNF/EBNF style).

Key features:
- Fully typed parsers (input and output types are statically checked).
- Expression parsing with precedence/associativity schemes.
- Indentation-sensitive languages (Python/YAML style).
- Comment preservation via lexer channels (ANTLR-style channels).
- Reusable generic lexer.

NuGet package: [`sly`](https://www.nuget.org/packages/sly) — `dotnet add package sly` / `Install-Package sly`.
Docs: [github wiki](https://github.com/b3b00/csly/wiki). Getting started: [wiki/getting-started](https://github.com/b3b00/csly/wiki/getting-started).

---

## Repository layout

```
/                      # repo root
  README.md            # project presentation + install instructions
  CONTRIBUTE.md        # contribution guidelines (branch from `dev`, add xUnit tests)
  sly.sln              # Visual Studio solution
  .gitignore           # ignores bin/obj, *.user, coverage, *.nupkg, *.dtp*
  .vscode/             # VS Code launch/settings/tasks
  .devcontainer/       # dev container (Dockerfile + devcontainer.json)
  .github/
    workflows/
      dotnetcore.yml   # CI: build + test (coverlet, threshold 80) + nuget publish + release
    dependabot.yml
    refresh-badges.yml
  src/
    sly/               # main library project (src/sly/sly.csproj)
      sly.csproj       # targets net8.0;net7.0;netstandard2.0;netstandard2.1;net6.0 ; LangVersion latest
      lexer/           # lexer engine: Lexer.cs, Token.cs, LexerAttribute.cs, GenericLexer.cs, ...
      parser/          # parser engine + LR generator + fluent API + syntax tree
      i18n/            # embedded translations (en.txt, fr.txt, zh-Hans.txt)
      EnumConverter.cs
    samples/           # example languages (each is its own csproj)
      jsonparser/      # JSON: JSONLexer.cs + EBNFJSONParser.cs + EbnfJsonGenericParser.cs
      expressionParser/
      SimpleExpressionParser/
      while/           # indented while-like language
      IndentedWhile/
      indented/
      XML/
      SimpleTemplate/
      GenericLexerWithCallbacks/
      postProcessedLexerParser/
      SlowEOS/
      ParserExample/
      HandMadeExpressions/
      Issue244/
    bench/             # BenchmarkDotNet benchmarks
    bench2.4/
    benchCurrent/
    profiler/
    logos/
  tests/
    ParserTests/       # xUnit + NFluent test project (tests/ParserTests/ParserTests.csproj)
      ParserTests.csproj   # targets net8.0; references sly + all samples
      EBNFTests.cs
      ExpressionTests.cs
      ErrorTests.cs
      FluentTests.cs
      IssuesTests.cs
      ... (many more)
      IssueNNN/        # folders per reported issue (reproductions + regression tests)
      lexer/
      samples/
      stack/
      comments/
      cover.sh
      coverage.ps1
      stryker-config.json
  profiling/           # profiling artifacts
  *.dtp / *.dtp.State  # dotTrace performance session files (gitignored)
```

Default branch: **`dev`** (CI builds `dev`, `master`, `feature/implicit-tokens`, `tech/actions`).

---

## How to build

From the repo root:

```powershell
# Restore + build everything in Release (CI configuration)
dotnet build --configuration Release

# Debug build
dotnet build --configuration Debug

# Build only the main library
dotnet build src/sly/sly.csproj --configuration Release

# Build only the tests
dotnet build tests/ParserTests/ParserTests.csproj --configuration Debug
```

Target frameworks of the main library: `net8.0`, `net7.0`, `netstandard2.0`, `netstandard2.1`, `net6.0`.  
LangVersion: `latest`.

The solution `sly.sln` includes the library, all samples, and the test project. Samples reference `..\..\sly\sly.csproj`.

---

## How to test

Tests use **xUnit** + **NFluent** (`tests/ParserTests/ParserTests.csproj`, targets `net8.0`).

```powershell
# Run the full test suite
dotnet test tests/ParserTests/ParserTests.csproj --configuration Debug

# Run with coverage (coverlet.msbuild is already referenced)
dotnet test tests/ParserTests/ParserTests.csproj --configuration Release \
  /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:CoverletOutput=lcov.info
```

CI enforces a **80% coverage threshold** on the main library (see `.github/workflows/dotnetcore.yml`, `threshold: 80`). Test results are published as `.trx` files; coverage goes to coveralls.

### Test conventions to follow

- Test project: `tests/ParserTests`.
- Framework: xUnit (`[Fact]`, `[Theory]`).
- Assertions: prefer **NFluent** (`Check.That(...).IsEqualTo(...)`).
- Use the custom check extensions in `tests/ParserTests/NFluentParseExtensions.cs` (`IsOkParsing`, `IsOkLexing`, `IsOk`, `HasError`, `IsEqualTo`, `IsSingle`, …) for legible assertions and better error reporting on parse/lex/build results.
- When fixing a bug or reproducing an issue, add a folder `tests/ParserTests/IssueNNN/` with a minimal reproduction and regression test(s), then register the test in the appropriate test class (e.g. `IssuesTests.cs` or a dedicated `*Tests.cs`).
- Features must come with unit tests (per `CONTRIBUTE.md`).
- Sample languages under `src/samples/` are referenced by the test project and double as integration tests.

---

## Coding conventions

### Language definition pattern (the csly way)

**Lexer** — an `enum` of tokens plus an `ILexer<TToken>` implementation (or attribute-driven lexer):

```csharp
public enum JsonToken { STRING, INT, DOUBLE, BOOLEAN, NULL, CROG, CROD, ACCG, ACCD, COLON, COMMA, ... }

public class JSONLexer : ILexer<JsonToken>
{
    public void AddDefinition(TokenDefinition<JsonToken> tokenDefinition) { }
    public LexerResult<JsonToken> Tokenize(string source) { ... }
    public LexerResult<JsonToken> Tokenize(ReadOnlyMemory<char> source) { ... }
}
```

**Parser** — a class decorated with `[ParserRoot("ruleName")]`, with `[Production("rule : symbols")]` methods:

```csharp
[ParserRoot("root")]
public class MyParser
{
    [Production("root : value")]
    public JSon Root(JSon value) => value;

    [Production("value : INT")]
    public JSon IntValue(Token<JsonToken> intToken) => new JValue(intToken.IntValue);
}
```

Common attributes seen in the codebase: `[Production]`, `[ParserRoot]`, `[BroadenTokenWindow]`, `[Lexer]`, `[Token]`, `[TokenChannel]`, `[Ignore]`, `[Expression]`, `[Priority]`, `[Associativity]`.

### General style

- C# files use **PascalCase** namespaces and types, `camelCase` locals/parameters.
- Namespace typically matches the sample folder (e.g. `namespace jsonparser`).
- Project files: `<Project Sdk="Microsoft.NET.Sdk">`, `Authors = b3b00`, `NoWarn 1701;1702;1705;1591` in Debug.
- `using` directives at the top; `sly.lexer`, `sly.parser.generator` are the main namespaces.
- Samples target `net8.0;net7.0` (or just `net8.0`) and reference the local `sly.csproj`.

### Do NOT commit

Per `.gitignore`: `bin/`, `obj/`, `Debug/`, `Release/`, `*.user`, `*.suo`, `.vs/`, `.idea/`, coverage files (`coverage.info`, `lcov.*`, `coverage.json`), `*.nupkg`, `*.snupkg`, `*.dtp`, `*.dtp.state`, `BenchmarkDotNet.Artifacts`, `AotTester/*`, `src/samples/HandMadeExpres*`.

---

## Contribution workflow (from CONTRIBUTE.md)

1. Work from the **`dev`** branch: create your feature/fix branch off `dev`.
2. Bug fixes and new features must include **unit tests (xUnit)**.
3. Open a pull request against `dev`.
4. The PR triggers a CI build; the build must succeed and **all unit tests must pass**.
5. Version bump in `src/sly/sly.csproj` (`<version>` / `<PackageVersion>`, currently `3.8.1`) triggers a NuGet publish + GitHub release on CI.

---

## CI summary (`.github/workflows/dotnetcore.yml`)

- Triggers: push to `dev` / `master` / `tech/actions` / `feature/implicit-tokens`, pull requests, workflow dispatch.
- Job `build` on `ubuntu-latest`, .NET 8.0.x.
- Steps: checkout → setup dotnet → clean → `dotnet build --configuration Release` → `dotnet test` (coverlet, lcov, threshold 80) → publish test results → coveralls → conditional NuGet publish (`PUBLISH_NUGET=true`) → conditional GitHub release.
- Relevant env vars: `TESTS_PROJECT`, `MAIN_CSPROJ`, `PUBLISH_NUGET`, `RUN_TESTS`.

---

## Useful commands cheat sheet

```powershell
dotnet build --configuration Release                    # build all
dotnet test tests/ParserTests/ParserTests.csproj        # run tests
dotnet run --project src/samples/jsonparser             # run a sample (if it has an entry point)
dotnet pack src/sly/sly.csproj --configuration Release  # create the nupkg locally (outputs to ./nupkg)
```

---

## When in doubt

- Read `README.md` for the project pitch and feature list.
- Read `CONTRIBUTE.md` for the branching/PR policy.
- Look at `src/samples/jsonparser/` for a complete, idiomatic lexer + parser example.
- Look at `tests/ParserTests/EBNFTests.cs` and `tests/ParserTests/IssuesTests.cs` for test-writing patterns.
- The wiki (linked from README) is the authoritative documentation for BNF/EBNF parser definition, expression parsing, indented languages, and channels.
