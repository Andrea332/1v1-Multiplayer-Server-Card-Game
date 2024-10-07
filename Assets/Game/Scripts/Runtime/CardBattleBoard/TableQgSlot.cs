using System.Collections.Generic;
using Mirror;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Utilities;

namespace Game
{
    public class TableQgSlot : TableSlot
    {
      
        public override void OnBeginDragWithCard(BaseCard card)
        {
            
        }

        public override void OnEndDragWithCard(BaseCard card)
        {
           
        }

        public override void EnterPointerOverCard(BaseCard card)
        {
           
        }

        public override void ExitPointerOverCard(BaseCard card)
        {
            card.transform.localScale = Vector3.one;
        }

        public override void RemoveCardFromCardInteraction(BaseCard card)
        {
           
        }
        
        public override void OnDrag(PointerEventData eventData)
        {
        
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            
        }

        public override void OnEndDrag(PointerEventData eventData)
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
    }
}
