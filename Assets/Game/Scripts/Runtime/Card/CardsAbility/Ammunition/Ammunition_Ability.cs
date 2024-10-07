using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "Ammunition_Ability", menuName = "Cards/Ability/Ammunition")]
    public class Ammunition_Ability : BaseCardAbility
    {
        public int attackToAdd;
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
       
           if (abilitySlotInfo.slotSupportCardData.optionalParameter is ServerAmmunitionAbility ability)
           {
               ability.AbilityUsed = true;
           }
           else
           {
               ability = new ServerAmmunitionAbility(cardBattleBoardManager, abilitySlotInfo.slotSupportCardData, abilitySlotInfo.slotCardData, attackToAdd, mainCardDamageBeforeDestroying);
               ability.AddAbilityUsed();
           }
           
           ClientAmmunitionParameter clientParameter = new()
           {
               abilityCardInventoryId = abilitySlotInfo.slotSupportCardData.cardInventoryId,
               abilityOwnerId = playerTableInfo.playerId,
               cardToAddAttackInventoryId = abilitySlotInfo.slotCardData.cardInventoryId,
               attackToAdd = attackToAdd
           };
           
           cardBattleBoardManager.ClientRpcExecuteCardAbility(JsonUtility.ToJson(clientParameter), cardEconomyId);
        }
        
        public override void ClientDoAbility(CardBattleBoardManager cardBattleBoardManager, string clientJsonParameter)
        {
            var clientParameter = JsonUtility.FromJson<ClientAmmunitionParameter>(clientJsonParameter);
            
            if (clientParameter.abilityOwnerId == AuthenticationService.Instance.PlayerId)
            {
                SetSupportCard(cardBattleBoardManager, cardBattleBoardManager.playerCards, clientParameter);
                return;
            }
            
            SetSupportCard(cardBattleBoardManager, cardBattleBoardManager.enemyCards, clientParameter);
        }
        
        private void SetSupportCard(CardBattleBoardManager cardBattleBoardManager, List<BaseCard> cardList, ClientAmmunitionParameter clientParameter)
        {
            BaseCard supportCard = cardList.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == clientParameter.abilityCardInventoryId);
            BaseCard cardToAddAttack = cardList.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == clientParameter.cardToAddAttackInventoryId);

            if (!supportCard.gameObject.TryGetComponent(out ClientAmmunitionAbilityComponent abilityComponent))
            {
                abilityComponent = supportCard.gameObject.AddComponent<ClientAmmunitionAbilityComponent>();
            }
      
            abilityComponent.CardBattleBoardManager = cardBattleBoardManager;
            abilityComponent.AttackToAdd = clientParameter.attackToAdd;
            abilityComponent.battleCardToUse = (BattleCard)supportCard;
            abilityComponent.battleCardToAddAttack = (BattleCard)cardToAddAttack;
            abilityComponent.AddAbilityUsed();
        }
        
        private struct ClientAmmunitionParameter
        {
            public string abilityCardInventoryId;
            public string abilityOwnerId;
            public string cardToAddAttackInventoryId;
            public int attackToAdd;
        }
    }
}
