using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utilities;

namespace Game
{
    public class DeckCard : BaseCard, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public ICardInteractable lastCardInteractable;
        public LayoutElement layoutElement;
        public Canvas canvas;
       
        public void EnableSortingCanvas()
        {
            canvas.overrideSorting = true;
        }
        
        public void DisableSortingOverride()
        {
            canvas.overrideSorting = false;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
        }
        
        public override void OnBeginDrag(PointerEventData eventData)
        {
            canvasGroup.interactable = false;
            List<RaycastResult> raycastResults = new();
            EventSystem.current.RaycastAll(eventData, raycastResults);
            foreach (var raycastResult in raycastResults)
            {
               if (raycastResult.gameObject.TryGetComponent(out ICardInteractable cardInteractable))
               {
                   cardInteractable.OnBeginDragWithCard(this);
                   return;
               }
            }
        } 
        
        public override void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.interactable = true;
            List<RaycastResult> raycastResults = new();
            EventSystem.current.RaycastAll(eventData, raycastResults);
            foreach (var raycastResult in raycastResults)
            {
                if (raycastResult.gameObject.TryGetComponent(out ICardInteractable cardInteractable))
                {
                    cardInteractable.AddCardFromCardInteraction(this, lastCardInteractable);
                    return;
                }
            }
            lastCardInteractable?.OnEndDragWithCard(this);
        }
        
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if(eventData.dragging) return;
            lastCardInteractable?.EnterPointerOverCard(this);
        }
        
        public override void OnPointerExit(PointerEventData eventData)
        {
            if(eventData.dragging) return;
            lastCardInteractable?.ExitPointerOverCard(this);
        }
    }
}
