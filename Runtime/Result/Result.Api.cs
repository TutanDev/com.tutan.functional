using System;
using System.Runtime.CompilerServices;

namespace Tutan.Functional
{
    public static partial class ResultExtensions
    {
        /// <summary>Map: applies <paramref name="func"/> to the value on success; propagates the error.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<R> Then<T, R>(this Result<T> result, Func<T, R> func) => result.Map(func);

        /// <summary>Side-effect pass-through: runs <paramref name="action"/> on the value on success, then returns the result unchanged.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> Then<T>(this Result<T> result, Action<T> action)
        {
            // Returns `result` itself rather than re-wrapping through Success(): no Tee closure, and an
            // object destroyed inside `action` cannot silently turn the pass-through into an error.
            if (result.IsSuccess) action(result._value);
            return result;
        }

        /// <summary>Bind (flat-map): chains to a function that itself returns a <see cref="Result{T}"/>; propagates the error.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<R> Then<T, R>(this Result<T> result, Func<T, Result<R>> func) => result.Bind(func);

        /// <summary>Returns the value on success, otherwise <paramref name="fallback"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Or<T>(this Result<T> result, T fallback)
            => result.IsSuccess ? result._value : fallback;

        /// <summary>Returns the value on success, otherwise the result of <paramref name="fallback"/> (evaluated lazily).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T OrElse<T>(this Result<T> result, Func<T> fallback)
            => result.IsSuccess ? result._value : fallback();

        /// <summary>Returns the value on success, otherwise the result of <paramref name="fallback"/> applied to the error.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T OrElse<T>(this Result<T> result, Func<Error, T> fallback)
            => result.IsSuccess ? result._value : fallback(result._error);

        /// <summary>Converts a failing predicate into <c>Error("Predicate not satisfied")</c>; passes success through when it holds.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> Filter<T>(this Result<T> result, Func<T, bool> predicate)
            => result.IsSuccess && !predicate(result._value) ? new Result<T>(Error("Predicate not satisfied")) : result;

        /// <summary>
        /// State-passing map: <paramref name="state"/> is handed to <paramref name="func"/> instead of being
        /// captured in a closure, so the call is allocation-free when the delegate is capture-free.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<R> Then<T, TState, R>(this Result<T> result, TState state, Func<T, TState, R> func) => result.Map(state, func);

        /// <summary>State-passing side-effect: runs <paramref name="action"/> with the value and <paramref name="state"/> on success, then returns the result unchanged.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> Then<T, TState>(this Result<T> result, TState state, Action<T, TState> action)
        {
            if (result.IsSuccess) action(result._value, state);
            return result;
        }

        /// <summary>State-passing bind: <paramref name="state"/> is handed to <paramref name="func"/> instead of being captured in a closure.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<R> Then<T, TState, R>(this Result<T> result, TState state, Func<T, TState, Result<R>> func) => result.Bind(state, func);

        /// <summary>State-passing filter: <paramref name="state"/> is handed to <paramref name="predicate"/>; a failing predicate becomes <c>Error("Predicate not satisfied")</c>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> Filter<T, TState>(this Result<T> result, TState state, Func<T, TState, bool> predicate)
            => result.IsSuccess && !predicate(result._value, state) ? new Result<T>(Error("Predicate not satisfied")) : result;

        /// <summary>Out-param extraction: assigns the value and returns <c>true</c> on success; returns <c>false</c> (and <c>default</c>) on error.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSuccess<T>(this Result<T> result, out T value)
        {
            if (result.IsSuccess)
            {
                value = result._value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>Returns the value, throwing <see cref="InvalidOperationException"/> on error. Use only after guaranteeing success.</summary>
        public static T ValueUnsafe<T>(this Result<T> @this)
            => @this.IsSuccess
                ? @this._value
                : throw new InvalidOperationException($"ValueUnsafe<{typeof(T).FullName}> was called on an Error ({@this._error}). Ensure the Result is Success before using ValueUnsafe");

        /// <summary>Returns the error, throwing <see cref="InvalidOperationException"/> on success. Use only after guaranteeing failure.</summary>
        public static Error ErrorUnsafe<T>(this Result<T> @this)
            => @this.IsError
                ? @this._error
                : throw new InvalidOperationException($"ErrorUnsafe<{typeof(T).FullName}> was called on a Success. Ensure the Result is an Error before using ErrorUnsafe");
    }
}
