using System;
using UnityEngine;

namespace Tutan.Functional
{
    /// <summary>
    /// Inspector-serializable counterpart to <see cref="Optional{T}"/>.
    /// Use as a <c>[SerializeField]</c> field; convert to/from <see cref="Optional{T}"/> at the API boundary.
    /// </summary>
    [Serializable]
    public struct SerializableOptional<T>
    {
        [SerializeField] private bool _hasValue;
        [SerializeField] private T _value;

        /// <summary><c>true</c> when a value is present (the Inspector toggle is on).</summary>
        public bool HasValue => _hasValue;

        /// <summary>The wrapped value; undefined when <see cref="HasValue"/> is <c>false</c>.</summary>
        public T Value => _value;

        /// <summary>Wraps a value and marks it as present.</summary>
        public SerializableOptional(T value)
        {
            _hasValue = true;
            _value = value;
        }

        /// <summary>Converts to <see cref="Optional{T}"/>: <c>Some(Value)</c> when present, otherwise <c>None</c>.</summary>
        public Optional<T> ToOptional() => _hasValue ? F.Some(_value) : default;

        /// <summary>Converts from <see cref="Optional{T}"/>: <c>None</c> becomes the default (unset) instance.</summary>
        public static SerializableOptional<T> From(Optional<T> opt)
            => opt.HasValue(out var v) ? new SerializableOptional<T>(v) : default;

        /// <summary>Implicit conversion to <see cref="Optional{T}"/> via <see cref="ToOptional"/>.</summary>
        public static implicit operator Optional<T>(SerializableOptional<T> s) => s.ToOptional();

        /// <summary>Implicit conversion from <see cref="Optional{T}"/> via <see cref="From"/>.</summary>
        public static implicit operator SerializableOptional<T>(Optional<T> o) => From(o);
    }
}
