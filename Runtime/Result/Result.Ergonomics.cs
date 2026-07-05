using System;
using System.Runtime.CompilerServices;

namespace Tutan.Functional
{
    public static partial class F
    {
        /// <summary>Void-result failure from an <see cref="Error"/>: the partner of <see cref="Success()"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Unit> Fail(Error error) => error;

        /// <summary>Void-result failure from a message string: the partner of <see cref="Success()"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Unit> Fail(string message) => new Error(message);

        /// <summary>Typed failure from an <see cref="Error"/>: explicit alternative to the implicit <c>Error → Result&lt;T&gt;</c> conversion.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> Fail<T>(Error error) => error;

        /// <summary>Typed failure from a message string: explicit alternative to the implicit conversion chain.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> Fail<T>(string message) => new Error(message);
    }

    public static partial class ResultExtensions
    {
        // ── IfFail (runs only on the error branch) ──────────────

        /// <summary>Result-space fallback: recovers an error into another <see cref="Result{T}"/>; passes success through untouched.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> IfFail<T>(this Result<T> result, Func<Error, Result<T>> fallback)
            => result.IsSuccess ? result : fallback(result._error);

        /// <summary>Side-effecting fallback: observes the error with <paramref name="onError"/>, then passes the result through unchanged.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> IfFail<T>(this Result<T> result, Action<Error> onError)
        {
            if (!result.IsSuccess) onError(result._error);
            return result;
        }

        // ── Result<Unit> Match without the throwaway Unit arg ───
        // Disambiguated from the instance Match by the success delegate's arity.

        /// <summary>Pattern match on <c>Result&lt;Unit&gt;</c> with a parameterless success branch: write <c>() => …</c> instead of <c>_ => …</c>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static R Match<R>(this Result<Unit> result, Func<Error, R> onError, Func<R> onSuccess)
            => result.IsSuccess ? onSuccess() : onError(result._error);

        /// <summary>Void pattern match on <c>Result&lt;Unit&gt;</c> with a parameterless success branch.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Match(this Result<Unit> result, Action<Error> onError, Action onSuccess)
        {
            if (result.IsSuccess) onSuccess();
            else onError(result._error);
            return default;
        }
    }
}
