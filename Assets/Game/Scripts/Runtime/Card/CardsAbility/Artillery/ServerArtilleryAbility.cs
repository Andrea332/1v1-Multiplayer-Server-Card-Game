using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ServerArtilleryAbility : ServerAbilityManager
    {
        public CardData MainCardData { get; set; }
        public int MainCardDamageBeforeDestroying { get; set; }
        
        public ServerArtilleryAbility(CardBattleBoardManager cardBattleBoardManager, CardData cardDataUsed, CardData mainCardData, int mainCardDamageBeforeDestroying) : base(cardBattleBoardManager, cardDataUsed)
        {
            CardBattleBoardManager = cardBattleBoardManager;
            CardDataUsed = cardDataUsed;
            MainCardData = mainCardData;
            MainCardDamageBeforeDestroying = mainCardDamageBeforeDestroying;
        }
        public override void AddAbilityUsed()
        {
            AbilityUsed = true;
            CardDataUsed.optionalParameter = this;
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

        public void OnMainCardDamaged(int currentHealth, int defaultHealth, int damage)
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
