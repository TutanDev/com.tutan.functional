using NUnit.Framework;
using static Tutan.Functional.F;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tutan.Functional.Tests
{
    [TestFixture]
    public class ResultTests
    {
        // ──────────────────────────────────────────────
        // Result<T> struct  (Result.cs)
        // ──────────────────────────────────────────────

        [Test]
        public void Success_WithValue_IsSuccess()
        {
            var result = Success(42);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsError, Is.False);
        }

        [Test]
        public void Success_WithValue_StoresValue()
        {
            var result = Success("hello");
            Assert.That(result.ValueUnsafe(), Is.EqualTo("hello"));
        }

        [Test]
        public void Error_Result_IsError()
        {
            Result<int> result = Error("boom");
            Assert.That(result.IsError, Is.True);
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void Error_Result_StoresError()
        {
            Result<int> result = Error("boom");
            Assert.That(result.ErrorUnsafe().Message, Is.EqualTo("boom"));
        }

        [Test]
        public void ImplicitConversion_FromValue_CreatesSuccess()
        {
            Result<int> result = 42;
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ValueUnsafe(), Is.EqualTo(42));
        }

        [Test]
        public void ImplicitConversion_FromError_CreatesError()
        {
            Result<int> result = new Error("fail");
            Assert.That(result.IsError, Is.True);
            Assert.That(result.ErrorUnsafe().Message, Is.EqualTo("fail"));
        }

        [Test]
        public void Match_OnSuccess_CallsOnSuccess()
        {
            var result = Success(10);
            var matched = result.Match(
                onError: e => -1,
                onSuccess: v => v * 2);
            Assert.That(matched, Is.EqualTo(20));
        }

        [Test]
        public void Match_OnError_CallsOnError()
        {
            Result<int> result = Error("oops");
            var matched = result.Match(
                onError: e => e.Message,
                onSuccess: v => v.ToString());
            Assert.That(matched, Is.EqualTo("oops"));
        }

        [Test]
        public void Match_ActionOverload_OnSuccess_CallsOnSuccess()
        {
            var result = Success(5);
            var called = false;
            result.Match(
                onError: e => { },
                onSuccess: v => { called = true; });
            Assert.That(called, Is.True);
        }

        [Test]
        public void Match_ActionOverload_OnError_CallsOnError()
        {
            Result<int> result = Error("err");
            string captured = null;
            result.Match(
                onError: e => { captured = e.Message; },
                onSuccess: v => { });
            Assert.That(captured, Is.EqualTo("err"));
        }

        [Test]
        public void ToString_Success_FormatsCorrectly()
        {
            var result = Success(42);
            Assert.That(result.ToString(), Is.EqualTo("Success: 42"));
        }

        [Test]
        public void ToString_Error_FormatsCorrectly()
        {
            Result<int> result = Error("broken");
            Assert.That(result.ToString(), Is.EqualTo("Error: broken"));
        }

        [Test]
        public void ValueUnsafe_OnSuccess_ReturnsValue()
        {
            var result = Success(99);
            Assert.That(result.ValueUnsafe(), Is.EqualTo(99));
        }

        [Test]
        public void ErrorUnsafe_OnError_ReturnsError()
        {
            Result<int> result = Error("msg");
            Assert.That(result.ErrorUnsafe().Message, Is.EqualTo("msg"));
        }

        // ──────────────────────────────────────────────
        // Result.Traits.cs  (ResultExtensions)
        // ──────────────────────────────────────────────

        [Test]
        public void AsEnumerable_Success_ReturnsSingleItem()
        {
            var result = Success(7);
            var items = result.AsEnumerable().ToList();
            Assert.That(items.Count, Is.EqualTo(1));
            Assert.That(items[0], Is.EqualTo(7));
        }

        [Test]
        public void AsEnumerable_Error_ReturnsEmpty()
        {
            Result<int> result = Error("nope");
            var items = result.AsEnumerable().ToList();
            Assert.That(items, Is.Empty);
        }

        [Test]
        public void ToOptional_Success_ReturnsSome()
        {
            var result = Success(3);
            var opt = result.ToOptional();
            Assert.That(opt.IsSome, Is.True);
            Assert.That(opt.Match(() => -1, v => v), Is.EqualTo(3));
        }

        [Test]
        public void ToOptional_Error_ReturnsNone()
        {
            Result<int> result = Error("x");
            var opt = result.ToOptional();
            Assert.That(opt.IsNone, Is.True);
        }

        [Test]
        public void Map_OnSuccess_TransformsValue()
        {
            var result = Success(5);
            var mapped = result.Map(x => x * 3);
            Assert.That(mapped.IsSuccess, Is.True);
            Assert.That(mapped.ValueUnsafe(), Is.EqualTo(15));
        }

        [Test]
        public void Map_OnError_PropagatesError()
        {
            Result<int> result = Error("err");
            var mapped = result.Map(x => x * 3);
            Assert.That(mapped.IsError, Is.True);
            Assert.That(mapped.ErrorUnsafe().Message, Is.EqualTo("err"));
        }

        [Test]
        public void Map_MultiArity2_CurriesFunction()
        {
            var result = Success(2);
            Func<int, int, int> add = (a, b) => a + b;
            var partial = result.Map(add);
            Assert.That(partial.IsSuccess, Is.True);
            var final = partial.Apply(Success(3));
            Assert.That(final.ValueUnsafe(), Is.EqualTo(5));
        }

        [Test]
        public void Map_MultiArity3_CurriesFunction()
        {
            var result = Success(1);
            Func<int, int, int, int> add3 = (a, b, c) => a + b + c;
            var partial = result.Map(add3);
            Assert.That(partial.IsSuccess, Is.True);
        }

        [Test]
        public void ForEach_OnSuccess_ExecutesAction()
        {
            var result = Success(10);
            int captured = 0;
            result.ForEach(v => { captured = v; });
            Assert.That(captured, Is.EqualTo(10));
        }

        [Test]
        public void ForEach_OnError_DoesNotExecuteAction()
        {
            Result<int> result = Error("nah");
            bool called = false;
            result.ForEach(v => { called = true; });
            Assert.That(called, Is.False);
        }

        [Test]
        public void Bind_OnSuccess_FlatMaps()
        {
            var result = Success(10);
            var bound = result.Bind(v => Success(v + 5));
            Assert.That(bound.IsSuccess, Is.True);
            Assert.That(bound.ValueUnsafe(), Is.EqualTo(15));
        }

        [Test]
        public void Bind_OnSuccess_CanReturnError()
        {
            var result = Success(10);
            var bound = result.Bind<int, int>(v => Error("computed error"));
            Assert.That(bound.IsError, Is.True);
            Assert.That(bound.ErrorUnsafe().Message, Is.EqualTo("computed error"));
        }

        [Test]
        public void Bind_OnError_PropagatesError()
        {
            Result<int> result = Error("original");
            var bound = result.Bind(v => Success(v + 1));
            Assert.That(bound.IsError, Is.True);
            Assert.That(bound.ErrorUnsafe().Message, Is.EqualTo("original"));
        }

        [Test]
        public void Bind_ToEnumerable_OnSuccess_ReturnsItems()
        {
            var result = Success(3);
            var items = result.Bind(v => Enumerable.Range(0, v)).ToList();
            Assert.That(items, Is.EqualTo(new List<int> { 0, 1, 2 }));
        }

        [Test]
        public void Bind_ToEnumerable_OnError_ReturnsEmpty()
        {
            Result<int> result = Error("x");
            var items = result.Bind(v => Enumerable.Range(0, v)).ToList();
            Assert.That(items, Is.Empty);
        }

        [Test]
        public void Select_LINQ_TransformsValue()
        {
            var result = Success(4);
            var selected = from x in result select x * 10;
            Assert.That(selected.IsSuccess, Is.True);
            Assert.That(selected.ValueUnsafe(), Is.EqualTo(40));
        }

        [Test]
        public void SelectMany_LINQ_ComposesResults()
        {
            var a = Success(3);
            var b = Success(7);
            var combined =
                from x in a
                from y in b
                select x + y;
            Assert.That(combined.IsSuccess, Is.True);
            Assert.That(combined.ValueUnsafe(), Is.EqualTo(10));
        }

        [Test]
        public void SelectMany_LINQ_PropagatesError()
        {
            var a = Success(3);
            Result<int> b = Error("fail");
            var combined =
                from x in a
                from y in b
                select x + y;
            Assert.That(combined.IsError, Is.True);
        }

        [Test]
        public void Apply_OnSuccess_AppliesFunction()
        {
            Func<int, string> fn = x => $"val={x}";
            Result<Func<int, string>> wrappedFn = Success(fn);
            var applied = wrappedFn.Apply(Success(42));
            Assert.That(applied.IsSuccess, Is.True);
            Assert.That(applied.ValueUnsafe(), Is.EqualTo("val=42"));
        }

        [Test]
        public void Apply_ErrorFunction_PropagatesError()
        {
            Result<Func<int, string>> wrappedFn = Error("no fn");
            var applied = wrappedFn.Apply(Success(42));
            Assert.That(applied.IsError, Is.True);
            Assert.That(applied.ErrorUnsafe().Message, Is.EqualTo("no fn"));
        }

        [Test]
        public void Apply_ErrorArg_PropagatesError()
        {
            Func<int, string> fn = x => $"val={x}";
            Result<Func<int, string>> wrappedFn = Success(fn);
            Result<int> arg = Error("bad arg");
            var applied = wrappedFn.Apply(arg);
            Assert.That(applied.IsError, Is.True);
            Assert.That(applied.ErrorUnsafe().Message, Is.EqualTo("bad arg"));
        }

        [Test]
        public void Apply_MultiArity_ChainsPartialApplication()
        {
            Func<int, int, int> add = (a, b) => a + b;
            var result = Success(add)
                .Apply(Success(10))
                .Apply(Success(20));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ValueUnsafe(), Is.EqualTo(30));
        }

        // ──────────────────────────────────────────────
        // Result.Api.cs  (ResultExtensions)
        // ──────────────────────────────────────────────

        [Test]
        public void Then_FuncTR_OnSuccess_TransformsValue()
        {
            var result = Success(2);
            var then = result.Then(x => x * 5);
            Assert.That(then.IsSuccess, Is.True);
            Assert.That(then.ValueUnsafe(), Is.EqualTo(10));
        }

        [Test]
        public void Then_FuncTR_OnError_PropagatesError()
        {
            Result<int> result = Error("e");
            var then = result.Then(x => x * 5);
            Assert.That(then.IsError, Is.True);
        }

        [Test]
        public void Then_Action_OnSuccess_ExecutesSideEffect()
        {
            var result = Success(8);
            int captured = 0;
            var returned = result.Then(v => { captured = v; });
            Assert.That(captured, Is.EqualTo(8));
            Assert.That(returned.IsSuccess, Is.True);
            Assert.That(returned.ValueUnsafe(), Is.EqualTo(8));
        }

        [Test]
        public void Then_FuncBind_OnSuccess_FlatMaps()
        {
            var result = Success(6);
            var then = result.Then(v => Success(v.ToString()));
            Assert.That(then.IsSuccess, Is.True);
            Assert.That(then.ValueUnsafe(), Is.EqualTo("6"));
        }

        [Test]
        public void Or_OnSuccess_ReturnsValue()
        {
            var result = Success(42);
            Assert.That(result.Or(0), Is.EqualTo(42));
        }

        [Test]
        public void Or_OnError_ReturnsFallback()
        {
            Result<int> result = Error("err");
            Assert.That(result.Or(99), Is.EqualTo(99));
        }

        [Test]
        public void OrElse_OnSuccess_ReturnsValue()
        {
            var result = Success(42);
            Assert.That(result.OrElse(e => 0), Is.EqualTo(42));
        }

        [Test]
        public void OrElse_OnError_CallsFallbackWithError()
        {
            Result<string> result = Error("reason");
            var value = result.OrElse(e => $"fallback: {e.Message}");
            Assert.That(value, Is.EqualTo("fallback: reason"));
        }

        [Test]
        public void Filter_OnSuccess_PredicateTrue_KeepsValue()
        {
            var result = Success(10);
            var filtered = result.Filter(x => x > 5);
            Assert.That(filtered.IsSuccess, Is.True);
            Assert.That(filtered.ValueUnsafe(), Is.EqualTo(10));
        }

        [Test]
        public void Filter_OnSuccess_PredicateFalse_BecomesError()
        {
            var result = Success(2);
            var filtered = result.Filter(x => x > 5);
            Assert.That(filtered.IsError, Is.True);
        }

        [Test]
        public void Filter_OnError_PreservesOriginalError()
        {
            Result<int> result = Error("original error");
            var filtered = result.Filter(x => x > 5);
            Assert.That(filtered.IsError, Is.True);
            Assert.That(filtered.ErrorUnsafe().Message, Is.EqualTo("original error"));
        }

        [Test]
        public void IsSuccess_OutParam_OnSuccess_ReturnsTrueAndValue()
        {
            var result = Success(77);
            var success = result.IsSuccess(out var value);
            Assert.That(success, Is.True);
            Assert.That(value, Is.EqualTo(77));
        }

        [Test]
        public void IsSuccess_OutParam_OnError_ReturnsFalseAndDefault()
        {
            Result<int> result = Error("err");
            var success = result.IsSuccess(out var value);
            Assert.That(success, Is.False);
            Assert.That(value, Is.EqualTo(default(int)));
        }

        [Test]
        public void ValueUnsafe_Extension_OnSuccess_ReturnsValue()
        {
            var result = Success(55);
            Assert.That(result.ValueUnsafe(), Is.EqualTo(55));
        }

        [Test]
        public void ValueUnsafe_Extension_OnError_ThrowsInvalidOperationException()
        {
            Result<int> result = Error("nope");
            Assert.Throws<InvalidOperationException>(() => result.ValueUnsafe());
        }

        [Test]
        public void ErrorUnsafe_Extension_OnError_ReturnsError()
        {
            Result<int> result = Error("msg");
            Assert.That(result.ErrorUnsafe().Message, Is.EqualTo("msg"));
        }

        [Test]
        public void ErrorUnsafe_Extension_OnSuccess_ThrowsInvalidOperationException()
        {
            var result = Success(1);
            Assert.Throws<InvalidOperationException>(() => result.ErrorUnsafe());
        }

        // ── State-passing Map / Bind (Result.Traits.cs) ───────────────

        [Test]
        public void Map_WithState_OnSuccess_TransformsValue()
        {
            var result = Success(4);
            int factor = 5;
            var mapped = result.Map(factor, static (v, f) => v * f);
            Assert.That(mapped.IsSuccess, Is.True);
            Assert.That(mapped.ValueUnsafe(), Is.EqualTo(20));
        }

        [Test]
        public void Map_WithState_OnError_PropagatesError()
        {
            Result<int> result = Error("fail");
            var mapped = result.Map(10, static (v, s) => v + s);
            Assert.That(mapped.IsError, Is.True);
            Assert.That(mapped.ErrorUnsafe().Message, Is.EqualTo("fail"));
        }

        [Test]
        public void Bind_WithState_OnSuccess_FlatMaps()
        {
            var result = Success(6);
            int offset = 4;
            var bound = result.Bind(offset, static (v, o) => Success(v + o));
            Assert.That(bound.IsSuccess, Is.True);
            Assert.That(bound.ValueUnsafe(), Is.EqualTo(10));
        }

        [Test]
        public void Bind_WithState_OnSuccess_CanReturnError()
        {
            var result = Success(1);
            var bound = result.Bind("state", static (v, s) => (Result<int>)Error("computed"));
            Assert.That(bound.IsError, Is.True);
            Assert.That(bound.ErrorUnsafe().Message, Is.EqualTo("computed"));
        }

        [Test]
        public void Bind_WithState_OnError_PropagatesError()
        {
            Result<int> result = Error("original");
            var bound = result.Bind(99, static (v, s) => Success(v + s));
            Assert.That(bound.IsError, Is.True);
            Assert.That(bound.ErrorUnsafe().Message, Is.EqualTo("original"));
        }

        // ── State-passing Match / Then / Filter ───────────────────────

        [Test]
        public void Match_WithState_OnSuccess_UsesValue()
        {
            var result = Success(7);
            int offset = 3;
            int matched = result.Match(offset, static (e, o) => -o, static (v, o) => v + o);
            Assert.That(matched, Is.EqualTo(10));
        }

        [Test]
        public void Match_WithState_OnError_PassesStateAndError()
        {
            Result<int> result = Error("boom");
            string prefix = "got: ";
            string matched = result.Match(prefix, static (e, p) => p + e.Message, static (v, p) => p + v);
            Assert.That(matched, Is.EqualTo("got: boom"));
        }

        [Test]
        public void Match_WithState_ActionVariant_OnError_RunsOnError()
        {
            Result<int> result = Error("boom");
            var sink = new List<string>();
            result.Match(sink, static (e, s) => s.Add(e.Message), static (v, s) => s.Add(v.ToString()));
            Assert.That(sink, Is.EqualTo(new[] { "boom" }));
        }

        [Test]
        public void Then_WithState_OnSuccess_Maps()
        {
            var result = Success(4);
            int factor = 5;
            var mapped = result.Then(factor, static (v, f) => v * f);
            Assert.That(mapped.ValueUnsafe(), Is.EqualTo(20));
        }

        [Test]
        public void Then_WithState_OnSuccess_Binds()
        {
            var result = Success(4);
            int threshold = 3;
            var bound = result.Then(threshold, static (v, t) => v > t ? Success(v) : (Result<int>)Error("too small"));
            Assert.That(bound.IsSuccess, Is.True);
        }

        [Test]
        public void Then_WithState_OnSuccess_RunsActionAndPassesThrough()
        {
            var result = Success(9);
            var sink = new List<int>();
            var passed = result.Then(sink, static (v, s) => s.Add(v));
            Assert.That(sink, Is.EqualTo(new[] { 9 }));
            Assert.That(passed.ValueUnsafe(), Is.EqualTo(9));
        }

        [Test]
        public void Then_WithState_OnError_SkipsActionAndPropagates()
        {
            Result<int> result = Error("fail");
            var sink = new List<int>();
            var passed = result.Then(sink, static (v, s) => s.Add(v));
            Assert.That(sink, Is.Empty);
            Assert.That(passed.ErrorUnsafe().Message, Is.EqualTo("fail"));
        }

        [Test]
        public void Filter_WithState_PredicateTrue_KeepsSuccess()
        {
            var result = Success(8);
            int min = 5;
            var filtered = result.Filter(min, static (v, m) => v >= m);
            Assert.That(filtered.IsSuccess, Is.True);
        }

        [Test]
        public void Filter_WithState_PredicateFalse_ReturnsError()
        {
            var result = Success(2);
            int min = 5;
            var filtered = result.Filter(min, static (v, m) => v >= m);
            Assert.That(filtered.IsError, Is.True);
            Assert.That(filtered.ErrorUnsafe().Message, Is.EqualTo("Predicate not satisfied"));
        }

        [Test]
        public void Filter_WithState_OnError_PropagatesOriginalError()
        {
            Result<int> result = Error("original");
            var filtered = result.Filter(0, static (v, m) => true);
            Assert.That(filtered.IsError, Is.True);
            Assert.That(filtered.ErrorUnsafe().Message, Is.EqualTo("original"));
        }
    }
}
