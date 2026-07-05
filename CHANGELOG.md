# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-07-05

First Unity Asset Store release.

### Added
- XML documentation comments (`<summary>`, and `<param>`/`<remarks>` where they add information) across the entire public API: `Optional<T>`, `Result<T>`, `Error`, the `F` module, all extension classes, the async surface, and the `UniTask.Void`/`UniTask.WaitUntil` additions.
- Documented the `UniTask.Void` / state-passing `UniTask.WaitUntil` additions (shipped since 0.2.0 but previously absent from the docs) in `Documentation~/Async.md` and the API reference.
- `Third Party Notices.md` now contains the actual UniTask MIT license text (was an unfilled template).
- `keywords`, `documentationUrl`, and `changelogUrl` in `package.json`.

### Changed
- **UniTask is now a documented manual prerequisite instead of a `package.json` dependency.** UPM does not support Git URLs in a package's `dependencies` block, so the previous declaration could not resolve; README, `Documentation~`, and the store listing now state the requirement explicitly. The compile-time dependency is unchanged.
- Corrected docs that claimed the package ships `global using static Tutan.Functional.F` for consuming assemblies - C# global usings do not cross assembly boundaries; consumers add the using themselves.
- Documented that `default(Result<T>)` is an error carrying an empty-message `Error`, and that `EnumerableExt.Match` enumerates its source twice.

### Fixed
- `EnumerableExt.FindFirst` now returns `None` for a null source instead of throwing, matching `Head`.

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
