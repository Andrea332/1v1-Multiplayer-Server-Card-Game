using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ServerRadioAbility : ServerAbilityManager
    {
         private CardData MainCardData { get; }
        private int AttackTimesToAdd { get; }
        private int MainCardDamageBeforeDestroying { get; }
        
        public ServerRadioAbility(CardBattleBoardManager cardBattleBoardManager, CardData cardDataUsed, CardData mainCardData, int attackTimesToAdd, int mainCardDamageBeforeDestroying) : base(cardBattleBoardManager, cardDataUsed)
        {
            CardBattleBoardManager = cardBattleBoardManager;
            CardDataUsed = cardDataUsed;
            MainCardData = mainCardData;
            AttackTimesToAdd = attackTimesToAdd;
            MainCardDamageBeforeDestroying = mainCardDamageBeforeDestroying;
        }
        public override void AddAbilityUsed()
        {
            AbilityUsed = true;
            CardDataUsed.optionalParameter = this;
            AddAttackTimesAmount(AttackTimesToAdd);
            MainCardData.onHealthChanged.AddListener(OnMainCardDamaged);
        }

        public override void RemoveAbilityUsed()
        {
            AbilityUsed = false;
            CardDataUsed.optionalParameter = null;
            MainCardData.onHealthChanged.RemoveListener(OnMainCardDamaged);
        }

        private protected override void OnTurnChanged()
        {
            AbilityUsed = false;
        }

        private void AddAttackTimesAmount(int attackTimesToAdd)
        {
            //Remove one attack to permit one more attack, example: -1 < 1 so we can do two attacks -1 and 0
            MainCardData.currentAttackTimes -= attackTimesToAdd;
        }
        
        private void OnMainCardDamaged(int currentHealth, int defaultHealth, int damage)
        {
            if (currentHealth >= defaultHealth - MainCardDamageBeforeDestroying && currentHealth > 0) return;

            CardDataUsed.ResetToDefaultValues();

            var supportTableInfo = CardBattleBoardManager.tableInfos.Find(tableInfo =>
            {
                return tableInfo.slotInfos.Find(slotInfo =>
                {
                    if (slotInfo.slotSupportCardData == null) return false;

                    return slotInfo.slotSupportCardData == CardDataUsed;
                }) != null;
            });

            supportTableInfo.cemeteryCardDatas.Add(CardDataUsed);

            var slotInfo = supportTableInfo.slotInfos.Find(slotInfo => slotInfo.slotSupportCardData == CardDataUsed);

            slotInfo.slotSupportCardData = null;

            RemoveAbilityUsed();
            
            CardBattleBoardManager.ClientRpcDestroySlotSupportCard(slotInfo.id, supportTableInfo.playerId);
            CardBattleBoardManager.ClientRpcUpdateUI(TableInfo.TableInfosToStruct(CardBattleBoardManager.tableInfos));
        }
    }
}
