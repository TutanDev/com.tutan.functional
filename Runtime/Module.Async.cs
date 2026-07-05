using System;
using Cysharp.Threading.Tasks;

namespace Tutan.Functional
{
    public static partial class F
    {
        /// <summary>Awaits <paramref name="f"/>, catching any exception: returns <c>Success</c> or <c>Error(ex.ToString())</c>. Use at every external async boundary that might throw.</summary>
        public static async UniTask<Result<T>> TryAsync<T>(Func<UniTask<T>> f)
        {
            try { return Success(await f()); }
            catch (Exception ex) { return new Error(ex.ToString()); }
        }

        /// <summary>Awaits <paramref name="action"/>, catching any exception: returns <c>Success()</c> or <c>Error(ex.ToString())</c>.</summary>
        public static async UniTask<Result<Unit>> TryAsync(Func<UniTask> action)
        {
            try { await action(); return Success(Unit()); }
            catch (Exception ex) { return new Error(ex.ToString()); }
        }
    }
}
