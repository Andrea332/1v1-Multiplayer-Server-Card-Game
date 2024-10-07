using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "FuelCardPlaceOnGround", menuName = "Cards/Place On Ground/Fuel Card Place On Ground")]
    public class FuelCardPlaceOnGround : SupportCardPlaceOnGround
    {
        public override bool CanNotBePlaced(CardBattleBoardManager cardBattleBoardManager, int slotId, string slotOwnerId, string cardToPlaceInventoryId, string cardPlayerOwnerId, string cardToPlaceType)
        {
            TableInfo otherPlayerTableInfo = cardBattleBoardManager.tableInfos.Find(x => x.playerId != slotOwnerId);
            
            if (CheckIfSetterPlayerIsSlotOwner(cardBattleBoardManager, slotId, slotOwnerId, cardToPlaceInventoryId, cardPlayerOwnerId)) return true;
            if (CheckIfSlotHasnotMainCard(cardBattleBoardManager, slotId, slotOwnerId, cardToPlaceInventoryId)) return true;
            if (CheckSmokescreens(cardBattleBoardManager, slotId, slotOwnerId, cardToPlaceInventoryId, cardPlayerOwnerId, otherPlayerTableInfo)) return true;

            return false;
        }
    }
}
