[Home](index) · [Why this library](Functional) · [Optional](Optional) · [Result](Result) · [Error](Error) · [Validation](Validation) · [Utilities](Utilities) · [Async](Async) · **API Reference**

---

# API Reference

Scannable reference for every public member. Signatures are simplified (type constraints omitted unless essential). See the linked guides for usage examples.

---

## F — static module (`Tutan.Functional.F`)

Brought into scope automatically via `global using static Tutan.Functional.F`.

### Core

| Signature | Description |
|---|---|
| `Unit Unit()` | Returns the empty value (`default(ValueTuple)`) |
| `IEnumerable<T> List<T>(params T[] items)` | Creates an `IEnumerable<T>` from inline values |

### Optional construction

| Signature | Description |
|---|---|
| `NoneType None` | The absence sentinel; implicitly converts to `Optional<T>` |
| `Optional<T> Some<T>(T value)` | Wraps a value in `Some`. Returns `None` for null references and destroyed `UnityEngine.Object` references. |

### Result construction

| Signature | Description |
|---|---|
| `Result<Unit> Success()` | Creates a successful `Result` with no value |
| `Result<T> Success<T>(T value)` | Creates a successful `Result`. Returns `Error("Value is null")` for null references and destroyed Unity objects. |

### Error construction

| Signature | Description |
|---|---|
| `Error Error(string message)` | Simple error with a message |
| `Error Error(string message, int code)` | Error with a machine-readable code |
| `Error Error(string message, Error inner)` | Nested error — message + cause |
| `Error Error(string message, int code, Error inner)` | Nested error with a code |
| `Error Error(IEnumerable<Error> errors)` | Composite error — joins all messages; stores all as inner errors |

### Exception wrapping

| Signature | Description |
|---|---|
| `Result<T> Try<T>(Func<T> f)` | Calls `f`, catches any exception, returns `Success` or `Error(ex.ToString())` |
| `Result<Unit> Try(Action action)` | Same, for actions with no return value |
| `UniTask<Result<T>> TryAsync<T>(Func<UniTask<T>> f)` | Async version of `Try` |
| `UniTask<Result<Unit>> TryAsync(Func<UniTask> action)` | Async version for async actions |

### Function composition

| Signature | Description |
|---|---|
| `Func<Unit> Tee(Action function)` | Lifts `Action` to `Func<Unit>` for pipeline use |
| `Func<T, T> Tee<T>(Action<T> function)` | Runs side-effect and passes `T` through unchanged |
| `R Pipe<T, R>(this T input, Func<T, R> func)` | Applies `func` to `input`; enables left-to-right chaining |
| `T Pipe<T>(this T input, Action<T> func)` | Runs `func(input)` and returns `input` |

### Curry / partial application

| Signature | Description |
|---|---|
| `Func<T1, Func<T2, R>> Curry<T1,T2,R>(this Func<T1,T2,R> func)` | Curries a 2-argument function |
| `Func<T1, Func<T2, Func<T3,R>>> Curry<T1,T2,T3,R>(this Func<T1,T2,T3,R> func)` | Curries a 3-argument function |
| `Func<T1, Func<T2,T3,R>> CurryFirst<T1,T2,T3,R>(this Func<T1,T2,T3,R> @this)` | Fixes first argument; remainder stay as tuple (3→9 argument overloads) |

### Validation

| Signature | Description |
|---|---|
| `Validator<T> FailFast<T>(params Validator<T>[] validators)` | Runs validators in order; stops at first failure |
| `Validator<T> FailFast<T>(IEnumerable<Validator<T>> validators)` | Enumerable overload |
| `Validator<T> HarvestErrors<T>(params Validator<T>[] validators)` | Runs all validators; accumulates all failures |
| `Validator<T> HarvestErrors<T>(IEnumerable<Validator<T>> validators)` | Enumerable overload |

---

## `Optional<T>` (`readonly record struct`)

### Construction

| Signature | Description |
|---|---|
| `(implicit) Optional<T>(T value)` | Via `Some(value)` or implicit cast from `T` |
| `(implicit) Optional<T>(NoneType _)` | Via `None` or implicit cast from `NoneType` |

### Properties

| Signature | Description |
|---|---|
| `bool IsSome` | `true` when a value is present |
| `bool IsNone` | `true` when absent |

### Core methods

| Signature | Description |
|---|---|
| `R Match<R>(Func<R> onNone, Func<T,R> onSome)` | Extracts the value by pattern matching |
| `Unit Match(Action onNone, Action<T> onSome)` | Void pattern match |

### Extension methods (`OptionalExtensions`)

#### Extraction

| Signature | Description |
|---|---|
| `bool HasValue<T>(this Optional<T> opt, out T value)` | Out-param extraction; returns `false` for `None` |
| `T ValueUnsafe<T>(this Optional<T> opt)` | Throws `InvalidOperationException` on `None`; use only after guard |
| `T Or<T>(this Optional<T> opt, T fallback)` | Returns the value or `fallback` |
| `T OrElse<T>(this Optional<T> opt, Func<T> fallback)` | Returns the value or the result of `fallback()` |

#### Transformation

| Signature | Description |
|---|---|
| `Optional<R> Then<T,R>(this Optional<T> opt, Func<T,R> func)` | Map — applies `func` if `Some` |
| `Optional<T> Then<T>(this Optional<T> opt, Action<T> action)` | Side-effect pass-through |
| `Optional<R> Then<T,R>(this Optional<T> opt, Func<T,Optional<R>> func)` | Bind — flatMap |
| `Optional<R> Map<T,R>(this Optional<T> opt, Func<T,R> f)` | Alias for `Then(Func<T,R>)` |
| `Optional<R> Bind<T,R>(this Optional<T> opt, Func<T,Optional<R>> f)` | Alias for `Then(Func<T,Optional<R>>)` |
| `Optional<T> Filter<T>(this Optional<T> opt, Func<T,bool> predicate)` | Returns `None` if predicate fails |

#### Conversion

| Signature | Description |
|---|---|
| `IEnumerable<T> AsEnumerable<T>(this Optional<T> opt)` | Zero or one element; safe to iterate |
| `IEnumerable<R> Bind<T,R>(this Optional<T> opt, Func<T,IEnumerable<R>> func)` | Flat-maps into a sequence (empty when `None`) |
| `IEnumerable<Optional<R>> Traverse<T,R>(this Optional<T> opt, Func<T,IEnumerable<R>> func)` | Distributes the `Optional` over a sequence-producing function |
| `Result<T> ToResult<T>(this Optional<T> opt, Func<Error> onNone)` | Converts `None` to an error |
| `Optional<T> ToOptional<T>(this T? nullable) where T : struct` | Converts a nullable struct |

#### Applicative

| Signature | Description |
|---|---|
| `Optional<R> Apply<T,R>(this Optional<Func<T,R>> opt, Optional<T> arg)` | Applies a lifted function to a lifted value |
| *(2–8 argument overloads)* | Allow multi-argument lifted functions |

#### LINQ query syntax

| Signature | Description |
|---|---|
| `Optional<R> Select<T,R>(this Optional<T> opt, Func<T,R> func)` | Enables `from x in opt select ...` |
| `Optional<T> Where<T>(this Optional<T> opt, Func<T,bool> predicate)` | Enables `where` clause |
| `Optional<RR> SelectMany<T,R,RR>(...)` | Enables multiple `from` clauses |

#### State-passing (avoids closure capture; see [Performance & Hot Paths](Functional#performance--hot-paths))

| Signature | Description |
|---|---|
| `Optional<R> Map<T,TState,R>(this Optional<T> opt, TState state, Func<T,TState,R> f)` | Map without capturing state in a closure |
| `Optional<R> Bind<T,TState,R>(this Optional<T> opt, TState state, Func<T,TState,Optional<R>> f)` | Bind without closure capture |
| `R Match<TState,R>(TState state, Func<TState,R> onNone, Func<T,TState,R> onSome)` | Match without closure capture (instance method) |
| `Unit Match<TState>(TState state, Action<TState> onNone, Action<T,TState> onSome)` | Side-effect match without closure capture (instance method) |
| `Optional<R> Then<T,TState,R>(this Optional<T> opt, TState state, Func<T,TState,R> func)` | State-passing map alias |
| `Optional<R> Then<T,TState,R>(this Optional<T> opt, TState state, Func<T,TState,Optional<R>> func)` | State-passing bind alias |
| `Optional<T> Then<T,TState>(this Optional<T> opt, TState state, Action<T,TState> action)` | State-passing side-effect, passes value through |
| `Optional<T> Filter<T,TState>(this Optional<T> opt, TState state, Func<T,TState,bool> predicate)` | Filter without closure capture |

#### Async (see also [Async.md](Async.md))

| Signature | Description |
|---|---|
| `UniTask<Optional<R>> ThenAsync<T,R>(this Optional<T> opt, Func<T,UniTask<R>> func)` | Async map |
| `UniTask<Optional<R>> ThenAsync<T,R>(this Optional<T> opt, Func<T,UniTask<Optional<R>>> func)` | Async bind |
| `UniTask<Optional<T>> ThenAsync<T>(this Optional<T> opt, Func<T,UniTask> action)` | Async side-effect pass-through |
| `UniTask<Optional<R>> Then<T,R>(this UniTask<Optional<T>> optTask, Func<T,R> func)` | Sync map on async optional |
| `UniTask<Optional<R>> Then<T,R>(this UniTask<Optional<T>> optTask, Func<T,Optional<R>> func)` | Sync bind on async optional |
| `UniTask<Optional<T>> Then<T>(this UniTask<Optional<T>> optTask, Action<T> action)` | Sync side-effect on async optional |
| `UniTask<Optional<R>> ThenAsync<T,R>(this UniTask<Optional<T>> optTask, Func<T,UniTask<R>> func)` | Async map on async optional |
| `UniTask<Optional<R>> ThenAsync<T,R>(this UniTask<Optional<T>> optTask, Func<T,UniTask<Optional<R>>> func)` | Async bind on async optional |
| `UniTask<Optional<T>> ThenAsync<T>(this UniTask<Optional<T>> optTask, Func<T,UniTask> action)` | Async side-effect on async optional |
| `UniTask<R> MatchAsync<T,R>(this Optional<T> opt, Func<UniTask<R>> onNone, Func<T,UniTask<R>> onSome)` | Async match on sync optional |
| `UniTask<R> Match<T,R>(this UniTask<Optional<T>> optTask, Func<R> onNone, Func<T,R> onSome)` | Sync match on async optional |
| `UniTask Match<T>(this UniTask<Optional<T>> optTask, Action onNone, Action<T> onSome)` | Void sync match on async optional |
| `UniTask<R> MatchAsync<T,R>(this UniTask<Optional<T>> optTask, Func<UniTask<R>> onNone, Func<T,UniTask<R>> onSome)` | Async match on async optional |

> `MapAsync` and `BindAsync` are the underlying primitives behind these async operators — same three sync/async receiver-and-function forms, plus the sync-function-on-async-receiver forms named `Map`/`Bind`. The `ThenAsync`/`Then` rows above are unified aliases over them, exactly as in the sync API.

---

## `SerializableOptional<T>` (`[Serializable] struct`)

Inspector-serializable counterpart to `Optional<T>`. Use as a `[SerializeField]` field; convert to/from `Optional<T>` at the API boundary.

### Construction

| Signature | Description |
|---|---|
| `SerializableOptional(T value)` | Wraps a value (sets `_hasValue = true`) |
| `static SerializableOptional<T> From(Optional<T> opt)` | Converts from `Optional<T>`; `None` → `default` |

### Properties

| Signature | Description |
|---|---|
| `bool HasValue` | `true` when a value is present |
| `T Value` | The wrapped value (undefined when `HasValue` is `false`) |

### Conversion

| Signature | Description |
|---|---|
| `Optional<T> ToOptional()` | Converts to `Optional<T>` |
| `(implicit) operator Optional<T>(SerializableOptional<T>)` | Implicit cast to `Optional<T>` |
| `(implicit) operator SerializableOptional<T>(Optional<T>)` | Implicit cast from `Optional<T>` |

### Editor — `SerializableOptionalDrawer`

`CustomPropertyDrawer` for `SerializableOptional<>` (applied to all closed generics via `useForChildren: true`). UI Toolkit-based: renders a `Toggle` bound to `_hasValue` next to a `PropertyField` for `_value`. The value field is enabled only while the toggle is on.

---

## `Result<T>` (`readonly record struct`)

### Construction

| Signature | Description |
|---|---|
| `(implicit) Result<T>(T value)` | Via `Success(value)` or implicit cast from `T` |
| `(implicit) Result<T>(Error error)` | Via implicit cast from `Error` |

### Properties

| Signature | Description |
|---|---|
| `bool IsSuccess` | `true` when the operation succeeded |
| `bool IsError` | `true` when the operation failed |

### Core methods

| Signature | Description |
|---|---|
| `R Match<R>(Func<Error,R> onError, Func<T,R> onSuccess)` | Pattern match — extract value or handle error |
| `Unit Match(Action<Error> onError, Action<T> onSuccess)` | Void pattern match |

### Extension methods (`ResultExtensions`)

#### Extraction

| Signature | Description |
|---|---|
| `bool IsSuccess<T>(this Result<T> result, out T value)` | Out-param extraction; returns `false` on error |
| `T ValueUnsafe<T>(this Result<T> @this)` | Throws on error; use only after guard |
| `Error ErrorUnsafe<T>(this Result<T> @this)` | Throws on success; use only after guard |
| `T Or<T>(this Result<T> result, T fallback)` | Returns value or `fallback` |
| `T OrElse<T>(this Result<T> result, Func<T> fallback)` | Returns value or `fallback()` |
| `T OrElse<T>(this Result<T> result, Func<Error,T> fallback)` | Returns value or `fallback(error)` |

#### Transformation

| Signature | Description |
|---|---|
| `Result<R> Then<T,R>(this Result<T> result, Func<T,R> func)` | Map — applies `func` on success |
| `Result<T> Then<T>(this Result<T> result, Action<T> action)` | Side-effect pass-through |
| `Result<R> Then<T,R>(this Result<T> result, Func<T,Result<R>> func)` | Bind — flatMap |
| `Result<R> Map<T,R>(this Result<T> result, Func<T,R> f)` | Alias for `Then(Func<T,R>)` |
| `Result<R> Bind<T,R>(this Result<T> result, Func<T,Result<R>> f)` | Alias for `Then(Func<T,Result<R>>)` |
| `Result<Unit> ForEach<T>(this Result<T> result, Action<T> action)` | Side-effect, returns `Result<Unit>` |
| `Result<T> Filter<T>(this Result<T> result, Func<T,bool> predicate)` | Converts failing predicate to `Error("Predicate not satisfied")` |

#### Conversion

| Signature | Description |
|---|---|
| `IEnumerable<T> AsEnumerable<T>(this Result<T> result)` | Zero or one element; safe to iterate |
| `IEnumerable<R> Bind<T,R>(this Result<T> result, Func<T,IEnumerable<R>> func)` | Flat-maps into a sequence (empty on error) |
| `Optional<T> ToOptional<T>(this Result<T> result)` | Discards the error; converts success to `Some` |

#### Applicative

| Signature | Description |
|---|---|
| `Result<R> Apply<T,R>(this Result<Func<T,R>> @this, Result<T> arg)` | Applies a lifted function to a lifted value |
| *(2–8 argument overloads)* | Allow multi-argument lifted functions |

#### LINQ query syntax

| Signature | Description |
|---|---|
| `Result<R> Select<T,R>(this Result<T> result, Func<T,R> f)` | Enables `from x in result select ...` |
| `Result<RR> SelectMany<T,R,RR>(...)` | Enables multiple `from` clauses |

#### State-passing (avoids closure capture; see [Performance & Hot Paths](Functional#performance--hot-paths))

| Signature | Description |
|---|---|
| `Result<R> Map<T,TState,R>(this Result<T> result, TState state, Func<T,TState,R> f)` | Map without closure capture |
| `Result<R> Bind<T,TState,R>(this Result<T> result, TState state, Func<T,TState,Result<R>> f)` | Bind without closure capture |
| `R Match<TState,R>(TState state, Func<Error,TState,R> onError, Func<T,TState,R> onSuccess)` | Match without closure capture (instance method) |
| `Unit Match<TState>(TState state, Action<Error,TState> onError, Action<T,TState> onSuccess)` | Side-effect match without closure capture (instance method) |
| `Result<R> Then<T,TState,R>(this Result<T> result, TState state, Func<T,TState,R> func)` | State-passing map alias |
| `Result<R> Then<T,TState,R>(this Result<T> result, TState state, Func<T,TState,Result<R>> func)` | State-passing bind alias |
| `Result<T> Then<T,TState>(this Result<T> result, TState state, Action<T,TState> action)` | State-passing side-effect, passes value through |
| `Result<T> Filter<T,TState>(this Result<T> result, TState state, Func<T,TState,bool> predicate)` | Filter without closure capture; `Error("Predicate not satisfied")` on false |

#### Async (see also [Async.md](Async.md))

| Signature | Description |
|---|---|
| `UniTask<Result<R>> ThenAsync<T,R>(this Result<T> result, Func<T,UniTask<R>> func)` | Async map |
| `UniTask<Result<R>> ThenAsync<T,R>(this Result<T> result, Func<T,UniTask<Result<R>>> func)` | Async bind |
| `UniTask<Result<T>> ThenAsync<T>(this Result<T> result, Func<T,UniTask> action)` | Async side-effect pass-through |
| `UniTask<Result<R>> Then<T,R>(this UniTask<Result<T>> resultTask, Func<T,R> func)` | Sync map on async result |
| `UniTask<Result<R>> Then<T,R>(this UniTask<Result<T>> resultTask, Func<T,Result<R>> func)` | Sync bind on async result |
| `UniTask<Result<T>> Then<T>(this UniTask<Result<T>> resultTask, Action<T> action)` | Sync side-effect on async result |
| `UniTask<Result<R>> ThenAsync<T,R>(this UniTask<Result<T>> resultTask, Func<T,UniTask<R>> func)` | Async map on async result |
| `UniTask<Result<R>> ThenAsync<T,R>(this UniTask<Result<T>> resultTask, Func<T,UniTask<Result<R>>> func)` | Async bind on async result |
| `UniTask<Result<T>> ThenAsync<T>(this UniTask<Result<T>> resultTask, Func<T,UniTask> action)` | Async side-effect on async result |
| `UniTask<R> MatchAsync<T,R>(this Result<T> result, Func<Error,UniTask<R>> onError, Func<T,UniTask<R>> onSuccess)` | Async match on sync result |
| `UniTask<R> Match<T,R>(this UniTask<Result<T>> resultTask, Func<Error,R> onError, Func<T,R> onSuccess)` | Sync match on async result |
| `UniTask Match<T>(this UniTask<Result<T>> resultTask, Action<Error> onError, Action<T> onSuccess)` | Void sync match on async result |
| `UniTask<R> MatchAsync<T,R>(this UniTask<Result<T>> resultTask, Func<Error,UniTask<R>> onError, Func<T,UniTask<R>> onSuccess)` | Async match on async result |

> `MapAsync` and `BindAsync` are the underlying primitives behind these async operators — same three sync/async receiver-and-function forms, plus the sync-function-on-async-receiver forms named `Map`/`Bind`. The `ThenAsync`/`Then` rows above are unified aliases over them, exactly as in the sync API.

---

## `Error` (`readonly struct`, `IEquatable<Error>`)

### Construction

| Signature | Description |
|---|---|
| `Error(string message)` | Simple error |
| `Error(string message, int code)` | Error with a machine-readable code |
| `Error(string message, Error inner)` | Nested error |
| `Error(string message, int code, Error inner)` | Nested error with a code |
| `Error(IEnumerable<Error> errors)` | Composite — joins all messages; stores all as inner |
| `(implicit) Error(string message)` | Implicit cast from `string` |

### Properties

| Signature | Description |
|---|---|
| `string Message` | Human-readable error description |
| `int Code` | Optional machine-readable error code; `0` when unspecified |
| `ReadOnlySpan<Error> InnerErrors` | Inner errors (empty span when none). Cannot cross async boundaries — call `.ToArray()` if needed. |

### Methods

| Signature | Description |
|---|---|
| `IEnumerable<Error> AsEnumerable()` | Leaf errors: `{ this }` for simple/nested; inner errors for composite |
| `override string ToString()` | Returns `Message` (or `string.Empty` for default) |
| `bool Equals(Error other)` | Deep equality — compares `Message`, `Code`, and all inner errors |

---

## Validation

### `Validator<T>` delegate

```csharp
public delegate Result<T> Validator<T>(T t);
```

A function that validates a value and returns `Success(t)` or an `Error`.

### Factory methods (on `F`)

| Signature | Description |
|---|---|
| `Validator<T> FailFast<T>(params Validator<T>[] validators)` | Stops at first failure |
| `Validator<T> FailFast<T>(IEnumerable<Validator<T>> validators)` | Enumerable overload |
| `Validator<T> HarvestErrors<T>(params Validator<T>[] validators)` | Collects all failures |
| `Validator<T> HarvestErrors<T>(IEnumerable<Validator<T>> validators)` | Enumerable overload |

---

## Extension methods

### `ActionExtensions`

| Signature | Description |
|---|---|
| `Func<Unit> ToFunc(this Action action)` | Lifts `Action` to `Func<Unit>` |
| `Func<T, Unit> ToFunc<T>(this Action<T> action)` | Lifts `Action<T>` to `Func<T, Unit>` |

### `EnumerableExt`

| Signature | Description |
|---|---|
| `Optional<T> Head<T>(this IEnumerable<T> list)` | First element as `Optional`; `None` on empty or null |
| `Optional<T> FindFirst<T>(this IEnumerable<T> source, Func<T,bool> predicate)` | First matching element as `Optional` |
| `IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> list)` | Flattens one level of nesting |
| `R Match<T,R>(this IEnumerable<T> list, Func<R> Empty, Func<T,IEnumerable<T>,R> Otherwise)` | Structural list match (head + tail) |
| `IEnumerable<T> DropWhile<T>(this IEnumerable<T> source, Func<T,bool> pred)` | Skips leading elements matching predicate |
| `Func<T, IEnumerable<T>> Return<T>()` | Wraps a value in a singleton sequence |
| `IEnumerable<R> Map<T,R>(this IEnumerable<T> list, Func<T,R> func)` | Alias for `Select` |
| `IEnumerable<R> Bind<T,R>(this IEnumerable<T> list, Func<T,IEnumerable<R>> func)` | Alias for `SelectMany` |
| `IEnumerable<R> Bind<T,R>(this IEnumerable<T> list, Func<T,Optional<R>> func)` | Flat-map filtering `None` |
| `IEnumerable<Unit> ForEach<T>(this IEnumerable<T> ts, Action<T> action)` | Side-effect over sequence (lazy) |

### `LookupExtensions`

| Signature | Description |
|---|---|
| `Optional<T> LookupComponent<T>(this GameObject go) where T : Component` | Safe `TryGetComponent`; `None` when null/destroyed or component absent |
| `Optional<Transform> LookupParent(this Transform t)` | Safe parent access; `None` when no parent or destroyed |
| `Optional<T> Lookup<K,T>(this IDictionary<K,T> dict, K key)` | Safe dictionary access; `None` on missing key |
| `Optional<T> Alive<T>(this Optional<T> opt) where T : UnityEngine.Object` | Re-checks Unity lifetime; `None` if the wrapped object was destroyed after `Some` |
