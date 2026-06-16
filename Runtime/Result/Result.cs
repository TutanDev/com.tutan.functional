using System;
using System.Runtime.CompilerServices;

namespace Tutan.Functional
{
    public static partial class F
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Unit> Success() => new(Unit());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> Success<T>(T value)
        {
            if (typeof(T).IsValueType)
                return new(value);

            if (value is null || value is UnityEngine.Object uo && uo == null)
                return new("Value is null");

            return new(value);
        }
    }

    public readonly record struct Result<T>
    {
        internal readonly T _value;
        internal readonly Error _error;
        private readonly bool _isSuccess;

        public bool IsSuccess => _isSuccess;
        public bool IsError => !_isSuccess;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Result(T data)
            => (_isSuccess, _value, _error) = (true, data, default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Result(Error error)
            => (_isSuccess, _value, _error) = (false, default, error);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public R Match<R>(Func<Error, R> onError, Func<T, R> onSuccess) => _isSuccess ? onSuccess(_value) : onError(_error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Unit Match(Action<Error> onError, Action<T> onSuccess)
        {
            if (_isSuccess) onSuccess(_value);
            else onError(_error);
            return default;
        }

        // state-passing (avoids closure capture; allocation-free when the delegates are capture-free)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public R Match<TState, R>(TState state, Func<Error, TState, R> onError, Func<T, TState, R> onSuccess)
            => _isSuccess ? onSuccess(_value, state) : onError(_error, state);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Unit Match<TState>(TState state, Action<Error, TState> onError, Action<T, TState> onSuccess)
        {
            if (_isSuccess) onSuccess(_value, state);
            else onError(_error, state);
            return default;
        }


        public override string ToString() => _isSuccess ? $"Success: {_value}" : $"Error: {_error}";
        public static implicit operator Result<T>(T value) => Success(value);
        public static implicit operator Result<T>(Error error) => new(error);

        // Enables `if (result)` / `result && other` short-circuiting on the success branch.
        public static bool operator true(Result<T> result) => result._isSuccess;
        public static bool operator false(Result<T> result) => !result._isSuccess;
    }
}
