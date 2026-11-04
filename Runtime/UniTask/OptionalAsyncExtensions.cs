#if TUTAN_UNITASK
// Compiled only when UniTask is installed: TUTAN_UNITASK is set by this assembly's versionDefines
// for com.cysharp.unitask (or manually, for UniTask copied into Assets/).
using System;
using Cysharp.Threading.Tasks;
using static Tutan.Functional.F;

namespace Tutan.Functional
{
    /// <summary>
    /// The async operators on <see cref="Optional{T}"/> and <c>UniTask&lt;Optional&lt;T&gt;&gt;</c>. They live in
    /// the <c>Tutan.Functional.UniTask</c> assembly, so an assembly that references only
    /// <c>Tutan.Functional</c> never meets a UniTask type when it calls <c>Map</c>, <c>Bind</c> or
    /// <c>Then</c>, and need not reference UniTask.
    /// </summary>
    public static class OptionalAsyncExtensions
    {
        // ── MapAsync ─────────────────────────────────────────────────────────

        /// <summary>Async map on a sync optional: awaits <paramref name="func"/> when <c>Some</c>; propagates <c>None</c>.</summary>
        public static async UniTask<Optional<R>> MapAsync<T, R>(this Optional<T> opt, Func<T, UniTask<R>> func)
        {
            if (!opt.HasValue(out var value)) return default;
            return Some(await func(value));
        }

        /// <summary>Sync map on an async optional: awaits the task, then applies <paramref name="func"/> when <c>Some</c>.</summary>
        public static async UniTask<Optional<R>> Map<T, R>(this UniTask<Optional<T>> optTask, Func<T, R> func)
        {
            var opt = await optTask;
            if (!opt.HasValue(out var value)) return default;
            return Some(func(value));
        }

        /// <summary>Async map on an async optional: awaits the task, then awaits <paramref name="func"/> when <c>Some</c>.</summary>
        public static async UniTask<Optional<R>> MapAsync<T, R>(this UniTask<Optional<T>> optTask, Func<T, UniTask<R>> func)
        {
            var opt = await optTask;
            if (!opt.HasValue(out var value)) return default;
            return Some(await func(value));
        }


        // ── BindAsync ────────────────────────────────────────────────────────

        /// <summary>Async bind on a sync optional: awaits a function that returns an <see cref="Optional{T}"/>; propagates <c>None</c>.</summary>
        public static async UniTask<Optional<R>> BindAsync<T, R>(this Optional<T> opt, Func<T, UniTask<Optional<R>>> func)
        {
            if (!opt.HasValue(out var value)) return default;
            return await func(value);
        }

        /// <summary>Sync bind on an async optional: awaits the task, then chains to <paramref name="func"/> when <c>Some</c>.</summary>
        public static async UniTask<Optional<R>> Bind<T, R>(this UniTask<Optional<T>> optTask, Func<T, Optional<R>> func)
        {
            var opt = await optTask;
            if (!opt.HasValue(out var value)) return default;
            return func(value);
        }

        /// <summary>Async bind on an async optional: awaits the task, then awaits <paramref name="func"/> when <c>Some</c>.</summary>
        public static async UniTask<Optional<R>> BindAsync<T, R>(this UniTask<Optional<T>> optTask, Func<T, UniTask<Optional<R>>> func)
        {
            var opt = await optTask;
            if (!opt.HasValue(out var value)) return default;
            return await func(value);
        }


        // ── ThenAsync (on Optional<T>) ───────────────────────────────────────

        /// <summary>Async map: unified <c>Then</c> alias for <see cref="MapAsync{T,R}(Optional{T}, Func{T, UniTask{R}})"/>.</summary>
        public static UniTask<Optional<R>> ThenAsync<T, R>(this Optional<T> opt, Func<T, UniTask<R>> func)
            => opt.MapAsync(func);

        /// <summary>Async bind: unified <c>Then</c> alias for <see cref="BindAsync{T,R}(Optional{T}, Func{T, UniTask{Optional{R}}})"/>.</summary>
        public static UniTask<Optional<R>> ThenAsync<T, R>(this Optional<T> opt, Func<T, UniTask<Optional<R>>> func)
            => opt.BindAsync(func);

        /// <summary>Async side-effect pass-through: awaits <paramref name="action"/> when <c>Some</c>, then returns the optional unchanged.</summary>
        public static async UniTask<Optional<T>> ThenAsync<T>(this Optional<T> opt, Func<T, UniTask> action)
        {
            if (opt.HasValue(out var value)) await action(value);
            return opt;
        }


        // ── Then (on UniTask<Optional<T>>) ───────────────────────────────────

        /// <summary>Sync map on an async optional: chains a synchronous step into an async pipeline.</summary>
        public static UniTask<Optional<R>> Then<T, R>(this UniTask<Optional<T>> optTask, Func<T, R> func)
            => optTask.Map(func);

        /// <summary>Sync bind on an async optional: chains a synchronous optional-returning step into an async pipeline.</summary>
        public static UniTask<Optional<R>> Then<T, R>(this UniTask<Optional<T>> optTask, Func<T, Optional<R>> func)
            => optTask.Bind(func);

        /// <summary>Sync side-effect on an async optional: awaits the task, runs <paramref name="action"/> when <c>Some</c>, and passes the optional through.</summary>
        public static async UniTask<Optional<T>> Then<T>(this UniTask<Optional<T>> optTask, Action<T> action)
        {
            var opt = await optTask;
            if (opt.HasValue(out var value)) action(value);
            return opt;
        }

        /// <summary>Async map on an async optional: unified <c>Then</c> alias for <see cref="MapAsync{T,R}(UniTask{Optional{T}}, Func{T, UniTask{R}})"/>.</summary>
        public static UniTask<Optional<R>> ThenAsync<T, R>(this UniTask<Optional<T>> optTask, Func<T, UniTask<R>> func)
            => optTask.MapAsync(func);

        /// <summary>Async bind on an async optional: unified <c>Then</c> alias for <see cref="BindAsync{T,R}(UniTask{Optional{T}}, Func{T, UniTask{Optional{R}}})"/>.</summary>
        public static UniTask<Optional<R>> ThenAsync<T, R>(this UniTask<Optional<T>> optTask, Func<T, UniTask<Optional<R>>> func)
            => optTask.BindAsync(func);

        /// <summary>Async side-effect on an async optional: awaits the task and the action, then passes the optional through.</summary>
        public static async UniTask<Optional<T>> ThenAsync<T>(this UniTask<Optional<T>> optTask, Func<T, UniTask> action)
        {
            var opt = await optTask;
            if (opt.HasValue(out var value)) await action(value);
            return opt;
        }


        // ── MatchAsync ───────────────────────────────────────────────────────

        /// <summary>Async pattern match on a sync optional: awaits the branch selected by <c>Some</c>/<c>None</c>.</summary>
        public static async UniTask<R> MatchAsync<T, R>(this Optional<T> opt, Func<UniTask<R>> onNone, Func<T, UniTask<R>> onSome)
            => opt.HasValue(out var value) ? await onSome(value) : await onNone();

        /// <summary>Sync pattern match on an async optional: awaits the task, then calls the matching branch.</summary>
        public static async UniTask<R> Match<T, R>(this UniTask<Optional<T>> optTask, Func<R> onNone, Func<T, R> onSome)
        {
            var opt = await optTask;
            return opt.HasValue(out var value) ? onSome(value) : onNone();
        }

        /// <summary>Async pattern match on an async optional: awaits the task, then awaits the matching branch.</summary>
        public static async UniTask<R> MatchAsync<T, R>(this UniTask<Optional<T>> optTask, Func<UniTask<R>> onNone, Func<T, UniTask<R>> onSome)
        {
            var opt = await optTask;
            return opt.HasValue(out var value) ? await onSome(value) : await onNone();
        }

        /// <summary>Void sync pattern match on an async optional: awaits the task, then runs the matching action.</summary>
        public static async UniTask Match<T>(this UniTask<Optional<T>> optTask, Action onNone, Action<T> onSome)
        {
            var opt = await optTask;
            if (opt.HasValue(out var value)) onSome(value);
            else onNone();
        }
    }
}
#endif
