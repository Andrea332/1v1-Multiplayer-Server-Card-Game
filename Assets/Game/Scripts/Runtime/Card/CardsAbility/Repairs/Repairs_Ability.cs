using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Authentication;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "Repairs_Ability", menuName = "Cards/Ability/Repairs")]
    public class Repairs_Ability : BaseCardAbility
    {
        public int healthToRestore;
        public int damageTimes;
        [SerializeField] private int uiPosition;
        [SerializeField] private SlotAvailabilityManagerRepairs slotAvailabilityManagerPrefab;
        
        public override void ClientPrepareAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId)
        {
            var slotAvailabilityManager = Instantiate(slotAvailabilityManagerPrefab, cardBattleBoardManager.transform);
            slotAvailabilityManager.transform.SetSiblingIndex(uiPosition);
            BaseCard baseCard = cardBattleBoardManager.playerCards.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == cardInventoryId);
            slotAvailabilityManager.onConfirmSelection = cardBattleBoardManager.ClientExecuteCardAbility;
            slotAvailabilityManager.OnCardDragToFindSlotBegin(baseCard, cardBattleBoardManager.playerTableSlots, cardBattleBoardManager.enemyTableSlots, damageTimes);
        }

        public override void ServerDoAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId, string serverJsonParameter)
        {
            var parameter = JsonUtility.FromJson<ServerRepairsParameter>(serverJsonParameter);
    
            List<TableInfo> tableInfos = cardBattleBoardManager.tableInfos;
 
            string currentTurnOwnerId = cardBattleBoardManager.currentTurnOwnerId;
     
            TableInfo playerTableInfo = tableInfos.Find(x => x.playerId == currentTurnOwnerId);

            var slotToRepair = playerTableInfo.slotInfos.Find(slotInfo => slotInfo.slotSupportCardData?.cardInventoryId == cardInventoryId);
       
           if (slotToRepair == null)
           {
               string message = $"Table slot with card id {cardInventoryId} not found on server";
               Debug.Log(message);
               cardBattleBoardManager.PingToClient(message);
               return;
           }
      
           if (slotToRepair.slotCardData == null)
           {
               string message = $"Table slot {slotToRepair.id} doesn't have a card on it";
               Debug.Log(message);
               cardBattleBoardManager.PingToClient(message);
               return;
           }

           if (parameter.cardInventoryIds.Length > damageTimes)
           {
               string message = $"Ability is trying to attack more than {damageTimes} times";
               Debug.Log(message);
               cardBattleBoardManager.PingToClient(message);
               return;
           }

           if (IsAbilityAlreadyUsed(cardBattleBoardManager, slotToRepair.slotSupportCardData)) return;
           
           ServerRepairsAbility ability = new(cardBattleBoardManager, slotToRepair.slotSupportCardData, slotToRepair.slotCardData, healthToRestore, parameter.cardInventoryIds.ToList());
           ability.AddAbilityUsed();
           
           ClientRepairsParameter clientParameter = new()
           {
               abilityCardInventoryId = slotToRepair.slotSupportCardData.cardInventoryId,
               abilityOwnerId = playerTableInfo.playerId,
               cardToRestoreInventoryId = slotToRepair.slotCardData.cardInventoryId,
               healthToRestore = healthToRestore,
               defaultHealth = slotToRepair.slotCardData.defaultHealth,
               cardInventoryIdsToAttack = parameter.cardInventoryIds
           };
           
           cardBattleBoardManager.ClientRpcExecuteCardAbility(JsonUtility.ToJson(clientParameter), cardEconomyId);
        }

        public override void ClientDoAbility(CardBattleBoardManager cardBattleBoardManager, string clientJsonParameter)
        {
            var clientParameter = JsonUtility.FromJson<ClientRepairsParameter>(clientJsonParameter);
            
            if (clientParameter.abilityOwnerId == AuthenticationService.Instance.PlayerId)
            {
                SetSupportCard(cardBattleBoardManager, cardBattleBoardManager.playerCards, cardBattleBoardManager.enemyCards, clientParameter);
                return;
            }
            
            SetSupportCard(cardBattleBoardManager, cardBattleBoardManager.enemyCards,cardBattleBoardManager.playerCards, clientParameter);
        }
        
        private void SetSupportCard(CardBattleBoardManager cardBattleBoardManager, List<BaseCard> abilityOwnerCardList, List<BaseCard> otherOwnerCardList, ClientRepairsParameter clientParameter)
        {
            BaseCard supportCard = abilityOwnerCardList.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == clientParameter.abilityCardInventoryId);
            BaseCard cardToRestore = abilityOwnerCardList.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == clientParameter.cardToRestoreInventoryId);

            List<BaseCard> cardsToDamage = new();
            foreach (var cardInventoryId in clientParameter.cardInventoryIdsToAttack)
            {
                var card = otherOwnerCardList.Find(card => card.GetComponent<CardInventoryIdComponent>().Parameter == cardInventoryId);
                cardsToDamage.Add(card);
            }
            
            var abilityComponent = supportCard.gameObject.AddComponent<ClientRepairsAbilityComponent>();
            abilityComponent.CardBattleBoardManager = cardBattleBoardManager;
            abilityComponent.battleCardToUse = (BattleCard)supportCard;
            abilityComponent.cardDataToRestore = (BattleCard)cardToRestore;
            abilityComponent.healthToRestore = clientParameter.healthToRestore;
            abilityComponent.defaultHealth = clientParameter.defaultHealth;
            abilityComponent.cardsToDamage = cardsToDamage;
            abilityComponent.AddAbilityUsed();
        }

        public struct ServerRepairsParameter
        {
            public string[] cardInventoryIds;
        }
        private struct ClientRepairsParameter
        {
            public string abilityCardInventoryId;
            public string abilityOwnerId;
            public string cardToRestoreInventoryId;
            public int healthToRestore;
            public int defaultHealth;
            public string[] cardInventoryIdsToAttack;
        }
    }
}
