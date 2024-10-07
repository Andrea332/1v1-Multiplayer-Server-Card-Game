using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utilities;

namespace Game
{
    public abstract class BaseCard : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public CanvasGroup canvasGroup;
        public UnityEvent<BaseCard> onCardClicked;
        public virtual void FireCardClickedEvent()
        {
            onCardClicked?.Invoke(this);
        }
        public void DisableCardInteraction()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        public void EnableCardInteraction()
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public abstract void OnDrag(PointerEventData eventData);
        
        public abstract void OnBeginDrag(PointerEventData eventData);
        
        public abstract void OnEndDrag(PointerEventData eventData);
        
        public abstract void OnPointerEnter(PointerEventData eventData);
        
        public abstract void OnPointerExit(PointerEventData eventData);

        
    }
}
