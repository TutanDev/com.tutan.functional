using System;
using System.Runtime.CompilerServices;

namespace Tutan.Functional
{
    public static partial class ResultExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<R> Then<T, R>(this Result<T> result, Func<T, R> func) => result.Map(func);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> Then<T>(this Result<T> result, Action<T> action) => result.Map(F.Tee(action));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<R> Then<T, R>(this Result<T> result, Func<T, Result<R>> func) => result.Bind(func);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Or<T>(this Result<T> result, T fallback)
            => result.Match(_ => fallback, t => t);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T OrElse<T>(this Result<T> result, Func<T> fallback)
            => result.Match(e => fallback(), t => t);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T OrElse<T>(this Result<T> result, Func<Error, T> fallback)
            => result.Match(e => fallback(e), t => t);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> Filter<T>(this Result<T> result, Func<T, bool> predicate)
            => result.Match(
                onError: e => new Result<T>(e),
                onSuccess: v => predicate(v) ? result : new Result<T>(Error("Predicate not satisfied")));

        // state-passing (avoids closure capture; allocation-free when the delegate is capture-free)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<R> Then<T, TState, R>(this Result<T> result, TState state, Func<T, TState, R> func) => result.Map(state, func);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> Then<T, TState>(this Result<T> result, TState state, Action<T, TState> action)
        {
            if (result.IsSuccess) action(result._value, state);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<R> Then<T, TState, R>(this Result<T> result, TState state, Func<T, TState, Result<R>> func) => result.Bind(state, func);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> Filter<T, TState>(this Result<T> result, TState state, Func<T, TState, bool> predicate)
            => result.IsSuccess && !predicate(result._value, state) ? new Result<T>(Error("Predicate not satisfied")) : result;

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

        public static T ValueUnsafe<T>(this Result<T> @this)
            => @this.Match(
                (fail) => { throw new InvalidOperationException($"ValueUnsafe<{typeof(T).FullName}> was called on an Error ({fail}). Ensure the Result is Success before using ValueUnsafe"); },
                (t) => t);

        public static Error ErrorUnsafe<T>(this Result<T> @this)
            => @this.Match(
                (fail) => fail,
                (t) => { throw new InvalidOperationException("ErrorUnsafe was called on a Success"); });
    }
}
