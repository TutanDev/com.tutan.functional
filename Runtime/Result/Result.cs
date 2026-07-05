using System;
using System.Runtime.CompilerServices;

namespace Tutan.Functional
{
    public static partial class F
    {
        /// <summary>Creates a successful <see cref="Result{T}"/> with no value (<c>Result&lt;Unit&gt;</c>). The partner of <c>Fail(...)</c>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<Unit> Success() => new(Unit());

        /// <summary>
        /// Creates a successful <see cref="Result{T}"/>. Null references and destroyed
        /// <see cref="UnityEngine.Object"/> references become <c>Error("Value is null")</c>,
        /// so missing assets surface as errors instead of hiding inside a success.
        /// </summary>
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

    /// <summary>
    /// The outcome of an operation: either a success value <c>T</c> or an <see cref="Error"/>.
    /// Replaces exception-based error handling with composable, type-safe results.
    /// Construct via <see cref="F.Success{T}(T)"/>, <see cref="F.Fail{T}(Error)"/>, or the implicit conversions.
    /// Note: <c>default(Result&lt;T&gt;)</c> is an error carrying an empty-message <see cref="Error"/>.
    /// </summary>
    public readonly record struct Result<T>
    {
        internal readonly T _value;
        internal readonly Error _error;
        private readonly bool _isSuccess;

        /// <summary><c>true</c> when the operation succeeded.</summary>
        public bool IsSuccess => _isSuccess;

        /// <summary><c>true</c> when the operation failed.</summary>
        public bool IsError => !_isSuccess;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Result(T data)
            => (_isSuccess, _value, _error) = (true, data, default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Result(Error error)
            => (_isSuccess, _value, _error) = (false, default, error);


        /// <summary>Extracts the outcome by pattern matching: calls <paramref name="onSuccess"/> with the value, or <paramref name="onError"/> with the error.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public R Match<R>(Func<Error, R> onError, Func<T, R> onSuccess) => _isSuccess ? onSuccess(_value) : onError(_error);

        /// <summary>Void pattern match: runs <paramref name="onSuccess"/> with the value, or <paramref name="onError"/> with the error.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Unit Match(Action<Error> onError, Action<T> onSuccess)
        {
            if (_isSuccess) onSuccess(_value);
            else onError(_error);
            return default;
        }

        /// <summary>
        /// State-passing pattern match: <paramref name="state"/> is handed to the branch instead of being
        /// captured in a closure, so the call is allocation-free when the delegates are capture-free.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public R Match<TState, R>(TState state, Func<Error, TState, R> onError, Func<T, TState, R> onSuccess)
            => _isSuccess ? onSuccess(_value, state) : onError(_error, state);

        /// <summary>
        /// State-passing void pattern match: <paramref name="state"/> is handed to the branch instead of being
        /// captured in a closure, so the call is allocation-free when the delegates are capture-free.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Unit Match<TState>(TState state, Action<Error, TState> onError, Action<T, TState> onSuccess)
        {
            if (_isSuccess) onSuccess(_value, state);
            else onError(_error, state);
            return default;
        }


        /// <summary>Returns <c>"Success: {value}"</c> or <c>"Error: {error}"</c>.</summary>
        public override string ToString() => _isSuccess ? $"Success: {_value}" : $"Error: {_error}";

        /// <summary>Lifts a value into <c>Success</c> via <see cref="F.Success{T}(T)"/> (null and destroyed Unity objects become an error).</summary>
        public static implicit operator Result<T>(T value) => Success(value);

        /// <summary>Lifts an <see cref="Error"/> into a failed result.</summary>
        public static implicit operator Result<T>(Error error) => new(error);

        // Enables `if (result)` / `result && other` short-circuiting on the success branch.

        /// <summary>Truthy on success: enables <c>if (result)</c> and <c>&amp;&amp;</c> short-circuiting on the success branch.</summary>
        public static bool operator true(Result<T> result) => result._isSuccess;

        /// <summary>Falsy on error: the counterpart required for <c>operator true</c>.</summary>
        public static bool operator false(Result<T> result) => !result._isSuccess;
    }
}
