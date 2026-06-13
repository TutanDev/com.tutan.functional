# Tutan.Functional

A lightweight functional programming library for Unity providing core FP primitives for C#.

## Core Types

- **`Optional<T>`** — A value that may or may not exist. Replaces null checks with explicit `Some`/`None` semantics. Handles Unity's fake-null (`UnityEngine.Object`) transparently.
- **`Result<T>`** — An operation outcome carrying either a success value `T` or an `Error`. Replaces exception-based error handling with composable, type-safe results.
- **`Error`** — A record type with `Message`, optional `Inner` error, and composite `InnerErrors` for validation scenarios.
- **`Unit`** — Alias for `System.ValueTuple`, used as a void substitute in functional signatures.

## Key Features

- **Monadic API** — `Map`, `Bind`, `Apply`, and LINQ query syntax (`from x in opt select ...`) on both `Optional<T>` and `Result<T>`.
- **Fluent chaining** — `Then` as a unified synonym for `Map`/`Bind`, plus `Or`/`OrElse` fallbacks, and `Filter` predicates.
- **Currying & Piping** — `Curry`, `CurryFirst` (up to 9 type parameters), `Pipe`, and `Tee` for point-free composition.
- **Validation** — `FailFast` and `HarvestErrors` combinators for composing `Validator<T>` pipelines.
- **Safe exception handling** — `Try` wraps throwing code into `Result<T>`.
- **IEnumerable extensions** — `Head`, `FindFirst`, `Flatten`, `DropWhile`, `Match` (head/tail decomposition), and monadic `Map`/`Bind`/`ForEach`.
- **Unity integration** — `LookupComponent<T>`, `LookupParent` returning `Optional<T>` instead of null, and `Alive()` to re-check object lifetime at point of use. Optionals over `UnityEngine.Object` are scope-local: `Some` snapshots fake-null at creation, so call `Alive()` whenever one crosses a frame boundary.

## Performance Notes

The core types (`Optional<T>`, `Result<T>`, `Error`) are allocation-free structs — but the fluent operators are only as free as the lambdas you pass them. A lambda that captures locals or `this` allocates a closure per call; a capture-free lambda is cached by the compiler and costs nothing per call.

Practical rule: write for clarity in system-level code (loading, validation, config, UI events); in per-frame hot paths keep lambdas capture-free (mark them `static`), use the state-passing overloads (`Map`, `Bind`, `Then`, `Filter`, and `Match` all take a `TState`), or exit the pipeline with `HasValue(out var v)` / `IsSuccess(out var v)`. Full guidance in [Documentation~/Functional.md → Performance & Hot Paths](Documentation~/Functional.md#performance--hot-paths).

## Installation

Add to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.tutan.functional": "https://github.com/TutanDev/com.tutan.functional.git"
  }
}
```

Or clone/copy the package folder into your project's `Packages/` directory.

Consumer assemblies must add `Tutan.Functional` to their `.asmdef` references.

## Quick Example

```csharp
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
- C# 10 (enabled via `csc.rsp`)
