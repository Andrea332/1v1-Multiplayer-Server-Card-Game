using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace Game
{
    public class TimelineController : MonoBehaviour
    {
        [SerializeField] private PlayableDirector playableDirector;
        public float timelineProgressSpeed = 1;
        private double _maxAnimationProgress;
        private CancellationTokenSource _cancellationTokenSource;

        private void Start()
        {
            _maxAnimationProgress = playableDirector.playableAsset.duration;
        }
    
        [ContextMenu("StartOrResumeTimeline")]
        public void StartOrResumeTimeline()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            StartOrResumeTimelineTask(_cancellationTokenSource.Token).Forget();
        }
    
        [ContextMenu("StartOrResumeReverseTimeline")]
        public void StartOrResumeReverseTimeline()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            StartOrResumeTimelineReverseTask(_cancellationTokenSource.Token).Forget();
        }
        private async UniTaskVoid StartOrResumeTimelineTask(CancellationToken cancellationToken)
        {
            while (playableDirector.time < _maxAnimationProgress)
            {
                await UniTask.Yield(cancellationToken);
                playableDirector.time += Time.deltaTime * timelineProgressSpeed;
                playableDirector.DeferredEvaluate();
            }
            playableDirector.Stop();
        }
        private async UniTaskVoid StartOrResumeTimelineReverseTask(CancellationToken cancellationToken)
        {
            if (playableDirector.time <= 0)
            {
                playableDirector.time = playableDirector.playableAsset.duration;
            }

            while (playableDirector.time > 0)
            {
                await UniTask.Yield(cancellationToken);
                playableDirector.time -= Time.deltaTime * timelineProgressSpeed;
                playableDirector.DeferredEvaluate();
            }
            playableDirector.Stop();
        }
    
        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }

    }
}


