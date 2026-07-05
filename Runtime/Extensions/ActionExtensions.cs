using System;
using System.Runtime.CompilerServices;

namespace Tutan.Functional
{
    /// <summary>Adapters that lift <see cref="Action"/> delegates into <c>Func</c> equivalents returning <c>Unit</c>, so side-effects fit functional pipelines.</summary>
    public static class ActionExtensions
    {
        /// <summary>Lifts an <see cref="Action"/> to <c>Func&lt;Unit&gt;</c>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<Unit> ToFunc(this Action action)
            => () =>
            {
                action();
                return default;
            };

        /// <summary>Lifts an <see cref="Action{T}"/> to <c>Func&lt;T, Unit&gt;</c>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T, Unit> ToFunc<T>(this Action<T> action)
            => t =>
            {
                action(t);
                return default;
            };
    }
}
