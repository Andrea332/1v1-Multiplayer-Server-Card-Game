using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Services.Authentication;
using UnityEngine;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "ShipCardAttack", menuName = "Cards/Attack/Ship Card Attack")]
    public class ShipCardAttack : BaseCardAttack
    {
        public List<BaseItemString> attackAbleOverrides;
        private protected override void ServerAttackCard(CardBattleBoardManager cardBattleBoardManager, string receiverSlotOwnerId, SlotInfo attackerSlotInfo, SlotInfo receiverSlotInfo, TableInfo attackerTableInfo, TableInfo receiverTableInfo)
        {
            //Search for mines
            if(MineCheck(cardBattleBoardManager, attackerSlotInfo, attackerTableInfo, receiverTableInfo)) return;
            
            base.ServerAttackCard(cardBattleBoardManager, receiverSlotOwnerId, attackerSlotInfo, receiverSlotInfo,  attackerTableInfo, receiverTableInfo);
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
