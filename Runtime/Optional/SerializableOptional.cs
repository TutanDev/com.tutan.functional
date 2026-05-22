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

        public bool HasValue => _hasValue;
        public T Value => _value;

        public SerializableOptional(T value)
        {
            _hasValue = true;
            _value = value;
        }

        public Optional<T> ToOptional() => _hasValue ? F.Some(_value) : default;

        public static SerializableOptional<T> From(Optional<T> opt)
            => opt.HasValue(out var v) ? new SerializableOptional<T>(v) : default;

        public static implicit operator Optional<T>(SerializableOptional<T> s) => s.ToOptional();
        public static implicit operator SerializableOptional<T>(Optional<T> o) => From(o);
    }
}
