using System;
using System.Collections.Generic;
using System.Linq;

namespace Tutan.Functional
{
    /// <summary>Functional extensions over <see cref="IEnumerable{T}"/>: optional-returning accessors and monadic <c>Map</c>/<c>Bind</c>/<c>ForEach</c>.</summary>
    public static class EnumerableExt
    {
        /// <summary>Returns the first element as <c>Some</c>, or <c>None</c> when the sequence is empty or null. Never throws.</summary>
        public static Optional<T> Head<T>(this IEnumerable<T> list)
        {
            if (list == null) return default;
            using var enumerator = list.GetEnumerator();
            return enumerator.MoveNext() ? Some(enumerator.Current) : default;
        }

        /// <summary>Returns the first element matching <paramref name="predicate"/> as <c>Some</c>, or <c>None</c> when nothing matches or the source is null. Never throws.</summary>
        public static Optional<T> FindFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate)
            => source == null ? default : source.Where(predicate).Head();

        /// <summary>Flattens one level of nesting; equivalent to <c>SelectMany(x => x)</c>.</summary>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> list)
            => list.SelectMany(x => x);

        /// <summary>
        /// Structural match: deconstructs the sequence into head and tail, or calls <paramref name="Empty"/> when it has no elements.
        /// Enumerates the source twice (head, then tail), so pass a repeatable sequence, not a one-shot iterator.
        /// </summary>
        public static R Match<T, R>(this IEnumerable<T> list,
            Func<R> Empty,
            Func<T, IEnumerable<T>, R> Otherwise)
            => list.Head()
                .Match(Empty, head => Otherwise(head, list.Skip(1)));

        /// <summary>Skips elements while <paramref name="pred"/> holds, then yields everything from the first non-matching element onward.</summary>
        public static IEnumerable<T> DropWhile<T>(this IEnumerable<T> @this, Func<T, bool> pred)
        {
            bool clean = true;
            foreach (var item in @this)
            {
                if (!clean || !pred(item))
                {
                    yield return item;
                    clean = false;
                }
            }
        }

        // ── Return ──────────────────────────────────────────────

        /// <summary>The monadic return for sequences: a function that wraps a value in a singleton sequence.</summary>
        public static Func<T, IEnumerable<T>> Return<T>() => t => List(t);

        // ── Map ────────────────────────────────────────────────

        /// <summary>Map: alias for <c>Select</c>.</summary>
        public static IEnumerable<R> Map<T, R>
            (this IEnumerable<T> list, Func<T, R> func)
            => list.Select(func);

        /// <summary>Maps a two-argument function, currying it so each element becomes a function awaiting the second argument.</summary>
        public static IEnumerable<Func<T2, R>> Map<T1, T2, R>
            (this IEnumerable<T1> list, Func<T1, T2, R> func)
            => list.Map(func.Curry());

        /// <summary>Maps a three-argument function, currying it so each element becomes a chain of single-argument functions.</summary>
        public static IEnumerable<Func<T2, Func<T3, R>>> Map<T1, T2, T3, R>
            (this IEnumerable<T1> opt, Func<T1, T2, T3, R> func)
            => opt.Map(func.Curry());

        // ── ForEach ─────────────────────────────────────────────

        /// <summary>Side-effect over the sequence. Lazy: enumerate the result (or <c>.ToList()</c>) to force execution.</summary>
        public static IEnumerable<Unit> ForEach<T>(this IEnumerable<T> ts, Action<T> action)
            => ts.Map(action.ToFunc());

        // ── Bind ───────────────────────────────────────────────

        /// <summary>Bind (flat-map): alias for <c>SelectMany</c>.</summary>
        public static IEnumerable<R> Bind<T, R>
            (this IEnumerable<T> list, Func<T, IEnumerable<R>> func)
            => list.SelectMany(func);

        /// <summary>Flat-maps through an <see cref="Optional{T}"/>-returning function, filtering out <c>None</c> results automatically.</summary>
        public static IEnumerable<R> Bind<T, R>
            (this IEnumerable<T> list, Func<T, Optional<R>> func)
          => list.Bind(t => func(t).AsEnumerable());
    }
}
