using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Services.Authentication;
using UnityEngine;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "TankCardAttack", menuName = "Cards/Attack/Tank Card Attack")]
    public class TankCardAttack : BaseCardAttack
    {
        public List<BaseItemString> attackAbleOverrides;
        public BaseItemString antiTankId;
        private protected override void ServerAttackCard(CardBattleBoardManager cardBattleBoardManager, string receiverSlotOwnerId, SlotInfo attackerSlotInfo, SlotInfo receiverSlotInfo, TableInfo attackerTableInfo, TableInfo receiverTableInfo)
        {
            if(AntiTankOnGround(cardBattleBoardManager, receiverSlotInfo, receiverTableInfo)) return;
            
            if(MineCheck(cardBattleBoardManager, attackerSlotInfo, attackerTableInfo, receiverTableInfo)) return;
            
            if(BunkerCheck(cardBattleBoardManager, attackerSlotInfo, receiverSlotInfo, attackerTableInfo, receiverTableInfo)) return;

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

        public bool AntiTankOnGround(CardBattleBoardManager cardBattleBoardManager, SlotInfo receiverSlotInfo, TableInfo receiverTableInfo)
        {
            List<BattleSlotInfo> destroyerSlotInfo = receiverTableInfo.slotInfos.FindAll(x => x.slotCardData?.optionalParameter is ServerAntiTankAbility);

            if (destroyerSlotInfo.Count == 0)
            {
                return false;
            }

            if (destroyerSlotInfo.Count != 0 && receiverSlotInfo.slotCardData.cardEconomyId == antiTankId.ItemValue)
            {
                return false;
            }
           
            string message = "Tank Battlecard is attacking another card (Not an Anti Tank) while there are one or more antitank on the ground";
            Debug.Log(message);
            cardBattleBoardManager.PingToClient(message);
            
            return true;
        }
    }
}
