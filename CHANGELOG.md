# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

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
