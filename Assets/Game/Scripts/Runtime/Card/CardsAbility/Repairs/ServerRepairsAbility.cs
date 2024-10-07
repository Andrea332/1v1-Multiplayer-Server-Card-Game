using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ServerRepairsAbility : ServerAbilityManager
    {
        private CardData CardDataToRestore { get; set; }
        private int HealthToRestore { get; set; }
        private List<string> InventoryIdsToDamage { get; set; }
        
        public ServerRepairsAbility(CardBattleBoardManager cardBattleBoardManager, CardData cardDataUsed, CardData cardDataToRestore, int healthToRestore, List<string> cardInventoryIdsToDamage) : base(cardBattleBoardManager, cardDataUsed)
        {
            CardBattleBoardManager = cardBattleBoardManager;
            CardDataUsed = cardDataUsed;
            CardDataToRestore = cardDataToRestore;
            HealthToRestore = healthToRestore;
            InventoryIdsToDamage = cardInventoryIdsToDamage;
        }
        public override void AddAbilityUsed()
        {
            base.AddAbilityUsed();

            RestoreHealth(HealthToRestore);

            DamageCards(InventoryIdsToDamage);
        }

        private void RestoreHealth(int currentHealthToRestore)
        {
            var tempHealth = CardDataToRestore.health + currentHealthToRestore;

            if (tempHealth <= CardDataToRestore.defaultHealth)
            {
                CardDataToRestore.health = tempHealth;
                return;
            }
            
            CardDataToRestore.health = CardDataToRestore.defaultHealth;
        }

        private void DamageCards(List<string> cardInventoryIds)
        {
            foreach (var cardInventoryId in cardInventoryIds)
            {
                CardBattleBoardManager.tableInfos.ForEach(tableInfo =>
                    {
                        tableInfo.slotInfos.ForEach(slotInfo =>
                        {
                            if (slotInfo.slotCardData?.cardInventoryId == cardInventoryId)
                            {
                                slotInfo.slotCardData.DamageCard(1);
                            }
                        });
                     
                    }
                );
            }
        }
    }
}
