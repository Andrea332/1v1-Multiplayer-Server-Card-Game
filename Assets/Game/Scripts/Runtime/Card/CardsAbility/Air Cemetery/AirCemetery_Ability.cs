using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Authentication;
using UnityEngine;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "AirCemetery_Ability", menuName = "Cards/Ability/Air Cemetery")]
    public class AirCemetery_Ability : BaseCardAbility
    {
        [SerializeField] private BaseItemString aircraftType;
        [SerializeField] private AirCemeteryManager airCemeteryManager;
        [SerializeField] private BattleCardBuilder cardBuilder;
        public override void ClientPrepareAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId)
        {
            AirCemeteryManager newAirCemeteryManager = Instantiate(airCemeteryManager, cardBattleBoardManager.transform);
            
            newAirCemeteryManager.onConfirmSelection = cardBattleBoardManager.ClientExecuteCardAbility;
            
            newAirCemeteryManager.BuildAirCemeteryCards(cardBattleBoardManager, cardEconomyId, cardInventoryId);
        }

        public override void ServerDoAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId, string serverJsonParameter)
        {
            var cemeteryAircraftsToResurrect =  JsonUtility.FromJson<AirCemeteryManager.CemeteryAircraftsToResurrect>(serverJsonParameter);

            List<TableInfo> tableInfos = cardBattleBoardManager.tableInfos;
            
            string currentTurnOwnerId = cardBattleBoardManager.currentTurnOwnerId;
            
            TableInfo playerTableInfo = tableInfos.Find(x => x.playerId == currentTurnOwnerId);
            
            if (playerTableInfo.qgSlotInfo.slotCardData == null)
            {
                cardBattleBoardManager.PingToClient("Qg not present");
                return;
            }

            List<BattleSlotInfo> slotsToUse = playerTableInfo.slotInfos.FindAll(battleSlotInfo => 
            {
                if (battleSlotInfo.slotCardData != null) return false;
                
                var aircraftTypeFound = battleSlotInfo.slotData.types.Find(type => type == aircraftType.ItemValue);
                
                return !string.IsNullOrEmpty(aircraftTypeFound);
            });

            List<CardData> cardDataToUse = new();
            
            foreach (var currentCardInventoryId in cemeteryAircraftsToResurrect.cardInventoryIds)
            {
                CardData cardDataToSet = playerTableInfo.cemeteryCardDatas.Find(x => x.cardInventoryId == currentCardInventoryId);
                if(cardDataToSet.type != aircraftType.ItemValue) continue;
                cardDataToUse.Add(cardDataToSet);
            }

            List<BattleSlotInfo> slotInfosUsed = new();

            foreach (var slotInfo in slotsToUse)
            {
                if(cardDataToUse.Count == 0) break;
                int randomNumber = Random.Range(0, cardDataToUse.Count);
                slotInfo.slotCardData = cardDataToUse[randomNumber];
                playerTableInfo.cemeteryCardDatas.Remove(cardDataToUse[randomNumber]);
                cardDataToUse.RemoveAt(randomNumber);
                slotInfosUsed.Add(slotInfo);
            }

            AirCemeteryParameter parameter = new()
            {
                slotInfosAffected = slotInfosUsed.Select(battleSlotInfo => battleSlotInfo.ToStruct()).ToArray(),
                abilityOwnerId = playerTableInfo.playerId
            };

            string clientJsonParameter = JsonUtility.ToJson(parameter);
           
            cardBattleBoardManager.ClientRpcExecuteCardAbility(clientJsonParameter, cardEconomyId);
        }
        
        public override void ClientDoAbility(CardBattleBoardManager cardBattleBoardManager, string jsonParameter)
        {
            AirCemeteryParameter parameter = JsonUtility.FromJson<AirCemeteryParameter>(jsonParameter);
            
            TableSlot tableSlot;
            if (parameter.abilityOwnerId != AuthenticationService.Instance.PlayerId)
            {
                Destroy(cardBattleBoardManager.enemyQgCard);

                cardBattleBoardManager.enemyQgCard = null;
                
                foreach (var slotInfoAffected in parameter.slotInfosAffected)
                {
                    BattleCard battleCardToSet = (BattleCard)cardBattleBoardManager.cemeteryCards.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == slotInfoAffected.slotCardData.cardInventoryId);
                    tableSlot = cardBattleBoardManager.playerTableSlots.Find(x => x.Id == slotInfoAffected.id);
                    cardBattleBoardManager.cemeteryCards.Remove(battleCardToSet);
                    cardBattleBoardManager.playerCards.Add(battleCardToSet);
                    tableSlot.SetCard(battleCardToSet);
                    battleCardToSet.GetComponentInChildren<MaxDeckablePresenter>(true).gameObject.SetActive(false);
                }
               
                return;
            }
            
            cardBattleBoardManager.enemyCards.Remove(cardBattleBoardManager.playerQgSlot.CurrentBattleCard);
               
            cardBattleBoardManager.cemeteryCards.Add(cardBattleBoardManager.playerQgSlot.CurrentBattleCard);
                
            cardBattleBoardManager.playerQgSlot.CurrentBattleCard.transform.SetParent(cardBattleBoardManager.playerCemeteryCardPosition, false);
                
            cardBattleBoardManager.playerQgSlot.CurrentBattleCard = null;
            
            foreach (var slotInfoAffected in parameter.slotInfosAffected)
            {
                BaseCard fakeCard = cardBattleBoardManager.enemyHandFakeCards[0];
                cardBattleBoardManager.enemyHandFakeCards.RemoveAt(0);
                Destroy(fakeCard);
                BattleCard battleCard = (BattleCard)cardBuilder.BuildCardWithReturnType(slotInfoAffected.slotCardData);
                tableSlot = cardBattleBoardManager.enemyTableSlots.Find(x => x.Id == slotInfoAffected.id);
                tableSlot.SetCard(battleCard);
                cardBattleBoardManager.enemyCards.Add(battleCard);
                battleCard.GetComponentInChildren<MaxDeckablePresenter>(true).gameObject.SetActive(false);
            }
        }
        
        private struct AirCemeteryParameter
        {
            public NetworkedBattleSlotInfo[] slotInfosAffected;
            public string abilityOwnerId;
        }
    }
}
