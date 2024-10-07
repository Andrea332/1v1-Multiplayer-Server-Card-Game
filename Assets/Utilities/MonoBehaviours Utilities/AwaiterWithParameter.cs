using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Utilities
{
    public class AwaiterWithParameter : MonoBehaviour
    {
        [SerializeField] private float secondsToAwait;
        public UnityEvent<object> awaitStart;
        public UnityEvent<object> awaitEnd;
        public UnityEvent<OperationCanceledException> awaitStopped;
        private CancellationTokenSource _cancellationTokenSource;

        public async UniTaskVoid AwaitAndDoUniTask(CancellationToken cancellationToken, object parameter)
        {
            try
            {
                awaitStart?.Invoke(parameter);
                await UniTask.Delay(TimeSpan.FromSeconds(secondsToAwait), ignoreTimeScale: true, cancellationToken: cancellationToken);
                awaitEnd?.Invoke(parameter);
            }
            catch (OperationCanceledException ex)
            {
                awaitStopped?.Invoke(ex);
            }
        }
 
        public void AwaitAndDo(object parameter)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            AwaitAndDoUniTask(_cancellationTokenSource.Token, parameter).Forget();
        }

        public void StopAwaitAndDo()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
