using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ScriptableObjectEvents
{
    public class ScriptableObjectEventHandler : MonoBehaviour
    {
        [Tooltip("Default subscribe on enable, this bool override it in the Awake")]
        public bool subscribeAtAwake = false;
        public ScriptableObjectEvent scriptableObjectEvent;
        public UnityEvent onEventEmitted;

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

        [ContextMenu("FireEvent")]
        private void OnEvent()
        {
            onEventEmitted?.Invoke();
        }
    }
}
