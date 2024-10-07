using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Utilities
{
    public class UnityEventPerformerWithDateTime : MonoBehaviour
    {
        [SerializeField] private UnityEvent<DateTime> onEventPerformed;

        public void PerformEvent(DateTime dateTime)
        {
            onEventPerformed?.Invoke(dateTime);
        }
    }
}
