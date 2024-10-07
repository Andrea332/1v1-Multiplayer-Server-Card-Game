using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Game
{
    [Serializable]
    public class CardData
    {
        public string cardName;
        public string cardEconomyId;
        public string cardInventoryId;
        public string effectDescription;
        public string requirement;
        public int attack;
        public int maxAttackTimes = 1;
        public int cost;
        public int health;
        public int defaultAttack;
        public int defaultCost;
        public int defaultHealth;
        public int maxDeckable;
        public string rarity;
        public List<string> targetType;
        public List<string> defaultTargetType;
        public string type;
        
        public object optionalParameter;
        public int currentAttackTimes;
        public bool AttackDone => currentAttackTimes >= maxAttackTimes;

        public void DamageCard(int damageAmount)
        {
            health -= damageAmount;
            onHealthChanged?.Invoke(health, defaultHealth, damageAmount);
        }

        public UnityEvent<int, int, int> onHealthChanged;


        public void ResetToDefaultValues()
        {
            health = defaultHealth;
            attack = defaultAttack;
            cost = defaultCost;
            currentAttackTimes = maxAttackTimes;
        }

        public NetworkedCardData ToStruct()
        {
            return new NetworkedCardData
            {
                cardName = cardName,
                cardEconomyId = cardEconomyId,
                cardInventoryId = cardInventoryId,
                effectDescription = effectDescription,
                requirement = requirement,
                attack = attack,
                cost = cost,
                health = health,
                maxDeckable = maxDeckable,
                rarity = rarity,
                targetType = targetType?.ToArray(),
                type = type
            };
        }
    }

  
}
