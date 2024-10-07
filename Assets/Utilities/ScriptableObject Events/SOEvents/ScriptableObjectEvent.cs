using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ScriptableObjectEvents
{
    [Serializable]
    [CreateAssetMenu(fileName = "ScriptableObjectEvent", menuName = "ScriptableObjectEvents/Scriptable Object Event")]
    public class ScriptableObjectEvent : ScriptableObject
    {
        public UnityEvent onEvent;

        [ContextMenu("FireEvent")]
        public void FireEvent()
        {
            onEvent?.Invoke();
        }
    }
}
