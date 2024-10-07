using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Utilities;

namespace Game
{
    public class CardBuilderEvent : MonoBehaviour
    {
        [SerializeField] private BaseCardBuilder cardBuilder;
        [SerializeField] private UnityEvent<List<BaseCard>> onCardsBuilded;

        private void OnEnable()
        {
            cardBuilder.onCardsBuilded += OnCardsBuilded;
        }
        
        private void OnDisable()
        {
            cardBuilder.onCardsBuilded -= OnCardsBuilded;
        }
        
        private void OnCardsBuilded(List<BaseCard> obj)
        {
            onCardsBuilded?.Invoke(obj);
        }
    }
}
