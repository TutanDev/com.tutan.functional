using System;
using System.Threading;

namespace Cysharp.Threading.Tasks
{
    public partial struct UniTask
    {
        public static void Void<T1, T2>(Func<T1, T2, UniTaskVoid> asyncAction, T1 arg1, T2 arg2)
            => asyncAction(arg1, arg2).Forget();

        public static void Void<T1, T2, T3>(Func<T1, T2, T3, UniTaskVoid> asyncAction, T1 arg1, T2 arg2, T3 arg3)
            => asyncAction(arg1, arg2, arg3).Forget();

        public static void Void<T1, T2, T3, T4>(Func<T1, T2, T3, T4, UniTaskVoid> asyncAction, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            => asyncAction(arg1, arg2, arg3, arg4).Forget();

        public static void Void<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, UniTaskVoid> asyncAction, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
            => asyncAction(arg1, arg2, arg3, arg4, arg5).Forget();

        public static UniTask WaitUntil<TState>(Func<TState, bool> predicate,
                                                TState state,
                                                PlayerLoopTiming timing = PlayerLoopTiming.Update,
                                                CancellationToken cancellationToken = default)
            => new UniTask(WaitUntilPromiseCustom<TState>.Create(predicate, state, timing, cancellationToken, out var token), token);

        sealed class WaitUntilPromiseCustom<T> : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<WaitUntilPromiseCustom<T>>
        {
            static TaskPool<WaitUntilPromiseCustom<T>> pool;
            WaitUntilPromiseCustom<T> nextNode;
            public ref WaitUntilPromiseCustom<T> NextNode => ref nextNode;

            static WaitUntilPromiseCustom()
            {
                TaskPool.RegisterSizeGetter(typeof(WaitUntilPromiseCustom<T>), () => pool.Size);
            }

            Func<T, bool> predicate;

            // Note: similar promises like WaitUntilValueChanged wraps its state with a WeakReference instance,
            // automatically cancelling the operation if the state is garbage collected. We do not do this.
            // This should be handled through the CancellationToken.
            T state;
            CancellationToken cancellationToken;

            UniTaskCompletionSourceCore<object> core;

            WaitUntilPromiseCustom()
            {
            }

            public static IUniTaskSource Create(Func<T, bool> predicate, T state, PlayerLoopTiming timing, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new WaitUntilPromiseCustom<T>();
                }

                result.predicate = predicate;
                result.cancellationToken = cancellationToken;
                result.state = state;

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            public void GetResult(short token)
            {
                try
                {
                    core.GetResult(token);
                }
                finally
                {
                    TryReturn();
                }
            }

            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                try
                {
                    if (!predicate(state))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    core.TrySetException(ex);
                    return false;
                }

                core.TrySetResult(null);
                return false;
            }

            bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                predicate = default;
                cancellationToken = default;
                return pool.TryPush(this);
            }
        }
    }
}
