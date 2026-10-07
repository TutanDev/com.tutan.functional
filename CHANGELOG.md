# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-07-05

First Unity Asset Store release.

### Added
- XML documentation comments (`<summary>`, and `<param>`/`<remarks>` where they add information) across the entire public API: `Optional<T>`, `Result<T>`, `Error`, the `F` module, all extension classes, the async surface, and `UniTaskF`.
- Documented the state-passing `Void` / `WaitUntil` helpers (shipped since 0.2.0 but previously absent from the docs, now `UniTaskF`) in `Documentation~/Async.md` and the API reference.
- `Third Party Notices.md` now contains the actual UniTask MIT license text (was an unfilled template).
- `keywords`, `documentationUrl`, and `changelogUrl` in `package.json`.

### Changed
- **UniTask is now an optional dependency.** It was a hard compile-time dependency, so importing the package into a project without UniTask produced compile errors. UPM can't declare it either, because it doesn't support Git URLs in a package's `dependencies` block. Now the runtime and test asmdefs define `TUTAN_UNITASK` via *Version Defines* when `com.cysharp.unitask` is installed, and every async file is compiled only under that define. If UniTask was copied into `Assets/` rather than installed as a package, add `TUTAN_UNITASK` to the Scripting Define Symbols manually.
- **Breaking:** `UniTask.Void(...)` / `UniTask.WaitUntil<TState>(...)` moved to `Tutan.Functional.UniTaskF.Void(...)` / `UniTaskF.WaitUntil(...)`, with the same signatures. They used to be injected into UniTask's own assembly via `UniTaskRef.asmref`, which can't be made conditional (it would point to a missing assembly when UniTask is absent), so the `.asmref` is removed. Migrate by renaming the receiver.
- Corrected docs that claimed the package ships `global using static Tutan.Functional.F` for consuming assemblies - C# global usings do not cross assembly boundaries; consumers add the using themselves.
- Documented that `default(Result<T>)` is an error carrying `default(Error)` (null `Message`), and that `EnumerableExt.Match` enumerates its source twice.

### Fixed
- `EnumerableExt.FindFirst` now returns `None` for a null source instead of throwing, matching `Head`.
- **Per-call closure allocations in the core operators.** `Map`, `Bind`, `Where`, `SelectMany`, `Apply`, `Or`, `OrElse`, `Filter`, `ToResult`, `ToOptional`, `ValueUnsafe`/`ErrorUnsafe` and `Result.ForEach` were implemented by routing through `Match` with lambdas that captured the caller's delegate, so every call allocated a closure and a delegate even when the caller's own lambda was capture-free. That contradicted the documented hot-path guidance. The operators now branch directly, and `AllocationTests` guards the contract.
- `Then(Action<T>)` (the side-effect pass-through) on `Optional<T>`/`Result<T>` now returns the original instance. It used to rebuild it through `Map(F.Tee(action))`, which allocated a closure per call and re-ran the null/fake-null check. As a result, an object destroyed inside the action with `DestroyImmediate` turned the pass-through into `None`/`Error`.
- `UniTaskF.WaitUntil<TState>` (formerly `UniTask.WaitUntil<TState>`): the pooled promise now clears its `state` field when it returns to the pool. Before, the pool (static, lives for the whole domain) kept the last caller's state object reachable after the wait completed.
- `SerializableOptionalDrawer` now also implements `OnGUI`/`GetPropertyHeight`. Before, IMGUI-drawn inspectors (custom `Editor`s, third-party inspectors) showed "No GUI implemented". The UI Toolkit path now tracks the serialized `_hasValue`, so undo/redo and multi-object edits refresh the field's enabled state.
- `Error.AsEnumerable()` on a **nested** error (`Error(msg, inner)`) returned the inner cause instead of `{ this }`, contrary to its documentation. The bug was that nested and composite errors both stored their inner errors and weren't told apart. As a result, `HarvestErrors` dropped a nested validator error's high-level message. `Error` now records whether it is a composite (no size change, still 24 bytes). Equality now also compares that shape.
- Docs: fixed samples that did not compile (`Utilities.md` JSON chain and `Apply` example, duplicate declarations in `Error.md`) and missing `using`s / `Unit` alias notes. Removed redundant `Filter(x => x != null)` after `Try`. Intra-doc links now use the `.md` form so they work when browsing on GitHub. Added missing members to the API reference.
- Docs: `Result<T>`'s `operator true`/`operator false` enable `if (result)`, `while (result)` and `result ? a : b`. They do **not** enable `&&`/`||`, which would also need `operator &`/`|`. The 0.5.0 note and the API reference claimed otherwise.

## [0.5.0] - 2026-06-16

### Added
- `F.Fail(...)` / `F.Fail<T>(...)` failure factories (`string` or `Error`) — the partner of `Success()`/`Success<T>(value)`, for when an explicit fail reads better than relying on the implicit `Error → Result<T>` conversion.
- `IfFail` extensions on `Result<T>` — a `Func<Error, Result<T>>` form for Result-space recovery and an `Action<Error>` form that observes the error and passes the `Result` through unchanged.
- `Match` overloads on `Result<Unit>` that take a parameterless success branch (`Func<R>` / `Action`) so void results read as `() => …` instead of `_ => …`. Disambiguated from the instance `Match` by the success delegate's arity.
- `operator true` / `operator false` on `Result<T>`, enabling `if (result)` and `&&`/`||` short-circuiting on the success branch.

## [0.4.1] - 2026-06-13

### Fixed
- Removed an unused type parameter from `F.Tee(Action)` — the overload was declared `Tee<T>(Action)` but never used `T`, forcing callers to supply a meaningless type argument. The `Tee<T>(Action<T>)` pass-through overload is unaffected.

## [0.4.0] - 2026-06-12

### Added
- `Alive()` extension on `Optional<T>` (constrained to `UnityEngine.Object`) — re-checks Unity lifetime at point of use, returning `None` if the wrapped object was destroyed after `Some`. The monadic analogue of `if (obj)`.
- Documented the **scope-local rule**: `Optional<T>` over a `UnityEngine.Object` snapshots fake-null at creation and is only valid within the frame/method that created it; re-lift at point of use or call `Alive()` after a frame boundary.
- State-passing (`TState`) overloads for `Match`, `Then` (map/bind/side-effect shapes), and `Filter` on both `Optional<T>` and `Result<T>`, completing capture-free coverage of the hot-path operator set alongside the existing `Map`/`Bind`.

### Changed
- Softened "zero-allocation" wording across docs and code comments: the core structs are allocation-free, but capturing lambdas allocate per call. Added a *Performance & Hot Paths* guide (docs/Functional.md) with hot-path guidance — capture-free lambdas, state-passing `Map`/`Bind` overloads, and out-param accessors.

### Fixed
- `SerializableOptionalDrawer` moved from the editor *test* assembly into a dedicated `Editor/` assembly so the drawer actually ships to consumers.

## [0.3.0] - 2026-05-21

### Added
- `SerializableOptional<T>` in the `Tutan.Functional` runtime assembly — `[Serializable]` inspector-friendly wrapper around `Optional<T>` with implicit conversions both ways.
- `SerializableOptionalDrawer` — UI Toolkit `PropertyDrawer` rendering a toggle plus the inner value field. (Initially shipped in the editor test assembly; moved to a dedicated `Editor/` assembly in 0.4.0.)
- `Error.Code` — optional `int` error code with `Error(message, code)` / `Error(message, code, inner)` constructors and matching `F.Error` helpers. Participates in equality.

## [0.2.0] - 2026-02-25

*(entry reconstructed from git history; it was missing at release time)*

### Added
- Async support built on UniTask, mirroring the sync operator set on both `Optional<T>` and `Result<T>`: `MapAsync`, `BindAsync`, `ThenAsync`, `MatchAsync`, plus `Then`/`Map`/`Bind`/`Match` overloads on `UniTask<Optional<T>>` / `UniTask<Result<T>>`.
- `F.TryAsync` for wrapping throwing async code into `UniTask<Result<T>>`.
- `UniTask.Void` (2-5 argument overloads) and state-passing `UniTask.WaitUntil<TState>`, compiled into the UniTask assembly via `UniTaskRef.asmref`.
- UniTask became a required dependency of the package.

## [0.1.0] - 2026-02-12

### Added
- `Optional<T>` — option type with `Some`/`None`, `Match`, `Map`, `Bind`, `Apply`, LINQ support, and Unity fake-null handling.
- `Result<T>` — result type with `Success`/`Error`, `Match`, `Map`, `Bind`, `Apply`, LINQ support, and `Filter`.
- `Error` record with composite error support (`Inner`, `InnerErrors`, `AsEnumerable`).
- Fluent API: `Then`, `Or`/`OrElse`, `Filter`, `HasValue`/`IsSuccess` (out-param), `ValueUnsafe`/`ErrorUnsafe`.
- `Try` helper for wrapping throwing code into `Result<T>`.
- `ToOptional` for `Nullable<T>`, `ToResult`/`ToOptional` conversions between `Optional<T>` and `Result<T>`.
- Currying (`Curry`, `CurryFirst`) up to 9 type parameters.
- `Pipe` and `Tee` for fluent composition.
- `IEnumerable<T>` extensions: `Head`, `FindFirst`, `Flatten`, `DropWhile`, `Match`, `Map`, `Bind`, `ForEach`.
- `Validator<T>` delegate with `FailFast` and `HarvestErrors` combinators.
- Unity integration: `LookupComponent<T>`, `LookupParent`, dictionary `Lookup`.
- `ActionExtensions`: `ToFunc()` adapters.
