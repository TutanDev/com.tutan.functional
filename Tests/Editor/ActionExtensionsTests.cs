using System;
using NUnit.Framework;

namespace Tutan.Functional.Tests
{
    using Unit = System.ValueTuple;

    [TestFixture]
    public class ActionExtensionsTests
    {
        [Test]
        public void ToFunc_Action_ExecutesSideEffect()
        {
            var called = false;
            Action action = () => called = true;

            Func<Unit> func = action.ToFunc();
            func();

            Assert.IsTrue(called);
        }

        [Test]
        public void ToFunc_Action_ReturnsUnit()
        {
            Action action = () => { };

            Func<Unit> func = action.ToFunc();
            Unit result = func();

            Assert.AreEqual(default(Unit), result);
        }

        [Test]
        public void ToFunc_ActionT_ExecutesSideEffect()
        {
            var called = false;
            Action<int> action = _ => called = true;

            Func<int, Unit> func = action.ToFunc();
            func(42);

            Assert.IsTrue(called);
        }

        [Test]
        public void ToFunc_ActionT_ReturnsUnit()
        {
            Action<int> action = _ => { };

            Func<int, Unit> func = action.ToFunc();
            Unit result = func(42);

            Assert.AreEqual(default(Unit), result);
        }

        [Test]
        public void ToFunc_ActionT_ReceivesCorrectArgument()
        {
            int received = 0;
            Action<int> action = x => received = x;

            Func<int, Unit> func = action.ToFunc();
            func(99);

            Assert.AreEqual(99, received);
        }
    }
}
