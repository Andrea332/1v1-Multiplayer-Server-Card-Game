using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class OnDestroyed : MonoBehaviour
    {
        public UnityEvent onDestroy;
        
        [ContextMenu("ExecuteDestroy")]
        private void OnDestroy()
        {
            onDestroy?.Invoke();
        }
    }
}
