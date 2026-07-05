using System;
using Cysharp.Threading.Tasks;

namespace Tutan.Functional
{
    public static partial class ResultExtensions
    {
        // ── MapAsync ─────────────────────────────────────────────────────────

        /// <summary>Async map on a sync result: awaits <paramref name="func"/> on success; propagates the error.</summary>
        public static async UniTask<Result<R>> MapAsync<T, R>(this Result<T> result, Func<T, UniTask<R>> func)
        {
            if (result.IsError) return new Result<R>(result._error);
            return Success(await func(result._value));
        }

        /// <summary>Sync map on an async result: awaits the task, then applies <paramref name="func"/> on success.</summary>
        public static async UniTask<Result<R>> Map<T, R>(this UniTask<Result<T>> resultTask, Func<T, R> func)
        {
            var result = await resultTask;
            if (result.IsError) return new Result<R>(result._error);
            return Success(func(result._value));
        }

        /// <summary>Async map on an async result: awaits the task, then awaits <paramref name="func"/> on success.</summary>
        public static async UniTask<Result<R>> MapAsync<T, R>(this UniTask<Result<T>> resultTask, Func<T, UniTask<R>> func)
        {
            var result = await resultTask;
            if (result.IsError) return new Result<R>(result._error);
            return Success(await func(result._value));
        }


        // ── BindAsync ────────────────────────────────────────────────────────

        /// <summary>Async bind on a sync result: awaits a function that returns a <see cref="Result{T}"/>; propagates the error.</summary>
        public static async UniTask<Result<R>> BindAsync<T, R>(this Result<T> result, Func<T, UniTask<Result<R>>> func)
        {
            if (result.IsError) return new Result<R>(result._error);
            return await func(result._value);
        }

        /// <summary>Sync bind on an async result: awaits the task, then chains to <paramref name="func"/> on success.</summary>
        public static async UniTask<Result<R>> Bind<T, R>(this UniTask<Result<T>> resultTask, Func<T, Result<R>> func)
        {
            var result = await resultTask;
            if (result.IsError) return new Result<R>(result._error);
            return func(result._value);
        }

        /// <summary>Async bind on an async result: awaits the task, then awaits <paramref name="func"/> on success.</summary>
        public static async UniTask<Result<R>> BindAsync<T, R>(this UniTask<Result<T>> resultTask, Func<T, UniTask<Result<R>>> func)
        {
            var result = await resultTask;
            if (result.IsError) return new Result<R>(result._error);
            return await func(result._value);
        }


        // ── ThenAsync (on Result<T>) ─────────────────────────────────────────

        /// <summary>Async map: unified <c>Then</c> alias for <see cref="MapAsync{T,R}(Result{T}, Func{T, UniTask{R}})"/>.</summary>
        public static UniTask<Result<R>> ThenAsync<T, R>(this Result<T> result, Func<T, UniTask<R>> func)
            => result.MapAsync(func);

        /// <summary>Async bind: unified <c>Then</c> alias for <see cref="BindAsync{T,R}(Result{T}, Func{T, UniTask{Result{R}}})"/>.</summary>
        public static UniTask<Result<R>> ThenAsync<T, R>(this Result<T> result, Func<T, UniTask<Result<R>>> func)
            => result.BindAsync(func);

        /// <summary>Async side-effect pass-through: awaits <paramref name="action"/> on success, then returns the result unchanged.</summary>
        public static async UniTask<Result<T>> ThenAsync<T>(this Result<T> result, Func<T, UniTask> action)
        {
            if (result.IsSuccess) await action(result._value);
            return result;
        }


        // ── Then (on UniTask<Result<T>>) ─────────────────────────────────────

        /// <summary>Sync map on an async result: chains a synchronous step into an async pipeline.</summary>
        public static UniTask<Result<R>> Then<T, R>(this UniTask<Result<T>> resultTask, Func<T, R> func)
            => resultTask.Map(func);

        /// <summary>Sync bind on an async result: chains a synchronous result-returning step into an async pipeline.</summary>
        public static UniTask<Result<R>> Then<T, R>(this UniTask<Result<T>> resultTask, Func<T, Result<R>> func)
            => resultTask.Bind(func);

        /// <summary>Sync side-effect on an async result: awaits the task, runs <paramref name="action"/> on success, and passes the result through.</summary>
        public static async UniTask<Result<T>> Then<T>(this UniTask<Result<T>> resultTask, Action<T> action)
        {
            var result = await resultTask;
            if (result.IsSuccess) action(result._value);
            return result;
        }

        /// <summary>Async map on an async result: unified <c>Then</c> alias for <see cref="MapAsync{T,R}(UniTask{Result{T}}, Func{T, UniTask{R}})"/>.</summary>
        public static UniTask<Result<R>> ThenAsync<T, R>(this UniTask<Result<T>> resultTask, Func<T, UniTask<R>> func)
            => resultTask.MapAsync(func);

        /// <summary>Async bind on an async result: unified <c>Then</c> alias for <see cref="BindAsync{T,R}(UniTask{Result{T}}, Func{T, UniTask{Result{R}}})"/>.</summary>
        public static UniTask<Result<R>> ThenAsync<T, R>(this UniTask<Result<T>> resultTask, Func<T, UniTask<Result<R>>> func)
            => resultTask.BindAsync(func);

        /// <summary>Async side-effect on an async result: awaits the task and the action, then passes the result through.</summary>
        public static async UniTask<Result<T>> ThenAsync<T>(this UniTask<Result<T>> resultTask, Func<T, UniTask> action)
        {
            var result = await resultTask;
            if (result.IsSuccess) await action(result._value);
            return result;
        }


        // ── MatchAsync ───────────────────────────────────────────────────────

        /// <summary>Async pattern match on a sync result: awaits the branch selected by success/error.</summary>
        public static async UniTask<R> MatchAsync<T, R>(this Result<T> result, Func<Error, UniTask<R>> onError, Func<T, UniTask<R>> onSuccess)
            => result.IsSuccess ? await onSuccess(result._value) : await onError(result._error);

        /// <summary>Sync pattern match on an async result: awaits the task, then calls the matching branch.</summary>
        public static async UniTask<R> Match<T, R>(this UniTask<Result<T>> resultTask, Func<Error, R> onError, Func<T, R> onSuccess)
        {
            var result = await resultTask;
            return result.IsSuccess ? onSuccess(result._value) : onError(result._error);
        }

        /// <summary>Async pattern match on an async result: awaits the task, then awaits the matching branch.</summary>
        public static async UniTask<R> MatchAsync<T, R>(this UniTask<Result<T>> resultTask, Func<Error, UniTask<R>> onError, Func<T, UniTask<R>> onSuccess)
        {
            var result = await resultTask;
            return result.IsSuccess ? await onSuccess(result._value) : await onError(result._error);
        }

        /// <summary>Void sync pattern match on an async result: awaits the task, then runs the matching action.</summary>
        public static async UniTask Match<T>(this UniTask<Result<T>> resultTask, Action<Error> onError, Action<T> onSuccess)
        {
            var result = await resultTask;
            if (result.IsSuccess) onSuccess(result._value);
            else onError(result._error);
        }
    }
}
