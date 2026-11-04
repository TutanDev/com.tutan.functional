#if TUTAN_UNITASK
// Compiled only when UniTask is installed: TUTAN_UNITASK is set by the asmdef versionDefines
// for com.cysharp.unitask (or manually, for UniTask copied into Assets/).
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Tutan.Functional.Tests
{
    // Covers only the synchronous paths: completing a WaitUntil needs a PlayerLoop tick,
    // which EditMode tests do not drive deterministically.
    [TestFixture]
    public class UniTaskFTests
    {
        private static async UniTaskVoid Record(List<int> sink, int a, int b)
        {
            sink.Add(a + b);
            await UniTask.CompletedTask;
        }

        [Test]
        public void Void_PassesArgumentsAndRunsSynchronousPart()
        {
            var sink = new List<int>();
            UniTaskF.Void(Record, sink, 1, 2);
            Assert.That(sink, Is.EqualTo(new[] { 3 }));
        }

        [Test]
        public void WaitUntil_WithAlreadyCancelledToken_ReturnsCanceledTask()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var task = UniTaskF.WaitUntil(static _ => false, 0, cancellationToken: cts.Token);

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));
        }
    }
}
#endif
