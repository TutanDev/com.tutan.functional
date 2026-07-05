using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Tutan.Functional
{
    public static partial class OptionalExtensions
    {
        // ── Conversions ─────────────────────────────────────────

        /// <summary>Converts to a sequence of zero (<c>None</c>) or one (<c>Some</c>) elements; always safe to iterate.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> AsEnumerable<T>(this Optional<T> opt)
        {
            if (opt.IsSome) yield return opt._value!;
        }

        /// <summary>Converts to a <see cref="Result{T}"/>: <c>Some</c> becomes <c>Success</c>, <c>None</c> becomes the error produced by <paramref name="onNone"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> ToResult<T>(this Optional<T> opt, Func<Error> onNone)
            => opt.Match(
                () => onNone(),
                (t) => Success(t));


        // ── Monad ───────────────────────────────────────────────

        /// <summary>Mapping over <see cref="F.None"/> is always <c>None</c>; lets pipelines start from the sentinel.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<R> Map<T, R>(this NoneType _, Func<T, R> f) => None;

        /// <summary>Map: applies <paramref name="f"/> to the value when <c>Some</c>; propagates <c>None</c>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<R> Map<T, R>(this Optional<T> optT, Func<T, R> f)
            => optT.Match(() => default, (t) => Some(f(t)));

        /// <summary>Maps a two-argument function, currying it so the result is an optional function awaiting the second argument.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<Func<T2, R>> Map<T1, T2, R>(this Optional<T1> opt, Func<T1, T2, R> func)
            => opt.Map(func.Curry());

        /// <summary>Maps a three-argument function, fixing the first argument; the rest stay as a tuple.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<Func<T2, T3, R>> Map<T1, T2, T3, R>(this Optional<T1> opt, Func<T1, T2, T3, R> func)
            => opt.Map(func.CurryFirst());

        /// <summary>Bind (flat-map): chains to a function that itself returns an <see cref="Optional{T}"/>; propagates <c>None</c>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<R> Bind<T, R>(this Optional<T> opt, Func<T, Optional<R>> f)
            => opt.Match(
                () => default,
                (t) => f(t));

        /// <summary>Flat-maps into a sequence: empty when <c>None</c>, otherwise the elements produced from the value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<R> Bind<T, R>(this Optional<T> opt, Func<T, IEnumerable<R>> func)
            => opt.AsEnumerable().Bind(func);

        /// <summary>
        /// State-passing map: <paramref name="state"/> is handed to <paramref name="f"/> instead of being
        /// captured in a closure, so the call is allocation-free when the delegate is capture-free.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<R> Map<T, TState, R>(this Optional<T> opt, TState state, Func<T, TState, R> f)
            => opt.IsSome ? Some(f(opt._value, state)) : default;

        /// <summary>State-passing bind: <paramref name="state"/> is handed to <paramref name="f"/> instead of being captured in a closure.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<R> Bind<T, TState, R>(this Optional<T> opt, TState state, Func<T, TState, Optional<R>> f)
            => opt.IsSome ? f(opt._value, state) : default;


        // ── Linq ────────────────────────────────────────────────

        /// <summary>LINQ support: enables <c>from x in opt select ...</c> query syntax (alias for <c>Map</c>).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<R> Select<T, R>(this Optional<T> opt, Func<T, R> func)
            => opt.Map(func);

        /// <summary>LINQ support: enables <c>where</c> clauses (alias for <c>Filter</c>).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<T> Where<T>(this Optional<T> optT, Func<T, bool> predicate)
           => optT.Match(
               () => default,
               (t) => predicate(t) ? optT : default);

        /// <summary>LINQ support: enables multiple <c>from</c> clauses.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<RR> SelectMany<T, R, RR>(this Optional<T> opt, Func<T, Optional<R>> bind, Func<T, R, RR> project)
           => opt.Match(
               () => default,
               (t) => bind(t).Match(
                   () => default,
                   (r) => Some(project(t, r))));


        // ── Applicative ─────────────────────────────────────────

        /// <summary>Applies a lifted function to a lifted value: <c>Some(f)</c> applied to <c>Some(x)</c> is <c>Some(f(x))</c>; anything else is <c>None</c>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<R> Apply<T, R>(this Optional<Func<T, R>> opt, Optional<T> arg)
            => opt.Match(
            () => default,
            (func) => arg.Match(
                () => default,
                (val) => Some(func(val))));

        /// <summary>Partially applies a lifted 2-argument function to its first argument.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<Func<T2, R>> Apply<T1, T2, R>
         (this Optional<Func<T1, T2, R>> opt, Optional<T1> arg)
            => Apply(opt.Map(F.Curry), arg);

        /// <summary>Partially applies a lifted 3-argument function to its first argument.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<Func<T2, T3, R>> Apply<T1, T2, T3, R>
           (this Optional<Func<T1, T2, T3, R>> opt, Optional<T1> arg)
           => Apply(opt.Map(F.CurryFirst), arg);

        /// <summary>Partially applies a lifted 4-argument function to its first argument.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<Func<T2, T3, T4, R>> Apply<T1, T2, T3, T4, R>
           (this Optional<Func<T1, T2, T3, T4, R>> opt, Optional<T1> arg)
           => Apply(opt.Map(F.CurryFirst), arg);

        /// <summary>Partially applies a lifted 5-argument function to its first argument.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<Func<T2, T3, T4, T5, R>> Apply<T1, T2, T3, T4, T5, R>
           (this Optional<Func<T1, T2, T3, T4, T5, R>> opt, Optional<T1> arg)
           => Apply(opt.Map(F.CurryFirst), arg);

        /// <summary>Partially applies a lifted 6-argument function to its first argument.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<Func<T2, T3, T4, T5, T6, R>> Apply<T1, T2, T3, T4, T5, T6, R>
           (this Optional<Func<T1, T2, T3, T4, T5, T6, R>> opt, Optional<T1> arg)
           => Apply(opt.Map(F.CurryFirst), arg);

        /// <summary>Partially applies a lifted 7-argument function to its first argument.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<Func<T2, T3, T4, T5, T6, T7, R>> Apply<T1, T2, T3, T4, T5, T6, T7, R>
           (this Optional<Func<T1, T2, T3, T4, T5, T6, T7, R>> opt, Optional<T1> arg)
           => Apply(opt.Map(F.CurryFirst), arg);

        /// <summary>Partially applies a lifted 8-argument function to its first argument.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<Func<T2, T3, T4, T5, T6, T7, T8, R>> Apply<T1, T2, T3, T4, T5, T6, T7, T8, R>
           (this Optional<Func<T1, T2, T3, T4, T5, T6, T7, T8, R>> opt, Optional<T1> arg)
           => Apply(opt.Map(F.CurryFirst), arg);

        /// <summary>Partially applies a lifted 9-argument function to its first argument.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<Func<T2, T3, T4, T5, T6, T7, T8, T9, R>> Apply<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>
           (this Optional<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>> opt, Optional<T1> arg)
           => Apply(opt.Map(F.CurryFirst), arg);
    }
}
