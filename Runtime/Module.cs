global using static Tutan.Functional.F;
global using Unit = System.ValueTuple;

using System;
using System.Collections.Generic;
using System.Linq;


namespace Tutan.Functional
{
    public static partial class F
    {
        public static Unit Unit() => default;

        public static IEnumerable<T> List<T>(params T[] items) => items.AsEnumerable();

        public static Func<Unit> Tee<T>(Action function) => () =>
        {
            function();
            return default;
        };

        public static Func<T, T> Tee<T>(Action<T> function) => (t) =>
        {
            function(t);
            return t;
        };


        public static R Pipe<T, R>(this T @this, Func<T, R> func) => func(@this);

        public static T Pipe<T>(this T input, Action<T> func) => Tee(func)(input);


        public static Result<T> Try<T>(Func<T> f)
        {
            try { return Success(f()); }
            catch (Exception ex) { return new Error(ex.ToString()); }
        }

        public static Result<Unit> Try(Action action)
        {
            try { action(); return Success(Unit()); }
            catch (Exception ex) { return new Error(ex.ToString()); }
        }


        // ── Curry ───────────────────────────────────────────────

        public static Func<T1, Func<T2, R>> Curry<T1, T2, R>(this Func<T1, T2, R> func)
            => t1 => t2 => func(t1, t2);

        public static Func<T1, Func<T2, Func<T3, R>>> Curry<T1, T2, T3, R>(this Func<T1, T2, T3, R> func)
            => t1 => t2 => t3 => func(t1, t2, t3);

        public static Func<T1, Func<T2, T3, R>> CurryFirst<T1, T2, T3, R>
            (this Func<T1, T2, T3, R> @this) => t1 => (t2, t3) => @this(t1, t2, t3);

        public static Func<T1, Func<T2, T3, T4, R>> CurryFirst<T1, T2, T3, T4, R>
           (this Func<T1, T2, T3, T4, R> @this) => t1 => (t2, t3, t4) => @this(t1, t2, t3, t4);

        public static Func<T1, Func<T2, T3, T4, T5, R>> CurryFirst<T1, T2, T3, T4, T5, R>
           (this Func<T1, T2, T3, T4, T5, R> @this) => t1 => (t2, t3, t4, t5) => @this(t1, t2, t3, t4, t5);

        public static Func<T1, Func<T2, T3, T4, T5, T6, R>> CurryFirst<T1, T2, T3, T4, T5, T6, R>
           (this Func<T1, T2, T3, T4, T5, T6, R> @this) => t1 => (t2, t3, t4, t5, t6) => @this(t1, t2, t3, t4, t5, t6);

        public static Func<T1, Func<T2, T3, T4, T5, T6, T7, R>> CurryFirst<T1, T2, T3, T4, T5, T6, T7, R>
           (this Func<T1, T2, T3, T4, T5, T6, T7, R> @this) => t1 => (t2, t3, t4, t5, t6, t7) => @this(t1, t2, t3, t4, t5, t6, t7);

        public static Func<T1, Func<T2, T3, T4, T5, T6, T7, T8, R>> CurryFirst<T1, T2, T3, T4, T5, T6, T7, T8, R>
           (this Func<T1, T2, T3, T4, T5, T6, T7, T8, R> @this) => t1 => (t2, t3, t4, t5, t6, t7, t8) => @this(t1, t2, t3, t4, t5, t6, t7, t8);

        public static Func<T1, Func<T2, T3, T4, T5, T6, T7, T8, T9, R>> CurryFirst<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>
           (this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, R> @this) => t1 => (t2, t3, t4, t5, t6, t7, t8, t9) => @this(t1, t2, t3, t4, t5, t6, t7, t8, t9);
    }
}
