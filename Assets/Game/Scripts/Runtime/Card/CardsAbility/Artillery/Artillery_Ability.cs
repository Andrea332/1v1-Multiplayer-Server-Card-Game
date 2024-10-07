using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "Artillery_Ability", menuName = "Cards/Ability/Artillery")]
    public class Artillery_Ability : BaseCardAbility
    {
        [SerializeField] private int uiPosition;
        [SerializeField] private BaseItemString tankItemId;
        public int mainCardDamageBeforeDestroying;
        [SerializeField] private SlotAvailabilityManager slotAvailabilityManagerPrefab;
        private SlotAvailabilityManager spawnedSlotAvailabilityManager;
        private string cardEconomyId;
        private string cardInventoryId;
       
        public override void ClientPrepareAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId)
        {
            this.cardEconomyId = cardEconomyId;
            this.cardInventoryId = cardInventoryId;
            spawnedSlotAvailabilityManager = Instantiate(slotAvailabilityManagerPrefab, cardBattleBoardManager.transform, false);
            spawnedSlotAvailabilityManager.transform.SetSiblingIndex(uiPosition);
            BaseCard baseCard = cardBattleBoardManager.playerCards.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == cardInventoryId);
            spawnedSlotAvailabilityManager.OnCardDragToFindSlotBegin(baseCard, cardBattleBoardManager.playerTableSlots, cardBattleBoardManager.enemyTableSlots, tankItemId);
            foreach (var enemyTableSlot in cardBattleBoardManager.enemyTableSlots)
            {
                enemyTableSlot.onSlotClicked.RemoveAllListeners();
                enemyTableSlot.onSlotClicked.AddListener(OnSlotClicked);
            }
        }

        private void OnSlotClicked(CardBattleBoardManager cardBattleBoardManager, TableSlot tableSlot, BaseCard baseCard)
        {
            spawnedSlotAvailabilityManager.OnCardDragToFindSlotEnd(baseCard);
            Destroy(spawnedSlotAvailabilityManager.gameObject);
            spawnedSlotAvailabilityManager = null;
            foreach (var enemyTableSlot in cardBattleBoardManager.enemyTableSlots)
            {
                enemyTableSlot.onSlotClicked.RemoveAllListeners();
                enemyTableSlot.onSlotClicked.AddListener(cardBattleBoardManager.cardActionsMenuManager.SetMenu);
            }

            ServerArtilleryParameter aircraftParameter = new()
            {
                slotToDestroyId = tableSlot.Id
            };
            
            cardBattleBoardManager.ClientExecuteCardAbility(cardEconomyId, cardInventoryId, JsonUtility.ToJson(aircraftParameter));
        }

        public override void ServerDoAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId, string serverJsonParameter)
        {
            var parameter = JsonUtility.FromJson<ServerArtilleryParameter>(serverJsonParameter);
    
            List<TableInfo> tableInfos = cardBattleBoardManager.tableInfos;
 
            string currentTurnOwnerId = cardBattleBoardManager.currentTurnOwnerId;
     
            TableInfo playerTableInfo = tableInfos.Find(x => x.playerId == currentTurnOwnerId);
         
            TableInfo enemyTableInfo = tableInfos.Find(x => x.playerId != currentTurnOwnerId);
            
           var slotToDestroy = enemyTableInfo.slotInfos.Find(x => x.id == parameter.slotToDestroyId);
       
           if (slotToDestroy == null)
           {
               string message = $"Table slot {parameter.slotToDestroyId} not found on server";
               Debug.Log(message);
               cardBattleBoardManager.PingToClient(message);
               return;
           }
      
           if (slotToDestroy.slotCardData == null)
           {
               string message = $"Table slot {slotToDestroy.id} doesn't have a card on it";
               Debug.Log(message);
               cardBattleBoardManager.PingToClient(message);
               return;
           }
        
           if (slotToDestroy.slotCardData.type != tankItemId.ItemValue)
           {
               string message = $"Table slot {parameter.slotToDestroyId} is not of type {tankItemId.ItemValue}";
               Debug.Log(message);
               cardBattleBoardManager.PingToClient(message);
               return;
           }
      
           BattleSlotInfo abilitySlotInfo = playerTableInfo.slotInfos.Find(x =>
           {
               if (x.slotSupportCardData == null) return false;
               
               return x.slotSupportCardData.cardInventoryId == cardInventoryId;
           });
          
           if (IsAbilityAlreadyUsed(cardBattleBoardManager, abilitySlotInfo.slotSupportCardData)) return;
       
           if (slotToDestroy.slotCardData.optionalParameter is IAbilityAble abilityManager)
           {
               abilityManager.RemoveAbilityUsed();
           }
           
           slotToDestroy.slotCardData.ResetToDefaultValues();
           playerTableInfo.points += slotToDestroy.slotCardData.cost;
           enemyTableInfo.cemeteryCardDatas.Add(slotToDestroy.slotCardData);
           slotToDestroy.slotCardData = null;
        
           cardBattleBoardManager.ClientRpcDestroySlotMainCard(slotToDestroy.id, enemyTableInfo.playerId);
       
           if (abilitySlotInfo.slotSupportCardData.optionalParameter is ServerArtilleryAbility ability)
           {
               ability.AbilityUsed = true;
           }
           else
           {
               ability = new ServerArtilleryAbility(cardBattleBoardManager, abilitySlotInfo.slotSupportCardData, abilitySlotInfo.slotCardData, mainCardDamageBeforeDestroying);
               ability.AddAbilityUsed();
           }
           
           ClientArtilleryParameter clientParameter = new()
           {
               abilityCardInventoryId = abilitySlotInfo.slotSupportCardData.cardInventoryId,
               abilityOwnerId = playerTableInfo.playerId
           };
           
           cardBattleBoardManager.ClientRpcExecuteCardAbility(JsonUtility.ToJson(clientParameter), cardEconomyId);
        }
        
        public override void ClientDoAbility(CardBattleBoardManager cardBattleBoardManager, string clientJsonParameter)
        {
            var clientParameter = JsonUtility.FromJson<ClientArtilleryParameter>(clientJsonParameter);
            
            if (clientParameter.abilityOwnerId == AuthenticationService.Instance.PlayerId)
            {
                SetSupportCard(cardBattleBoardManager, cardBattleBoardManager.playerCards, clientParameter);
                return;
            }
            
            SetSupportCard(cardBattleBoardManager, cardBattleBoardManager.enemyCards, clientParameter);
        }
        
        private void SetSupportCard(CardBattleBoardManager cardBattleBoardManager, List<BaseCard> cardList, ClientArtilleryParameter clientParameter)
        {
            BaseCard supportCard = cardList.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == clientParameter.abilityCardInventoryId);

            if (!supportCard.gameObject.TryGetComponent(out ClientArtilleryAbilityComponent abilityComponent))
            {
                abilityComponent = supportCard.gameObject.AddComponent<ClientArtilleryAbilityComponent>();
            }
      
            abilityComponent.CardBattleBoardManager = cardBattleBoardManager;
            abilityComponent.battleCardToUse = (BattleCard)supportCard;
            abilityComponent.AddAbilityUsed();
        }

        private struct ServerArtilleryParameter
        {
            public int slotToDestroyId;
        }
        private struct ClientArtilleryParameter
        {
            public string abilityCardInventoryId;
            public string abilityOwnerId;
        }
    }
}
