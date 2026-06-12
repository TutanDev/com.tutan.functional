using System;
using System.Collections.Generic;
using System.Linq;

namespace Tutan.Functional
{
    public static partial class F
    {
        public static Error Error(string message) => new Error(message);
        public static Error Error(string message, int code) => new Error(message, code);
        public static Error Error(string message, Error inner) => new Error(message, inner);
        public static Error Error(string message, int code, Error inner) => new Error(message, code, inner);
        public static Error Error(IEnumerable<Error> errors) => new Error(errors);
    }

    /// <summary>
    /// An immutable error value (16 bytes on 64-bit). The struct itself never allocates;
    /// constructors that take an inner error or a collection allocate the inner array.
    /// <para>
    /// <see cref="InnerErrors"/> returns a <see cref="ReadOnlySpan{T}"/> which cannot be
    /// stored in class fields, used across async boundaries, or passed to LINQ without
    /// first calling <c>.ToArray()</c>.
    /// </para>
    /// </summary>
    public readonly struct Error : IEquatable<Error>
    {
        private readonly string _message;
        private readonly int _code;
        private readonly Error[] _inner;

        public string Message => _message;
        public int Code => _code;

        /// <remarks>Returns a <see cref="ReadOnlySpan{T}"/>; cannot cross async boundaries.</remarks>
        public ReadOnlySpan<Error> InnerErrors => _inner ?? Array.Empty<Error>();

        private bool HasInner => _inner is { Length: > 0 };

        public Error(string message)
        {
            _message = message;
            _code = 0;
            _inner = null;
        }

        public Error(string message, int code)
        {
            _message = message;
            _code = code;
            _inner = null;
        }

        public Error(string message, Error inner)
        {
            _message = message;
            _code = 0;
            _inner = new[] { inner };
        }

        public Error(string message, int code, Error inner)
        {
            _message = message;
            _code = code;
            _inner = new[] { inner };
        }

        public Error(IEnumerable<Error> errors)
        {
            var arr = errors.ToArray();
            _message = string.Join("; ", arr.Select(e => e.Message));
            _code = 0;
            _inner = arr;
        }

        public static implicit operator Error(string message) => new(message);

        public IEnumerable<Error> AsEnumerable()
            => HasInner ? _inner : new[] { this };

        public override string ToString() => _message ?? string.Empty;

        public bool Equals(Error other)
            => _message == other._message && _code == other._code && ErrorArraysEqual(_inner, other._inner);

        public override bool Equals(object obj) => obj is Error other && Equals(other);

        public override int GetHashCode()
        {
            var h = _message?.GetHashCode() ?? 0;
            h = h * 31 + _code;
            if (_inner != null)
                h = h * 31 + _inner.Length;
            return h;
        }

        public static bool operator ==(Error left, Error right) => left.Equals(right);
        public static bool operator !=(Error left, Error right) => !left.Equals(right);

        private static bool ErrorArraysEqual(Error[] a, Error[] b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            if (a.Length != b.Length) return false;
            for (var i = 0; i < a.Length; i++)
                if (!a[i].Equals(b[i])) return false;
            return true;
        }
    }
}
