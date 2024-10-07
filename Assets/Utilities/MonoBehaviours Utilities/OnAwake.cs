using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Utilities
{
    public class OnAwake : MonoBehaviour
    {
        public UnityEvent onAwake;
        
        [ContextMenu("ExecuteAwake")]
        public void Awake()
        {
            onAwake?.Invoke();
        }

    }
}
