using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "Bunker_Ability", menuName = "Cards/Ability/Bunker")]
    public class Bunker_Ability : BaseCardAbility
    {
        [SerializeField] private BaseItemString tankItemId;
        [SerializeField] private BaseItemString aircraftItemId;
        [SerializeField] private int uiPosition;
        [SerializeField] private SlotAvailabilityManagerBunkerAbility slotAvailabilityManagerBunkerPrefab;
        private SlotAvailabilityManagerBunkerAbility spawnedSlotAvailabilityBunkerManager;
        private string cardEconomyId;
        private string cardInventoryId;
        public override void ClientPrepareAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId)
        {
            this.cardEconomyId = cardEconomyId;
            this.cardInventoryId = cardInventoryId;
            spawnedSlotAvailabilityBunkerManager = Instantiate(slotAvailabilityManagerBunkerPrefab, cardBattleBoardManager.transform, false);
            spawnedSlotAvailabilityBunkerManager.transform.SetSiblingIndex(uiPosition);
            BaseCard baseCard = cardBattleBoardManager.playerCards.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == cardInventoryId);
            spawnedSlotAvailabilityBunkerManager.OnCardDragToFindSlotBegin(baseCard, cardBattleBoardManager.playerTableSlots, cardBattleBoardManager.enemyTableSlots);
            foreach (var playerTableSlot in  cardBattleBoardManager.playerTableSlots)
            {
                playerTableSlot.onSlotClicked.RemoveAllListeners();
                playerTableSlot.onSlotClicked.AddListener(OnSlotClicked);
            }
        }
        
        private void OnSlotClicked(CardBattleBoardManager cardBattleBoardManager, TableSlot tableSlot, BaseCard baseCard)
        {
            spawnedSlotAvailabilityBunkerManager.OnCardDragToFindSlotEnd(baseCard);
            Destroy(spawnedSlotAvailabilityBunkerManager.gameObject);
            spawnedSlotAvailabilityBunkerManager = null;
            foreach (var playerTableSlot in cardBattleBoardManager.playerTableSlots)
            {
                playerTableSlot.onSlotClicked.RemoveAllListeners();
                playerTableSlot.onSlotClicked.AddListener(cardBattleBoardManager.cardActionsMenuManager.SetMenu);
            }

            ServerBunkerParameter parameter = new()
            {
                bunkerInventoryId = cardInventoryId,
                slotToProtectId = tableSlot.Id,
                bunkerOwnerId = tableSlot.PlayerOwnerId
            };
            
            cardBattleBoardManager.ClientExecuteCardAbility(cardEconomyId, cardInventoryId, JsonUtility.ToJson(parameter));
        }

        public override void ServerDoAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId, string serverJsonParameter)
        {
            var serverParameter = JsonUtility.FromJson<ServerBunkerParameter>(serverJsonParameter);
            
            List<TableInfo> tableInfos = cardBattleBoardManager.tableInfos;
            
            string currentTurnOwnerId = cardBattleBoardManager.currentTurnOwnerId;
            
            TableInfo playerTableInfo = tableInfos.Find(x => x.playerId == currentTurnOwnerId);

            var slotToProtect = playerTableInfo.slotInfos.Find(x => x.id == serverParameter.slotToProtectId);
           
            if (slotToProtect == null)
            {
                string message = "Table slot not found on server";
                Debug.Log(message);
                cardBattleBoardManager.PingToClient(message);
                return;
            }

            if (slotToProtect.slotCardData == null)
            {
                string message = $"Table slot {slotToProtect.id} doesn't have a card on it";
                Debug.Log(message);
                cardBattleBoardManager.PingToClient(message);
                return;
            }
            
            if (slotToProtect.slotCardData.type != tankItemId.ItemValue && slotToProtect.slotCardData.type != aircraftItemId.ItemValue)
            {
                string message = $"Table slot {slotToProtect.id} has a card with different type from {tankItemId.ItemValue} and {aircraftItemId.ItemValue}";
                Debug.Log(message);
                cardBattleBoardManager.PingToClient(message);
                return;
            }

            BattleSlotInfo bunkerSlotInfo = playerTableInfo.slotInfos.Find(x =>
            {
                if (x.slotCardData == null) return false;
               
                return x.slotCardData.cardInventoryId == cardInventoryId;
            });
            
            if (IsAbilityAlreadyUsed(cardBattleBoardManager, bunkerSlotInfo.slotCardData)) return;
            
            ServerBunkerAbility ability = new(cardBattleBoardManager, bunkerSlotInfo.slotCardData, slotToProtect.slotCardData);
            ability.AddAbilityUsed();
            
            ClientBunkerParameter clientParameter = new()
            {
                bunkerInventoryId = cardInventoryId,
                cardToProtectInventoryId = slotToProtect.slotCardData.cardInventoryId,
                bunkerOwnerId = playerTableInfo.playerId
            };
            
            cardBattleBoardManager.ClientRpcExecuteCardAbility(JsonUtility.ToJson(clientParameter), cardEconomyId);
        }

        public override void ClientDoAbility(CardBattleBoardManager cardBattleBoardManager, string clientJsonParameter)
        {
            var clientParameter = JsonUtility.FromJson<ClientBunkerParameter>(clientJsonParameter);
            
            if (clientParameter.bunkerOwnerId == AuthenticationService.Instance.PlayerId)
            {
                SetBunker(cardBattleBoardManager, cardBattleBoardManager.playerCards, clientParameter);
                return;
            }
            
            SetBunker(cardBattleBoardManager, cardBattleBoardManager.enemyCards, clientParameter);
        }

        private void SetBunker(CardBattleBoardManager cardBattleBoardManager, List<BaseCard> cardList,  ClientBunkerParameter clientParameter)
        {
            BaseCard bunkerCard = cardList.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == clientParameter.bunkerInventoryId);
            BaseCard cardToProtect = cardList.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == clientParameter.cardToProtectInventoryId);
            ClientBunkerAbilityComponent ability = bunkerCard.gameObject.AddComponent<ClientBunkerAbilityComponent>();
            ability.CardBattleBoardManager = cardBattleBoardManager;
            ability.battleCardToUse = (BattleCard)bunkerCard;
            ability.battleCardToProtect = (BattleCard)cardToProtect;
            ability.AddAbilityUsed();
        }

        public struct ServerBunkerParameter
        {
            public string bunkerInventoryId;
            public long slotToProtectId;
            public string bunkerOwnerId;
        }
        
        public struct ClientBunkerParameter
        {
            public string bunkerInventoryId;
            public string cardToProtectInventoryId;
            public string bunkerOwnerId;
        }
    }
}
