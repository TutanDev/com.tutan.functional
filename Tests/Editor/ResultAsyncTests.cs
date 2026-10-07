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
    public class ResultAsyncTests
    {
        // ── TryAsync ─────────────────────────────────────────────────────────

        [Test]
        public async Task TryAsync_T_Success_ReturnsSuccessResult()
        {
            var result = await TryAsync(() => UniTask.FromResult(42));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ValueUnsafe(), Is.EqualTo(42));
        }

        [Test]
        public async Task TryAsync_T_Throws_ReturnsErrorResult()
        {
            var result = await TryAsync<int>(() => throw new InvalidOperationException("boom"));
            Assert.That(result.IsError, Is.True);
            StringAssert.Contains("boom", result.ErrorUnsafe().Message);
        }

        [Test]
        public async Task TryAsync_Action_Success_ReturnsSuccessUnit()
        {
            var result = await TryAsync(() => UniTask.CompletedTask);
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public async Task TryAsync_Action_Throws_ReturnsError()
        {
            var result = await TryAsync(() => throw new Exception("fail"));
            Assert.That(result.IsError, Is.True);
            StringAssert.Contains("fail", result.ErrorUnsafe().Message);
        }


        // ── MapAsync A (Result<T> → async func) ──────────────────────────────

        [Test]
        public async Task MapAsync_A_OnSuccess_TransformsValue()
        {
            var result = await Success(5).MapAsync(x => UniTask.FromResult(x * 2));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ValueUnsafe(), Is.EqualTo(10));
        }

        [Test]
        public async Task MapAsync_A_OnError_PropagatesError()
        {
            Result<int> src = Error("err");
            var result = await src.MapAsync(x => UniTask.FromResult(x * 2));
            Assert.That(result.IsError, Is.True);
            Assert.That(result.ErrorUnsafe().Message, Is.EqualTo("err"));
        }


        // ── Map B (UniTask<Result<T>> → sync func) ───────────────────────────

        [Test]
        public async Task Map_B_OnSuccess_TransformsValue()
        {
            var result = await UniTask.FromResult(Success(3)).Map(x => x + 1);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ValueUnsafe(), Is.EqualTo(4));
        }

        [Test]
        public async Task Map_B_OnError_PropagatesError()
        {
            var src = UniTask.FromResult((Result<int>)Error("e"));
            var result = await src.Map(x => x + 1);
            Assert.That(result.IsError, Is.True);
        }


        // ── MapAsync C (UniTask<Result<T>> → async func) ─────────────────────

        [Test]
        public async Task MapAsync_C_OnSuccess_TransformsValue()
        {
            var result = await UniTask.FromResult(Success(7))
                .MapAsync(x => UniTask.FromResult(x.ToString()));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ValueUnsafe(), Is.EqualTo("7"));
        }


        // ── BindAsync A ───────────────────────────────────────────────────────

        [Test]
        public async Task BindAsync_A_OnSuccess_FlatMaps()
        {
            var result = await Success(4)
                .BindAsync(x => UniTask.FromResult(Success(x * 3)));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ValueUnsafe(), Is.EqualTo(12));
        }

        [Test]
        public async Task BindAsync_A_FuncReturnsError_PropagatesError()
        {
            var result = await Success(4)
                .BindAsync<int, int>(x => UniTask.FromResult((Result<int>)Error("inner")));
            Assert.That(result.IsError, Is.True);
            Assert.That(result.ErrorUnsafe().Message, Is.EqualTo("inner"));
        }

        [Test]
        public async Task BindAsync_A_OnError_PropagatesError()
        {
            Result<int> src = Error("outer");
            var result = await src.BindAsync(x => UniTask.FromResult(Success(x)));
            Assert.That(result.IsError, Is.True);
            Assert.That(result.ErrorUnsafe().Message, Is.EqualTo("outer"));
        }


        // ── Then aliases ─────────────────────────────────────────────────────

        [Test]
        public async Task ThenAsync_MapAlias_OnSuccess_TransformsValue()
        {
            var result = await Success(10)
                .ThenAsync(x => UniTask.FromResult(x + 5));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ValueUnsafe(), Is.EqualTo(15));
        }

        [Test]
        public async Task ThenAsync_BindAlias_OnSuccess_FlatMaps()
        {
            var result = await Success(10)
                .ThenAsync(x => UniTask.FromResult(Success(x.ToString())));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ValueUnsafe(), Is.EqualTo("10"));
        }

        [Test]
        public async Task ThenAsync_SideEffect_OnSuccess_ExecutesAndPassesThrough()
        {
            int captured = 0;
            var result = await Success(99)
                .ThenAsync(x => { captured = x; return UniTask.CompletedTask; });
            Assert.That(captured, Is.EqualTo(99));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ValueUnsafe(), Is.EqualTo(99));
        }

        [Test]
        public async Task ThenAsync_SideEffect_OnError_DoesNotExecute()
        {
            bool called = false;
            Result<int> src = Error("e");
            var result = await src.ThenAsync(x => { called = true; return UniTask.CompletedTask; });
            Assert.That(called, Is.False);
            Assert.That(result.IsError, Is.True);
        }

        [Test]
        public async Task Then_SyncMap_OnUniTaskResult_TransformsValue()
        {
            var result = await UniTask.FromResult(Success(6))
                .Then(x => x * 7);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ValueUnsafe(), Is.EqualTo(42));
        }

        [Test]
        public async Task Then_SyncBind_OnUniTaskResult_FlatMaps()
        {
            var result = await UniTask.FromResult(Success(3))
                .Then(x => Success(x + 1));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ValueUnsafe(), Is.EqualTo(4));
        }

        [Test]
        public async Task Then_SyncSideEffect_OnUniTaskResult_ExecutesAndPassesThrough()
        {
            int captured = 0;
            var result = await UniTask.FromResult(Success(5))
                .Then(x => { captured = x; });
            Assert.That(captured, Is.EqualTo(5));
            Assert.That(result.ValueUnsafe(), Is.EqualTo(5));
        }

        [Test]
        public async Task ThenAsync_AsyncMap_OnUniTaskResult_TransformsValue()
        {
            var result = await UniTask.FromResult(Success(2))
                .ThenAsync(x => UniTask.FromResult(x * 10));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ValueUnsafe(), Is.EqualTo(20));
        }

        [Test]
        public async Task ThenAsync_AsyncBind_OnUniTaskResult_FlatMaps()
        {
            var result = await UniTask.FromResult(Success(5))
                .ThenAsync(x => UniTask.FromResult(Success(x - 1)));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ValueUnsafe(), Is.EqualTo(4));
        }

        [Test]
        public async Task ThenAsync_AsyncSideEffect_OnUniTaskResult_ExecutesAndPassesThrough()
        {
            int captured = 0;
            var result = await UniTask.FromResult(Success(8))
                .ThenAsync(x => { captured = x; return UniTask.CompletedTask; });
            Assert.That(captured, Is.EqualTo(8));
            Assert.That(result.ValueUnsafe(), Is.EqualTo(8));
        }


        // ── MatchAsync ───────────────────────────────────────────────────────

        [Test]
        public async Task MatchAsync_A_OnSuccess_CallsOnSuccess()
        {
            var value = await Success(100).MatchAsync(
                onError: e => UniTask.FromResult("error"),
                onSuccess: v => UniTask.FromResult($"ok:{v}"));
            Assert.That(value, Is.EqualTo("ok:100"));
        }

        [Test]
        public async Task MatchAsync_A_OnError_CallsOnError()
        {
            Result<int> src = Error("bad");
            var value = await src.MatchAsync(
                onError: e => UniTask.FromResult(e.Message),
                onSuccess: v => UniTask.FromResult("ok"));
            Assert.That(value, Is.EqualTo("bad"));
        }

        [Test]
        public async Task Match_B_OnSuccess_CallsOnSuccess()
        {
            var value = await UniTask.FromResult(Success(42))
                .Match(onError: e => -1, onSuccess: v => v);
            Assert.That(value, Is.EqualTo(42));
        }

        [Test]
        public async Task Match_B_OnError_CallsOnError()
        {
            var value = await UniTask.FromResult((Result<int>)Error("x"))
                .Match(onError: e => -1, onSuccess: v => v);
            Assert.That(value, Is.EqualTo(-1));
        }

        [Test]
        public async Task MatchAsync_C_OnSuccess_CallsOnSuccess()
        {
            var value = await UniTask.FromResult(Success(7)).MatchAsync(
                onError: e => UniTask.FromResult("err"),
                onSuccess: v => UniTask.FromResult($"val:{v}"));
            Assert.That(value, Is.EqualTo("val:7"));
        }

        [Test]
        public async Task Match_VoidB_OnSuccess_CallsOnSuccess()
        {
            bool successCalled = false;
            await UniTask.FromResult(Success(1))
                .Match(onError: e => { }, onSuccess: v => { successCalled = true; });
            Assert.That(successCalled, Is.True);
        }

        [Test]
        public async Task Match_VoidB_OnError_CallsOnError()
        {
            bool errorCalled = false;
            await UniTask.FromResult((Result<int>)Error("e"))
                .Match(onError: e => { errorCalled = true; }, onSuccess: v => { });
            Assert.That(errorCalled, Is.True);
        }


        // ── Chained pipeline (integration) ───────────────────────────────────

        [Test]
        public async Task Pipeline_ChainedThenAndMatch_ProducesCorrectResult()
        {
            var output = await TryAsync(() => UniTask.FromResult(5))
                .Then(x => x * 2)
                .ThenAsync(x => UniTask.FromResult(x + 1))
                .Match(onError: e => -1, onSuccess: v => v);

            Assert.That(output, Is.EqualTo(11));
        }

        [Test]
        public async Task Pipeline_ErrorShortCircuits_SkipsRemainingSteps()
        {
            bool step2Called = false;

            var output = await TryAsync<int>(() => throw new Exception("fail"))
                .ThenAsync(x => { step2Called = true; return UniTask.FromResult(x + 1); })
                .Match(onError: e => e.Message, onSuccess: v => "ok");

            Assert.That(step2Called, Is.False);
            StringAssert.Contains("fail", output);
        }
    }
}
#endif
