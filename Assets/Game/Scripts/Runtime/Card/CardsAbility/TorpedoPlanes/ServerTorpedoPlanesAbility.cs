using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ServerTorpedoPlanesAbility : ServerAbilityManager
    { 
        private CardData MainCardData { get; }
        private int AttackToAdd { get; }
        private int MainCardDamageBeforeDestroying { get; }
        
        public ServerTorpedoPlanesAbility(CardBattleBoardManager cardBattleBoardManager, CardData cardDataUsed, CardData mainCardData, int attackToAdd, int mainCardDamageBeforeDestroying) : base(cardBattleBoardManager, cardDataUsed)
        {
            CardBattleBoardManager = cardBattleBoardManager;
            CardDataUsed = cardDataUsed;
            MainCardData = mainCardData;
            AttackToAdd = attackToAdd;
            MainCardDamageBeforeDestroying = mainCardDamageBeforeDestroying;
        }
         public override void AddAbilityUsed()
        {
            AbilityUsed = true;
            CardDataUsed.optionalParameter = this;
            AddAttackAmount(AttackToAdd);
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

        private void AddAttackAmount(int attackToAdd)
        {
            MainCardData.attack += attackToAdd;
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
