[Home](index.md) · [Why this library](Functional.md) · [Optional](Optional.md) · [Result](Result.md) · **Error** · [Validation](Validation.md) · [Utilities](Utilities.md) · [Async](Async.md) · [API Reference](API-Reference.md)

---

# Error

`Error` is an immutable value type (24 bytes on 64-bit) that carries a human-readable message, an optional integer `Code`, and optional nested or composite inner errors. It is the failure payload carried by `Result<T>`.

---

## Construction

### Simple error — a single message

```csharp
Error e1 = new Error("Save file not found");
// or via the F module helper:
Error e2 = Error("Save file not found");
// or implicitly from string:
Error e3 = "Save file not found";

// An Error converts implicitly to any Result<T>:
Result<SaveData> result = Error("Save file not found");
// Result<SaveData> bad = "Save file not found";  // does not compile: string → Error → Result is two user-defined conversions
```

### Nested error — wraps a cause

```csharp
Error inner = Error("Disk read failed");
Error outer = Error("Could not load save", inner);
```

Use nesting when you want a high-level message alongside a lower-level cause, similar to `Exception.InnerException`.

### Error with a code — attach a machine-readable code

```csharp
Error e = Error("Save file not found", 404);
// or via the constructor:
Error e = new Error("Save file not found", 404);
// combined with a nested cause:
Error outer = Error("Could not load save", 500, inner);
```

The optional `Code` is an `int` (default `0` when unspecified). Use it when callers should branch on a stable code rather than parse the message — HTTP-style status codes, a domain error enum cast to `int`, and so on. `Code` participates in equality.

### Composite error — collects multiple failures

```csharp
var errors = new[]
{
    Error("DisplayName is required"),
    Error("Level must be >= 1"),
    Error("Currency must be >= 0"),
};
Error composite = Error(errors);
// composite.Message == "DisplayName is required; Level must be >= 1; Currency must be >= 0"
```

The composite constructor joins messages with `"; "` and stores all errors as inner errors. This is the format produced by `HarvestErrors` — see [Validation.md](Validation.md).

### From an exception

`F.Try` and `F.TryAsync` automatically convert exceptions to `Error`:

```csharp
Result<int> result = Try(() => int.Parse("bad"));
// On failure, the carried Error's Message is the exception's full ToString():
result.IfFail(e => Debug.LogError(e.Message));
```

You can also do this manually:

```csharp
// needs: using System; using Unit = System.ValueTuple;
try { /* ... */ }
catch (Exception ex)
{
    Result<Unit> result = Error(ex.ToString());
}
```

---

## Properties

| Member | Type | Description |
|---|---|---|
| `Message` | `string` | The human-readable description of this error |
| `Code` | `int` | Optional machine-readable error code; `0` when unspecified |
| `InnerErrors` | `ReadOnlySpan<Error>` | Inner errors (empty span when none). **Cannot cross async boundaries** — call `.ToArray()` first if you need to store or pass it. |

---

## `AsEnumerable()`

Flattens an error into an `IEnumerable<Error>` suitable for logging or iteration:

- For a **simple** or **nested** error, returns `{ this }`. A nested error keeps its high-level message; reach the cause through `InnerErrors`.
- For a **composite** error, returns its inner errors (one level, not recursive).

```csharp
Error composite = Error(new[] { Error("A"), Error("B"), Error("C") });

foreach (var e in composite.AsEnumerable())
    Debug.Log(e.Message);
// Logs: A, B, C
```

---

## How `Error` flows through `Result<T>`

`Result<T>` holds either a `T` value or an `Error`. Errors short-circuit `Then` / `Map` / `Bind` chains automatically:

```csharp
Result<int> result = Error("bad input");

// Every Then is skipped because result is already an Error
Result<string> display = result
    .Then(n => n * 2)       // skipped
    .Then(n => n.ToString()); // skipped

display.Match(
    onError: e => Debug.LogError(e.Message),   // "bad input"
    onSuccess: s => Debug.Log(s));
```

---

## Chaining errors across service calls

```csharp
using UnityEngine;
using Tutan.Functional;
using static Tutan.Functional.F;

public class GameSaveService
{
    public Result<SaveData> Load(string path)
    {
        return Try(() => System.IO.File.ReadAllText(path))
            .Then(json => ParseSave(json));

        Result<SaveData> ParseSave(string json)
        {
            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) return Error("JSON parsed to null", Error($"Input: {json[..System.Math.Min(json.Length, 40)]}"));
            return Success(data);
        }
    }
}
```

---

## Logging composite errors

```csharp
void LogErrors(Error error)
{
    foreach (var e in error.AsEnumerable())
        Debug.LogError(e.Message);
}

// Usage after HarvestErrors validation:
configResult.Match(
    onError: LogErrors,
    onSuccess: cfg => ApplyConfig(cfg));
```

---

## Equality and `ToString`

`Error` implements `IEquatable<Error>`. Two errors are equal when their `Message`, `Code`, and inner error arrays are all equal (deep comparison).

`ToString()` returns `Message` (or `string.Empty` for a default `Error`).
