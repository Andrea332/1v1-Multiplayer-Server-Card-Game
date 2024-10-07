using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Game
{
    public class ServerMineAbility : ServerAbilityManager
    {
        public BaseItemString MineTarget { get; set; }
        public int MineDamage { get; set; }
        
        public ServerMineAbility(CardBattleBoardManager cardBattleBoardManager, CardData cardDataUsed, BaseItemString mineTarget, int mineDamage) : base(cardBattleBoardManager, cardDataUsed)
        {
            CardBattleBoardManager = cardBattleBoardManager;
            CardDataUsed = cardDataUsed;
            MineTarget = mineTarget;
            MineDamage = mineDamage;
        }

        public override void AddAbilityUsed()
        {
            AbilityUsed = true;
            CardDataUsed.optionalParameter = this;
        }

        public override void RemoveAbilityUsed()
        {
            AbilityUsed = false;
            CardDataUsed.optionalParameter = null;
        }
    }
}
