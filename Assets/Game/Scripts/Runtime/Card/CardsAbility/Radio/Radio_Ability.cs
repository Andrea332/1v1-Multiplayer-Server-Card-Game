using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "Radio_Ability", menuName = "Cards/Ability/Radio")]
    public class Radio_Ability : BaseCardAbility
    {
        public int attackTimesToAdd;
        public int mainCardDamageBeforeDestroying;
        public override void ServerDoAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId, string serverJsonParameter)
        {
            List<TableInfo> tableInfos = cardBattleBoardManager.tableInfos;
 
            string currentTurnOwnerId = cardBattleBoardManager.currentTurnOwnerId;
     
            TableInfo playerTableInfo = tableInfos.Find(x => x.playerId == currentTurnOwnerId);
      
           BattleSlotInfo abilitySlotInfo = playerTableInfo.slotInfos.Find(slotInfo => slotInfo.slotSupportCardData?.cardInventoryId == cardInventoryId);
           
           if (abilitySlotInfo == null)
           {
               string message = $"Table slot not found on server";
               Debug.Log(message);
               cardBattleBoardManager.PingToClient(message);
               return;
           }
      
           if (abilitySlotInfo.slotCardData == null)
           {
               string message = $"Table slot {abilitySlotInfo.id} doesn't have a card on it";
               Debug.Log(message);
               cardBattleBoardManager.PingToClient(message);
               return;
           }
          
           if (IsAbilityAlreadyUsed(cardBattleBoardManager, abilitySlotInfo.slotSupportCardData)) return;
       
           if (abilitySlotInfo.slotCardData.optionalParameter is IAbilityAble abilityManager)
           {
               abilityManager.RemoveAbilityUsed();
           }
       
           if (abilitySlotInfo.slotSupportCardData.optionalParameter is ServerRadioAbility ability)
           {
               ability.AbilityUsed = true;
           }
           else
           {
               ability = new ServerRadioAbility(cardBattleBoardManager, abilitySlotInfo.slotSupportCardData, abilitySlotInfo.slotCardData, attackTimesToAdd, mainCardDamageBeforeDestroying);
               ability.AddAbilityUsed();
           }
           
           ClientRadioParameter clientParameter = new()
           {
               abilityCardInventoryId = abilitySlotInfo.slotSupportCardData.cardInventoryId,
               abilityOwnerId = playerTableInfo.playerId
           };
           
           cardBattleBoardManager.ClientRpcExecuteCardAbility(JsonUtility.ToJson(clientParameter), cardEconomyId);
        }
        
        public override void ClientDoAbility(CardBattleBoardManager cardBattleBoardManager, string clientJsonParameter)
        {
            var clientParameter = JsonUtility.FromJson<ClientRadioParameter>(clientJsonParameter);
            
            if (clientParameter.abilityOwnerId == AuthenticationService.Instance.PlayerId)
            {
                SetSupportCard(cardBattleBoardManager, cardBattleBoardManager.playerCards, clientParameter);
                return;
            }
            
            SetSupportCard(cardBattleBoardManager, cardBattleBoardManager.enemyCards, clientParameter);
        }
        
        private void SetSupportCard(CardBattleBoardManager cardBattleBoardManager, List<BaseCard> cardList, ClientRadioParameter clientParameter)
        {
            BaseCard supportCard = cardList.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == clientParameter.abilityCardInventoryId);

            if (!supportCard.gameObject.TryGetComponent(out ClientRadioAbilityComponent abilityComponent))
            {
                abilityComponent = supportCard.gameObject.AddComponent<ClientRadioAbilityComponent>();
            }
      
            abilityComponent.CardBattleBoardManager = cardBattleBoardManager;
            abilityComponent.battleCardToUse = (BattleCard)supportCard;
            abilityComponent.AddAbilityUsed();
        }
        
        private struct ClientRadioParameter
        {
            public string abilityCardInventoryId;
            public string abilityOwnerId;
        }
    }
}
