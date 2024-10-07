using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Utilities
{
    public class OnEnabled : MonoBehaviour
    {
        public UnityEvent onEnable;
        
        [ContextMenu("ExecuteEnable")]
        public void OnEnable()
        {
            onEnable?.Invoke();
        }
    }
}
