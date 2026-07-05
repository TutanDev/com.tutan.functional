using System;
using Cysharp.Threading.Tasks;

namespace Tutan.Functional
{
    public static partial class OptionalExtensions
    {
        // ── MapAsync ─────────────────────────────────────────────────────────

        /// <summary>Async map on a sync optional: awaits <paramref name="func"/> when <c>Some</c>; propagates <c>None</c>.</summary>
        public static async UniTask<Optional<R>> MapAsync<T, R>(this Optional<T> opt, Func<T, UniTask<R>> func)
        {
            if (opt.IsNone) return default;
            return Some(await func(opt._value));
        }

        /// <summary>Sync map on an async optional: awaits the task, then applies <paramref name="func"/> when <c>Some</c>.</summary>
        public static async UniTask<Optional<R>> Map<T, R>(this UniTask<Optional<T>> optTask, Func<T, R> func)
        {
            var opt = await optTask;
            if (opt.IsNone) return default;
            return Some(func(opt._value));
        }

        /// <summary>Async map on an async optional: awaits the task, then awaits <paramref name="func"/> when <c>Some</c>.</summary>
        public static async UniTask<Optional<R>> MapAsync<T, R>(this UniTask<Optional<T>> optTask, Func<T, UniTask<R>> func)
        {
            var opt = await optTask;
            if (opt.IsNone) return default;
            return Some(await func(opt._value));
        }


        // ── BindAsync ────────────────────────────────────────────────────────

        /// <summary>Async bind on a sync optional: awaits a function that returns an <see cref="Optional{T}"/>; propagates <c>None</c>.</summary>
        public static async UniTask<Optional<R>> BindAsync<T, R>(this Optional<T> opt, Func<T, UniTask<Optional<R>>> func)
        {
            if (opt.IsNone) return default;
            return await func(opt._value);
        }

        /// <summary>Sync bind on an async optional: awaits the task, then chains to <paramref name="func"/> when <c>Some</c>.</summary>
        public static async UniTask<Optional<R>> Bind<T, R>(this UniTask<Optional<T>> optTask, Func<T, Optional<R>> func)
        {
            var opt = await optTask;
            if (opt.IsNone) return default;
            return func(opt._value);
        }

        /// <summary>Async bind on an async optional: awaits the task, then awaits <paramref name="func"/> when <c>Some</c>.</summary>
        public static async UniTask<Optional<R>> BindAsync<T, R>(this UniTask<Optional<T>> optTask, Func<T, UniTask<Optional<R>>> func)
        {
            var opt = await optTask;
            if (opt.IsNone) return default;
            return await func(opt._value);
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
            if (opt.IsSome) await action(opt._value);
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
            if (opt.IsSome) action(opt._value);
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
            if (opt.IsSome) await action(opt._value);
            return opt;
        }


        // ── MatchAsync ───────────────────────────────────────────────────────

        /// <summary>Async pattern match on a sync optional: awaits the branch selected by <c>Some</c>/<c>None</c>.</summary>
        public static async UniTask<R> MatchAsync<T, R>(this Optional<T> opt, Func<UniTask<R>> onNone, Func<T, UniTask<R>> onSome)
            => opt.IsSome ? await onSome(opt._value) : await onNone();

        /// <summary>Sync pattern match on an async optional: awaits the task, then calls the matching branch.</summary>
        public static async UniTask<R> Match<T, R>(this UniTask<Optional<T>> optTask, Func<R> onNone, Func<T, R> onSome)
        {
            var opt = await optTask;
            return opt.IsSome ? onSome(opt._value) : onNone();
        }

        /// <summary>Async pattern match on an async optional: awaits the task, then awaits the matching branch.</summary>
        public static async UniTask<R> MatchAsync<T, R>(this UniTask<Optional<T>> optTask, Func<UniTask<R>> onNone, Func<T, UniTask<R>> onSome)
        {
            var opt = await optTask;
            return opt.IsSome ? await onSome(opt._value) : await onNone();
        }

        /// <summary>Void sync pattern match on an async optional: awaits the task, then runs the matching action.</summary>
        public static async UniTask Match<T>(this UniTask<Optional<T>> optTask, Action onNone, Action<T> onSome)
        {
            var opt = await optTask;
            if (opt.IsSome) onSome(opt._value);
            else onNone();
        }
    }
}
