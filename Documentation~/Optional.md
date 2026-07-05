[Home](index) · [Why this library](Functional) · **Optional** · [Result](Result) · [Error](Error) · [Validation](Validation) · [Utilities](Utilities) · [Async](Async) · [API Reference](API-Reference)

---

# Optional in Unity

`Optional<T>` represents a value that might exist (`Some`) or might be missing (`None`).
In Unity projects this is especially useful for replacing `null` checks with explicit handling.

## Why `Optional<T>` is useful

- **Makes absence explicit**: APIs clearly communicate that a value can be missing.
- **Reduces null-reference bugs**: You must decide what to do in the `None` case.
- **Composes cleanly**: Chain logic with `Then`, `Filter`, and `Or` instead of nested `if` statements.
- **Unity-friendly**: `Some` treats destroyed `UnityEngine.Object` references as `None`, handling Unity's fake-null behavior. For objects that may be destroyed later, `Alive()` re-checks at the point of use (see [Unity object lifetimes](#unity-object-lifetimes-the-scope-local-rule)).

## Unity-tailored examples

## 1) Safe component lookup

```csharp
using UnityEngine;
using Tutan.Functional;
using static Tutan.Functional.F;

public class HealthReader : MonoBehaviour
{
    [SerializeField] private GameObject target;

    private void Start()
    {
        Optional<Health> maybeHealth = Some(target)
            .Then(go => go.GetComponent<Health>());

        int hp = maybeHealth
            .Then(h => h.Current)
            .Or(0);

        Debug.Log($"Current HP: {hp}");
    }
}

public class Health : MonoBehaviour
{
    public int Current = 100;
}
```

## 2) UI fallback when data is missing

```csharp
using TMPro;
using UnityEngine;
using Tutan.Functional;
using static Tutan.Functional.F;

public class PlayerNameView : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    public void Render(Optional<string> playerName)
    {
        label.text = playerName
            .Filter(name => name.Length > 0)
            .Then(name => $"Pilot: {name}")
            .Or("Pilot: Guest");
    }
}
```

## 3) Branching with `Match`

```csharp
using UnityEngine;
using Tutan.Functional;
using static Tutan.Functional.F;

public class SpawnPointResolver : MonoBehaviour
{
    [SerializeField] private Transform fallbackSpawn;

    public Vector3 ResolveSpawn(Optional<Transform> maybeSpawn)
    {
        return maybeSpawn.Match(
            onNone: () => fallbackSpawn.position,
            onSome: t => t.position);
    }
}
```

## 4) Save data read

```csharp
using Tutan.Functional;
using static Tutan.Functional.F;

public static class SaveReader
{
    public static Optional<int> TryReadLevel(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return None;

        return int.TryParse(raw, out var level)
            ? Some(level)
            : None;
    }
}

// Usage
// int levelToLoad = SaveReader.TryReadLevel(playerPrefsValue).Or(1);
```

## 5) Inspector-serializable optionals

`Optional<T>` is a `readonly record struct` and is intentionally not Unity-serializable — exposing it on a `MonoBehaviour` would defeat the explicit `Some`/`None` distinction. For fields you want to author in the Inspector, use `SerializableOptional<T>`:

```csharp
using UnityEngine;
using Tutan.Functional;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private SerializableOptional<Transform> overrideSpawn;
    [SerializeField] private SerializableOptional<int> startingWaveOverride;

    private void Start()
    {
        Vector3 spawn = overrideSpawn
            .ToOptional()
            .Then(t => t.position)
            .Or(transform.position);

        int wave = startingWaveOverride.ToOptional().Or(1);
    }
}
```

`SerializableOptional<T>` is a `[Serializable]` struct with a `_hasValue` toggle and an inner `_value`. It converts both ways with `Optional<T>`:

- `someOpt.ToOptional()` — explicit conversion to `Optional<T>`.
- `SerializableOptional<T>.From(opt)` — explicit conversion from `Optional<T>`.
- Implicit casts in both directions are also available, so a field typed as `SerializableOptional<T>` can be passed anywhere an `Optional<T>` is expected.

### Custom Inspector drawer

`SerializableOptionalDrawer` (UI Toolkit `PropertyDrawer`, applied to `SerializableOptional<>` and all closed generics via `useForChildren: true`) renders the field as a row containing:

- a **toggle** bound to `_hasValue`
- the inner `_value` field, **enabled only when the toggle is on**

This gives Inspector authors a clear "use this value / leave it unset" UX without exposing the wrapped value as a magic default.

### Practical guidance

- Use `SerializableOptional<T>` **only** at the serialization boundary (`[SerializeField]`, `ScriptableObject` data). Convert to `Optional<T>` as early as possible and keep the rest of the pipeline in terms of `Optional<T>`.
- Don't expose `SerializableOptional<T>` from public APIs — it's a UI/serialization concern, not a domain concern.

## Unity object lifetimes: the scope-local rule

`Optional<T>` is an immutable snapshot. `Some` checks Unity's fake-null **once, at creation** — it does not track the object afterwards. If the wrapped `UnityEngine.Object` is destroyed later, the optional still reports `IsSome == true` and will happily hand out a destroyed reference:

```csharp
Optional<GameObject> cached = Some(enemy);
Object.Destroy(enemy);

cached.IsSome;            // true — the snapshot is stale
cached.Alive().IsSome;    // false — Alive() re-checks Unity lifetime
```

This is deliberate. Unity itself doesn't track lifetimes ambiently either — a destroyed object sitting in a plain C# field is just as stale until you explicitly test it with `if (obj)`. Baking the check into `IsSome`/`Match` would make two equal optionals behave differently over time and tax every non-Unity `T` with a check it doesn't need.

So the rule is:

> **An `Optional<T>` over a `UnityEngine.Object` is scope-local — treat it as valid only within the frame/method that created it.**

When an optional has to live longer than that, you have two sanctioned options:

1. **Re-lift at point of use** (preferred): store the raw reference (or the `GameObject`) and create the optional where you consume it — `go.LookupComponent<T>()` is cheap.
2. **Re-check with `Alive()`**: if you do hold an `Optional<T>` across frames, call `Alive()` after the boundary, before consuming it. `Alive()` is the monadic analogue of `if (obj)` — same semantics, same cost, but it stays inside the pipeline:

```csharp
private Optional<Transform> _cachedTarget;

private void Update()
{
    _cachedTarget
        .Alive()                       // None if the target was destroyed
        .Then(t => Steer(t.position));
}
```

`Alive()` is constrained to `T : UnityEngine.Object`, so it doesn't exist (and costs nothing) for plain C# types — their snapshots can never go stale.

## Practical guidance

- Use `Optional<T>` for values that are truly optional (component may be absent, save field may be unset, query may return nothing).
- Keep `Optional` at API boundaries where missing data is expected.
- Prefer `Match`, `Or`, and `Then` over `ValueUnsafe` unless you've already guaranteed `Some`.
- Treat optionals over `UnityEngine.Object` as scope-local; call `Alive()` whenever one crosses a frame boundary.
