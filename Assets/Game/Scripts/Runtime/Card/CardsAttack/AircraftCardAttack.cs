using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Services.Authentication;
using UnityEngine;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "AircraftCardAttack", menuName = "Cards/Attack/Aircraft Card Attack")]
    public class AircraftCardAttack : BaseCardAttack
    {
        public List<BaseItemString> attackAbleOverrides;
        private protected override void ServerAttackCard(CardBattleBoardManager cardBattleBoardManager, string attackedSlotOwnerId, SlotInfo attackerSlotInfo, SlotInfo receiverSlotInfo, TableInfo attackerTableInfo, TableInfo receiverTableInfo)
        {
            if(BunkerCheck(cardBattleBoardManager, attackerSlotInfo, receiverSlotInfo, attackerTableInfo, receiverTableInfo)) return;

            base.ServerAttackCard(cardBattleBoardManager, attackedSlotOwnerId, attackerSlotInfo, receiverSlotInfo,  attackerTableInfo, receiverTableInfo);
        }
        private protected override bool AttackerCardTargetIsReceiverCardType(CardBattleBoardManager cardBattleBoardManager, SlotInfo attackerSlotInfo, SlotInfo receiverSlotInfo)
        {
            if (!base.AttackerCardTargetIsReceiverCardType(cardBattleBoardManager, attackerSlotInfo, receiverSlotInfo)) return false;
            
            BaseItemString itemTemplateToAttack = attackAbleOverrides.Find(x => x.ItemValue == receiverSlotInfo.slotCardData.cardEconomyId);
            
            if(itemTemplateToAttack != null)
            {
                return false;
            }

            string message = "BattleCard that is attacking can not attack a battleCard that has a type different from OVERRIDED target types";
            Debug.Log(message);
            cardBattleBoardManager.PingToClient(message);
            return true;
        }
    }
}
