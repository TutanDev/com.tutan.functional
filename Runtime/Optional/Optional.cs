using System;
using System.Runtime.CompilerServices;

namespace Tutan.Functional
{
    /// <summary>The absence sentinel. Implicitly converts to a <c>None</c> <see cref="Optional{T}"/> of any <c>T</c>.</summary>
    public struct NoneType { }

    public static partial class F
    {
        /// <summary>The absent value. Return or assign it wherever an <see cref="Optional{T}"/> is expected.</summary>
        public static NoneType None => default;


        /// <summary>
        /// Wraps a value in <c>Some</c>. Returns <c>None</c> for null references and destroyed
        /// <see cref="UnityEngine.Object"/> references. Unity fake-null is checked once, at creation;
        /// call <c>Alive()</c> to re-check when an optional crosses a frame boundary.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<T> Some<T>(T value)
        {
            if (typeof(T).IsValueType)
                return new(value);

            if (value is null || value is UnityEngine.Object uo && uo == null)
                return default;

            return new(value);
        }
    }

    /// <summary>
    /// A value that may be present (<c>Some</c>) or absent (<c>None</c>).
    /// Replaces null checks with explicit handling of the missing case.
    /// Construct via <see cref="F.Some{T}(T)"/>, <see cref="F.None"/>, or the implicit conversions.
    /// </summary>
    public readonly record struct Optional<T>
    {
        internal readonly T _value;
        private readonly bool _isSome;

        /// <summary><c>true</c> when a value is present.</summary>
        public bool IsSome => _isSome;

        /// <summary><c>true</c> when the value is absent.</summary>
        public bool IsNone => !_isSome;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Optional(T value) => (_isSome, _value) = (true, value);


        /// <summary>Extracts the value by pattern matching: calls <paramref name="onSome"/> with the value, or <paramref name="onNone"/> when absent.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public R Match<R>(Func<R> onNone, Func<T, R> onSome) => _isSome ? onSome(_value) : onNone();

        /// <summary>Void pattern match: runs <paramref name="onSome"/> with the value, or <paramref name="onNone"/> when absent.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Unit Match(Action onNone, Action<T> onSome)
        {
            if (_isSome) onSome(_value);
            else onNone();
            return default;
        }

        /// <summary>
        /// State-passing pattern match: <paramref name="state"/> is handed to the branch instead of being
        /// captured in a closure, so the call is allocation-free when the delegates are capture-free.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public R Match<TState, R>(TState state, Func<TState, R> onNone, Func<T, TState, R> onSome)
            => _isSome ? onSome(_value, state) : onNone(state);

        /// <summary>
        /// State-passing void pattern match: <paramref name="state"/> is handed to the branch instead of being
        /// captured in a closure, so the call is allocation-free when the delegates are capture-free.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Unit Match<TState>(TState state, Action<TState> onNone, Action<T, TState> onSome)
        {
            if (_isSome) onSome(_value, state);
            else onNone(state);
            return default;
        }


        /// <summary>Returns <c>"Some: {value}"</c> or <c>"None"</c>.</summary>
        public override string ToString() => _isSome ? $"Some: {_value}" : "None";

        /// <summary>Lifts a value into <c>Some</c> via <see cref="F.Some{T}(T)"/> (null and destroyed Unity objects become <c>None</c>).</summary>
        public static implicit operator Optional<T>(T value) => Some(value);

        /// <summary>Converts the <see cref="F.None"/> sentinel into an absent optional.</summary>
        public static implicit operator Optional<T>(NoneType _) => default;
    }
}
