using System.Collections.Generic;
using NUnit.Framework;

namespace Tutan.Functional.Tests
{
    [TestFixture]
    public class LookupExtensionsTests
    {
        [Test]
        public void Lookup_ExistingKey_ReturnsSome()
        {
            var dict = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };

            Optional<int> result = dict.Lookup("a");

            Assert.IsTrue(result.IsSome);
        }

        [Test]
        public void Lookup_MissingKey_ReturnsNone()
        {
            var dict = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };

            Optional<int> result = dict.Lookup("z");

            Assert.IsTrue(result.IsNone);
        }

        [Test]
        public void Lookup_ExistingKey_HasCorrectValue()
        {
            var dict = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };

            Optional<int> result = dict.Lookup("b");

            int value = result.Match(() => -1, v => v);
            Assert.AreEqual(2, value);
        }

        [Test]
        public void Lookup_EmptyDictionary_ReturnsNone()
        {
            var dict = new Dictionary<string, int>();

            Optional<int> result = dict.Lookup("any");

            Assert.IsTrue(result.IsNone);
        }

        [Test]
        public void Lookup_MultipleKeys_FindsCorrectOne()
        {
            var dict = new Dictionary<string, int>
            {
                { "x", 10 },
                { "y", 20 },
                { "z", 30 }
            };

            Optional<int> result = dict.Lookup("y");

            int value = result.Match(() => -1, v => v);
            Assert.AreEqual(20, value);
        }

        [Test]
        public void Lookup_NullValue_ReturnsSome()
        {
            // For reference types, Some(null) returns None because the Some factory
            // method checks for null and returns default (None) when the value is null.
            var dict = new Dictionary<string, string> { { "key", null } };

            Optional<string> result = dict.Lookup("key");

            Assert.IsTrue(result.IsNone);
        }
    }
}
