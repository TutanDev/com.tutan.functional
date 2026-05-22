using System;
using System.Linq;
using NUnit.Framework;
using static Tutan.Functional.F;

namespace Tutan.Functional.Tests
{


    [TestFixture]
    public class ModuleTests
    {
        // 1. Unit() returns default ValueTuple
        [Test]
        public void Unit_ReturnsDefaultValueTuple()
        {
            Unit result = Unit();

            Assert.AreEqual(default(ValueTuple), result);
        }

        // 2. List() creates enumerable from params
        [Test]
        public void List_WithParams_CreatesEnumerableWithItems()
        {
            var result = List(1, 2, 3).ToList();

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(2, result[1]);
            Assert.AreEqual(3, result[2]);
        }

        // 3. List() with no args returns empty
        [Test]
        public void List_WithNoArgs_ReturnsEmptyEnumerable()
        {
            var result = List<int>().ToList();

            Assert.AreEqual(0, result.Count);
        }

        // 4. Tee (Action overload) - executes side effect, returns Unit func
        [Test]
        public void Tee_ActionOverload_ExecutesSideEffectAndReturnsUnitFunc()
        {
            bool executed = false;
            Action sideEffect = () => executed = true;

            Func<Unit> teed = Tee<int>(sideEffect);
            Unit result = teed();

            Assert.IsTrue(executed);
            Assert.AreEqual(default(ValueTuple), result);
        }

        // 5. Tee (Action<T> overload) - executes side effect, returns input
        [Test]
        public void Tee_ActionOfTOverload_ExecutesSideEffectAndReturnsInput()
        {
            int captured = 0;
            Action<int> sideEffect = x => captured = x;

            Func<int, int> teed = Tee(sideEffect);
            int result = teed(42);

            Assert.AreEqual(42, captured);
            Assert.AreEqual(42, result);
        }

        // 6. Pipe (Func overload) - transforms value
        [Test]
        public void Pipe_FuncOverload_TransformsValue()
        {
            int result = 5.Pipe(x => x * 2);

            Assert.AreEqual(10, result);
        }

        // 7. Pipe (Action overload) - executes action, returns input
        [Test]
        public void Pipe_ActionOverload_ExecutesActionAndReturnsInput()
        {
            int captured = 0;
            int result = 7.Pipe(x => captured = x);

            Assert.AreEqual(7, captured);
            Assert.AreEqual(7, result);
        }

        // 8. Try (Func) - success case returns Success result
        [Test]
        public void Try_FuncSuccess_ReturnsSuccessResult()
        {
            Result<int> result = Try(() => 42);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(42, result.ValueUnsafe());
        }

        // 9. Try (Func) - exception case returns Error result with message
        [Test]
        public void Try_FuncException_ReturnsErrorResultWithMessage()
        {
            Result<int> result = Try<int>(() => throw new InvalidOperationException("test error"));

            Assert.IsTrue(result.IsError);
            StringAssert.Contains("test error", result.ErrorUnsafe().Message);
        }

        // 10. Try (Action) - success case returns Success(Unit)
        [Test]
        public void Try_ActionSuccess_ReturnsSuccessUnit()
        {
            bool executed = false;
            Result<Unit> result = Try(() => { executed = true; });

            Assert.IsTrue(executed);
            Assert.IsTrue(result.IsSuccess);
        }

        // 11. Try (Action) - exception case returns Error result
        [Test]
        public void Try_ActionException_ReturnsErrorResult()
        {
            Result<Unit> result = Try(() => throw new InvalidOperationException("action failed"));

            Assert.IsTrue(result.IsError);
            StringAssert.Contains("action failed", result.ErrorUnsafe().Message);
        }

        // 12. Curry 2-arity - curries function
        [Test]
        public void Curry_TwoArity_CurriesFunction()
        {
            Func<int, int, int> add = (a, b) => a + b;

            Func<int, Func<int, int>> curried = add.Curry();
            int result = curried(3)(4);

            Assert.AreEqual(7, result);
        }

        // 13. Curry 3-arity - curries function
        [Test]
        public void Curry_ThreeArity_CurriesFunction()
        {
            Func<int, int, int, int> addThree = (a, b, c) => a + b + c;

            Func<int, Func<int, Func<int, int>>> curried = addThree.Curry();
            int result = curried(1)(2)(3);

            Assert.AreEqual(6, result);
        }

        // 14. CurryFirst 3-arity - curries first arg only
        [Test]
        public void CurryFirst_ThreeArity_CurriesFirstArgOnly()
        {
            Func<string, int, int, string> format = (prefix, a, b) => $"{prefix}: {a + b}";

            Func<string, Func<int, int, string>> curried = format.CurryFirst();
            Func<int, int, string> withPrefix = curried("Sum");
            string result = withPrefix(3, 4);

            Assert.AreEqual("Sum: 7", result);
        }

        // 15. CurryFirst 4-arity test
        [Test]
        public void CurryFirst_FourArity_CurriesFirstArgOnly()
        {
            Func<string, int, int, int, string> format = (prefix, a, b, c) => $"{prefix}: {a + b + c}";

            Func<string, Func<int, int, int, string>> curried = format.CurryFirst();
            Func<int, int, int, string> withPrefix = curried("Total");
            string result = withPrefix(1, 2, 3);

            Assert.AreEqual("Total: 6", result);
        }

        // 16. Pipe chaining - multiple pipes
        [Test]
        public void Pipe_Chaining_MultiplePipesTransformSequentially()
        {
            string result = 5
                .Pipe(x => x * 2)
                .Pipe(x => x + 1)
                .Pipe(x => x.ToString());

            Assert.AreEqual("11", result);
        }

        // 17. Tee in a chain doesn't change value
        [Test]
        public void Tee_InPipeChain_DoesNotChangeValue()
        {
            int sideEffectValue = 0;

            int result = 10
                .Pipe(x => x + 5)
                .Pipe((Action<int>)(x => sideEffectValue = x))
                .Pipe(x => x * 2);

            Assert.AreEqual(15, sideEffectValue);
            Assert.AreEqual(30, result);
        }

        // 18. Try with null reference exception
        [Test]
        public void Try_NullReferenceException_ReturnsErrorWithMessage()
        {
            Result<int> result = Try<int>(() =>
            {
                string s = null;
                return s.Length;
            });

            Assert.IsTrue(result.IsError);
            Assert.IsNotNull(result.ErrorUnsafe().Message);
            Assert.IsTrue(result.ErrorUnsafe().Message.Length > 0);
        }
    }
}
