using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ScriptableObjectEvents
{
    public abstract class ScriptableObjectMultiEventHandlerWithParameter<T> : MonoBehaviour
    {
        [Tooltip("Default subscribe on enable, this bool override it in the Awake")]
        public bool subscribeAtAwake = false;
        public List<GameEventsHandler<T>> gameEvents;

        private void Awake()
        {
            if(!subscribeAtAwake) return;

            foreach (var gameEventsHandler in gameEvents)
            {
                gameEventsHandler.SubscribeEvent();
            }
        }

        private void OnEnable()
        {
            if(subscribeAtAwake) return;
            
            foreach (var gameEventsHandler in gameEvents)
            {
                gameEventsHandler.SubscribeEvent();
            }
        }

        private void OnDisable()
        {
            if(subscribeAtAwake) return;
            
            foreach (var gameEventsHandler in gameEvents)
            {
                gameEventsHandler.UnsubscribeEvent();
            }
        }

        private void OnDestroy()
        {
            if(!subscribeAtAwake) return;
            
            foreach (var gameEventsHandler in gameEvents)
            {
                gameEventsHandler.UnsubscribeEvent();
            }
        }
    }
    [Serializable]
    public class GameEventsHandler<T>
    {
        public ScriptableObjectEventWithParameter<T> scriptableObjectEvent;
        public UnityEvent<T> onEventEmitted;

        public void SubscribeEvent()
        {
            scriptableObjectEvent.onEvent.AddListener(OnEvent);
        }
        
        public void UnsubscribeEvent()
        {
            scriptableObjectEvent.onEvent.RemoveListener(OnEvent);
        }
        private protected void OnEvent(T parameter)
        {
            onEventEmitted?.Invoke(parameter);
        }
    }
}
