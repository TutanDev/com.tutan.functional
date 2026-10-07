---
title: com.tutan.functional
---

[Home](index.md) · [Why this library](Functional.md) · [Optional](Optional.md) · [Result](Result.md) · [Error](Error.md) · [Validation](Validation.md) · [Utilities](Utilities.md) · [Async](Async.md) · [API Reference](API-Reference.md)

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
| ✅ **Accumulating validation** | `HarvestErrors` collects every failure before reporting; `FailFast` stops at the first |
| ⚡ **Async parity** | The core operators (`Map`, `Bind`, `Then`, `Match`, `Try`) have `UniTask` counterparts — async pipelines look identical to sync (optional; enabled when UniTask is installed) |
| 🛡️ **Unity-aware** | Handles Unity's fake-null problem; `LookupComponent`, `LookupParent`, and `Alive()` lifetime re-checks |

---

## 📚 Documentation

| | Guide | What it covers |
|---|---|---|
| 📖 | [Why this library](Functional.md) | The problem, the approach, performance & hot paths, quick install |
| ❓ | [Optional\<T\>](Optional.md) | Construction, `Then`, `Or`, `Filter`, `Match`, Unity examples |
| ⚠️ | [Result\<T\>](Result.md) | Construction, `Then`, `Filter`, `Match`, pipeline patterns |
| 🔴 | [Error](Error.md) | Simple, nested, composite errors; logging; converting exceptions |
| ✅ | [Validation](Validation.md) | `Validator<T>`, `FailFast`, `HarvestErrors`, combining validators |
| 🔧 | [Utilities](Utilities.md) | `F` module, `IEnumerable` extensions, Unity lookup helpers |
| ⚡ | [Async](Async.md) | `ThenAsync`, `TryAsync`, mixing sync/async pipelines |
| 📋 | [API Reference](API-Reference.md) | Every public member — signature and one-line description |
