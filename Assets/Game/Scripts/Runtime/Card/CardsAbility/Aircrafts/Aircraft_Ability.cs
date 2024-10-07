using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "Aircraft_Ability", menuName = "Cards/Ability/Aircraft")]
    public class Aircraft_Ability : BaseCardAbility
    {
        [SerializeField] private int uiPosition;
        [SerializeField] private int slotShift;
        [SerializeField] private SlotAvailabilityManagerAircraftAbility slotAvailabilityManagerAircraftPrefab;
        private SlotAvailabilityManagerAircraftAbility spawnedSlotAvailabilityManagerAircraft;
        private string cardEconomyId;
        private string cardInventoryId;
        public override void ClientPrepareAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId)
        {
            this.cardEconomyId = cardEconomyId;
            this.cardInventoryId = cardInventoryId;
            spawnedSlotAvailabilityManagerAircraft = Instantiate(slotAvailabilityManagerAircraftPrefab, cardBattleBoardManager.transform, false);
            spawnedSlotAvailabilityManagerAircraft.transform.SetSiblingIndex(uiPosition);
            BaseCard baseCard = cardBattleBoardManager.playerCards.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == cardInventoryId);
            spawnedSlotAvailabilityManagerAircraft.OnCardDragToFindSlotBegin(baseCard, cardBattleBoardManager.playerTableSlots, cardBattleBoardManager.enemyTableSlots, slotShift);
            foreach (var playerTableSlot in  cardBattleBoardManager.playerTableSlots)
            {
                playerTableSlot.onSlotClicked.RemoveAllListeners();
                playerTableSlot.onSlotClicked.AddListener(OnSlotClicked);
            }
        }

        private void OnSlotClicked(CardBattleBoardManager cardBattleBoardManager, TableSlot tableSlot, BaseCard baseCard)
        {
            spawnedSlotAvailabilityManagerAircraft.OnCardDragToFindSlotEnd(baseCard);
            Destroy(spawnedSlotAvailabilityManagerAircraft.gameObject);
            spawnedSlotAvailabilityManagerAircraft = null;
            foreach (var playerTableSlot in cardBattleBoardManager.playerTableSlots)
            {
                playerTableSlot.onSlotClicked.RemoveAllListeners();
                playerTableSlot.onSlotClicked.AddListener(cardBattleBoardManager.cardActionsMenuManager.SetMenu);
            }

            AircraftParameter aircraftParameter = new()
            {
                cardToMoveInventoryId = cardInventoryId,
                slotToUseId = tableSlot.Id,
                abilityOwnerId = tableSlot.PlayerOwnerId
            };
            
            cardBattleBoardManager.ClientExecuteCardAbility(cardEconomyId, cardInventoryId, JsonUtility.ToJson(aircraftParameter));
        }

        public override void ServerDoAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId, string serverJsonParameter)
        {
            var parameter = JsonUtility.FromJson<AircraftParameter>(serverJsonParameter);
            
            List<TableInfo> tableInfos = cardBattleBoardManager.tableInfos;
            
            string currentTurnOwnerId = cardBattleBoardManager.currentTurnOwnerId;
            
            TableInfo playerTableInfo = tableInfos.Find(x => x.playerId == currentTurnOwnerId);

           var slotToFill = playerTableInfo.slotInfos.Find(x => x.id == parameter.slotToUseId);
           
           if (slotToFill == null)
           {
               string message = $"Table slot {parameter.slotToUseId} not found on server";
               Debug.Log(message);
               cardBattleBoardManager.PingToClient(message);
               return;
           }

           if (slotToFill.slotCardData != null)
           {
               string message = $"Table slot {parameter.slotToUseId} has already a card on it";
               Debug.Log(message);
               cardBattleBoardManager.PingToClient(message);
               return;
           }
           
           BattleSlotInfo oldSlotInfo = playerTableInfo.slotInfos.Find(x =>
           {
               if (x.slotCardData == null) return false;
               
               return x.slotCardData.cardInventoryId == cardInventoryId;
           });

           if (IsAbilityAlreadyUsed(cardBattleBoardManager, oldSlotInfo.slotCardData)) return;

           if (slotToFill.id > oldSlotInfo.id + slotShift || slotToFill.id < oldSlotInfo.id - slotShift )
           {
               string message = $"Table slot {parameter.slotToUseId} is over the shift move permitted of {slotShift}";
               Debug.Log(message);
               cardBattleBoardManager.PingToClient(message);
               return;
           }

           ServerAbilityManager aircraftMove = new(cardBattleBoardManager,  oldSlotInfo.slotCardData);
           
           aircraftMove.AddAbilityUsed();
           
           slotToFill.slotCardData = oldSlotInfo.slotCardData;

           oldSlotInfo.slotCardData = null;
           
           cardBattleBoardManager.ClientRpcExecuteCardAbility(serverJsonParameter, cardEconomyId);
        }

        public override void ClientDoAbility(CardBattleBoardManager cardBattleBoardManager, string clientJsonParameter)
        {
            var parameter = JsonUtility.FromJson<AircraftParameter>(clientJsonParameter);
            
            if (parameter.abilityOwnerId == AuthenticationService.Instance.PlayerId)
            {
                MoveCard(cardBattleBoardManager, cardBattleBoardManager.playerTableSlots, cardBattleBoardManager.playerCards, parameter);
            
                return;
            }
            
            MoveCard(cardBattleBoardManager, cardBattleBoardManager.enemyTableSlots, cardBattleBoardManager.enemyCards, parameter);
        }

        private void MoveCard(CardBattleBoardManager cardBattleBoardManager, List<TableBattleSlot> tableBattleSlots, List<BaseCard> cards, AircraftParameter parameter)
        {
            BaseCard cardToMove = cards.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == parameter.cardToMoveInventoryId);
                
            ClientAbilityManager clientAircraftMove = cardToMove.gameObject.AddComponent<ClientAbilityManager>();
            clientAircraftMove.CardBattleBoardManager = cardBattleBoardManager;
            clientAircraftMove.battleCardToUse = (BattleCard)cardToMove;
            clientAircraftMove.AddAbilityUsed();
            
            TableSlot oldTableSlot = tableBattleSlots.Find(x =>
            {
                if (x.CurrentBattleCard == null) return false;
                        
                return x.CurrentBattleCard.GetComponent<CardInventoryIdComponent>().Parameter == parameter.cardToMoveInventoryId;
            });
                
            TableSlot newTableSlot = tableBattleSlots.Find(x =>
            {
                if (x.CurrentBattleCard != null) return false;
                        
                return x.Id == parameter.slotToUseId;
            });
            
            newTableSlot.SetCard(oldTableSlot.CurrentBattleCard);
                
            oldTableSlot.UnSetCard();
        }

        private struct AircraftParameter
        {
            public string cardToMoveInventoryId;
            public int slotToUseId;
            public string abilityOwnerId;
        }
    }
}
