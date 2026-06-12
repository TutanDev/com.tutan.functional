using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Tutan.Functional
{
    public static class LookupExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<T> LookupComponent<T>(this GameObject go) where T : Component
            => go && go.TryGetComponent<T>(out var c) ? Some(c) : default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<Transform> LookupParent(this Transform t)
            => t && t.parent ? Some(t.parent) : default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<T> Lookup<K, T>(this IDictionary<K, T> dict, K key)
           => dict.TryGetValue(key, out T value) ? Some(value) : default;

        /// <summary>
        /// Re-checks Unity lifetime: returns <c>None</c> if the wrapped object has been
        /// destroyed since the <see cref="Optional{T}"/> was created. The monadic analogue
        /// of <c>if (obj)</c> — call it whenever an optional crosses a frame boundary.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<T> Alive<T>(this Optional<T> opt) where T : Object
            => opt.IsSome && opt._value ? opt : default;
    }
}
