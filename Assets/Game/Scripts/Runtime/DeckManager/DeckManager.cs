using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.Events;
using Utilities;

namespace Game
{
    public class DeckManager : MonoBehaviour
    {
        public int MaxDeckCards { get; set; }
        [SerializeField] private List<BaseRule> deckRules;
        private ExtendedObservableCollection<BaseCard> deckCardsSpawned = new();
        [SerializeField] private UnityEvent<string> onRuleNotRespected;
        [SerializeField] private UnityEvent<int> onDeckCardsNumber;
        [SerializeField] private UnityEvent<List<BaseCard>> onCardsAdded;
        [SerializeField] private UnityEvent<BaseCard> onCardRemoved;

        private void Awake()
        {
            deckCardsSpawned.CollectionChanged += DeckCardsSpawnedOnCollectionChanged;
        }

        private void OnDestroy()
        {
            deckCardsSpawned.CollectionChanged -= DeckCardsSpawnedOnCollectionChanged;
        }

        private void DeckCardsSpawnedOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                {
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        onCardRemoved?.Invoke(e.OldItems[i] as BaseCard);
                    }
                   
                    break;
                }
                case NotifyCollectionChangedAction.Add:
                {
                    List<BaseCard> newCards = new();
                    var cards = e.NewItems.OfType<BaseCard>();
                    foreach (var card in cards)
                    {
                        newCards.Add(card);
                    }
                  
                    onCardsAdded?.Invoke(newCards);

                    break;
                }
            }
            
            onDeckCardsNumber?.Invoke(deckCardsSpawned.Count);
        }

        public void AddCards(List<BaseCard> cards)
        {
            if (CardsCanBeAdded(cards))
            {
                foreach (var card in cards)
                {
                    DeckCard deckCard = (DeckCard)card;
                    deckCard.lastCardInteractable?.RemoveCardFromCardInteraction(deckCard);
                }
             
                deckCardsSpawned.AddRange(cards);
                return;
            }
            
            foreach (var card in cards)
            {
                DeckCard deckCard = (DeckCard)card;
                deckCard.lastCardInteractable?.OnEndDragWithCard(deckCard);
            }
        }

        public void ClearSpawnedDeckCards()
        {
            deckCardsSpawned.Clear();
        }

        public void AddCard(BaseCard card)
        {
            DeckCard deckCard = (DeckCard)card;
            if (CardCanBeAdded(deckCard))
            {
                deckCard.lastCardInteractable?.RemoveCardFromCardInteraction(deckCard);
                deckCardsSpawned.Add(card);
                return;
            }
            
            deckCard.lastCardInteractable?.OnEndDragWithCard(deckCard);
        }
        
        public void RemoveCard(BaseCard card)
        {
            deckCardsSpawned.Remove(card);
        }

        private bool CardCanBeAdded(BaseCard card)
        {
            if (!IsUnderMaxDeckCardsQuantity(1)) return false;
            
            List<BaseCard> deckCardsSpawnedList = deckCardsSpawned.ToList();

            if (!IsRulesPassed(card, deckCardsSpawnedList)) return false;

            return true;
        }
        
        private bool CardsCanBeAdded(List<BaseCard> cards)
        {
            if (!IsUnderMaxDeckCardsQuantity(cards.Count)) return false;
            
            List<BaseCard> deckCardsSpawnedList = deckCardsSpawned.ToList();
            
            foreach (var card in cards)
            {
                if (!IsRulesPassed(card, deckCardsSpawnedList)) return false;
            }

            return true;
        }

        private bool IsRulesPassed(BaseCard card, List<BaseCard> deckCardsSpawnedList)
        {
            foreach (var baseRule in deckRules)
            {
                if (baseRule.IsRuleRespected(card, deckCardsSpawnedList))
                {
                    continue;
                }
                onRuleNotRespected?.Invoke(baseRule.ruleNotRespectedDescription);
                return false;
            }

            return true;
        }

        private bool IsUnderMaxDeckCardsQuantity(int newCardsQuantity)
        {
            int futureCardsNumber = deckCardsSpawned.Count + newCardsQuantity;
            if (futureCardsNumber > MaxDeckCards)
            {
                onRuleNotRespected?.Invoke("Maximum of deck's cards quantity reached");
                return false;
            }

            return true;
        }
        
    }
}
