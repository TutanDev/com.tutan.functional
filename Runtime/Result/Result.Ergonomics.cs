using System;
using System.Runtime.CompilerServices;

namespace Tutan.Functional
{
    public static partial class F
    {
        // Void-result failure — the partner of Success(). Mirrors Success()/Success<T>(value).
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Unit> Fail(Error error) => error;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Unit> Fail(string message) => new Error(message);

        // Typed failure — when the success branch would carry a value.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> Fail<T>(Error error) => error;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> Fail<T>(string message) => new Error(message);
    }

    public static partial class ResultExtensions
    {
        // ── IfFail (runs only on the error branch) ──────────────

        // Result-space fallback: recover an Error into another Result<T>.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> IfFail<T>(this Result<T> result, Func<Error, Result<T>> fallback)
            => result.IsSuccess ? result : fallback(result._error);

        // Side-effecting fallback: observe the Error, pass the Result through unchanged.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> IfFail<T>(this Result<T> result, Action<Error> onError)
        {
            if (!result.IsSuccess) onError(result._error);
            return result;
        }

        // ── Result<Unit> Match without the throwaway Unit arg ───
        // Lets you write `() => …` / `Action` instead of `_ => …` / `Action<Unit>`.
        // Disambiguated from the instance Match by the success delegate's arity.

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static R Match<R>(this Result<Unit> result, Func<Error, R> onError, Func<R> onSuccess)
            => result.IsSuccess ? onSuccess() : onError(result._error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Match(this Result<Unit> result, Action<Error> onError, Action onSuccess)
        {
            if (result.IsSuccess) onSuccess();
            else onError(result._error);
            return default;
        }
    }
}
