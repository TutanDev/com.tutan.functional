using NUnit.Framework;
using static Tutan.Functional.F;
using System;
using System.Linq;

namespace Tutan.Functional.Tests
{
    [TestFixture]
    public class OptionalTests
    {
        // ── Optional<T> struct (Optional.cs) ──────────────────────────

        [Test]
        public void Some_WithValue_IsSome()
        {
            var opt = Some(42);
            Assert.That(opt.IsSome, Is.True);
            Assert.That(opt.IsNone, Is.False);
        }

        [Test]
        public void Some_WithNullReference_ReturnsNone()
        {
            var opt = Some<string>(null);
            Assert.That(opt.IsNone, Is.True);
        }

        [Test]
        public void Some_WithStringValue_IsSome()
        {
            var opt = Some("hello");
            Assert.That(opt.IsSome, Is.True);
        }

        [Test]
        public void None_ReturnsNoneType()
        {
            NoneType n = None;
            Assert.That(n, Is.EqualTo(default(NoneType)));
        }

        [Test]
        public void ImplicitConversion_FromValue_CreatesSome()
        {
            Optional<int> opt = 10;
            Assert.That(opt.IsSome, Is.True);
            Assert.That(opt.Match(() => -1, v => v), Is.EqualTo(10));
        }

        [Test]
        public void ImplicitConversion_FromNone_CreatesNone()
        {
            Optional<int> opt = None;
            Assert.That(opt.IsNone, Is.True);
        }

        [Test]
        public void Match_OnSome_CallsOnSome()
        {
            var opt = Some(5);
            var result = opt.Match(() => "none", v => $"some:{v}");
            Assert.That(result, Is.EqualTo("some:5"));
        }

        [Test]
        public void Match_OnNone_CallsOnNone()
        {
            Optional<int> opt = None;
            var result = opt.Match(() => "none", v => $"some:{v}");
            Assert.That(result, Is.EqualTo("none"));
        }

        [Test]
        public void MatchAction_OnSome_CallsOnSome()
        {
            var opt = Some(7);
            var called = "";
            opt.Match(() => called = "none", v => called = $"some:{v}");
            Assert.That(called, Is.EqualTo("some:7"));
        }

        [Test]
        public void MatchAction_OnNone_CallsOnNone()
        {
            Optional<string> opt = None;
            var called = "";
            opt.Match(() => called = "none", v => called = $"some:{v}");
            Assert.That(called, Is.EqualTo("none"));
        }

        [Test]
        public void ToString_Some_ReturnsSomeString()
        {
            var opt = Some(42);
            Assert.That(opt.ToString(), Is.EqualTo("Some: 42"));
        }

        [Test]
        public void ToString_None_ReturnsNoneString()
        {
            Optional<int> opt = None;
            Assert.That(opt.ToString(), Is.EqualTo("None"));
        }

        [Test]
        public void Default_Optional_IsNone()
        {
            var opt = default(Optional<int>);
            Assert.That(opt.IsNone, Is.True);
        }

        // ── Optional.Traits.cs ────────────────────────────────────────

        [Test]
        public void AsEnumerable_Some_ReturnsSingleElement()
        {
            var opt = Some(99);
            var list = opt.AsEnumerable().ToList();
            Assert.That(list, Has.Count.EqualTo(1));
            Assert.That(list[0], Is.EqualTo(99));
        }

        [Test]
        public void AsEnumerable_None_ReturnsEmpty()
        {
            Optional<int> opt = None;
            var list = opt.AsEnumerable().ToList();
            Assert.That(list, Is.Empty);
        }

        [Test]
        public void ToResult_Some_ReturnsSuccess()
        {
            var opt = Some(10);
            var result = opt.ToResult(() => new Error("missing"));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ValueUnsafe(), Is.EqualTo(10));
        }

        [Test]
        public void ToResult_None_ReturnsError()
        {
            Optional<int> opt = None;
            var result = opt.ToResult(() => new Error("missing"));
            Assert.That(result.IsError, Is.True);
            Assert.That(result.ErrorUnsafe().Message, Is.EqualTo("missing"));
        }

        [Test]
        public void Map_Some_TransformsValue()
        {
            var opt = Some(3);
            var mapped = opt.Map(x => x * 10);
            Assert.That(mapped.Match(() => -1, v => v), Is.EqualTo(30));
        }

        [Test]
        public void Map_None_ReturnsNone()
        {
            Optional<int> opt = None;
            var mapped = opt.Map(x => x * 10);
            Assert.That(mapped.IsNone, Is.True);
        }

        [Test]
        public void Map_OnNoneType_ReturnsNone()
        {
            var mapped = None.Map<int, string>(x => x.ToString());
            Assert.That(mapped.IsNone, Is.True);
        }

        [Test]
        public void Map_MultiArity2_CurriesFunction()
        {
            Func<int, int, int> add = (a, b) => a + b;
            var opt = Some(3);
            var partial = opt.Map(add);
            Assert.That(partial.IsSome, Is.True);
            var result = partial.Apply(Some(7));
            Assert.That(result.Match(() => -1, v => v), Is.EqualTo(10));
        }

        [Test]
        public void Map_MultiArity3_CurriesFunction()
        {
            Func<int, int, int, int> add3 = (a, b, c) => a + b + c;
            var opt = Some(1);
            var partial = opt.Map(add3);
            Assert.That(partial.IsSome, Is.True);
        }

        [Test]
        public void Bind_Some_FlatMaps()
        {
            var opt = Some(5);
            var bound = opt.Bind(x => x > 3 ? Some(x.ToString()) : (Optional<string>)None);
            Assert.That(bound.Match(() => "none", v => v), Is.EqualTo("5"));
        }

        [Test]
        public void Bind_Some_ReturnsNoneWhenFunctionReturnsNone()
        {
            var opt = Some(1);
            var bound = opt.Bind(x => x > 3 ? Some(x.ToString()) : (Optional<string>)None);
            Assert.That(bound.IsNone, Is.True);
        }

        [Test]
        public void Bind_None_ReturnsNone()
        {
            Optional<int> opt = None;
            var bound = opt.Bind(x => Some(x * 2));
            Assert.That(bound.IsNone, Is.True);
        }

        [Test]
        public void Bind_ToEnumerable_Some_ReturnsItems()
        {
            var opt = Some(3);
            var items = opt.Bind(x => Enumerable.Range(0, x)).ToList();
            Assert.That(items, Is.EqualTo(new[] { 0, 1, 2 }));
        }

        [Test]
        public void Bind_ToEnumerable_None_ReturnsEmpty()
        {
            Optional<int> opt = None;
            var items = opt.Bind(x => Enumerable.Range(0, x)).ToList();
            Assert.That(items, Is.Empty);
        }

        [Test]
        public void Select_Some_TransformsValue()
        {
            var opt = Some(4);
            var selected = opt.Select(x => x + 1);
            Assert.That(selected.Match(() => -1, v => v), Is.EqualTo(5));
        }

        [Test]
        public void Where_Some_PredicateTrue_KeepsValue()
        {
            var opt = Some(10);
            var filtered = opt.Where(x => x > 5);
            Assert.That(filtered.IsSome, Is.True);
        }

        [Test]
        public void Where_Some_PredicateFalse_ReturnsNone()
        {
            var opt = Some(2);
            var filtered = opt.Where(x => x > 5);
            Assert.That(filtered.IsNone, Is.True);
        }

        [Test]
        public void SelectMany_LinqSyntax_CombinesOptionals()
        {
            var result =
                from a in Some(3)
                from b in Some(4)
                select a + b;
            Assert.That(result.Match(() => -1, v => v), Is.EqualTo(7));
        }

        [Test]
        public void SelectMany_LinqSyntax_NoneShortCircuits()
        {
            var result =
                from a in Some(3)
                from b in (Optional<int>)None
                select a + b;
            Assert.That(result.IsNone, Is.True);
        }

        [Test]
        public void Apply_BothSome_AppliesFunction()
        {
            Optional<Func<int, string>> funcOpt = Some<Func<int, string>>(x => $"v:{x}");
            var result = funcOpt.Apply(Some(42));
            Assert.That(result.Match(() => "none", v => v), Is.EqualTo("v:42"));
        }

        [Test]
        public void Apply_FuncNone_ReturnsNone()
        {
            Optional<Func<int, string>> funcOpt = None;
            var result = funcOpt.Apply(Some(42));
            Assert.That(result.IsNone, Is.True);
        }

        [Test]
        public void Apply_ArgNone_ReturnsNone()
        {
            Optional<Func<int, string>> funcOpt = Some<Func<int, string>>(x => $"v:{x}");
            Optional<int> argOpt = None;
            var result = funcOpt.Apply(argOpt);
            Assert.That(result.IsNone, Is.True);
        }

        [Test]
        public void Apply_MultiArity_ChainsApplication()
        {
            Func<int, int, int> add = (a, b) => a + b;
            Optional<Func<int, int, int>> funcOpt = Some(add);
            var result = funcOpt
                .Apply(Some(10))
                .Apply(Some(20));
            Assert.That(result.Match(() => -1, v => v), Is.EqualTo(30));
        }

        // ── Optional.Api.cs ──────────────────────────────────────────

        [Test]
        public void HasValue_Some_ReturnsTrueAndSetsOut()
        {
            var opt = Some("abc");
            Assert.That(opt.HasValue(out var val), Is.True);
            Assert.That(val, Is.EqualTo("abc"));
        }

        [Test]
        public void HasValue_None_ReturnsFalse()
        {
            Optional<string> opt = None;
            Assert.That(opt.HasValue(out var val), Is.False);
            Assert.That(val, Is.Null);
        }

        [Test]
        public void ValueUnsafe_Some_ReturnsValue()
        {
            var opt = Some(77);
            Assert.That(opt.ValueUnsafe(), Is.EqualTo(77));
        }

        [Test]
        public void ValueUnsafe_None_ThrowsInvalidOperationException()
        {
            Optional<int> opt = None;
            Assert.Throws<InvalidOperationException>(() => opt.ValueUnsafe());
        }

        [Test]
        public void Then_WithMapFunc_TransformsValue()
        {
            var opt = Some(2);
            var result = opt.Then(x => x * 3);
            Assert.That(result.Match(() => -1, v => v), Is.EqualTo(6));
        }

        [Test]
        public void Then_WithAction_ExecutesSideEffect()
        {
            int captured = 0;
            var opt = Some(5);
            var result = opt.Then(x => captured = x);
            Assert.That(captured, Is.EqualTo(5));
            Assert.That(result.IsSome, Is.True);
        }

        [Test]
        public void Then_WithBindFunc_FlatMaps()
        {
            var opt = Some(8);
            var result = opt.Then(x => x > 5 ? Some($"big:{x}") : (Optional<string>)None);
            Assert.That(result.Match(() => "none", v => v), Is.EqualTo("big:8"));
        }

        [Test]
        public void Or_Some_ReturnsValue()
        {
            var opt = Some(10);
            Assert.That(opt.Or(99), Is.EqualTo(10));
        }

        [Test]
        public void Or_None_ReturnsFallback()
        {
            Optional<int> opt = None;
            Assert.That(opt.Or(99), Is.EqualTo(99));
        }

        [Test]
        public void OrElse_Some_ReturnsValueWithoutCallingFactory()
        {
            bool called = false;
            var opt = Some(10);
            var val = opt.OrElse(() => { called = true; return 99; });
            Assert.That(val, Is.EqualTo(10));
            Assert.That(called, Is.False);
        }

        [Test]
        public void OrElse_None_CallsFactoryAndReturnsFallback()
        {
            Optional<int> opt = None;
            var val = opt.OrElse(() => 99);
            Assert.That(val, Is.EqualTo(99));
        }

        [Test]
        public void Filter_Some_PredicateTrue_RetainsSome()
        {
            var opt = Some(6);
            var filtered = opt.Filter(x => x % 2 == 0);
            Assert.That(filtered.IsSome, Is.True);
            Assert.That(filtered.Match(() => -1, v => v), Is.EqualTo(6));
        }

        [Test]
        public void Filter_Some_PredicateFalse_ReturnsNone()
        {
            var opt = Some(7);
            var filtered = opt.Filter(x => x % 2 == 0);
            Assert.That(filtered.IsNone, Is.True);
        }

        [Test]
        public void Filter_None_ReturnsNone()
        {
            Optional<int> opt = None;
            var filtered = opt.Filter(x => true);
            Assert.That(filtered.IsNone, Is.True);
        }

        [Test]
        public void ToOptional_NullableWithValue_ReturnsSome()
        {
            int? nullable = 42;
            var opt = nullable.ToOptional();
            Assert.That(opt.IsSome, Is.True);
            Assert.That(opt.Match(() => -1, v => v), Is.EqualTo(42));
        }

        [Test]
        public void ToOptional_NullableNull_ReturnsNone()
        {
            int? nullable = null;
            var opt = nullable.ToOptional();
            Assert.That(opt.IsNone, Is.True);
        }

        [Test]
        public void Traverse_Some_MapsToEnumerableOfOptionals()
        {
            var opt = Some(3);
            var result = opt.Traverse(x => Enumerable.Range(1, x)).ToList();
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0].Match(() => -1, v => v), Is.EqualTo(1));
            Assert.That(result[1].Match(() => -1, v => v), Is.EqualTo(2));
            Assert.That(result[2].Match(() => -1, v => v), Is.EqualTo(3));
        }

        [Test]
        public void Traverse_None_ReturnsSingleNone()
        {
            Optional<int> opt = None;
            var result = opt.Traverse(x => Enumerable.Range(1, x)).ToList();
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].IsNone, Is.True);
        }

        // ── State-passing Map / Bind (Optional.Traits.cs) ─────────────

        [Test]
        public void Map_WithState_Some_TransformsValue()
        {
            var opt = Some(5);
            int threshold = 3;
            var result = opt.Map(threshold, static (v, t) => v > t);
            Assert.That(result.IsSome, Is.True);
            Assert.That(result.Match(() => false, v => v), Is.True);
        }

        [Test]
        public void Map_WithState_None_ReturnsNone()
        {
            Optional<int> opt = None;
            var result = opt.Map(10, static (v, t) => v + t);
            Assert.That(result.IsNone, Is.True);
        }

        [Test]
        public void Bind_WithState_Some_FlatMaps()
        {
            var opt = Some(10);
            int multiplier = 3;
            var result = opt.Bind(multiplier, static (v, m) => Some(v * m));
            Assert.That(result.IsSome, Is.True);
            Assert.That(result.Match(() => -1, v => v), Is.EqualTo(30));
        }

        [Test]
        public void Bind_WithState_Some_CanReturnNone()
        {
            var opt = Some(1);
            int threshold = 5;
            var result = opt.Bind(threshold, static (v, t) => v > t ? Some(v) : (Optional<int>)None);
            Assert.That(result.IsNone, Is.True);
        }

        [Test]
        public void Bind_WithState_None_ReturnsNone()
        {
            Optional<int> opt = None;
            var result = opt.Bind(99, static (v, s) => Some(v + s));
            Assert.That(result.IsNone, Is.True);
        }
    }
}
