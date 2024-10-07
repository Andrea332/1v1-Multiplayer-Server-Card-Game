using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "TorpedoPlanesCardPlaceOnGround", menuName = "Cards/Place On Ground/Torpedo Planes Card Place On Ground")]
    public class TorpedoPlanesCardPlaceOnGround : SupportCardPlaceOnGround
    {
        public List<BaseItemString> placeAbleItems;
        public override bool CanNotBePlaced(CardBattleBoardManager cardBattleBoardManager, int slotId, string slotOwnerId, string cardToPlaceInventoryId, string cardPlayerOwnerId, string cardToPlaceType)
        {
            TableInfo otherPlayerTableInfo = cardBattleBoardManager.tableInfos.Find(x => x.playerId != slotOwnerId);
            
            if (CheckIfSetterPlayerIsSlotOwner(cardBattleBoardManager, slotId, slotOwnerId, cardToPlaceInventoryId, cardPlayerOwnerId)) return true;
            if (CheckIfSlotHasnotMainCard(cardBattleBoardManager, slotId, slotOwnerId, cardToPlaceInventoryId)) return true;
            if (CheckIfSlotCardTypeIsCompatible(cardBattleBoardManager, slotId, slotOwnerId, cardToPlaceInventoryId)) return true;
            if (CheckSmokescreens(cardBattleBoardManager, slotId, slotOwnerId, cardToPlaceInventoryId, cardPlayerOwnerId, otherPlayerTableInfo)) return true;

            return false;
        }
        /// <summary>
        /// Check if slot's card type is compatible
        /// </summary>
        /// <param name="slotId"></param>
        /// <param name="slotOwnerId"></param>
        /// <param name="cardInventoryId"></param>
        /// <returns></returns>
        private protected bool CheckIfSlotCardTypeIsCompatible(CardBattleBoardManager cardBattleBoardManager, int slotId, string slotOwnerId, string cardInventoryId)
        {
            TableInfo turnOwnerTableInfo = cardBattleBoardManager.tableInfos.Find(x => x.playerId == slotOwnerId);

            BattleSlotInfo slotInfo = turnOwnerTableInfo.slotInfos.Find(x => x.id == slotId);

            var baseItemFound = placeAbleItems.Find(baseItemString => baseItemString.ItemValue == slotInfo.slotCardData.type);
            
            if (baseItemFound != null) return false;
            
            string message = $"Slot {slotInfo.id} of player {slotOwnerId} has different type from placeable Items";
            cardBattleBoardManager.PingToClient(message);
            Debug.Log(message);
            cardBattleBoardManager.ClientRpcDenySetSlotWithCard(slotId, slotOwnerId, cardInventoryId);
            
            return true;

        }
    }
}
