using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Services.Authentication;
using UnityEngine;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "SubmarineDive_Ability", menuName = "Cards/Ability/SubmarineDive")]
    public class SubmarineDive_Ability : BaseCardAbility
    {
        [SerializeField] private int diveTurns;
        [SerializeField] private BaseItemString targetTypeOnDive;
        [SerializeField] private DivePresenter diveUi;
        
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
            
            if (IsAbilityAlreadyUsed(cardBattleBoardManager, currentCardSlot.slotCardData)) return;

            currentCardSlot.slotCardData.targetType = new(){targetTypeOnDive.ItemValue};
            
            ServerDiveAbility submarineDive = new(cardBattleBoardManager, currentCardSlot.slotCardData, currentTurnOwnerId, diveTurns);
            submarineDive.AddAbilityUsed();
            
            ClientSubmarineDiveParameter clientParameter = new()
            {
                abilityOwnerId = playerTableInfo.playerId,
                cardToDive = currentCardSlot.slotCardData.ToStruct()
            };
            
            cardBattleBoardManager.ClientRpcExecuteCardAbility(JsonUtility.ToJson(clientParameter), cardEconomyId);
        }
        
        public override void ClientDoAbility(CardBattleBoardManager cardBattleBoardManager, string clientJsonParameter)
        {
            var parameter = JsonUtility.FromJson<ClientSubmarineDiveParameter>(clientJsonParameter);

            BaseCard submarineCard;
            
            if (parameter.abilityOwnerId == AuthenticationService.Instance.PlayerId)
            {
                submarineCard = cardBattleBoardManager.playerCards.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == parameter.cardToDive.cardInventoryId);
            }
            else
            {
                submarineCard = cardBattleBoardManager.enemyCards.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == parameter.cardToDive.cardInventoryId);
            }

            DivePresenter divePresenter = Instantiate(diveUi, submarineCard.transform, false);
            
            divePresenter.SetParameter(diveTurns);

            ClientDiveAbilityComponent clientSubmarineDive = submarineCard.gameObject.AddComponent<ClientDiveAbilityComponent>();
            clientSubmarineDive.CardBattleBoardManager = cardBattleBoardManager;
            clientSubmarineDive.currentDiveTurns = diveTurns;
            clientSubmarineDive.battleCardToUse = (BattleCard)submarineCard;
            clientSubmarineDive.abilityOwnerId = parameter.abilityOwnerId;
            clientSubmarineDive.divePresenter = divePresenter;
            clientSubmarineDive.AddAbilityUsed();
        }
        
        private struct ClientSubmarineDiveParameter
        {
            public NetworkedCardData cardToDive;
            public string abilityOwnerId;
        }
    }
}
