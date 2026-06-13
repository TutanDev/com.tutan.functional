[Home](index) · [Why this library](Functional) · [Optional](Optional) · [Result](Result) · [Error](Error) · **Validation** · [Utilities](Utilities) · [Async](Async) · [API Reference](API-Reference)

---

# Validation

The validation API lets you compose individual field-level rules into a single validator that either stops at the first failure (`FailFast`) or collects every failure before reporting (`HarvestErrors`).

---

## `Validator<T>`

```csharp
public delegate Result<T> Validator<T>(T t);
```

A `Validator<T>` is just a function from `T` to `Result<T>`. If the value is valid it returns `Success(t)`; otherwise it returns an `Error`.

```csharp
Validator<PlayerConfig> nameRequired =
    cfg => string.IsNullOrWhiteSpace(cfg.DisplayName)
        ? Error("DisplayName is required")
        : Success(cfg);

Validator<PlayerConfig> levelPositive =
    cfg => cfg.Level >= 1
        ? Success(cfg)
        : Error("Level must be >= 1");
```

---

## `FailFast`

Runs validators **in order** and stops as soon as one fails. The remaining validators are skipped.

```csharp
var validate = FailFast(nameRequired, levelPositive);

Result<PlayerConfig> result = validate(config);
```

**Use case:** Form submission, command parsing, anywhere the first error already makes subsequent checks meaningless (e.g., if a required field is missing, format checks on that field are irrelevant).

### Overloads

```csharp
// params array — most common
Validator<T> FailFast<T>(params Validator<T>[] validators)

// IEnumerable — when you build the list dynamically
Validator<T> FailFast<T>(IEnumerable<Validator<T>> validators)
```

### Example — Unity form submit

```csharp
using UnityEngine;
using Tutan.Functional;
using static Tutan.Functional.F;

[System.Serializable]
public class RegistrationForm
{
    public string Username;
    public string Email;
    public int Age;
}

public class RegistrationValidator : MonoBehaviour
{
    static Validator<RegistrationForm> UsernameNotEmpty =
        f => string.IsNullOrWhiteSpace(f.Username)
            ? Error("Username is required")
            : Success(f);

    static Validator<RegistrationForm> EmailHasAt =
        f => f.Email.Contains('@')
            ? Success(f)
            : Error("Email must contain '@'");

    static Validator<RegistrationForm> AgeOver13 =
        f => f.Age >= 13
            ? Success(f)
            : Error("Must be 13 or older");

    // Stops at first failure — good for a submit button
    static readonly Validator<RegistrationForm> Validate =
        FailFast(UsernameNotEmpty, EmailHasAt, AgeOver13);

    public void OnSubmit(RegistrationForm form)
    {
        Validate(form).Match(
            onError: e => Debug.LogWarning(e.Message),
            onSuccess: f => Debug.Log($"Registered: {f.Username}"));
    }
}
```

---

## `HarvestErrors`

Runs **all** validators and accumulates every failure into a single composite `Error`. Returns `Success` only when every validator passes.

```csharp
var validate = HarvestErrors(nameRequired, levelPositive);

Result<PlayerConfig> result = validate(config);
// On failure, result.Match(onError: e => ...) receives a composite Error
// whose AsEnumerable() yields one Error per failing validator.
```

**Use case:** Config file validation, asset manifest checks, level editor rules — anywhere you want to show the user everything that needs fixing in one pass.

### Overloads

```csharp
Validator<T> HarvestErrors<T>(params Validator<T>[] validators)
Validator<T> HarvestErrors<T>(IEnumerable<Validator<T>> validators)
```

### Example — config validation with multiple fields

```csharp
using UnityEngine;
using Tutan.Functional;
using static Tutan.Functional.F;

[System.Serializable]
public class GameConfig
{
    public string MapName;
    public int MaxPlayers;
    public float RespawnDelay;
}

public class ConfigValidator
{
    static Validator<GameConfig> MapNameSet =
        c => string.IsNullOrWhiteSpace(c.MapName)
            ? Error("MapName is required")
            : Success(c);

    static Validator<GameConfig> MaxPlayersPositive =
        c => c.MaxPlayers > 0
            ? Success(c)
            : Error("MaxPlayers must be > 0");

    static Validator<GameConfig> RespawnDelayNonNegative =
        c => c.RespawnDelay >= 0f
            ? Success(c)
            : Error("RespawnDelay must be >= 0");

    // Collects all failures — good for surfacing config problems at startup
    public static readonly Validator<GameConfig> Validate =
        HarvestErrors(MapNameSet, MaxPlayersPositive, RespawnDelayNonNegative);

    public static void Apply(GameConfig cfg)
    {
        Validate(cfg).Match(
            onError: e =>
            {
                Debug.LogError("Config is invalid:");
                foreach (var err in e.AsEnumerable())
                    Debug.LogError($"  • {err.Message}");
            },
            onSuccess: valid => Debug.Log($"Config OK — map: {valid.MapName}"));
    }
}
```

---

## Combining validators

You can compose `Validator<T>` values with `FailFast` or `HarvestErrors` at any level:

```csharp
// Group related rules
Validator<PlayerConfig> identityRules =
    HarvestErrors(nameRequired, emailRequired);

Validator<PlayerConfig> progressionRules =
    HarvestErrors(levelPositive, currencyNonNegative);

// Then combine the groups with FailFast — identity must pass before checking progression
Validator<PlayerConfig> fullValidation =
    FailFast(identityRules, progressionRules);
```

---

## Practical guidance

- Prefer `HarvestErrors` at startup or in editor tooling where showing all problems saves iteration time.
- Prefer `FailFast` in hot paths or user-facing interactions where only the first error matters.
- Keep each `Validator<T>` focused on a single rule — they compose cleanly and are easy to unit-test individually.
- Use `e.AsEnumerable()` on the error from `HarvestErrors` to iterate individual rule failures for display.
