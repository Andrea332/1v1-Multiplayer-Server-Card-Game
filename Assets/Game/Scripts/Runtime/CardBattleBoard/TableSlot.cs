using System.Collections.Generic;
using Mirror;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Utilities;

namespace Game
{
    public class TableSlot : MonoBehaviour, ICardInteractable, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public int Id { get; set; }
        public string PlayerOwnerId { get; set; }
        public BattleCard CurrentBattleCard { get; set; }
        public CardBattleBoardManager CardBattleBoardManager { get; set; }

        public ICardInteractable tempCardInteractable;

        public UnityEvent<CardBattleBoardManager, TableSlot, BaseCard> onSlotClicked;
      
        public virtual void OnBeginDragWithCard(BaseCard card)
        {
            
        }

        public virtual void OnEndDragWithCard(BaseCard card)
        {
            string cardInventoryId = card.GetComponent<CardInventoryIdComponent>().Parameter;
            string cardEconomyId = card.GetComponent<CardEconomyIdComponent>().Parameter;
            CardBattleBoardManager.CmdTryAttackSlot(Id, PlayerOwnerId, cardEconomyId, cardInventoryId, AuthenticationService.Instance.PlayerId);
        }

        public virtual void EnterPointerOverCard(BaseCard card)
        {
           
        }

        public virtual void ExitPointerOverCard(BaseCard card)
        {
            card.transform.localScale = Vector3.one;
        }

        public virtual void RemoveCardFromCardInteraction(BaseCard card)
        {
           
        }
        
        [Client]
        public virtual void AddCardFromCardInteraction(BaseCard card, ICardInteractable oldInteractAble)
        {
            tempCardInteractable = oldInteractAble;
            CardBattleBoardManager.ClientTryPlaceCard(Id, PlayerOwnerId, card, AuthenticationService.Instance.PlayerId);
        }

        [Client]
        public virtual void SetCard(BaseCard card)
        {
            BattleCard battleCard = (BattleCard)card;
            CurrentBattleCard = battleCard;
            battleCard.DisableCardInteraction();
            Transform cardTransform = battleCard.transform;
            cardTransform.SetParent(transform, false);
            cardTransform.SetPositionAndRotation(cardTransform.position, new Quaternion());
            RectTransform cardRectTransform = cardTransform.GetComponent<RectTransform>();
            cardRectTransform.anchorMax = new Vector2(1, 1);
            cardRectTransform.anchorMin = new Vector2(0, 0);
            cardRectTransform.localPosition = Vector3.zero;
            cardRectTransform.anchoredPosition = Vector2.zero;
            cardRectTransform.sizeDelta = Vector2.zero;
            battleCard.layoutElement.ignoreLayout = false;
            battleCard.canvas.overrideSorting = false;
            tempCardInteractable?.RemoveCardFromCardInteraction(battleCard);
            tempCardInteractable = null;
        }
        
        [Client]
        public virtual void UnSetCard(BaseCard card = null)
        {
            CurrentBattleCard = null;
            tempCardInteractable?.OnEndDragWithCard(card);
            tempCardInteractable = null;
        }
        
        public virtual void OnDrag(PointerEventData eventData)
        {
          
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            List<RaycastResult> raycastResults = new();
            EventSystem.current.RaycastAll(eventData, raycastResults);
            foreach (var raycastResult in raycastResults)
            {
                if (raycastResult.gameObject.TryGetComponent(out TableSlot tableSlot))
                {
                    tableSlot.OnEndDragWithCard(CurrentBattleCard);
                    return;
                }
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            onSlotClicked?.Invoke(CardBattleBoardManager, this, CurrentBattleCard);
        }
    }
}
