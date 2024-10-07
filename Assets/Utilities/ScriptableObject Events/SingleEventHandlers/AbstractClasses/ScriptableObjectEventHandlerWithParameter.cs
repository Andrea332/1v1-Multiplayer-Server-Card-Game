using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ScriptableObjectEvents
{
    public abstract class ScriptableObjectEventHandlerWithParameter<T> : MonoBehaviour
    {
        [Tooltip("Default subscribe on enable, this bool override it in the Awake")]
        public bool subscribeAtAwake = false;
        public ScriptableObjectEventWithParameter<T> scriptableObjectEvent;
        public UnityEvent<T> onEventEmitted;
        
        private void Awake()
        {
            if(!subscribeAtAwake) return;
            scriptableObjectEvent.onEvent.AddListener(OnEvent);
        }

        private void OnEnable()
        {
            if(subscribeAtAwake) return;
            scriptableObjectEvent.onEvent.AddListener(OnEvent);
        }

        private void OnDisable()
        {
            if(subscribeAtAwake) return;
            scriptableObjectEvent.onEvent.RemoveListener(OnEvent);
        }

        private void OnDestroy()
        {
            if(!subscribeAtAwake) return;
            scriptableObjectEvent.onEvent.RemoveListener(OnEvent);
        }
       
        private protected void OnEvent(T parameter)
        {
            onEventEmitted?.Invoke(parameter);
        }
    }
}
