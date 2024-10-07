using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ScriptableObjectEvents
{
    [Serializable]
    public abstract class ScriptableObjectEventWithParameter<T> : ScriptableObject
    {
        public UnityEvent<T> onEvent;

        public virtual void FireEvent(T parameter)
        {
            onEvent?.Invoke(parameter);
        }
    }
}
