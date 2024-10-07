using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Services.Authentication;
using UnityEngine;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "Superkanone_Ability", menuName = "Cards/Ability/Superkanone")]
    public class Superkanone_Ability : BaseCardAbility
    {
        [SerializeField] private BaseItemString defenseType;
        
        public override void ServerDoAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId, string serverJsonParameter)
        {
            List<TableInfo> tableInfos = cardBattleBoardManager.tableInfos;
            
            string currentTurnOwnerId = cardBattleBoardManager.currentTurnOwnerId;
            
            TableInfo enemyTableInfo = tableInfos.Find(x => x.playerId != currentTurnOwnerId);
            
            TableInfo playerTableInfo = tableInfos.Find(x => x.playerId == currentTurnOwnerId);
            
            if (playerTableInfo.qgSlotInfo.slotCardData == null)
            {
                cardBattleBoardManager.PingToClient("Qg not present");
                return;
            }

            List<NetworkedCardData> cardsToDestroy = new();
            
            foreach (var slotInfo in enemyTableInfo.slotInfos)
            {
                if(slotInfo.slotCardData == null) continue;

                if (slotInfo.slotCardData.type != defenseType.ItemValue) continue;
                
                slotInfo.slotCardData.ResetToDefaultValues();
                playerTableInfo.points += slotInfo.slotCardData.cost;
                playerTableInfo.cemeteryCardDatas.Add(slotInfo.slotCardData);
                cardsToDestroy.Add(slotInfo.slotCardData.ToStruct());
                slotInfo.slotCardData = null;
            }

            SuperkanoneParameter parameter = new()
            {
                cardsToDestroy = cardsToDestroy.ToArray(),
                abilityOwnerId = enemyTableInfo.playerId
            };

            string jsonParameter = JsonUtility.ToJson(parameter);
           
            cardBattleBoardManager.ClientRpcExecuteCardAbility(jsonParameter, cardEconomyId);
        }
        
        public override void ClientDoAbility(CardBattleBoardManager cardBattleBoardManager, string jsonParameter)
        {
            SuperkanoneParameter parameter = JsonUtility.FromJson<SuperkanoneParameter>(jsonParameter);
            
            if (parameter.abilityOwnerId == AuthenticationService.Instance.PlayerId)
            {
                Destroy(cardBattleBoardManager.enemyQgCard);

                cardBattleBoardManager.enemyQgCard = null;
                
                foreach (var networkedCardData in parameter.cardsToDestroy)
                {
                    TableSlot tableSlot = cardBattleBoardManager.playerTableSlots.Find(x =>
                    {
                        if (x.CurrentBattleCard == null) return false;
                        
                        return x.CurrentBattleCard.GetComponent<CardInventoryIdComponent>().Parameter == networkedCardData.cardInventoryId;
                    });
                    if (tableSlot.CurrentBattleCard.TryGetComponent(out HealthComponent healthComponent))
                    {
                        healthComponent.ResetParameter();
                    }
                    cardBattleBoardManager.playerCards.Remove(tableSlot.CurrentBattleCard);
                    cardBattleBoardManager.cemeteryCards.Add(tableSlot.CurrentBattleCard);
                    tableSlot.CurrentBattleCard.transform.SetParent(cardBattleBoardManager.playerCemeteryCardPosition, false);
                    tableSlot.CurrentBattleCard = null;
                }
                return;
            }
            
            cardBattleBoardManager.enemyCards.Remove(cardBattleBoardManager.playerQgSlot.CurrentBattleCard);
               
            cardBattleBoardManager.cemeteryCards.Add(cardBattleBoardManager.playerQgSlot.CurrentBattleCard);
                
            cardBattleBoardManager.playerQgSlot.CurrentBattleCard.transform.SetParent(cardBattleBoardManager.playerCemeteryCardPosition, false);
                
            cardBattleBoardManager.playerQgSlot.CurrentBattleCard = null;
            
            foreach (var networkedCardData in parameter.cardsToDestroy)
            {
                TableSlot tableSlot = cardBattleBoardManager.enemyTableSlots.Find(x =>
                {
                    if (x.CurrentBattleCard == null) return false;
                    
                    return x.CurrentBattleCard.GetComponent<CardInventoryIdComponent>().Parameter == networkedCardData.cardInventoryId;
                });
                if (tableSlot.CurrentBattleCard.TryGetComponent(out HealthComponent healthComponent))
                {
                    healthComponent.ResetParameter();
                }
                cardBattleBoardManager.enemyCards.Remove(tableSlot.CurrentBattleCard);
                Destroy(tableSlot.CurrentBattleCard.gameObject);
                tableSlot.CurrentBattleCard = null;
            }
        }
        
        private struct SuperkanoneParameter
        {
            public NetworkedCardData[] cardsToDestroy;
            public string abilityOwnerId;
        }
    }

    
    
   
}
