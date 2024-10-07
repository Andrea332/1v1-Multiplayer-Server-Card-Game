using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "Sonar_Ability", menuName = "Cards/Ability/Sonar")]
    public class Sonar_Ability : BaseCardAbility
    {
       public BaseItemString attackAbleCardType;
       public int attackAmount;
        public override void ServerDoAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId, string serverJsonParameter)
        { 
            List<TableInfo> tableInfos = cardBattleBoardManager.tableInfos;
            Debug.Log("1");
            string currentTurnOwnerId = cardBattleBoardManager.currentTurnOwnerId;
            Debug.Log("2");
            TableInfo playerTableInfo = tableInfos.Find(x => x.playerId == currentTurnOwnerId);
            Debug.Log("3");
            TableInfo enemyTableInfo = tableInfos.Find(x => x.playerId != currentTurnOwnerId);
            Debug.Log("4");
            BattleSlotInfo abilitySlotInfo = playerTableInfo.slotInfos.Find(slotInfo => slotInfo.slotSupportCardData?.cardInventoryId == cardInventoryId);
            Debug.Log("5");
            if (abilitySlotInfo == null)
            {
               string message = $"Table slot not found on server";
               Debug.Log(message);
               cardBattleBoardManager.PingToClient(message);
               return;
            }
            Debug.Log("6");
            if (abilitySlotInfo.slotCardData == null)
            {
               string message = $"Table slot {abilitySlotInfo.id} doesn't have a card on it";
               Debug.Log(message);
               cardBattleBoardManager.PingToClient(message);
               return;
            }
            Debug.Log("7");
            if (abilitySlotInfo.slotSupportCardData == null)
            {
               string message = $"Table slot {abilitySlotInfo.id} doesn't have a support card on it";
               Debug.Log(message);
               cardBattleBoardManager.PingToClient(message);
               return;
            }
            Debug.Log("8");
            if (IsAbilityAlreadyUsed(cardBattleBoardManager, abilitySlotInfo.slotSupportCardData)) return;
            Debug.Log("9");
            SlotInfo submarineSlotInfo = enemyTableInfo.slotInfos.Find(slotInfo => slotInfo.slotCardData?.type == attackAbleCardType.ItemValue);
            Debug.Log("10");
            if(submarineSlotInfo == null) return;
            Debug.Log("11");
            var ability = new ServerSonarAbility(cardBattleBoardManager, abilitySlotInfo.slotSupportCardData, submarineSlotInfo, attackAmount);
            ability.AddAbilityUsed();
            Debug.Log("12");
            ClientSonarParameter clientParameter = new()
            {
               abilityCardInventoryId = abilitySlotInfo.slotSupportCardData.cardInventoryId,
               abilityOwnerId = playerTableInfo.playerId,
               cardToAttackInventoryId = submarineSlotInfo.slotCardData.cardInventoryId,
               attackAmount = attackAmount
            };
            Debug.Log("13");
            cardBattleBoardManager.ClientRpcExecuteCardAbility(JsonUtility.ToJson(clientParameter), cardEconomyId);
        }

        public override void ClientDoAbility(CardBattleBoardManager cardBattleBoardManager, string clientJsonParameter)
        {
           var clientParameter = JsonUtility.FromJson<ClientSonarParameter>(clientJsonParameter);
            
           if (clientParameter.abilityOwnerId == AuthenticationService.Instance.PlayerId)
           {
              SetSupportCard(cardBattleBoardManager, cardBattleBoardManager.playerCards,cardBattleBoardManager.enemyCards,clientParameter);
              return;
           }
            
           SetSupportCard(cardBattleBoardManager, cardBattleBoardManager.enemyCards,cardBattleBoardManager.playerCards, clientParameter);
        }
        
        private void SetSupportCard(CardBattleBoardManager cardBattleBoardManager, List<BaseCard> abilityOwnerCardsList,List<BaseCard> otherCardsList, ClientSonarParameter clientParameter)
        {
           BaseCard supportCard = abilityOwnerCardsList.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == clientParameter.abilityCardInventoryId);
           BaseCard attackedCard = otherCardsList.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == clientParameter.cardToAttackInventoryId);
           
           if(attackedCard.TryGetComponent(out IAbilityAble abilityAble))
           {
              abilityAble.RemoveAbilityUsed();
           }
           
           var abilityComponent = supportCard.gameObject.AddComponent<ClientAbilityManager>();
           abilityComponent.CardBattleBoardManager = cardBattleBoardManager;
           abilityComponent.battleCardToUse = (BattleCard)supportCard;
           abilityComponent.AddAbilityUsed();
        }
        
        private struct ClientSonarParameter
        {
           public string abilityCardInventoryId;
           public string abilityOwnerId;
           public string cardToAttackInventoryId;
           public int attackAmount;
        }
    }
}
