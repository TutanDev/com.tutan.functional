[Home](index) · [Why this library](Functional) · [Optional](Optional) · **Result** · [Error](Error) · [Validation](Validation) · [Utilities](Utilities) · [Async](Async) · [API Reference](API-Reference)

---

# Result in Unity

`Result<T>` represents the outcome of an operation:
- `Success(T value)` when everything works.
- `Error` when something fails.

This is useful in gameplay and service code where failures are expected and should be handled without exceptions controlling normal flow.

## Why `Result<T>` is useful

- **Explicit success/failure**: Function signatures show that errors are possible.
- **Composable error handling**: Chain operations with `Then` and handle failures once.
- **Cleaner control flow**: Avoid deeply nested `try/catch` blocks for expected failures.
- **Great for Unity boundaries**: Parsing config, loading resources, and validating user input all map naturally to `Result<T>`.

## Unity-tailored examples

## 1) Parse player input safely

```csharp
using UnityEngine;
using Tutan.Functional;
using static Tutan.Functional.F;

public class LivesParser : MonoBehaviour
{
    public Result<int> ParseLives(string raw)
    {
        return Try(() => int.Parse(raw))
            .Filter(v => v >= 0);
    }

    public void ApplyLives(string raw)
    {
        string message = ParseLives(raw).Match(
            onError: e => $"Invalid lives value: {e.Message}",
            onSuccess: lives => $"Lives set to {lives}");

        Debug.Log(message);
    }
}
```

## 2) Load resources with clear errors

```csharp
using UnityEngine;
using Tutan.Functional;
using static Tutan.Functional.F;

public class EnemyPrefabLoader
{
    public Result<GameObject> LoadEnemy(string path)
    {
        return Success(Resources.Load<GameObject>(path))
            .Then(prefab => Object.Instantiate(prefab));
    }
}
```

`Success` returns an error automatically if a reference-type value is null (including destroyed Unity objects), so missing assets are surfaced as `Result` errors.

## 3) Build a pipeline and handle once

```csharp
using UnityEngine;
using Tutan.Functional;
using static Tutan.Functional.F;

public class PlayerBootstrap : MonoBehaviour
{
    [SerializeField] private string configJson;

    public void Initialize()
    {
        Result<PlayerConfig> result = Try(() => JsonUtility.FromJson<PlayerConfig>(configJson))
            .Filter(cfg => cfg != null)
            .Then(cfg => Validate(cfg));

        result.Match(
            onError: e => Debug.LogError($"Bootstrap failed: {e.Message}"),
            onSuccess: cfg => Debug.Log($"Player initialized: {cfg.DisplayName}"));
    }

    private Result<PlayerConfig> Validate(PlayerConfig cfg)
    {
        if (string.IsNullOrWhiteSpace(cfg.DisplayName))
            return Error("DisplayName is required");

        return Success(cfg);
    }

    [System.Serializable]
    public class PlayerConfig
    {
        public string DisplayName;
    }
}
```

## Practical guidance

- Use `Result<T>` when you need to explain **why** something failed.
- Return `Optional<T>` for "not found is normal" cases, and `Result<T>` for true failure cases.
- Prefer `Match` at system boundaries (UI, logs, game state transitions) to centralize error handling.
