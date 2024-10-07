using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game
{
    public class BattleCardsPresenter : DeckCardsPresenter
    {
        public UnityEvent<BaseCard> onCardDragBegin;
        public UnityEvent<BaseCard> onCardDragEnd;
        public override void AddCard(BaseCard card, bool orderCards)
        {
            BattleCard battleCard = (BattleCard)card;
            battleCard.lastCardInteractable = this;
            battleCard.transform.localScale = Vector3.one;
            battleCard.transform.SetParent(cardsParent, false);
            SetCardAsIntern(battleCard);
            if(!orderCards) return;
            OrderCards();
        }

        private protected override void SetCardAsPointerOver(BaseCard card)
        {
            BattleCard battleCard = (BattleCard)card;
            battleCard.transform.localScale = Vector3.one * poinerOverCardScale;
            SetCardAsExtern(battleCard);
            battleCard.EnableSortingCanvas();
            RectTransform cardRectTransform = battleCard.GetComponent<RectTransform>();
            var anchoredPosition = cardRectTransform.anchoredPosition;
            anchoredPosition = new Vector2(anchoredPosition.x, anchoredPosition.y + 150);
            cardRectTransform.anchoredPosition = anchoredPosition;
            battleCard.layoutElement.ignoreLayout = true;
        }
        private protected override void SetCardAsPointerNotOver(BaseCard card)
        {
            BattleCard battleCard = (BattleCard)card;
            battleCard.transform.localScale = Vector3.one;
            battleCard.DisableSortingOverride();
            SetCardAsIntern(card);
        }

        public override void OnBeginDragWithCard(BaseCard card)
        {
            base.OnBeginDragWithCard(card);
            onCardDragBegin?.Invoke(card);
        }

        public override void OnEndDragWithCard(BaseCard card)
        {
            BattleCard battleCard = (BattleCard)card;
            battleCard.transform.localScale = Vector3.one;
            battleCard.DisableSortingOverride();
            SetCardAsIntern(card);
            onCardDragEnd?.Invoke(card);
        }
       
        private protected override void SetCardAsIntern(BaseCard card)
        {
            BattleCard battleCard = (BattleCard)card;
            
            battleCard.layoutElement.ignoreLayout = false;

            battleCard.GetComponentInChildren<MaxDeckablePresenter>(true).gameObject.SetActive(false);
            
            if (seatKeeper != null)
            {
                battleCard.transform.SetSiblingIndex(seatKeeper.transform.GetSiblingIndex());
                Destroy(seatKeeper.gameObject);
                seatKeeper = null;
            }
        }

        private protected override void SetCardAsExtern(BaseCard card)
        {
            BattleCard battleCard = (BattleCard)card;
            
            if(seatKeeper != null) return;
            
            var cardLastIndex = battleCard.transform.GetSiblingIndex();
            
            CreateSeatKeeper(battleCard, cardLastIndex);
        }

        public override void AddCardFromCardInteraction(BaseCard battleCard, ICardInteractable oldInteractAble)
        {
            onAddCardFromCardInteraction?.Invoke(battleCard);
        }

        private protected override void CreateSeatKeeper(BaseCard card, int cardIndex)
        {
            seatKeeper = Instantiate(card, cardsParent, false);
            (seatKeeper as BattleCard).DisableSortingOverride();
            seatKeeper.name = "SeatKeeper for: " + card.name;
            Transform seatKeeperTransform = seatKeeper.transform;
            seatKeeperTransform.SetSiblingIndex(cardIndex);
            seatKeeperTransform.localScale = Vector3.one;
            CanvasGroup canvasGroup = seatKeeper.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0.25f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public override void AddCard(BaseCard card)
        {
            BattleCard battleCard = (BattleCard)card;
            battleCard.lastCardInteractable = this;
            battleCard.transform.localScale = Vector3.one;
            battleCard.transform.SetParent(cardsParent, false);
            SetCardAsIntern(battleCard);
            OrderCards();
        }
    }

}

