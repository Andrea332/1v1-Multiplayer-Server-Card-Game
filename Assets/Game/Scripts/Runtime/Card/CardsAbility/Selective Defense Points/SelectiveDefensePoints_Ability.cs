using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "SelectiveDefensePoints_Ability", menuName = "Cards/Ability/Selective Defense Points")]
    public class SelectiveDefensePoints_Ability : BaseCardAbility
    {
        public int pointsToEarn;
        public List<BaseItemString> canBeDamageBy;
        
        public override void ServerDoAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId, string serverJsonParameter)
        {
            List<TableInfo> tableInfos = cardBattleBoardManager.tableInfos;
            
            string currentTurnOwnerId = cardBattleBoardManager.currentTurnOwnerId;
            
            TableInfo playerTableInfo = tableInfos.Find(x => x.playerId == currentTurnOwnerId);
            
            BattleSlotInfo currentCardSlot = playerTableInfo.slotInfos.Find(x =>
            {
                if (x.slotCardData == null) return false;
                        
                return x.slotCardData.cardInventoryId == cardInventoryId;
            });
            
            if(currentCardSlot == null) return;
            
            if (IsAbilityAlreadyUsed(cardBattleBoardManager, currentCardSlot.slotCardData)) return;
            
            playerTableInfo.points += pointsToEarn;
            
            ServerSCDAbility ability = new(cardBattleBoardManager, currentCardSlot.slotCardData, pointsToEarn, canBeDamageBy);
            ability.AddAbilityUsed();
            
            ClientSCDParameter parameter = new()
            {
                cardInventoryId = cardInventoryId,
                abilityOwnerId = playerTableInfo.playerId
            };
            
            cardBattleBoardManager.ClientRpcExecuteCardAbility(JsonUtility.ToJson(parameter), cardEconomyId);
        }

        public override void ClientDoAbility(CardBattleBoardManager cardBattleBoardManager, string clientJsonParameter)
        {
            var parameter = JsonUtility.FromJson<ClientSCDParameter>(clientJsonParameter);

            BaseCard card;
            
            if (parameter.abilityOwnerId == AuthenticationService.Instance.PlayerId)
            {
                card = cardBattleBoardManager.playerCards.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == parameter.cardInventoryId);
            }
            else
            {
                card = cardBattleBoardManager.enemyCards.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == parameter.cardInventoryId);
            }

            ClientSCDAbilityComponent clientSubmarineDive = card.gameObject.AddComponent<ClientSCDAbilityComponent>();
            clientSubmarineDive.CardBattleBoardManager = cardBattleBoardManager;
            clientSubmarineDive.battleCardToUse = (BattleCard)card;
            clientSubmarineDive.pointsToEarn = pointsToEarn;
            clientSubmarineDive.canBeDamageBy = canBeDamageBy;
            clientSubmarineDive.AddAbilityUsed();
        }

        public struct ClientSCDParameter
        {
            public string cardInventoryId;
            public string abilityOwnerId;
        }
    }
}
