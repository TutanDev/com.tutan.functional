using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Tutan.Functional
{
    public static partial class ResultExtensions
    {
        // ── Conversions ─────────────────────────────────────────

        /// <summary>Converts to a sequence of zero (error) or one (success) elements; always safe to iterate.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> AsEnumerable<T>(this Result<T> opt)
        {
            if (opt.IsSuccess) yield return opt._value!;
        }

        /// <summary>Converts to an <see cref="Optional{T}"/>, discarding the error: success becomes <c>Some</c>, error becomes <c>None</c>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<T> ToOptional<T>(this Result<T> result)
            => result.Match(
                (e) => default,
                (t) => Some(t));


        // ── Monad ───────────────────────────────────────────────

        /// <summary>Map: applies <paramref name="f"/> to the value on success; propagates the error.</summary>
        public static Result<R> Map<T, R>(this Result<T> result, Func<T, R> f)
            => result.Match(
                onSuccess: s => Success(f(s)),
                onError: e => e);

        /// <summary>Maps a two-argument function, currying it so the result is a lifted function awaiting the second argument.</summary>
        public static Result<Func<T2, R>> Map<T1, T2, R>(this Result<T1> @this, Func<T1, T2, R> func)
            => @this.Map(func.Curry());

        /// <summary>Maps a three-argument function, fixing the first argument; the rest stay as a tuple.</summary>
        public static Result<Func<T2, T3, R>> Map<T1, T2, T3, R>(this Result<T1> @this, Func<T1, T2, T3, R> func)
            => @this.Map(func.CurryFirst());

        /// <summary>Side-effect on success, collapsing to <c>Result&lt;Unit&gt;</c>: runs <paramref name="action"/> with the value and keeps only the outcome.</summary>
        public static Result<Unit> ForEach<T>(this Result<T> result, Action<T> action)
            => Map(result, action.ToFunc());

        /// <summary>Bind (flat-map): chains to a function that itself returns a <see cref="Result{T}"/>; propagates the error.</summary>
        public static Result<R> Bind<T, R>(this Result<T> result, Func<T, Result<R>> f)
            => result.Match(
                e => e,
                s => f(s));

        /// <summary>Flat-maps into a sequence: empty on error, otherwise the elements produced from the value.</summary>
        public static IEnumerable<R> Bind<T, R>(this Result<T> @this, Func<T, IEnumerable<R>> func)
            => @this.AsEnumerable().Bind(func);

        /// <summary>
        /// State-passing map: <paramref name="state"/> is handed to <paramref name="f"/> instead of being
        /// captured in a closure, so the call is allocation-free when the delegate is capture-free.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<R> Map<T, TState, R>(this Result<T> result, TState state, Func<T, TState, R> f)
            => result.IsSuccess ? Success(f(result._value, state)) : result._error;

        /// <summary>State-passing bind: <paramref name="state"/> is handed to <paramref name="f"/> instead of being captured in a closure.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<R> Bind<T, TState, R>(this Result<T> result, TState state, Func<T, TState, Result<R>> f)
            => result.IsSuccess ? f(result._value, state) : result._error;


        // ── Linq ────────────────────────────────────────────────

        /// <summary>LINQ support: enables <c>from x in result select ...</c> query syntax (alias for <c>Map</c>).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<R> Select<T, R>(this Result<T> result, Func<T, R> f)
            => result.Map(f);

        /// <summary>LINQ support: enables multiple <c>from</c> clauses.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<RR> SelectMany<T, R, RR>(this Result<T> result, Func<T, Result<R>> bind, Func<T, R, RR> project)
           => result.Match(
               (e) => e,
               (t) => bind(t).Match(
                   (e) => e,
                   (r) => Success(project(t, r))));


        // ── Applicative ─────────────────────────────────────────

        /// <summary>Applies a lifted function to a lifted value: both must be successes, otherwise the first error wins.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<R> Apply<T, R>(this Result<Func<T, R>> @this, Result<T> arg)
            => @this.Match(
                (errF) => errF,
                (f) => arg.Match(
                    onSuccess: (t) => Success(f(t)),
                    onError: (err) => err));

        /// <summary>Partially applies a lifted 2-argument function to its first argument.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Func<T2, R>> Apply<T1, T2, R>
         (this Result<Func<T1, T2, R>> @this, Result<T1> arg)
            => Apply(@this.Map(F.Curry), arg);

        /// <summary>Partially applies a lifted 3-argument function to its first argument.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Func<T2, T3, R>> Apply<T1, T2, T3, R>
           (this Result<Func<T1, T2, T3, R>> @this, Result<T1> arg)
           => Apply(@this.Map(F.CurryFirst), arg);

        /// <summary>Partially applies a lifted 4-argument function to its first argument.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Func<T2, T3, T4, R>> Apply<T1, T2, T3, T4, R>
           (this Result<Func<T1, T2, T3, T4, R>> @this, Result<T1> arg)
           => Apply(@this.Map(F.CurryFirst), arg);

        /// <summary>Partially applies a lifted 5-argument function to its first argument.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Func<T2, T3, T4, T5, R>> Apply<T1, T2, T3, T4, T5, R>
           (this Result<Func<T1, T2, T3, T4, T5, R>> @this, Result<T1> arg)
           => Apply(@this.Map(F.CurryFirst), arg);

        /// <summary>Partially applies a lifted 6-argument function to its first argument.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Func<T2, T3, T4, T5, T6, R>> Apply<T1, T2, T3, T4, T5, T6, R>
           (this Result<Func<T1, T2, T3, T4, T5, T6, R>> @this, Result<T1> arg)
           => Apply(@this.Map(F.CurryFirst), arg);

        /// <summary>Partially applies a lifted 7-argument function to its first argument.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Func<T2, T3, T4, T5, T6, T7, R>> Apply<T1, T2, T3, T4, T5, T6, T7, R>
           (this Result<Func<T1, T2, T3, T4, T5, T6, T7, R>> @this, Result<T1> arg)
           => Apply(@this.Map(F.CurryFirst), arg);

        /// <summary>Partially applies a lifted 8-argument function to its first argument.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Func<T2, T3, T4, T5, T6, T7, T8, R>> Apply<T1, T2, T3, T4, T5, T6, T7, T8, R>
           (this Result<Func<T1, T2, T3, T4, T5, T6, T7, T8, R>> @this, Result<T1> arg)
           => Apply(@this.Map(F.CurryFirst), arg);

        /// <summary>Partially applies a lifted 9-argument function to its first argument.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Func<T2, T3, T4, T5, T6, T7, T8, T9, R>> Apply<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>
           (this Result<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>> @this, Result<T1> arg)
           => Apply(@this.Map(F.CurryFirst), arg);
    }
}
