using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;

namespace Game
{
    public abstract class BaseCardBuilder : ScriptableObject
    {
        [SerializeField] private protected BaseCard cardPrefab;
        public Action<List<BaseCard>> onCardsBuilded;
        
        //Unity 2022.3.17 is not showing the functions on the UnityEvent if there is a return type on the functions
        public void BuildCard(NetworkedCardData cardData)
        {
            BuildCardWithReturnType(cardData);
        }
       
        public BaseCard BuildCardWithReturnType(NetworkedCardData cardData, bool sendEvent = true)
        {
            BaseCard baseCard = Instantiate(cardPrefab);
            return BuildCardWithReturnType(baseCard, cardData, sendEvent);
        }

        public abstract BaseCard BuildCardWithReturnType(BaseCard baseCard, NetworkedCardData cardData, bool sendEvent = true);
        
        public void BuildCards(NetworkedCardData[] cardDatas)
        {
            BuildCardsWithReturnType(cardDatas);
        }
        
        public List<BaseCard> BuildCardsWithReturnType(NetworkedCardData[] cardDatas)
        {
            List<BaseCard> cards = new List<BaseCard>();
            
            for (int i = 0; i < cardDatas.Length; i++)
            {
                BaseCard baseCard = BuildCardWithReturnType(cardDatas[i], false);
                cards.Add(baseCard);
            }
            onCardsBuilded?.Invoke(cards);
            return cards;
        }
    }
}
