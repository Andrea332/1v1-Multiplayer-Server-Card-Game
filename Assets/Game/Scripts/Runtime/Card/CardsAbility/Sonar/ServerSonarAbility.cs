using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Game
{
    public class ServerSonarAbility : ServerAbilityManager
    {
        private SlotInfo SlotInfoToAttack { get; }
        private int AttackAmount { get; }
        public ServerSonarAbility(CardBattleBoardManager cardBattleBoardManager, CardData cardDataUsed, SlotInfo slotInfoToAttack, int attackAmount) : base(cardBattleBoardManager, cardDataUsed)
        {
            CardBattleBoardManager = cardBattleBoardManager;
            CardDataUsed = cardDataUsed;
            SlotInfoToAttack = slotInfoToAttack;
            AttackAmount = attackAmount;
        }

        public override void AddAbilityUsed()
        {
            base.AddAbilityUsed();
            SearchSubmarinesAndAttack();
        }

        public void SearchSubmarinesAndAttack()
        {
            var abilityOwnerTableInfo = CardBattleBoardManager.tableInfos.Find(tableInfo => tableInfo.playerId == CardBattleBoardManager.currentTurnOwnerId);
            Debug.Log("11.1");
            var enemyTableInfo =  CardBattleBoardManager.tableInfos.Find(tableInfo => tableInfo.playerId != CardBattleBoardManager.currentTurnOwnerId);
            Debug.Log("11.2");
            if(SlotInfoToAttack == null) return;
            Debug.Log("11.3");
            if (SlotInfoToAttack.slotCardData.optionalParameter is ServerDiveAbility serverDiveAbility)
            {
                Debug.Log("11.4");
                serverDiveAbility.RemoveAbilityUsed();
                SlotInfoToAttack.slotCardData.DamageCard(AttackAmount);
                if (SlotInfoToAttack.slotCardData.health <= 0)
                {
                    Debug.Log("11.6");
                    SlotInfoToAttack.slotCardData.ResetToDefaultValues();
                    abilityOwnerTableInfo.points += SlotInfoToAttack.slotCardData.cost;
                    enemyTableInfo.cemeteryCardDatas.Add(SlotInfoToAttack.slotCardData);
                    SlotInfoToAttack.slotCardData = null;
                }
                Debug.Log("11.5");
                CardBattleBoardManager.ClientRpcAttackSlot(SlotInfoToAttack.id, enemyTableInfo.playerId, CardDataUsed.cardEconomyId, AttackAmount);
              
            }
        }
    }
}
