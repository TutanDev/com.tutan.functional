[Home](index) · [Why this library](Functional) · [Optional](Optional) · [Result](Result) · [Error](Error) · [Validation](Validation) · **Utilities** · [Async](Async) · [API Reference](API-Reference)

---

# Utilities — F module & extension methods

This document covers the utility belt that underpins the library: the `F` static module, `IEnumerable` extensions, and Unity-specific lookup helpers.

---

## F module

Add `using static Tutan.Functional.F;` to bring all helpers into scope. The package ships a `global using` for assemblies that reference it, so this is usually automatic.

---

### `Try` / `TryAsync` — wrapping exception-throwing code

Convert any exception-throwing call into a `Result<T>` without a try/catch at the call site.

```csharp
// Returns Result<T>
Result<T>    Try<T>(Func<T> f)
Result<Unit> Try(Action action)

// Returns UniTask<Result<T>>
UniTask<Result<T>>    TryAsync<T>(Func<UniTask<T>> f)
UniTask<Result<Unit>> TryAsync(Func<UniTask> action)
```

**Example — parsing and disk I/O**

```csharp
using Tutan.Functional;
using static Tutan.Functional.F;

// Parsing
Result<int> lives = Try(() => int.Parse(rawInput));

// File I/O
Result<string> json = Try(() => System.IO.File.ReadAllText(savePath));

// Chained — parse JSON then validate
Result<PlayerConfig> config = Try(() => JsonUtility.FromJson<PlayerConfig>(json))
    .Filter(c => c != null)
    .Then(c => Validate(c));
```

> The `Error` message on failure is the exception's full `ToString()`, which includes the stack trace. Strip it before showing to end users.

---

### `Tee` — side-effects without breaking pipelines

Run an `Action` inside a pipeline without changing the value or type being passed through.

```csharp
// Wraps Action in Func<Unit>
Func<Unit> Tee(Action function)

// Wraps Action<T> in Func<T, T> — passes T through unchanged
Func<T, T> Tee<T>(Action<T> function)
```

**Example — logging mid-pipeline**

```csharp
using UnityEngine;
using Tutan.Functional;
using static Tutan.Functional.F;

Result<PlayerConfig> result = Try(() => JsonUtility.FromJson<PlayerConfig>(json))
    .Then(Tee<PlayerConfig>(cfg => Debug.Log($"Parsed config for {cfg.DisplayName}")))
    .Then(cfg => Validate(cfg));
```

`Tee` is also how `Then(Action<T>)` is implemented on `Optional<T>` and `Result<T>` — it turns a side-effect into a pass-through mapping step.

---

### `Pipe` — forward function application

Apply a function to a value in a left-to-right, readable style.

```csharp
R    Pipe<T, R>(this T input, Func<T, R> func)  // returns func(input)
T    Pipe<T>(this T input, Action<T> func)       // runs func(input), returns input
```

**Example**

```csharp
int result = 5.Pipe(x => x * 2)   // 10
              .Pipe(x => x + 1);   // 11
```

`Pipe` is most useful when you want to apply a standalone function without creating an extension method.

---

### `Curry` / `CurryFirst` — partial application

Convert multi-argument functions into chains of single-argument functions.

```csharp
// Curry — curries all parameters one by one
Func<T1, Func<T2, R>>         Curry<T1, T2, R>(this Func<T1, T2, R> func)
Func<T1, Func<T2, Func<T3,R>>> Curry<T1, T2, T3, R>(this Func<T1, T2, T3, R> func)

// CurryFirst — fixes only the first argument; rest remain as a tuple
Func<T1, Func<T2, T3, R>>     CurryFirst<T1, T2, T3, R>(this Func<T1, T2, T3, R> @this)
// ... up to 9-argument overloads
```

**Example — factory with a fixed dependency**

```csharp
Func<string, int, Enemy> createEnemy = (name, hp) => new Enemy(name, hp);

// Fix the name, produce a function that only needs HP
Func<int, Enemy> createGoblin = createEnemy.Curry()("Goblin");

Enemy weak   = createGoblin(10);
Enemy strong = createGoblin(50);
```

**Example — applicative style with `Optional<T>.Apply`**

```csharp
Optional<Func<string, int, Enemy>> optFactory = Some(createEnemy).Map(F.Curry);
Optional<Enemy> enemy = optFactory.Apply(Some("Orc")).Apply(Some(30));
```

---

### `Unit` — the empty value

```csharp
Unit Unit()  // returns default(ValueTuple)
```

Used as the value type for `Result<Unit>` when an operation succeeds but has no meaningful return value (e.g., `Try(Action)`, `TryAsync(Func<UniTask>)`).

---

### `List<T>` — create an `IEnumerable<T>` from params

```csharp
IEnumerable<T> List<T>(params T[] items)
```

Convenience wrapper around `Array.AsEnumerable()`. Useful for inline collections in pipelines.

```csharp
IEnumerable<int> scores = List(10, 20, 30);
```

---

## `IEnumerable` extensions (`EnumerableExt`)

### `Head<T>` — first element as `Optional<T>`

```csharp
Optional<T> Head<T>(this IEnumerable<T> list)
```

Returns `Some(first)` or `None` if the sequence is empty or null. Never throws.

```csharp
Optional<Enemy> first = enemies.Head();
```

### `FindFirst<T>` — predicate search as `Optional<T>`

```csharp
Optional<T> FindFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate)
```

Returns the first matching element or `None`.

```csharp
Optional<Enemy> boss = enemies.FindFirst(e => e.IsBoss);
boss.Match(
    onNone: () => Debug.Log("No boss found"),
    onSome: e => Debug.Log($"Boss: {e.Name}"));
```

### `Flatten<T>` — flattens nested sequences

```csharp
IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> list)
```

Equivalent to `SelectMany(x => x)`.

```csharp
IEnumerable<Item> allItems = zones.Map(z => z.Items).Flatten();
```

### `Match<T, R>` — structural match on a sequence

```csharp
R Match<T, R>(this IEnumerable<T> list,
    Func<R> Empty,
    Func<T, IEnumerable<T>, R> Otherwise)
```

Deconstructs the list into head and tail. Returns `Empty()` for an empty sequence.

```csharp
string summary = enemies.Match(
    Empty: () => "No enemies",
    Otherwise: (first, rest) => $"{first.Name} and {rest.Count()} others");
```

### `DropWhile<T>` — skip leading elements matching a predicate

```csharp
IEnumerable<T> DropWhile<T>(this IEnumerable<T> source, Func<T, bool> pred)
```

Skips elements while the predicate is true, then yields everything from the first non-matching element onward.

```csharp
// Skip leading zeros: [0, 0, 3, 0, 5] → [3, 0, 5]
var trimmed = values.DropWhile(v => v == 0);
```

### `Map<T, R>` — alias for `Select`

```csharp
IEnumerable<R> Map<T, R>(this IEnumerable<T> list, Func<T, R> func)
```

### `Bind<T, R>` — `SelectMany` / flat-map, with `Optional<R>` overload

```csharp
IEnumerable<R> Bind<T, R>(this IEnumerable<T> list, Func<T, IEnumerable<R>> func)
IEnumerable<R> Bind<T, R>(this IEnumerable<T> list, Func<T, Optional<R>> func)
```

The `Optional<R>` overload filters out `None` results automatically:

```csharp
// Collect only enemies that have a weapon equipped
IEnumerable<Weapon> weapons = enemies.Bind(e => e.EquippedWeapon); // Optional<Weapon>
```

### `ForEach<T>` — side-effect over a sequence

```csharp
IEnumerable<Unit> ForEach<T>(this IEnumerable<T> ts, Action<T> action)
```

Note: this is lazy — enumerate the result or call `.ToList()` to force execution.

---

## Unity lookup helpers (`LookupExtensions`)

These are the most common Unity null-check patterns wrapped into `Optional<T>`-returning extension methods.

### `LookupComponent<T>` — safe `TryGetComponent`

```csharp
Optional<T> LookupComponent<T>(this GameObject go) where T : Component
```

Returns `None` if the `GameObject` is null/destroyed or the component is not attached.

```csharp
Optional<Rigidbody> rb = gameObject.LookupComponent<Rigidbody>();

rb.Match(
    onNone: () => Debug.LogWarning("No Rigidbody found"),
    onSome: r => r.AddForce(Vector3.up * 10f));
```

### `LookupParent` — safe parent transform access

```csharp
Optional<Transform> LookupParent(this Transform t)
```

Returns `None` if the transform is null/destroyed or has no parent.

```csharp
Optional<Transform> parent = transform.LookupParent();

Vector3 parentPos = parent
    .Then(p => p.position)
    .Or(Vector3.zero);
```

### `Lookup<K, T>` — safe dictionary access

```csharp
Optional<T> Lookup<K, T>(this IDictionary<K, T> dict, K key)
```

Returns `None` instead of throwing `KeyNotFoundException`.

```csharp
Dictionary<string, AudioClip> clips = /* ... */;

Optional<AudioClip> clip = clips.Lookup("explosion");
clip.Match(
    onNone: () => Debug.LogWarning("Clip not found"),
    onSome: c => audioSource.PlayOneShot(c));
```

---

## `ActionExtensions`

Internal helpers that convert `Action` / `Action<T>` to `Func` equivalents so they can be used in functional pipelines.

```csharp
Func<Unit>    ToFunc(this Action action)
Func<T, Unit> ToFunc<T>(this Action<T> action)
```

Used internally by `ForEach`, `Then(Action<T>)`, and `Tee`. Rarely called directly.
