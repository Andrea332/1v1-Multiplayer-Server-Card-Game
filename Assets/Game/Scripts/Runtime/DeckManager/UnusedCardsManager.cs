using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Utilities;

namespace Game
{
    public class UnusedCardsManager : MonoBehaviour
    {
        private ExtendedObservableCollection<BaseCard> unusedCardsSpawned = new();
        [SerializeField] private UnityEvent<List<BaseCard>> onCardsAdded;
        [SerializeField] private UnityEvent<BaseCard> onCardRemoved;
        private void Awake()
        {
            unusedCardsSpawned.CollectionChanged += UnusedCardsSpawnedOnCollectionChanged;
        }

        private void OnDestroy()
        {
            unusedCardsSpawned.CollectionChanged -= UnusedCardsSpawnedOnCollectionChanged;
        }

        private void UnusedCardsSpawnedOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                {
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        onCardRemoved?.Invoke((BaseCard)e.OldItems[i]);
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
        }

        public void AddCards(List<BaseCard> cards)
        {
            foreach (var card in cards)
            {
                DeckCard deckCard = (DeckCard)card;
                deckCard.lastCardInteractable?.RemoveCardFromCardInteraction(deckCard);
            }
            unusedCardsSpawned.AddRange(cards);
        }

        public void ClearSpawnedUnusedCards()
        {
            unusedCardsSpawned.Clear();
        }
        
        public void AddCard(BaseCard card)
        {
            DeckCard deckCard = (DeckCard)card;
            deckCard.lastCardInteractable?.RemoveCardFromCardInteraction(deckCard);
            unusedCardsSpawned.Add(card);
        }
        
        public void RemoveCard(BaseCard card)
        {
            unusedCardsSpawned.Remove(card);
        }
    }
}

