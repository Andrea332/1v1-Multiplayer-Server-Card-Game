using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Game
{
    public class ServerAntiTankAbility : ServerAbilityManager
    {
        public BaseItemString AntiTankTarget { get; set; }
        public ServerAntiTankAbility(CardBattleBoardManager cardBattleBoardManager, CardData cardDataUsed, BaseItemString antiTankTarget) : base(cardBattleBoardManager, cardDataUsed)
        {
            CardBattleBoardManager = cardBattleBoardManager;
            CardDataUsed = cardDataUsed;
            AntiTankTarget = antiTankTarget;
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
