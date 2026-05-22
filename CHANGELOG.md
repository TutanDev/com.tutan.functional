# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.3.0] - 2026-05-21

### Added
- `Tutan.Functional.Unity` assembly with `SerializableOptional<T>` — `[Serializable]` inspector-friendly wrapper around `Optional<T>` with implicit conversions both ways.
- `Tutan.Functional.Unity.Editor` assembly with `SerializableOptionalDrawer` — UI Toolkit `PropertyDrawer` rendering a toggle plus the inner value field.

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
