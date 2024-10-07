using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Utilities
{
    public class OnDisabled : MonoBehaviour
    {
        public UnityEvent onDisable;
        
        [ContextMenu("ExecuteDisable")]
        private void OnDisable()
        {
            onDisable?.Invoke();
        }

    }
}
