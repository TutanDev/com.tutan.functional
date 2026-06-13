[Home](index) · [Why this library](Functional) · [Optional](Optional) · [Result](Result) · [Error](Error) · [Validation](Validation) · [Utilities](Utilities) · **Async** · [API Reference](API-Reference)

---

# Async Patterns

Every synchronous operator on `Optional<T>` and `Result<T>` has an async counterpart built on **UniTask**. Async pipelines look structurally identical to synchronous ones.

---

## Why UniTask, not `Task<T>`

Unity's main thread is single-threaded. `Task<T>` continuations can resume on thread-pool threads, which means touching `UnityEngine` objects from a continuation causes `UnityException`. UniTask continuations resume on the Unity player loop by default, avoiding this entire class of bug. The library targets UniTask throughout for this reason.

Add the UniTask package before using async methods:

```json
// Packages/manifest.json
{
  "dependencies": {
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask"
  }
}
```

---

## Async operators overview

Each operator exists in three forms to cover all combinations of sync/async source and function:

| Receiver | Function | Method |
|---|---|---|
| `Optional<T>` (sync) | `Func<T, UniTask<R>>` | `ThenAsync` / `MapAsync` |
| `UniTask<Optional<T>>` (async) | `Func<T, R>` (sync) | `Then` / `Map` |
| `UniTask<Optional<T>>` (async) | `Func<T, UniTask<R>>` (async) | `ThenAsync` / `MapAsync` |

The same three-form pattern applies to `Result<T>` and `BindAsync` / `MatchAsync`.

---

## `TryAsync` — async exception wrapping

```csharp
UniTask<Result<T>>    TryAsync<T>(Func<UniTask<T>> f)
UniTask<Result<Unit>> TryAsync(Func<UniTask> action)
```

Wraps an async call that may throw into a `Result`. Use this at every external async boundary.

```csharp
using Cysharp.Threading.Tasks;
using Tutan.Functional;
using static Tutan.Functional.F;

public async UniTask<Result<string>> FetchJsonAsync(string url)
{
    return await TryAsync(async () =>
    {
        using var www = await UnityEngine.Networking.UnityWebRequest.Get(url).SendWebRequest();
        return www.downloadHandler.text;
    });
}
```

---

## `ThenAsync` on `Optional<T>`

### Map overload — transform the value asynchronously

```csharp
UniTask<Optional<R>> ThenAsync<T, R>(this Optional<T> opt, Func<T, UniTask<R>> func)
```

```csharp
Optional<string> path = Some("Prefabs/Enemy");

Optional<GameObject> prefab = await path.ThenAsync(async p =>
    await Resources.LoadAsync<GameObject>(p) as GameObject);
```

### Bind overload — chain to another async optional

```csharp
UniTask<Optional<R>> ThenAsync<T, R>(this Optional<T> opt, Func<T, UniTask<Optional<R>>> func)
```

```csharp
Optional<Enemy> enemy = await path
    .ThenAsync(LoadPrefabAsync)        // UniTask<Optional<GameObject>>
    .ThenAsync(SpawnEnemyAsync);       // UniTask<Optional<Enemy>>
```

### Side-effect overload — run an async action, pass value through

```csharp
UniTask<Optional<T>> ThenAsync<T>(this Optional<T> opt, Func<T, UniTask> action)
```

```csharp
await maybeEnemy.ThenAsync(async e => await e.PlaySpawnAnimationAsync());
```

---

## `ThenAsync` on `Result<T>`

Identical pattern to `Optional<T>`:

```csharp
// map
UniTask<Result<R>> ThenAsync<T, R>(this Result<T> result, Func<T, UniTask<R>> func)

// bind
UniTask<Result<R>> ThenAsync<T, R>(this Result<T> result, Func<T, UniTask<Result<R>>> func)

// side-effect (pass-through)
UniTask<Result<T>> ThenAsync<T>(this Result<T> result, Func<T, UniTask> action)
```

```csharp
Result<PlayerData> data = await Try(() => ReadSaveFilePath())
    .ThenAsync(path => ReadFileAsync(path))       // UniTask<Result<string>>
    .ThenAsync(json => ParsePlayerAsync(json));   // UniTask<Result<PlayerData>>
```

---

## `Then` on `UniTask<Optional<T>>` and `UniTask<Result<T>>`

When the receiver is already a `UniTask<Optional<T>>` (e.g., the result of a previous `ThenAsync`), you can chain synchronous steps directly with `Then`:

```csharp
// sync map on async optional
UniTask<Optional<R>> Then<T, R>(this UniTask<Optional<T>> optTask, Func<T, R> func)

// sync bind on async optional
UniTask<Optional<R>> Then<T, R>(this UniTask<Optional<T>> optTask, Func<T, Optional<R>> func)

// sync side-effect
UniTask<Optional<T>> Then<T>(this UniTask<Optional<T>> optTask, Action<T> action)
```

This allows you to mix sync and async steps in a single readable chain:

```csharp
Optional<Enemy> result = await Some("Prefabs/Boss")
    .ThenAsync(LoadPrefabAsync)          // async: UniTask<Optional<GameObject>>
    .Then(go => go.GetComponent<Enemy>()) // sync:  UniTask<Optional<Enemy>>
    .Then(Tee<Enemy>(e => Debug.Log($"Spawned: {e.name}")));
```

---

## `MatchAsync`

Terminal operator that awaits the result and calls the appropriate branch. Useful at the end of async pipelines.

### On `Optional<T>` — async branches

```csharp
UniTask<R> MatchAsync<T, R>(this Optional<T> opt,
    Func<UniTask<R>> onNone,
    Func<T, UniTask<R>> onSome)
```

### On `UniTask<Optional<T>>` — sync branches

```csharp
UniTask<R> Match<T, R>(this UniTask<Optional<T>> optTask,
    Func<R> onNone,
    Func<T, R> onSome)

// void variant
UniTask Match<T>(this UniTask<Optional<T>> optTask,
    Action onNone,
    Action<T> onSome)
```

### On `UniTask<Optional<T>>` — async branches

```csharp
UniTask<R> MatchAsync<T, R>(this UniTask<Optional<T>> optTask,
    Func<UniTask<R>> onNone,
    Func<T, UniTask<R>> onSome)
```

The same three-form pattern applies to `Result<T>` (with `onError` instead of `onNone`).

---

## Unity examples

### Async asset loading

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine;
using Tutan.Functional;
using static Tutan.Functional.F;

public class EnemySpawner : MonoBehaviour
{
    public async UniTaskVoid SpawnAsync(string prefabPath, Vector3 position)
    {
        await TryAsync(async () =>
                await Resources.LoadAsync<GameObject>(prefabPath) as GameObject)
            .Then(go => Success(Object.Instantiate(go, position, Quaternion.identity)))
            .Match(
                onError: e => Debug.LogError($"Spawn failed: {e.Message}"),
                onSuccess: enemy => Debug.Log($"Spawned {enemy.name}"));
    }
}
```

### Chained async operations with error short-circuit

```csharp
public async UniTask<Result<LeaderboardEntry>> FetchTopScoreAsync(string playerId)
{
    return await TryAsync(() => FetchPlayerAsync(playerId))
        .ThenAsync(player => FetchScoreAsync(player.Id))
        .ThenAsync(score => BuildLeaderboardEntryAsync(score));
    // Any step that returns an Error skips the remaining steps
}
```

### Async validation

```csharp
public async UniTask<Result<SaveData>> LoadAndValidateAsync(string path)
{
    return await TryAsync(async () => await File.ReadAllTextAsync(path))
        .Then(json => Try(() => JsonUtility.FromJson<SaveData>(json)))
        .ThenAsync(data => ValidateOnServerAsync(data));
}
```

---

## Practical guidance

- Use `TryAsync` at every async boundary that might throw.
- Use `ThenAsync` to keep async chains flat — avoid nesting `await` calls inside lambdas when you can chain instead.
- Use `Then` (sync overload on `UniTask<...>`) to interleave cheap synchronous transformations without adding extra `async/await` state machines.
- Always `await` the final `UniTask` — forgetting to await silently swallows errors.
