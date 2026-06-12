using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Tutan.Functional
{
    public static partial class ResultExtensions
    {
        // ── Conversions ─────────────────────────────────────────

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> AsEnumerable<T>(this Result<T> opt)
        {
            if (opt.IsSuccess) yield return opt._value!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<T> ToOptional<T>(this Result<T> result)
            => result.Match(
                (e) => default,
                (t) => Some(t));


        // ── Monad ───────────────────────────────────────────────

        // map
        public static Result<R> Map<T, R>(this Result<T> result, Func<T, R> f)
            => result.Match(
                onSuccess: s => Success(f(s)),
                onError: e => e);

        public static Result<Func<T2, R>> Map<T1, T2, R>(this Result<T1> @this, Func<T1, T2, R> func)
            => @this.Map(func.Curry());

        public static Result<Func<T2, T3, R>> Map<T1, T2, T3, R>(this Result<T1> @this, Func<T1, T2, T3, R> func)
            => @this.Map(func.CurryFirst());

        // foreach
        public static Result<Unit> ForEach<T>(this Result<T> result, Action<T> action)
            => Map(result, action.ToFunc());

        // bind
        public static Result<R> Bind<T, R>(this Result<T> result, Func<T, Result<R>> f)
            => result.Match(
                e => e,
                s => f(s));

        public static IEnumerable<R> Bind<T, R>(this Result<T> @this, Func<T, IEnumerable<R>> func)
            => @this.AsEnumerable().Bind(func);

        // state-passing (avoids closure capture; allocation-free when `f` is capture-free)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<R> Map<T, TState, R>(this Result<T> result, TState state, Func<T, TState, R> f)
            => result.IsSuccess ? Success(f(result._value, state)) : result._error;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<R> Bind<T, TState, R>(this Result<T> result, TState state, Func<T, TState, Result<R>> f)
            => result.IsSuccess ? f(result._value, state) : result._error;


        // ── Linq ────────────────────────────────────────────────

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<R> Select<T, R>(this Result<T> result, Func<T, R> f)
            => result.Map(f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<RR> SelectMany<T, R, RR>(this Result<T> result, Func<T, Result<R>> bind, Func<T, R, RR> project)
           => result.Match(
               (e) => e,
               (t) => bind(t).Match(
                   (e) => e,
                   (r) => Success(project(t, r))));


        // ── Applicative ─────────────────────────────────────────

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<R> Apply<T, R>(this Result<Func<T, R>> @this, Result<T> arg)
            => @this.Match(
                (errF) => errF,
                (f) => arg.Match(
                    onSuccess: (t) => Success(f(t)),
                    onError: (err) => err));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Func<T2, R>> Apply<T1, T2, R>
         (this Result<Func<T1, T2, R>> @this, Result<T1> arg)
            => Apply(@this.Map(F.Curry), arg);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Func<T2, T3, R>> Apply<T1, T2, T3, R>
           (this Result<Func<T1, T2, T3, R>> @this, Result<T1> arg)
           => Apply(@this.Map(F.CurryFirst), arg);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Func<T2, T3, T4, R>> Apply<T1, T2, T3, T4, R>
           (this Result<Func<T1, T2, T3, T4, R>> @this, Result<T1> arg)
           => Apply(@this.Map(F.CurryFirst), arg);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Func<T2, T3, T4, T5, R>> Apply<T1, T2, T3, T4, T5, R>
           (this Result<Func<T1, T2, T3, T4, T5, R>> @this, Result<T1> arg)
           => Apply(@this.Map(F.CurryFirst), arg);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Func<T2, T3, T4, T5, T6, R>> Apply<T1, T2, T3, T4, T5, T6, R>
           (this Result<Func<T1, T2, T3, T4, T5, T6, R>> @this, Result<T1> arg)
           => Apply(@this.Map(F.CurryFirst), arg);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Func<T2, T3, T4, T5, T6, T7, R>> Apply<T1, T2, T3, T4, T5, T6, T7, R>
           (this Result<Func<T1, T2, T3, T4, T5, T6, T7, R>> @this, Result<T1> arg)
           => Apply(@this.Map(F.CurryFirst), arg);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Func<T2, T3, T4, T5, T6, T7, T8, R>> Apply<T1, T2, T3, T4, T5, T6, T7, T8, R>
           (this Result<Func<T1, T2, T3, T4, T5, T6, T7, T8, R>> @this, Result<T1> arg)
           => Apply(@this.Map(F.CurryFirst), arg);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Func<T2, T3, T4, T5, T6, T7, T8, T9, R>> Apply<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>
           (this Result<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>> @this, Result<T1> arg)
           => Apply(@this.Map(F.CurryFirst), arg);
    }
}
