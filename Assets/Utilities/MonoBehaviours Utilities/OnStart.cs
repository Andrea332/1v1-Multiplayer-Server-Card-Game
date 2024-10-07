using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Utilities
{
    public class OnStart : MonoBehaviour
    {
        [SerializeField] private UnityEvent onStart;
        
        [ContextMenu("ExecuteStart")]
        public void Start()
        {
            onStart?.Invoke();
        }

    }
}
