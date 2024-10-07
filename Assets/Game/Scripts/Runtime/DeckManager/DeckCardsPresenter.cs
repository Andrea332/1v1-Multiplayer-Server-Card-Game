using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game
{
    public class DeckCardsPresenter : MonoBehaviour, ICardInteractable
    {
        [SerializeField] private GraphicRaycaster graphicRaycaster;
        [SerializeField] private protected Transform cardsParent;
        public UnityEvent<BaseCard> onAddCardFromCardInteraction;
        public UnityEvent<BaseCard> onRemoveCardFromCardInteraction;
        [SerializeField] private protected float poinerOverCardScale = 1;
        private protected BaseCard seatKeeper;
        
        private string filter;
        public string Filter
        {
            set
            {
                filter = value;
                OrderCards();
            }
        }
        
        private protected void OrderCards()
        {
            var disorderedCards = GetPresentedCards();

            foreach (var card in disorderedCards)
            {
                card.gameObject.SetActive(true);
            }
            
            if (!string.IsNullOrEmpty(filter))
            {
                var filteredCards = disorderedCards.FindAll(x => x.GetComponent<CardTypeComponent>().Parameter != filter);
                foreach (var card in filteredCards)
                {
                    card.gameObject.SetActive(false);
                }
            }

            var order = new List<string> { "tank", "ship", "aircraft", "submarine" , "defense", "support"};
            
            var sortedVehicles = disorderedCards
                .OrderBy(v => order.IndexOf(v.GetComponent<CardTypeComponent>().Parameter)) 
                .ThenBy(v => v.GetComponent<CardEconomyIdComponent>().Parameter)                   
                .ToList();

            disorderedCards = sortedVehicles;
           
            for (var index = 0; index < disorderedCards.Count; index++)
            {
                var card = disorderedCards[index];
                card.transform.SetSiblingIndex(index);
            }
        }

        private List<BaseCard> GetPresentedCards()
        {
            List<BaseCard> disorderedCards = new();

            for (int i = 0; i < cardsParent.childCount; i++)
            {
                if (cardsParent.GetChild(i).TryGetComponent(out BaseCard card))
                {
                    disorderedCards.Add(card);
                }
            }

            return disorderedCards;
        }

        public void AddCards(List<BaseCard> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                AddCard(cards[i], false);
            }
            OrderCards();
        }

        public virtual void AddCard(BaseCard card, bool orderCards)
        {
            DeckCard deckCard = (DeckCard)card;
            deckCard.lastCardInteractable = this;
            deckCard.transform.localScale = Vector3.one;
            deckCard.transform.SetParent(cardsParent, false);
            SetCardAsIntern(deckCard);
            if(!orderCards) return;
            OrderCards();
        }
        
        public virtual void AddCard(BaseCard card)
        {
            DeckCard deckCard = (DeckCard)card;
            deckCard.lastCardInteractable = this;
            deckCard.transform.localScale = Vector3.one;
            deckCard.transform.SetParent(cardsParent, false);
            SetCardAsIntern(deckCard);
            OrderCards();
        }

        public void RemoveCard(BaseCard card)
        {
            OrderCards();
            
            if(seatKeeper == null) return;
            
            Destroy(seatKeeper.gameObject);
            seatKeeper = null;
        }

        public virtual void AddCardFromCardInteraction(BaseCard card, ICardInteractable oldInteractAble)
        {
            onAddCardFromCardInteraction?.Invoke(card);
        }
        
        public void RemoveCardFromCardInteraction(BaseCard card)
        {
            onRemoveCardFromCardInteraction?.Invoke(card);
        }

        public virtual void OnBeginDragWithCard(BaseCard card)
        {
            SetCardAsPointerOver(card);
            SetCardAsExtern(card);
        }

        public virtual void OnEndDragWithCard(BaseCard card)
        {
            SetCardAsPointerNotOver(card);
            SetCardAsIntern(card);
        }

        public void EnterPointerOverCard(BaseCard card)
        {
            SetCardAsPointerOver(card);
        }

        public void ExitPointerOverCard(BaseCard card)
        {
            SetCardAsPointerNotOver(card);
        }

        private protected virtual void SetCardAsPointerOver(BaseCard card)
        {
            DeckCard deckCard = (DeckCard)card;
            deckCard.transform.localScale = Vector3.one * poinerOverCardScale;
            deckCard.EnableSortingCanvas();
        }

        private protected virtual void SetCardAsPointerNotOver(BaseCard card)
        {
            DeckCard deckCard = (DeckCard)card;
            deckCard.transform.localScale = Vector3.one;
            deckCard.DisableSortingOverride();
        }

        private protected virtual void SetCardAsExtern(BaseCard card)
        {
            DeckCard deckCard = (DeckCard)card;
            
            if(seatKeeper != null) return;
            
            var cardLastIndex = deckCard.transform.GetSiblingIndex();
            
            CreateSeatKeeper(deckCard, cardLastIndex);
            
            deckCard.layoutElement.ignoreLayout = true;
            
            deckCard.GetComponentInChildren<MaxDeckablePresenter>().gameObject.SetActive(false);
        }

        private protected virtual void CreateSeatKeeper(BaseCard card, int cardIndex)
        {
            seatKeeper = Instantiate(card, cardsParent, false);
            (seatKeeper as DeckCard).DisableSortingOverride();
            seatKeeper.name = "SeatKeeper for: " + card.name;
            Transform seatKeeperTransform = seatKeeper.transform;
            seatKeeperTransform.SetSiblingIndex(cardIndex);
            seatKeeperTransform.localScale = Vector3.one;
            CanvasGroup canvasGroup = seatKeeper.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0.25f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private protected virtual void SetCardAsIntern(BaseCard card)
        {
            DeckCard deckCard = (DeckCard)card;
            deckCard.layoutElement.ignoreLayout = false;

            deckCard.GetComponentInChildren<MaxDeckablePresenter>(true).gameObject.SetActive(true);
            if (seatKeeper != null)
            {
                deckCard.transform.SetSiblingIndex(seatKeeper.transform.GetSiblingIndex());
                Destroy(seatKeeper.gameObject);
                seatKeeper = null;
            }
        }
       
    }
}

