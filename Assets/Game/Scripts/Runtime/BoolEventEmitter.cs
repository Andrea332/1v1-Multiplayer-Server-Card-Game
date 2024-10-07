using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class BoolEventEmitter : MonoBehaviour
    {
        [SerializeField] private UnityEvent onTrueEvent;
        [SerializeField] private UnityEvent onFalseEvent;
    
        public void EmitEvent(bool value)
        {
            if (value)
            {
                onTrueEvent?.Invoke();
                return;
            }
        
            onFalseEvent?.Invoke();
        }
    }
}
