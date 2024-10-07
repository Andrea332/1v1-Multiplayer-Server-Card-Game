using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ClientRepairsAbilityComponent : ClientAbilityManager
    {
        public BattleCard cardDataToRestore;
        public int healthToRestore;
        public int defaultHealth;
        public List<BaseCard> cardsToDamage;
        public override void AddAbilityUsed()
        {
            base.AddAbilityUsed();

            RestoreHealth(healthToRestore, defaultHealth);

            DamageCards(cardsToDamage);
        }

        private void RestoreHealth(int currentHealthToRestore, int currentDefaultHealth)
        {
            if (!cardDataToRestore.TryGetComponent(out HealthComponent healthComponent)) return;
            
            var tempHealth = healthComponent.Parameter + currentHealthToRestore;

            if (tempHealth <= currentDefaultHealth)
            {
                healthComponent.Parameter = tempHealth;
                return;
            }
            
            healthComponent.Parameter = currentDefaultHealth;
        }
        
        private void DamageCards(List<BaseCard> cardsToDamage)
        {
            foreach (var card in cardsToDamage)
            {
                if (card.TryGetComponent(out HealthComponent healthComponent))
                {
                    healthComponent.Parameter--;
                }
            }
        }
    }
}
