using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static Tutan.Functional.F;

namespace Tutan.Functional.Tests
{
    using Unit = System.ValueTuple;

    [TestFixture]
    public class EnumerableExtTests
    {
        // 1. Head on non-empty list returns Some with first element
        [Test]
        public void Head_NonEmpty_ReturnsSome()
        {
            var result = List(1, 2, 3).Head();

            Assert.IsTrue(result.IsSome);
            result.Match(
                () => Assert.Fail("Expected Some"),
                v => Assert.AreEqual(1, v));
        }

        // 2. Head on empty list returns None
        [Test]
        public void Head_Empty_ReturnsNone()
        {
            var result = List<int>().Head();

            Assert.IsTrue(result.IsNone);
        }

        // 3. Head on null list returns None
        [Test]
        public void Head_Null_ReturnsNone()
        {
            IEnumerable<int> nullList = null;

            var result = nullList.Head();

            Assert.IsTrue(result.IsNone);
        }

        // 4. Head on single-element list returns Some
        [Test]
        public void Head_SingleElement_ReturnsSome()
        {
            var result = List(42).Head();

            Assert.IsTrue(result.IsSome);
            result.Match(
                () => Assert.Fail("Expected Some"),
                v => Assert.AreEqual(42, v));
        }

        // 5. FindFirst returns Some when match exists
        [Test]
        public void FindFirst_Exists_ReturnsSome()
        {
            var result = List(1, 2, 3, 4).FindFirst(x => x > 2);

            Assert.IsTrue(result.IsSome);
            result.Match(
                () => Assert.Fail("Expected Some"),
                v => Assert.AreEqual(3, v));
        }

        // 6. FindFirst returns None when no match
        [Test]
        public void FindFirst_NotExists_ReturnsNone()
        {
            var result = List(1, 2, 3).FindFirst(x => x > 10);

            Assert.IsTrue(result.IsNone);
        }

        // 7. FindFirst with multiple matches returns the first one
        [Test]
        public void FindFirst_MultipleMatches_ReturnsFirst()
        {
            var result = List(5, 10, 15, 20).FindFirst(x => x >= 10);

            Assert.IsTrue(result.IsSome);
            result.Match(
                () => Assert.Fail("Expected Some"),
                v => Assert.AreEqual(10, v));
        }

        // 8. Flatten merges nested lists into one
        [Test]
        public void Flatten_NestedLists_Flattens()
        {
            var nested = new List<IEnumerable<int>>
            {
                List(1, 2),
                List(3, 4),
                List(5)
            };

            var result = nested.Flatten().ToList();

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, result);
        }

        // 9. Flatten on empty outer returns empty
        [Test]
        public void Flatten_EmptyOuter_ReturnsEmpty()
        {
            var nested = new List<IEnumerable<int>>();

            var result = nested.Flatten().ToList();

            Assert.AreEqual(0, result.Count);
        }

        // 10. Flatten with empty inner lists skips them
        [Test]
        public void Flatten_WithEmptyInner_SkipsEmpty()
        {
            var nested = new List<IEnumerable<int>>
            {
                List(1, 2),
                List<int>(),
                List(3)
            };

            var result = nested.Flatten().ToList();

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
        }

        // 11. Match on non-empty list calls Otherwise with head and tail
        [Test]
        public void Match_NonEmpty_CallsOtherwise()
        {
            var result = List(10, 20, 30).Match(
                Empty: () => "empty",
                Otherwise: (head, tail) => $"{head}:{string.Join(",", tail)}");

            Assert.AreEqual("10:20,30", result);
        }

        // 12. Match on empty list calls Empty
        [Test]
        public void Match_Empty_CallsEmpty()
        {
            var result = List<int>().Match(
                Empty: () => "empty",
                Otherwise: (head, tail) => $"{head}");

            Assert.AreEqual("empty", result);
        }

        // 13. DropWhile drops matching prefix
        [Test]
        public void DropWhile_DropsMatchingPrefix()
        {
            var result = List(1, 2, 3, 4, 5).DropWhile(x => x < 3).ToList();

            CollectionAssert.AreEqual(new[] { 3, 4, 5 }, result);
        }

        // 14. DropWhile with no matching prefix returns all elements
        [Test]
        public void DropWhile_NoMatch_ReturnsAll()
        {
            var result = List(5, 4, 3, 2, 1).DropWhile(x => x > 10).ToList();

            CollectionAssert.AreEqual(new[] { 5, 4, 3, 2, 1 }, result);
        }

        // 15. DropWhile with all matching returns empty
        [Test]
        public void DropWhile_AllMatch_ReturnsEmpty()
        {
            var result = List(1, 2, 3).DropWhile(x => x < 100).ToList();

            Assert.AreEqual(0, result.Count);
        }

        // 16. DropWhile does not drop after the first non-matching element
        [Test]
        public void DropWhile_DoesNotDropAfterFirstFail()
        {
            var result = List(1, 2, 3, 1, 2).DropWhile(x => x < 3).ToList();

            CollectionAssert.AreEqual(new[] { 3, 1, 2 }, result);
        }
    }

    [TestFixture]
    public class EnumerableMonadTests
    {
        // 1. Return wraps a value in a singleton enumerable
        [Test]
        public void Return_CreatesEnumerable()
        {
            var wrap = EnumerableExt.Return<int>();
            var result = wrap(42).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(42, result[0]);
        }

        // 2. Map transforms each element
        [Test]
        public void Map_TransformsElements()
        {
            var result = List(1, 2, 3).Map(x => x * 10).ToList();

            CollectionAssert.AreEqual(new[] { 10, 20, 30 }, result);
        }

        // 3. Map on empty list returns empty
        [Test]
        public void Map_EmptyList_ReturnsEmpty()
        {
            var result = List<int>().Map(x => x * 2).ToList();

            Assert.AreEqual(0, result.Count);
        }

        // 4. Map with two-arity function curries it
        [Test]
        public void Map_TwoArity_CurriesFunction()
        {
            Func<int, int, int> add = (a, b) => a + b;
            var curriedResults = List(1, 2, 3).Map(add).ToList();

            // Each element should be a Func<int, int> with the first arg applied
            Assert.AreEqual(3, curriedResults.Count);
            Assert.AreEqual(11, curriedResults[0](10)); // 1 + 10
            Assert.AreEqual(12, curriedResults[1](10)); // 2 + 10
            Assert.AreEqual(13, curriedResults[2](10)); // 3 + 10
        }

        // 5. Map with three-arity function curries it
        [Test]
        public void Map_ThreeArity_CurriesFunction()
        {
            Func<int, int, int, int> addThree = (a, b, c) => a + b + c;
            var curriedResults = List(1, 2).Map(addThree).ToList();

            Assert.AreEqual(2, curriedResults.Count);
            // curriedResults[0] is Func<int, Func<int, int>> with a=1
            Assert.AreEqual(6, curriedResults[0](2)(3));  // 1 + 2 + 3
            Assert.AreEqual(12, curriedResults[1](4)(6)); // 2 + 4 + 6
        }

        // 6. ForEach executes side effects for each element
        [Test]
        public void ForEach_ExecutesSideEffects()
        {
            var collected = new List<int>();

            List(1, 2, 3).ForEach(x => collected.Add(x)).ToList(); // ToList forces evaluation

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, collected);
        }

        // 7. ForEach returns Unit for each element
        [Test]
        public void ForEach_ReturnsUnits()
        {
            var result = List(1, 2, 3).ForEach(_ => { }).ToList();

            Assert.AreEqual(3, result.Count);
            foreach (var u in result)
            {
                Assert.AreEqual(default(Unit), u);
            }
        }

        // 8. ForEach on empty list produces no side effects
        [Test]
        public void ForEach_EmptyList_NoSideEffects()
        {
            var collected = new List<int>();

            List<int>().ForEach(x => collected.Add(x)).ToList();

            Assert.AreEqual(0, collected.Count);
        }

        // 9. Bind flat-maps each element
        [Test]
        public void Bind_FlatMaps()
        {
            var result = List(1, 2, 3)
                .Bind(x => List(x, x * 10))
                .ToList();

            CollectionAssert.AreEqual(new[] { 1, 10, 2, 20, 3, 30 }, result);
        }

        // 10. Bind on empty list returns empty
        [Test]
        public void Bind_EmptyList_ReturnsEmpty()
        {
            var result = List<int>()
                .Bind(x => List(x, x))
                .ToList();

            Assert.AreEqual(0, result.Count);
        }

        // 11. Bind with Optional func filters out None values
        [Test]
        public void Bind_WithOptionalFunc_FiltersNone()
        {
            var result = List(1, 2, 3, 4, 5)
                .Bind(x => x % 2 == 0 ? Some(x) : (Optional<int>)None)
                .ToList();

            CollectionAssert.AreEqual(new[] { 2, 4 }, result);
        }

        // 12. Bind with Optional func keeps Some values
        [Test]
        public void Bind_WithOptionalFunc_KeepsSome()
        {
            var result = List("hello", "world")
                .Bind(s => Some(s.ToUpper()))
                .ToList();

            CollectionAssert.AreEqual(new[] { "HELLO", "WORLD" }, result);
        }
    }
}
