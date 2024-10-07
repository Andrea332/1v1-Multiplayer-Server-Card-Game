using System.Collections.Generic;
using Mirror;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Utilities;

namespace Game
{
    public class TableBattleSlot : TableSlot
    {
        public BattleCard CurrentSupportCard { get; set; }

        public GameObject supportUi;
        
        [SerializeField] private Aim aimPrefab;
        
        private Aim spawnedAim;
        
        public override void ExitPointerOverCard(BaseCard card)
        {
            card.transform.localScale = Vector3.one;
        }
        
        [Client]
        public void SetSupportCard(BaseCard card)
        {
            BattleCard battleCard = (BattleCard)card;
            CurrentSupportCard = battleCard;
            battleCard.DisableCardInteraction();
            Transform cardTransform = battleCard.transform;
            battleCard.gameObject.SetActive(false);
            cardTransform.SetParent(transform, false);
            cardTransform.SetPositionAndRotation(cardTransform.position, new Quaternion());
            supportUi.SetActive(true);
            supportUi.transform.SetAsLastSibling();
            tempCardInteractable?.RemoveCardFromCardInteraction(battleCard);
            tempCardInteractable = null;
        }
        [Client]
        public void UnSetSupportCard(BaseCard card = null)
        {
            supportUi.SetActive(false);
            CurrentSupportCard = null;
            tempCardInteractable?.OnEndDragWithCard(card);
            tempCardInteractable = null;
        }
        public override void OnDrag(PointerEventData eventData)
        {
            if(spawnedAim == null) return;
            spawnedAim.CalculateAim(transform.position, eventData.position);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if(CurrentBattleCard == null) return;
            
            if(PlayerOwnerId != AuthenticationService.Instance.PlayerId) return;
            
            spawnedAim = Instantiate(aimPrefab, transform.root, false);
            spawnedAim.SetAim();
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if(spawnedAim == null) return;
            Destroy(spawnedAim.gameObject);
            spawnedAim = null;
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
