using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "Fuel_Ability", menuName = "Cards/Ability/Fuel")]
    public class Fuel_Ability : BaseCardAbility
    {
        public int fuelToAdd;
        public override void ServerDoAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId, string serverJsonParameter)
        {
            List<TableInfo> tableInfos = cardBattleBoardManager.tableInfos;
 
            string currentTurnOwnerId = cardBattleBoardManager.currentTurnOwnerId;
     
            TableInfo playerTableInfo = tableInfos.Find(x => x.playerId == currentTurnOwnerId);

            var abilitySlot = playerTableInfo.slotInfos.Find(slotInfo => slotInfo.slotSupportCardData?.cardInventoryId == cardInventoryId);
       
           if (abilitySlot == null)
           {
               string message = $"Table slot with support card id {cardInventoryId} not found on server";
               Debug.Log(message);
               cardBattleBoardManager.PingToClient(message);
               return;
           }
      
           if (abilitySlot.slotCardData == null)
           {
               string message = $"Table slot {abilitySlot.id} doesn't have a card on it";
               Debug.Log(message);
               cardBattleBoardManager.PingToClient(message);
               return;
           }
           
           if (abilitySlot.slotSupportCardData == null)
           {
               string message = $"Table slot {abilitySlot.id} doesn't have a support card on it";
               Debug.Log(message);
               cardBattleBoardManager.PingToClient(message);
               return;
           }

           if (IsAbilityAlreadyUsed(cardBattleBoardManager, abilitySlot.slotSupportCardData)) return;
           
           ServerFuelAbility ability = new(cardBattleBoardManager, abilitySlot.slotSupportCardData, fuelToAdd, currentTurnOwnerId);
           ability.AddAbilityUsed();
           
           ClientFuelParameter clientParameter = new()
           {
               abilityCardInventoryId = cardInventoryId,
               abilityOwnerId = playerTableInfo.playerId,
               fuelToAdd = fuelToAdd
           };
           
           cardBattleBoardManager.ClientRpcExecuteCardAbility(JsonUtility.ToJson(clientParameter), cardEconomyId);
        }

        public override void ClientDoAbility(CardBattleBoardManager cardBattleBoardManager, string clientJsonParameter)
        {
            var clientParameter = JsonUtility.FromJson<ClientFuelParameter>(clientJsonParameter);
            
            if (clientParameter.abilityOwnerId == AuthenticationService.Instance.PlayerId)
            {
                SetSupportCard(cardBattleBoardManager, cardBattleBoardManager.playerCards, clientParameter);
                return;
            }
            
            SetSupportCard(cardBattleBoardManager, cardBattleBoardManager.enemyCards, clientParameter);
        }

        private void SetSupportCard(CardBattleBoardManager cardBattleBoardManager,  List<BaseCard> abilityOwnerCardList, ClientFuelParameter clientParameter)
        {
            BaseCard abilityCard = abilityOwnerCardList.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == clientParameter.abilityCardInventoryId);
            var abilityComponent = abilityCard.gameObject.AddComponent<ClientAbilityManager>();
            abilityComponent.CardBattleBoardManager = cardBattleBoardManager;
            abilityComponent.battleCardToUse = (BattleCard)abilityCard;
            abilityComponent.AddAbilityUsed();
        }

        private struct ClientFuelParameter
        {
            public string abilityCardInventoryId;
            public string abilityOwnerId;
            public int fuelToAdd;
        }
    }
}
