---
title: com.tutan.functional
---

# ⚡ Functional

> Stop writing defensive `null` checks and scattered `try/catch` blocks.
> Make missing values and failures **impossible to ignore**.

A functional programming toolkit for Unity. Every operation either succeeds or
fails — explicitly, as a value, composable in a pipeline.

---

## ✨ Features

| | |
|---|---|
| 🎯 **Explicit optionality** | `Optional<T>` replaces `null` — handle absence at compile time, not at runtime |
| 💥 **Explicit failure** | `Result<T>` replaces exceptions — failures become values you chain and transform |
| 🔗 **Composable pipelines** | `Then`, `Map`, `Bind`, `Match` — no more nested conditionals |
| ✅ **Accumulating validation** | `Validator<T>` collects every failure before reporting, not just the first |
| ⚡ **Async parity** | The core operators (`Map`, `Bind`, `Then`, `Match`, `Try`) have `UniTask` counterparts — async pipelines look identical to sync |
| 🛡️ **Unity-aware** | Handles Unity's fake-null problem; `LookupComponent`, `LookupParent`, and `Alive()` lifetime re-checks |

---

## 📚 Documentation

| | Guide | What it covers |
|---|---|---|
| 📖 | [Why this library](Functional) | The problem, the approach, performance & hot paths, quick install |
| ❓ | [Optional\<T\>](Optional) | Construction, `Then`, `Or`, `Filter`, `Match`, Unity examples |
| ⚠️ | [Result\<T\>](Result) | Construction, `Then`, `Filter`, `Match`, pipeline patterns |
| 🔴 | [Error](Error) | Simple, nested, composite errors; logging; converting exceptions |
| ✅ | [Validation](Validation) | `Validator<T>`, `FailFast`, `HarvestErrors`, combining validators |
| 🔧 | [Utilities](Utilities) | `F` module, `IEnumerable` extensions, Unity lookup helpers |
| ⚡ | [Async](Async) | `ThenAsync`, `TryAsync`, mixing sync/async pipelines |
| 📋 | [API Reference](API-Reference) | Every public member — signature and one-line description |
