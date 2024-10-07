using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "SupportCardPlaceOnGround", menuName = "Cards/Place On Ground/Support Card Place On Ground")]
    public class SupportCardPlaceOnGround : BaseCardPlaceOnGround
    {
        public override bool CanNotBePlaced(CardBattleBoardManager cardBattleBoardManager, int slotId, string slotOwnerId, string cardToPlaceInventoryId, string cardPlayerOwnerId, string cardToPlaceType)
        {
            TableInfo otherPlayerTableInfo = cardBattleBoardManager.tableInfos.Find(x => x.playerId != slotOwnerId);
            
            if (CheckIfSetterPlayerIsSlotOwner(cardBattleBoardManager, slotId, slotOwnerId, cardToPlaceInventoryId, cardPlayerOwnerId)) return true;
            if (CheckIfSlotHasnotMainCard(cardBattleBoardManager, slotId, slotOwnerId, cardToPlaceInventoryId)) return true;
            if (CheckSmokescreens(cardBattleBoardManager, slotId, slotOwnerId, cardToPlaceInventoryId, cardPlayerOwnerId, otherPlayerTableInfo)) return true;
            if (CheckFuel(cardBattleBoardManager, slotId, slotOwnerId, cardToPlaceInventoryId)) return true;

            return false;
        }
        /// <summary>
        /// Check if the slot has a main card on it
        /// </summary>
        /// <param name="slotId"></param>
        /// <param name="slotOwnerId"></param>
        /// <param name="cardInventoryId"></param>
        /// <param name="slotInfo"></param>
        /// <param name="turnOwnerTableInfo"></param>
        /// <returns></returns>
        private protected bool CheckIfSlotHasnotMainCard(CardBattleBoardManager cardBattleBoardManager, int slotId, string slotOwnerId, string cardInventoryId)
        {
            TableInfo turnOwnerTableInfo = cardBattleBoardManager.tableInfos.Find(x => x.playerId == slotOwnerId);

            BattleSlotInfo slotInfo = turnOwnerTableInfo.slotInfos.Find(x => x.id == slotId);
            
            if (slotInfo.slotCardData != null) return false;
            string message = $"Slot {slotInfo.id} of player {slotOwnerId} doesn't have a main card on it";
            cardBattleBoardManager.PingToClient(message);
            Debug.Log(message);
            cardBattleBoardManager.ClientRpcDenySetSlotWithCard(slotId, slotOwnerId, cardInventoryId);
            return true;

        }
        private protected override void ServerPlaceCardOnGround(CardBattleBoardManager cardBattleBoardManager, int slotId, string slotOwnerId, string cardPlayerOwnerId, string cardToPlaceInventoryId)
        {
            TableInfo turnOwnerTableInfo = cardBattleBoardManager.tableInfos.Find(x => x.playerId == slotOwnerId);

            BattleSlotInfo slotInfo = turnOwnerTableInfo.slotInfos.Find(x => x.id == slotId);
            
            CardData cardData = turnOwnerTableInfo.handCardDatas.Find(x => x.cardInventoryId == cardToPlaceInventoryId);
            
            turnOwnerTableInfo.handCardDatas.Remove(cardData);
        
            slotInfo.slotSupportCardData = cardData;

            turnOwnerTableInfo.currentFuel -= cardData.cost;
                
            cardBattleBoardManager.ClientRpcPlaceCard(slotId, slotOwnerId, cardData.ToStruct());
                
            var networkedTableInfos = TableInfo.TableInfosToStruct(cardBattleBoardManager.tableInfos);
        
            cardBattleBoardManager.ClientRpcUpdateUI(networkedTableInfos);
            
            cardBattleBoardManager.TryStartPrepareAbility(cardData, cardPlayerOwnerId);
        }

        public override void ClientPlaceCard(CardBattleBoardManager cardBattleBoardManager, int slotId, string slotOwnerId, NetworkedCardData networkedCardData)
        {
            TableBattleSlot tableSlot;
            if (slotOwnerId == AuthenticationService.Instance.PlayerId)
            {
                BattleCard battleCardToSet = (BattleCard)cardBattleBoardManager.playerCards.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == networkedCardData.cardInventoryId);
                tableSlot = cardBattleBoardManager.playerTableSlots.Find(x => x.Id == slotId);
                tableSlot.SetSupportCard(battleCardToSet);
                battleCardToSet.GetComponentInChildren<MaxDeckablePresenter>(true).gameObject.SetActive(false);
                return;
            }

            var enemyCardToUse = cardBattleBoardManager.enemyHandFakeCards.Find(card =>
            {
                if (card.TryGetComponent(out CardInventoryIdComponent cardInventoryIdComponent))
                {
                    return cardInventoryIdComponent.Parameter == networkedCardData.cardInventoryId;
                }
                
                return false;
            });
            
            if (enemyCardToUse == null)
            {
                BaseCard fakeCard = cardBattleBoardManager.enemyHandFakeCards[0];
                cardBattleBoardManager.enemyHandFakeCards.RemoveAt(0);
                Destroy(fakeCard.gameObject);
                enemyCardToUse = (BattleCard)cardBattleBoardManager.cardBuilder.BuildCardWithReturnType(networkedCardData);
            }
            else
            {
                cardBattleBoardManager.enemyHandFakeCards.Remove(enemyCardToUse);
            }
            
            cardBattleBoardManager.enemyCards.Add(enemyCardToUse);
            tableSlot = cardBattleBoardManager.enemyTableSlots.Find(x => x.Id == slotId);
            tableSlot.SetSupportCard(enemyCardToUse);
            enemyCardToUse.GetComponentInChildren<MaxDeckablePresenter>(true).gameObject.SetActive(false);
        }
    }
}
