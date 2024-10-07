using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Utilities
{
    public class UnityEventPerformer : MonoBehaviour
    {
        [SerializeField] private UnityEvent onEventPerformed;
        public void PerformEvent()
        {
            onEventPerformed?.Invoke();
        }
    }
}
