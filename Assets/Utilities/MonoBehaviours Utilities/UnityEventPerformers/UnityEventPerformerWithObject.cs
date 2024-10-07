using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Utilities
{
    public class UnityEventPerformerWithObject : MonoBehaviour
    {
        [SerializeField] private UnityEvent<object> onEventPerformed;

        public void PerformEvent(object value)
        {
            onEventPerformed?.Invoke(value);
        }
    }
}
