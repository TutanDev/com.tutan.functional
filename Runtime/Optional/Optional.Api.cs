using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Tutan.Functional
{
    public static partial class OptionalExtensions
    {
        /// <summary>Out-param extraction: assigns the value and returns <c>true</c> on <c>Some</c>; returns <c>false</c> (and <c>default</c>) on <c>None</c>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasValue<T>(this Optional<T> opt, out T value)
        {
            if (opt.IsSome)
            {
                value = opt._value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>Returns the value, throwing <see cref="InvalidOperationException"/> on <c>None</c>. Use only after guaranteeing <c>Some</c>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ValueUnsafe<T>(this Optional<T> opt)
            => opt.Match(
                () => { throw new InvalidOperationException($"ValueUnsafe<{typeof(T).FullName}> was called on None. Ensure the Option is Some before using ValueUnsafe"); },
                (t) => t);

        /// <summary>Map: applies <paramref name="func"/> to the value when <c>Some</c>; propagates <c>None</c>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<R> Then<T, R>(this Optional<T> opt, Func<T, R> func) => opt.Map(func);

        /// <summary>Side-effect pass-through: runs <paramref name="action"/> on the value when <c>Some</c>, then returns the optional unchanged.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<T> Then<T>(this Optional<T> opt, Action<T> action) => opt.Map(F.Tee(action));

        /// <summary>Bind (flat-map): chains to a function that itself returns an <see cref="Optional{T}"/>; propagates <c>None</c>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<R> Then<T, R>(this Optional<T> opt, Func<T, Optional<R>> func) => opt.Bind(func);

        /// <summary>Returns the value when <c>Some</c>, otherwise <paramref name="fallback"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Or<T>(this Optional<T> opt, T fallback) => opt.Match(() => fallback, t => t);

        /// <summary>Returns the value when <c>Some</c>, otherwise the result of <paramref name="fallback"/> (evaluated lazily).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T OrElse<T>(this Optional<T> opt, Func<T> fallback) => opt.Match(() => fallback(), t => t);

        /// <summary>Returns <c>None</c> when the predicate fails; passes <c>Some</c> through when it holds.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<T> Filter<T>(this Optional<T> opt, Func<T, bool> predicate) => opt.IsSome && predicate(opt._value) ? opt : default;

        /// <summary>
        /// State-passing map: <paramref name="state"/> is handed to <paramref name="func"/> instead of being
        /// captured in a closure, so the call is allocation-free when the delegate is capture-free.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<R> Then<T, TState, R>(this Optional<T> opt, TState state, Func<T, TState, R> func) => opt.Map(state, func);

        /// <summary>State-passing side-effect: runs <paramref name="action"/> with the value and <paramref name="state"/> when <c>Some</c>, then returns the optional unchanged.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<T> Then<T, TState>(this Optional<T> opt, TState state, Action<T, TState> action)
        {
            if (opt.IsSome) action(opt._value, state);
            return opt;
        }

        /// <summary>State-passing bind: <paramref name="state"/> is handed to <paramref name="func"/> instead of being captured in a closure.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<R> Then<T, TState, R>(this Optional<T> opt, TState state, Func<T, TState, Optional<R>> func) => opt.Bind(state, func);

        /// <summary>State-passing filter: <paramref name="state"/> is handed to <paramref name="predicate"/> instead of being captured in a closure.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<T> Filter<T, TState>(this Optional<T> opt, TState state, Func<T, TState, bool> predicate)
            => opt.IsSome && predicate(opt._value, state) ? opt : default;

        /// <summary>Converts a nullable struct: <c>Some(value)</c> when it has a value, otherwise <c>None</c>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<T> ToOptional<T>(this T? nullable) where T : struct
            => nullable.HasValue ? Some(nullable.Value) : default;

        /// <summary>Distributes the optional over a sequence-producing function: <c>Some</c> yields one <c>Some</c> per element, <c>None</c> yields a single <c>None</c>.</summary>
        public static IEnumerable<Optional<R>> Traverse<T, R>(this Optional<T> @this, Func<T, IEnumerable<R>> func)
         => @this.Match(
            () => List((Optional<R>)None),
            (t) => func(t).Map(r => Some(r)));
    }
}
