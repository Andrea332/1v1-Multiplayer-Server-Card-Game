using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "AntiTank_Ability", menuName = "Cards/Ability/AntiTank")]
    public class AntiTank_Ability : BaseCardAbility
    {
        public BaseItemString antiTankTarget;

        public override void ServerPrepareAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId, string cardOwnerId)
        {
            ServerAntiTankParameter parameter = new()
            {
                destroyerInventoryId = cardInventoryId,
                destroyerOwnerId = cardOwnerId
            };
            
            ServerDoAbility(cardBattleBoardManager, cardEconomyId, cardInventoryId, JsonUtility.ToJson(parameter));
        }

        public override void ServerDoAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId, string serverJsonParameter)
        {
            var serverParameter = JsonUtility.FromJson<ServerAntiTankParameter>(serverJsonParameter);
            
            TableInfo destroyerOwnerTableInfo = cardBattleBoardManager.tableInfos.Find(x => x.playerId == serverParameter.destroyerOwnerId);

            BattleSlotInfo destroyerSlotInfo = destroyerOwnerTableInfo.slotInfos.Find(x =>
            {
                if (x.slotCardData == null) return false;
                
                return x.slotCardData?.cardInventoryId == serverParameter.destroyerInventoryId;
            });
            
            if (IsAbilityAlreadyUsed(cardBattleBoardManager, destroyerSlotInfo.slotCardData)) return;
            
            ServerAntiTankAbility ability = new(cardBattleBoardManager, destroyerSlotInfo.slotCardData, antiTankTarget);
            
            ability.AddAbilityUsed();
            
            ClientAntiTankParameter parameter = new()
            {
                destroyerInventoryId = serverParameter.destroyerInventoryId,
                destroyerOwnerId = serverParameter.destroyerOwnerId
            };
           
            cardBattleBoardManager.ClientRpcExecuteCardAbility(JsonUtility.ToJson(parameter), cardEconomyId);
        }

        public override void ClientDoAbility(CardBattleBoardManager cardBattleBoardManager, string clientJsonParameter)
        {
            var parameter = JsonUtility.FromJson<ClientAntiTankParameter>(clientJsonParameter);
            
            BaseCard abilityCard;
            
            if (parameter.destroyerOwnerId == AuthenticationService.Instance.PlayerId)
            {
                abilityCard = cardBattleBoardManager.playerCards.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == parameter.destroyerInventoryId);
            }
            else
            {
                abilityCard = cardBattleBoardManager.enemyCards.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == parameter.destroyerInventoryId);
            }
            
            ClientAntiTankAbilityComponent abilityComponent = abilityCard.gameObject.AddComponent<ClientAntiTankAbilityComponent>();
            abilityComponent.CardBattleBoardManager = cardBattleBoardManager;
            abilityComponent.battleCardToUse = (BattleCard)abilityCard;
            abilityComponent.antiTankTarget = antiTankTarget;
            abilityComponent.AddAbilityUsed();
        }
        
        private struct ServerAntiTankParameter
        {
            public string destroyerInventoryId;
            public string destroyerOwnerId;
        }
        
        private struct ClientAntiTankParameter
        {
            public string destroyerInventoryId;
            public string destroyerOwnerId;
        }
    }
}
