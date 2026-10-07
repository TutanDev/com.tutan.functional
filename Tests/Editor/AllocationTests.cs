using NUnit.Framework;
using UnityEngine.TestTools.Constraints;
using static Tutan.Functional.F;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace Tutan.Functional.Tests
{
    /// <summary>
    /// Guards the "allocation-conscious" contract from Documentation~/Functional.md: with capture-free
    /// lambdas, the core operators must not allocate. Each delegate is invoked once before measuring so
    /// JIT and static-lambda cache initialisation are not counted.
    /// </summary>
    [TestFixture]
    public class AllocationTests
    {
        private static void AssertNoAlloc(System.Action action)
        {
            action();
            Assert.That(() => action(), Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void Optional_CoreOperators_WithStaticLambdas_DoNotAllocate()
        {
            var some = Some(21);
            var none = default(Optional<int>);
            var text = Some("text");

            AssertNoAlloc(() => some.Map(static x => x * 2));
            AssertNoAlloc(() => some.Bind(static x => Some(x + 1)));
            AssertNoAlloc(() => some.Then(static x => x * 2));
            AssertNoAlloc(() => some.Then(static (int _) => { }));
            AssertNoAlloc(() => some.Filter(static x => x > 0));
            AssertNoAlloc(() => some.Where(static x => x > 0));
            AssertNoAlloc(() => some.Or(0));
            AssertNoAlloc(() => none.OrElse(static () => 1));
            AssertNoAlloc(() => some.Match(static () => 0, static x => x));
            AssertNoAlloc(() => some.Map(3, static (x, s) => x * s));
            AssertNoAlloc(() => text.Map(static s => s.Length));
            AssertNoAlloc(() => some.ToResult(static () => Error("none")));
            AssertNoAlloc(() => some.ValueUnsafe());
        }

        [Test]
        public void Result_CoreOperators_WithStaticLambdas_DoNotAllocate()
        {
            Result<int> ok = Success(21);
            Result<int> err = Error("boom");

            AssertNoAlloc(() => ok.Map(static x => x * 2));
            AssertNoAlloc(() => ok.Bind(static x => Success(x + 1)));
            AssertNoAlloc(() => err.Bind(static x => Success(x + 1)));
            AssertNoAlloc(() => ok.Then(static x => x * 2));
            AssertNoAlloc(() => ok.Then(static (int _) => { }));
            AssertNoAlloc(() => ok.Filter(static x => x > 0));
            AssertNoAlloc(() => ok.Or(0));
            AssertNoAlloc(() => err.OrElse(static (Error _) => 1));
            AssertNoAlloc(() => ok.Match(static _ => 0, static x => x));
            AssertNoAlloc(() => ok.Map(3, static (x, s) => x * s));
            AssertNoAlloc(() => ok.ToOptional());
            AssertNoAlloc(() => err.IfFail(static (Error _) => { }));
        }
    }
}
