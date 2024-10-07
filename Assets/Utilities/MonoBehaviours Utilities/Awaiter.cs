using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Utilities
{
    public class Awaiter : MonoBehaviour
    {
        public UnityEvent awaitStart;
        public UnityEvent awaitEnd;
        public UnityEvent<OperationCanceledException> awaitStopped;
        private CancellationTokenSource _cancellationTokenSource;

        public async UniTaskVoid AwaitAndDoUniTask(CancellationToken cancellationToken, float secondsToAwait)
        {
            try
            {
                awaitStart?.Invoke();
                await UniTask.Delay(TimeSpan.FromSeconds(secondsToAwait), ignoreTimeScale: true, cancellationToken: cancellationToken);
                awaitEnd?.Invoke();
            }
            catch (OperationCanceledException ex)
            {
                awaitStopped?.Invoke(ex);
            }
        }
 
        public void AwaitAndDo(float secondsToAwait)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            AwaitAndDoUniTask(_cancellationTokenSource.Token, secondsToAwait).Forget();
        }

        public void StopAwaitAndDo()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
