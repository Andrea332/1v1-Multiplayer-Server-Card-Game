using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Utilities
{
    public class UnityEventPerformerWithString : MonoBehaviour
    {
        [SerializeField] private UnityEvent<string> onEventPerformed;

        public void PerformEvent(string stringValue)
        {
            onEventPerformed?.Invoke(stringValue);
        }
    }
}
