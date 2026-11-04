#if TUTAN_UNITASK
// Compiled only when UniTask is installed: TUTAN_UNITASK is set by the asmdef versionDefines
// for com.cysharp.unitask (or manually, for UniTask copied into Assets/).
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using static Tutan.Functional.F;

namespace Tutan.Functional.Tests
{
    [TestFixture]
    public class OptionalAsyncTests
    {
        // ── MapAsync A ────────────────────────────────────────────────────────

        [Test]
        public async Task MapAsync_A_OnSome_TransformsValue()
        {
            var result = await Some(5).MapAsync(x => UniTask.FromResult(x * 2));
            Assert.That(result.IsSome, Is.True);
            Assert.That(result._value, Is.EqualTo(10));
        }

        [Test]
        public async Task MapAsync_A_OnNone_ReturnsNone()
        {
            Optional<int> src = None;
            var result = await src.MapAsync(x => UniTask.FromResult(x * 2));
            Assert.That(result.IsNone, Is.True);
        }


        // ── Map B ─────────────────────────────────────────────────────────────

        [Test]
        public async Task Map_B_OnSome_TransformsValue()
        {
            var result = await UniTask.FromResult(Some(3)).Map(x => x + 1);
            Assert.That(result.IsSome, Is.True);
            Assert.That(result._value, Is.EqualTo(4));
        }

        [Test]
        public async Task Map_B_OnNone_ReturnsNone()
        {
            var result = await UniTask.FromResult((Optional<int>)None).Map(x => x + 1);
            Assert.That(result.IsNone, Is.True);
        }


        // ── BindAsync A ───────────────────────────────────────────────────────

        [Test]
        public async Task BindAsync_A_OnSome_FlatMaps()
        {
            var result = await Some(4).BindAsync(x => UniTask.FromResult(Some(x * 3)));
            Assert.That(result.IsSome, Is.True);
            Assert.That(result._value, Is.EqualTo(12));
        }

        [Test]
        public async Task BindAsync_A_FuncReturnsNone_ReturnsNone()
        {
            var result = await Some(4).BindAsync<int, int>(x => UniTask.FromResult((Optional<int>)None));
            Assert.That(result.IsNone, Is.True);
        }

        [Test]
        public async Task BindAsync_A_OnNone_ReturnsNone()
        {
            Optional<int> src = None;
            var result = await src.BindAsync(x => UniTask.FromResult(Some(x)));
            Assert.That(result.IsNone, Is.True);
        }


        // ── ThenAsync / Then aliases ──────────────────────────────────────────

        [Test]
        public async Task ThenAsync_MapAlias_OnSome_TransformsValue()
        {
            var result = await Some(10).ThenAsync(x => UniTask.FromResult(x + 5));
            Assert.That(result.IsSome, Is.True);
            Assert.That(result._value, Is.EqualTo(15));
        }

        [Test]
        public async Task ThenAsync_SideEffect_OnSome_ExecutesAndPassesThrough()
        {
            int captured = 0;
            var result = await Some(99).ThenAsync(x => { captured = x; return UniTask.CompletedTask; });
            Assert.That(captured, Is.EqualTo(99));
            Assert.That(result.IsSome, Is.True);
        }

        [Test]
        public async Task ThenAsync_SideEffect_OnNone_DoesNotExecute()
        {
            bool called = false;
            Optional<int> src = None;
            var result = await src.ThenAsync(x => { called = true; return UniTask.CompletedTask; });
            Assert.That(called, Is.False);
            Assert.That(result.IsNone, Is.True);
        }

        [Test]
        public async Task Then_SyncMap_OnUniTaskOptional_TransformsValue()
        {
            var result = await UniTask.FromResult(Some(6)).Then(x => x * 7);
            Assert.That(result.IsSome, Is.True);
            Assert.That(result._value, Is.EqualTo(42));
        }

        [Test]
        public async Task Then_SyncSideEffect_OnUniTaskOptional_ExecutesAndPassesThrough()
        {
            int captured = 0;
            var result = await UniTask.FromResult(Some(5)).Then(x => { captured = x; });
            Assert.That(captured, Is.EqualTo(5));
            Assert.That(result.IsSome, Is.True);
        }


        // ── MatchAsync ────────────────────────────────────────────────────────

        [Test]
        public async Task MatchAsync_A_OnSome_CallsOnSome()
        {
            var value = await Some(100).MatchAsync(
                onNone: () => UniTask.FromResult("none"),
                onSome: v => UniTask.FromResult($"some:{v}"));
            Assert.That(value, Is.EqualTo("some:100"));
        }

        [Test]
        public async Task MatchAsync_A_OnNone_CallsOnNone()
        {
            Optional<int> src = None;
            var value = await src.MatchAsync(
                onNone: () => UniTask.FromResult("was none"),
                onSome: v => UniTask.FromResult("has value"));
            Assert.That(value, Is.EqualTo("was none"));
        }

        [Test]
        public async Task Match_B_OnSome_CallsOnSome()
        {
            var value = await UniTask.FromResult(Some(42))
                .Match(onNone: () => -1, onSome: v => v);
            Assert.That(value, Is.EqualTo(42));
        }

        [Test]
        public async Task Match_B_OnNone_CallsOnNone()
        {
            var value = await UniTask.FromResult((Optional<int>)None)
                .Match(onNone: () => -1, onSome: v => v);
            Assert.That(value, Is.EqualTo(-1));
        }

        [Test]
        public async Task Match_VoidB_OnSome_CallsOnSome()
        {
            bool someCalled = false;
            await UniTask.FromResult(Some(1))
                .Match(onNone: () => { }, onSome: v => { someCalled = true; });
            Assert.That(someCalled, Is.True);
        }

        [Test]
        public async Task Match_VoidB_OnNone_CallsOnNone()
        {
            bool noneCalled = false;
            await UniTask.FromResult((Optional<int>)None)
                .Match(onNone: () => { noneCalled = true; }, onSome: v => { });
            Assert.That(noneCalled, Is.True);
        }


        // ── Chained pipeline ──────────────────────────────────────────────────

        [Test]
        public async Task Pipeline_ChainedThenAndMatch_ProducesCorrectResult()
        {
            var output = await UniTask.FromResult(Some(5))
                .Then(x => x * 2)
                .ThenAsync(x => UniTask.FromResult(x + 1))
                .Match(onNone: () => -1, onSome: v => v);

            Assert.That(output, Is.EqualTo(11));
        }

        [Test]
        public async Task Pipeline_NoneShortCircuits_SkipsRemainingSteps()
        {
            bool step2Called = false;

            var output = await UniTask.FromResult((Optional<int>)None)
                .ThenAsync(x => { step2Called = true; return UniTask.FromResult(x + 1); })
                .Match(onNone: () => -1, onSome: v => v);

            Assert.That(step2Called, Is.False);
            Assert.That(output, Is.EqualTo(-1));
        }
    }
}
#endif
