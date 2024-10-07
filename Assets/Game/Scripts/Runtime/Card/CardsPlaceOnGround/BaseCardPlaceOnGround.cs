using Unity.Services.Authentication;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "BaseCardPlaceOnGround", menuName = "Cards/Place On Ground/Base Card Place On Ground")]
    public class BaseCardPlaceOnGround : ScriptableObject
    {
        [SerializeField] private int uiPosition;
        [SerializeField] private SlotAvailabilityManager slotAvailabilityManagerPrefab;
        [HideInInspector] public SlotAvailabilityManager spawnedSlotAvailabilityManager;
        
        public void ClientPreparePlaceOnGround(CardBattleBoardManager cardBattleBoardManager, BaseCard cardToPlace)
        {
            spawnedSlotAvailabilityManager = Instantiate(slotAvailabilityManagerPrefab, cardBattleBoardManager.transform, false);
            spawnedSlotAvailabilityManager.transform.SetSiblingIndex(uiPosition);
            spawnedSlotAvailabilityManager.OnCardDragToFindSlotBegin(cardToPlace, cardBattleBoardManager.playerTableSlots, cardBattleBoardManager.enemyTableSlots);
        }

        public void DespawnAvailabilityManager(BaseCard card)
        {
            if (spawnedSlotAvailabilityManager == null) return;
            
            spawnedSlotAvailabilityManager.OnCardDragToFindSlotEnd(card);
            Destroy(spawnedSlotAvailabilityManager.gameObject);
            spawnedSlotAvailabilityManager = null;
        }

        public void ServerTryPlaceCardOnGround(CardBattleBoardManager cardBattleBoardManager, int slotId, string slotOwnerId, string cardToPlaceInventoryId, string cardPlayerOwnerId, string cardToPlaceType)
        {
            if(CanNotBePlaced(cardBattleBoardManager, slotId, slotOwnerId, cardToPlaceInventoryId, cardPlayerOwnerId, cardToPlaceType)) return;
      
            ServerPlaceCardOnGround(cardBattleBoardManager, slotId, slotOwnerId, cardPlayerOwnerId, cardToPlaceInventoryId);
        }

        public virtual bool CanNotBePlaced(CardBattleBoardManager cardBattleBoardManager, int slotId, string slotOwnerId, string cardToPlaceInventoryId, string cardPlayerOwnerId, string cardToPlaceType)
        {
            TableInfo otherPlayerTableInfo = cardBattleBoardManager.tableInfos.Find(x => x.playerId != slotOwnerId);
            
            if (CheckIfSetterPlayerIsSlotOwner(cardBattleBoardManager, slotId, slotOwnerId, cardToPlaceInventoryId, cardPlayerOwnerId)) return true;
            if (CheckSmokescreens(cardBattleBoardManager, slotId, slotOwnerId, cardToPlaceInventoryId, cardPlayerOwnerId, otherPlayerTableInfo)) return true;
            if (CheckIfSlotIsAlreadyUsed(cardBattleBoardManager, slotId, slotOwnerId, cardToPlaceInventoryId)) return true;
            if (CheckSlotAndCardType(cardBattleBoardManager, slotId, slotOwnerId, cardToPlaceInventoryId, cardToPlaceType)) return true;
            if (CheckFuel(cardBattleBoardManager, slotId, slotOwnerId, cardToPlaceInventoryId)) return true;
            
            return false;
        }

        /// <summary>
        /// Check for smokescreens
        /// </summary>
        /// <param name="slotId"></param>
        /// <param name="slotOwnerId"></param>
        /// <param name="cardInventoryId"></param>
        /// <param name="cardPlayerOwnerId"></param>
        /// <param name="otherPlayerTableInfo"></param>
        /// <returns></returns>
        private protected bool CheckSmokescreens(CardBattleBoardManager cardBattleBoardManager, int slotId, string slotOwnerId, string cardInventoryId, string cardPlayerOwnerId, TableInfo otherPlayerTableInfo)
        {
            BattleSlotInfo battleSlotInfo = otherPlayerTableInfo.slotInfos.Find(battleSlotInfo =>
            {
                if (battleSlotInfo.slotCardData == null) return false;

                if (battleSlotInfo.slotCardData.optionalParameter is ServerSmokescreenAbility) return true;

                return false;
            });
            
            if(battleSlotInfo == null) return false;

            if (((ServerSmokescreenAbility)battleSlotInfo.slotCardData.optionalParameter).SlotInfoToSmokescreen.id != slotId) return false;
            
            string message = $"Player {cardPlayerOwnerId} can not place Card {cardInventoryId} on slot {slotId} of player {slotOwnerId} because slot is covered by smoke";
            cardBattleBoardManager.PingToClient(message);
            Debug.Log(message);
            cardBattleBoardManager.ClientRpcDenySetSlotWithCard(slotId, slotOwnerId, cardInventoryId);
            return true;
        }

        /// <summary>
        /// Check if the selected slot is owned by this player, if not deny the action
        /// </summary>
        /// <param name="slotId"></param>
        /// <param name="slotOwnerId"></param>
        /// <param name="cardInventoryId"></param>
        /// <param name="cardPlayerOwnerId"></param>
        /// <returns></returns>
        private protected bool CheckIfSetterPlayerIsSlotOwner(CardBattleBoardManager cardBattleBoardManager, int slotId, string slotOwnerId, string cardInventoryId, string cardPlayerOwnerId)
        {
            if (slotOwnerId == cardPlayerOwnerId) return false;
            
            string message = $"Player {cardPlayerOwnerId} can not place Card {cardInventoryId} on slot {slotId} of player {slotOwnerId}";
            cardBattleBoardManager.PingToClient(message);
            Debug.Log(message);
            cardBattleBoardManager.ClientRpcDenySetSlotWithCard(slotId, slotOwnerId, cardInventoryId);
            return true;
        }

        /// <summary>
        /// Check if the slot is already used by another Card, if it is deny action
        /// </summary>
        /// <param name="slotId"></param>
        /// <param name="slotOwnerId"></param>
        /// <param name="cardInventoryId"></param>
        /// <param name="slotInfo"></param>
        /// <param name="turnOwnerTableInfo"></param>
        /// <returns></returns>
        private protected bool CheckIfSlotIsAlreadyUsed(CardBattleBoardManager cardBattleBoardManager, int slotId, string slotOwnerId, string cardInventoryId)
        {
            TableInfo turnOwnerTableInfo = cardBattleBoardManager.tableInfos.Find(x => x.playerId == slotOwnerId);

            BattleSlotInfo slotInfo = turnOwnerTableInfo.slotInfos.Find(x => x.id == slotId);
            
            if (slotInfo.slotCardData == null) return false;
            string message = $"Slot {slotInfo.id} of player {slotOwnerId} is already used by Card {slotInfo.slotCardData.cardInventoryId} of player {turnOwnerTableInfo.playerId}";
            cardBattleBoardManager.PingToClient(message);
            Debug.Log(message);
            cardBattleBoardManager.ClientRpcDenySetSlotWithCard(slotId, slotOwnerId, cardInventoryId);
            return true;

        }

        /// <summary>
        /// Check if the slot is of the same type of the Card, if not deny action
        /// </summary>
        /// <param name="slotId"></param>
        /// <param name="slotOwnerId"></param>
        /// <param name="cardInventoryId"></param>
        /// <param name="cardType"></param>
        /// <param name="slotInfo"></param>
        /// <returns></returns>
        private protected bool CheckSlotAndCardType(CardBattleBoardManager cardBattleBoardManager, int slotId, string slotOwnerId, string cardInventoryId, string cardType)
        {
            TableInfo turnOwnerTableInfo = cardBattleBoardManager.tableInfos.Find(x => x.playerId == slotOwnerId);
            
            BattleSlotInfo slotInfo = turnOwnerTableInfo.slotInfos.Find(x => x.id == slotId);
            
            if (slotInfo.slotData.types.Contains(cardType)) return false;
            
            string message = $"Slot {slotInfo.id} of player {slotOwnerId} is not of the Card type {cardType}";
            cardBattleBoardManager.PingToClient(message);
            Debug.Log(message);
            cardBattleBoardManager.ClientRpcDenySetSlotWithCard(slotId, slotOwnerId, cardInventoryId);
            return true;
        }

        /// <summary>
        /// Check if the battleCard cost is over the current available fuel for the player, if it is deny action
        /// </summary>
        /// <param name="slotId"></param>
        /// <param name="slotOwnerId"></param>
        /// <param name="cardInventoryId"></param>
        /// <param name="turnOwnerTableInfo"></param>
        /// <param name="cardData"></param>
        /// <returns></returns>
        private protected bool CheckFuel(CardBattleBoardManager cardBattleBoardManager, int slotId, string slotOwnerId, string cardToPlaceInventoryId)
        {
            TableInfo turnOwnerTableInfo = cardBattleBoardManager.tableInfos.Find(x => x.playerId == slotOwnerId);
            
            CardData cardData = turnOwnerTableInfo.handCardDatas.Find(x => x.cardInventoryId == cardToPlaceInventoryId);
            
            if (turnOwnerTableInfo.currentFuel >= cardData.cost) return false;
            
            string message = $"BattleCard cost {cardData.cost} is over current fuel {turnOwnerTableInfo.currentFuel}";
            cardBattleBoardManager.PingToClient(message);
            Debug.Log(message);
            cardBattleBoardManager.ClientRpcDenySetSlotWithCard(slotId, slotOwnerId, cardToPlaceInventoryId);
            return true;
        }
        private protected virtual void ServerPlaceCardOnGround(CardBattleBoardManager cardBattleBoardManager, int slotId, string slotOwnerId, string cardPlayerOwnerId, string cardToPlaceInventoryId)
        {
            TableInfo turnOwnerTableInfo = cardBattleBoardManager.tableInfos.Find(x => x.playerId == slotOwnerId);

            BattleSlotInfo slotInfo = turnOwnerTableInfo.slotInfos.Find(x => x.id == slotId);
            
            CardData cardData = turnOwnerTableInfo.handCardDatas.Find(x => x.cardInventoryId == cardToPlaceInventoryId);
            
            turnOwnerTableInfo.handCardDatas.Remove(cardData);
            
            slotInfo.slotCardData = cardData;

            turnOwnerTableInfo.currentFuel -= cardData.cost;

            cardBattleBoardManager.ClientRpcPlaceCard(slotId, slotOwnerId, cardData.ToStruct());
            
            var networkedTableInfos = TableInfo.TableInfosToStruct(cardBattleBoardManager.tableInfos);
            
            cardBattleBoardManager.ClientRpcUpdateUI(networkedTableInfos);
            
            cardBattleBoardManager.TryStartPrepareAbility(cardData, cardPlayerOwnerId);
        }
    
        public virtual void ClientPlaceCard(CardBattleBoardManager cardBattleBoardManager, int slotId, string slotOwnerId, NetworkedCardData networkedCardData)
        {
            TableBattleSlot tableSlot;
            if (slotOwnerId == AuthenticationService.Instance.PlayerId)
            {
                BattleCard battleCardToSet = (BattleCard)cardBattleBoardManager.playerCards.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == networkedCardData.cardInventoryId);
                tableSlot = cardBattleBoardManager.playerTableSlots.Find(x => x.Id == slotId);
                tableSlot.SetCard(battleCardToSet);
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
            tableSlot.SetCard(enemyCardToUse);
            enemyCardToUse.GetComponentInChildren<MaxDeckablePresenter>(true).gameObject.SetActive(false);
        }
        
    }
}
