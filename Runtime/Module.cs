global using static Tutan.Functional.F;
global using Unit = System.ValueTuple;

using System;
using System.Collections.Generic;
using System.Linq;


namespace Tutan.Functional
{
    /// <summary>
    /// The static module of construction and composition helpers (<c>Some</c>, <c>None</c>, <c>Success</c>,
    /// <c>Fail</c>, <c>Error</c>, <c>Try</c>, <c>Pipe</c>, <c>Tee</c>, <c>Curry</c>, validators).
    /// Bring it into scope with <c>using static Tutan.Functional.F;</c>.
    /// </summary>
    public static partial class F
    {
        /// <summary>Returns the empty value (<c>default(ValueTuple)</c>), used as the void substitute in functional signatures.</summary>
        public static Unit Unit() => default;

        /// <summary>Creates an <see cref="IEnumerable{T}"/> from inline values.</summary>
        public static IEnumerable<T> List<T>(params T[] items) => items.AsEnumerable();

        /// <summary>Lifts an <see cref="Action"/> to <c>Func&lt;Unit&gt;</c> so a side-effect can be used where a function is expected.</summary>
        public static Func<Unit> Tee(Action function) => () =>
        {
            function();
            return default;
        };

        /// <summary>Wraps a side-effect as a pass-through function: runs <paramref name="function"/> and returns its input unchanged.</summary>
        public static Func<T, T> Tee<T>(Action<T> function) => (t) =>
        {
            function(t);
            return t;
        };


        /// <summary>Forward function application: returns <c>func(@this)</c>, enabling left-to-right chaining.</summary>
        public static R Pipe<T, R>(this T @this, Func<T, R> func) => func(@this);

        /// <summary>Forward side-effect application: runs <paramref name="func"/> on the input and returns the input.</summary>
        public static T Pipe<T>(this T input, Action<T> func) => Tee(func)(input);


        /// <summary>Calls <paramref name="f"/>, catching any exception: returns <c>Success</c> or <c>Error(ex.ToString())</c>.</summary>
        public static Result<T> Try<T>(Func<T> f)
        {
            try { return Success(f()); }
            catch (Exception ex) { return new Error(ex.ToString()); }
        }

        /// <summary>Runs <paramref name="action"/>, catching any exception: returns <c>Success()</c> or <c>Error(ex.ToString())</c>.</summary>
        public static Result<Unit> Try(Action action)
        {
            try { action(); return Success(Unit()); }
            catch (Exception ex) { return new Error(ex.ToString()); }
        }


        // ── Curry ───────────────────────────────────────────────

        /// <summary>Curries a 2-argument function into a chain of single-argument functions.</summary>
        public static Func<T1, Func<T2, R>> Curry<T1, T2, R>(this Func<T1, T2, R> func)
            => t1 => t2 => func(t1, t2);

        /// <summary>Curries a 3-argument function into a chain of single-argument functions.</summary>
        public static Func<T1, Func<T2, Func<T3, R>>> Curry<T1, T2, T3, R>(this Func<T1, T2, T3, R> func)
            => t1 => t2 => t3 => func(t1, t2, t3);

        /// <summary>Fixes the first argument of a 3-argument function; the remainder stay as a tuple.</summary>
        public static Func<T1, Func<T2, T3, R>> CurryFirst<T1, T2, T3, R>
            (this Func<T1, T2, T3, R> @this) => t1 => (t2, t3) => @this(t1, t2, t3);

        /// <summary>Fixes the first argument of a 4-argument function; the remainder stay as a tuple.</summary>
        public static Func<T1, Func<T2, T3, T4, R>> CurryFirst<T1, T2, T3, T4, R>
           (this Func<T1, T2, T3, T4, R> @this) => t1 => (t2, t3, t4) => @this(t1, t2, t3, t4);

        /// <summary>Fixes the first argument of a 5-argument function; the remainder stay as a tuple.</summary>
        public static Func<T1, Func<T2, T3, T4, T5, R>> CurryFirst<T1, T2, T3, T4, T5, R>
           (this Func<T1, T2, T3, T4, T5, R> @this) => t1 => (t2, t3, t4, t5) => @this(t1, t2, t3, t4, t5);

        /// <summary>Fixes the first argument of a 6-argument function; the remainder stay as a tuple.</summary>
        public static Func<T1, Func<T2, T3, T4, T5, T6, R>> CurryFirst<T1, T2, T3, T4, T5, T6, R>
           (this Func<T1, T2, T3, T4, T5, T6, R> @this) => t1 => (t2, t3, t4, t5, t6) => @this(t1, t2, t3, t4, t5, t6);

        /// <summary>Fixes the first argument of a 7-argument function; the remainder stay as a tuple.</summary>
        public static Func<T1, Func<T2, T3, T4, T5, T6, T7, R>> CurryFirst<T1, T2, T3, T4, T5, T6, T7, R>
           (this Func<T1, T2, T3, T4, T5, T6, T7, R> @this) => t1 => (t2, t3, t4, t5, t6, t7) => @this(t1, t2, t3, t4, t5, t6, t7);

        /// <summary>Fixes the first argument of an 8-argument function; the remainder stay as a tuple.</summary>
        public static Func<T1, Func<T2, T3, T4, T5, T6, T7, T8, R>> CurryFirst<T1, T2, T3, T4, T5, T6, T7, T8, R>
           (this Func<T1, T2, T3, T4, T5, T6, T7, T8, R> @this) => t1 => (t2, t3, t4, t5, t6, t7, t8) => @this(t1, t2, t3, t4, t5, t6, t7, t8);

        /// <summary>Fixes the first argument of a 9-argument function; the remainder stay as a tuple.</summary>
        public static Func<T1, Func<T2, T3, T4, T5, T6, T7, T8, T9, R>> CurryFirst<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>
           (this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, R> @this) => t1 => (t2, t3, t4, t5, t6, t7, t8, t9) => @this(t1, t2, t3, t4, t5, t6, t7, t8, t9);
    }
}
