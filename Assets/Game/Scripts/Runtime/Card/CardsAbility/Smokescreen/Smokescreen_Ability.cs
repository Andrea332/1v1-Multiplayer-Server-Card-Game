using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Services.Authentication;
using UnityEngine;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "Smokescreen_Ability", menuName = "Cards/Ability/Smokescreen")]
    public class Smokescreen_Ability : BaseCardAbility
    {
        [SerializeField] private BaseItemString defenseItemId;
        [SerializeField] private int smokescreenTurns;
        [SerializeField] private SmokescreenPresenter smokescreenUi;
        [SerializeField] private int uiPosition;
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
            spawnedSlotAvailabilityManager.OnCardDragToFindSlotBegin(baseCard, cardBattleBoardManager.playerTableSlots, cardBattleBoardManager.enemyTableSlots);
            foreach (var enemyTableSlot in  cardBattleBoardManager.enemyTableSlots)
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
            }

            ServerSmokescreenParameter parameter = new()
            {
                slotToSmokescreenId = tableSlot.Id
            };
            
            cardBattleBoardManager.ClientExecuteCardAbility(cardEconomyId, cardInventoryId, JsonUtility.ToJson(parameter));
        }
        public override void ServerDoAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId, string serverJsonParameter)
        {
            var serverParameter = JsonUtility.FromJson<ServerSmokescreenParameter>(serverJsonParameter);
            
            List<TableInfo> tableInfos = cardBattleBoardManager.tableInfos;
            
            string currentTurnOwnerId = cardBattleBoardManager.currentTurnOwnerId;
            
            TableInfo playerTableInfo = tableInfos.Find(x => x.playerId == currentTurnOwnerId);
            
            TableInfo enemyTableInfo = tableInfos.Find(x => x.playerId != currentTurnOwnerId);
            
            BattleSlotInfo abilityCardSlot = playerTableInfo.slotInfos.Find(x =>
            {
                if (x.slotCardData == null) return false;
                        
                return x.slotCardData.cardInventoryId == cardInventoryId;
            });
            
            if (IsAbilityAlreadyUsed(cardBattleBoardManager, abilityCardSlot.slotCardData)) return;
            
            BattleSlotInfo currentSlotToSmokescreen = enemyTableInfo.slotInfos.Find(x =>
            {
                if (!string.IsNullOrEmpty(x.slotData.types.Find(slotType=>slotType == defenseItemId.ItemValue))) return false;
                        
                return x.id == serverParameter.slotToSmokescreenId;
            });
            
            ServerSmokescreenAbility ability = new(cardBattleBoardManager, abilityCardSlot.slotCardData, currentTurnOwnerId, smokescreenTurns, currentSlotToSmokescreen);
            ability.AddAbilityUsed();
            
            ClientSmokescreenParameter submarineDiveParameter = new()
            {
                abilityOwnerId = playerTableInfo.playerId,
                abilityCardInventoryId = abilityCardSlot.slotCardData.cardInventoryId,
                slotToSmokescreenId = currentSlotToSmokescreen.id
            };
            
            cardBattleBoardManager.ClientRpcExecuteCardAbility(JsonUtility.ToJson(submarineDiveParameter), cardEconomyId);
        }
        
        public override void ClientDoAbility(CardBattleBoardManager cardBattleBoardManager, string clientJsonParameter)
        {
            var parameter = JsonUtility.FromJson<ClientSmokescreenParameter>(clientJsonParameter);

            TableSlot slotToSmokescreen;
            BaseCard abilityCard;
            
            if (parameter.abilityOwnerId == AuthenticationService.Instance.PlayerId)
            {
                slotToSmokescreen = cardBattleBoardManager.enemyTableSlots.Find(x => x.Id == parameter.slotToSmokescreenId);
                abilityCard = cardBattleBoardManager.playerCards.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == parameter.abilityCardInventoryId);
            }
            else
            {
                slotToSmokescreen = cardBattleBoardManager.playerTableSlots.Find(x => x.Id == parameter.slotToSmokescreenId);
                abilityCard = cardBattleBoardManager.enemyCards.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == parameter.abilityCardInventoryId);
            }

            SmokescreenPresenter presenter = Instantiate(smokescreenUi, slotToSmokescreen.transform, false);
            
            presenter.SetParameter(smokescreenTurns);

            ClientSmokescreenAbilityComponent abilityComponent = abilityCard.gameObject.AddComponent<ClientSmokescreenAbilityComponent>();
            abilityComponent.CardBattleBoardManager = cardBattleBoardManager;
            abilityComponent.currentSmokescreenTurns = smokescreenTurns;
            abilityComponent.battleCardToUse = (BattleCard)abilityCard;
            abilityComponent.abilityOwnerId = parameter.abilityOwnerId;
            abilityComponent.presenter = presenter;
            abilityComponent.AddAbilityUsed();
        }
        private struct ServerSmokescreenParameter
        {
            public long slotToSmokescreenId;
        }
        private struct ClientSmokescreenParameter
        {
            public long slotToSmokescreenId;
            public string abilityCardInventoryId;
            public string abilityOwnerId;
        }
    }
}
