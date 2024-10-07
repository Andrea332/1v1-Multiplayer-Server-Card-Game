using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "Mine_Ability", menuName = "Cards/Ability/Mine")]
    public class Mine_Ability : BaseCardAbility
    {
        public BaseItemString mineTarget;
        public int mineDamage;
        public override void ServerPrepareAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId, string cardOwnerId)
        {
            ServerMineParameter parameter = new()
            {
                mineInventoryId = cardInventoryId,
                mineOwnerId = cardOwnerId
            };
            
            ServerDoAbility(cardBattleBoardManager, cardEconomyId, cardInventoryId, JsonUtility.ToJson(parameter));
        }

        public override void ServerDoAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId, string serverJsonParameter)
        {
            var mineParameter = JsonUtility.FromJson<ServerMineParameter>(serverJsonParameter);
            
            TableInfo mineOwnerTableInfo = cardBattleBoardManager.tableInfos.Find(x => x.playerId == mineParameter.mineOwnerId);
            
            BattleSlotInfo mineSlotInfo = mineOwnerTableInfo.slotInfos.Find(x =>
            {
                if (x.slotCardData == null) return false;
                
                return x.slotCardData?.cardInventoryId == mineParameter.mineInventoryId;
            });
            
            if (IsAbilityAlreadyUsed(cardBattleBoardManager, mineSlotInfo.slotCardData)) return;
            
            ServerMineAbility serverMineAbility = new(cardBattleBoardManager, mineSlotInfo.slotCardData, mineTarget, mineDamage);
            
            serverMineAbility.AddAbilityUsed();
            
            ClientMineParameter parameter = new()
            {
                mineInventoryId = mineParameter.mineInventoryId,
                mineOwnerId = mineParameter.mineOwnerId
            };
           
            cardBattleBoardManager.ClientRpcExecuteCardAbility(JsonUtility.ToJson(parameter), cardEconomyId);
        }

        public override void ClientDoAbility(CardBattleBoardManager cardBattleBoardManager, string clientJsonParameter)
        {
            var parameter = JsonUtility.FromJson<ClientMineParameter>(clientJsonParameter);
            
            BaseCard abilityCard;
            
            if (parameter.mineOwnerId == AuthenticationService.Instance.PlayerId)
            {
                abilityCard = cardBattleBoardManager.playerCards.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == parameter.mineInventoryId);
            }
            else
            {
                abilityCard = cardBattleBoardManager.enemyCards.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == parameter.mineInventoryId);
            }
            
            ClientMineAbilityComponent abilityComponent = abilityCard.gameObject.AddComponent<ClientMineAbilityComponent>();
            abilityComponent.CardBattleBoardManager = cardBattleBoardManager;
            abilityComponent.battleCardToUse = (BattleCard)abilityCard;
            abilityComponent.mineDamage = mineDamage;
            abilityComponent.mineTarget = mineTarget;
            abilityComponent.AddAbilityUsed();
        }
        
        private struct ServerMineParameter
        {
            public string mineInventoryId;
            public string mineOwnerId;
        }
        
        private struct ClientMineParameter
        {
            public string mineInventoryId;
            public string mineOwnerId;
        }
    }
}
