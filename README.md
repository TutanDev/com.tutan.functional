# Tutan.Functional

A lightweight functional programming library for Unity providing core FP primitives for C#.

## Core Types

- **`Optional<T>`** — A value that may or may not exist. Replaces null checks with explicit `Some`/`None` semantics. Null and destroyed `UnityEngine.Object` references become `None` at creation; call `Alive()` to re-check later.
- **`Result<T>`** — An operation outcome carrying either a success value `T` or an `Error`. Replaces exception-based error handling with composable, type-safe results.
- **`Error`** — An immutable struct with a `Message`, an optional integer `Code`, and nested/composite `InnerErrors` for validation scenarios.
- **`Unit`** — `System.ValueTuple`, used as a void substitute (`Result<Unit>`). The package's `Unit` alias is internal to its assembly; add `using Unit = System.ValueTuple;` in your own files.

## Key Features

- **Monadic API** — `Map`, `Bind`, `Apply`, and LINQ query syntax (`from x in opt select ...`) on both `Optional<T>` and `Result<T>`.
- **Fluent chaining** — `Then` as a unified synonym for `Map`/`Bind`, plus `Or`/`OrElse`/`IfFail` fallbacks, and `Filter` predicates.
- **Currying & Piping** — `Curry` (2–3 arguments), `CurryFirst` (3–9 arguments), `Pipe`, and `Tee` for point-free composition.
- **Validation** — `FailFast` and `HarvestErrors` combinators for composing `Validator<T>` pipelines.
- **Safe exception handling** — `Try` wraps throwing code into `Result<T>`.
- **IEnumerable extensions** — `Head`, `FindFirst`, `Flatten`, `DropWhile`, `Match` (head/tail decomposition), and monadic `Map`/`Bind`/`ForEach`.
- **Unity integration** — `LookupComponent<T>`, `LookupParent` returning `Optional<T>` instead of null, and `Alive()` to re-check object lifetime at point of use. Optionals over `UnityEngine.Object` are scope-local: `Some` snapshots fake-null at creation, so call `Alive()` whenever one crosses a frame boundary.

## Performance Notes

The core types (`Optional<T>`, `Result<T>`, `Error`) are allocation-free structs — but the fluent operators are only as free as the lambdas you pass them. A lambda that captures locals or `this` allocates a closure per call. A capture-free lambda is cached by the compiler, and the operators themselves add no allocations, so such a call costs nothing on the heap.

Practical rule: write for clarity in system-level code (loading, validation, config, UI events); in per-frame hot paths keep lambdas capture-free (mark them `static`), use the state-passing overloads (`Map`, `Bind`, `Then`, `Filter`, and `Match` all take a `TState`), or exit the pipeline with `HasValue(out var v)` / `IsSuccess(out var v)`. Full guidance in [Documentation~/Functional.md → Performance & Hot Paths](Documentation~/Functional.md#performance--hot-paths).

## Installation

**Optional:** [UniTask](https://github.com/Cysharp/UniTask) (`com.cysharp.unitask`) enables the async API (`MapAsync`, `ThenAsync`, `TryAsync`, `UniTaskF`, …). The rest of the library compiles and works without it.
- Installed as a UPM package (Git URL / OpenUPM): picked up automatically via the asmdef *Version Defines* (`TUTAN_UNITASK`).
- Copied into `Assets/`: add `TUTAN_UNITASK` to *Project Settings › Player › Scripting Define Symbols*.

To install via Git URLs, add the entries to your project's `Packages/manifest.json` (drop the UniTask line if you don't need async):

```json
{
  "dependencies": {
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.tutan.functional": "https://github.com/TutanDev/com.tutan.functional.git"
  }
}
```

Or clone/copy the package folder into your project's `Packages/` directory.

`Tutan.Functional` is auto-referenced by `Assembly-CSharp`. Assemblies with their own `.asmdef` must add it to their references.
To bring the `F` module helpers (`Some`, `None`, `Success`, `Try`, ...) into scope, add `using static Tutan.Functional.F;` per file. Alternatively, put `global using static Tutan.Functional.F;` once in your own assembly; that needs C# 10, i.e. a `csc.rsp` containing `-langversion:10` next to that assembly's `.asmdef`.

## Quick Example

```csharp
using Tutan.Functional;
using static Tutan.Functional.F;

// Optional
Optional<string> name = Some("Alice");
string greeting = name
    .Then(n => $"Hello, {n}!")
    .Or("Hello, stranger!");

// Result
Result<int> parsed = Try(() => int.Parse("42"));
string message = parsed
    .Then(n => n * 2)
    .Match(
        onError: e => $"Failed: {e.Message}",
        onSuccess: v => $"Result: {v}");
```

## Requirements

- Unity 6000.1+
- Optional: [UniTask](https://github.com/Cysharp/UniTask) 2.x (`com.cysharp.unitask`), installed separately, for the async API
- C# 10 (enabled via `csc.rsp`)
