using System;
using System.Runtime.CompilerServices;

namespace Tutan.Functional
{
    public struct NoneType { }
    public static partial class F
    {
        public static NoneType None => default;


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

    public readonly record struct Optional<T>
    {
        internal readonly T _value;
        private readonly bool _isSome;

        public bool IsSome => _isSome;
        public bool IsNone => !_isSome;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Optional(T value) => (_isSome, _value) = (true, value);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public R Match<R>(Func<R> onNone, Func<T, R> onSome) => _isSome ? onSome(_value) : onNone();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Unit Match(Action onNone, Action<T> onSome)
        {
            if (_isSome) onSome(_value);
            else onNone();
            return default;
        }

        // state-passing (avoids closure capture; allocation-free when the delegates are capture-free)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public R Match<TState, R>(TState state, Func<TState, R> onNone, Func<T, TState, R> onSome)
            => _isSome ? onSome(_value, state) : onNone(state);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Unit Match<TState>(TState state, Action<TState> onNone, Action<T, TState> onSome)
        {
            if (_isSome) onSome(_value, state);
            else onNone(state);
            return default;
        }


        public override string ToString() => _isSome ? $"Some: {_value}" : "None";
        public static implicit operator Optional<T>(T value) => Some(value);
        public static implicit operator Optional<T>(NoneType _) => default;
    }
}
