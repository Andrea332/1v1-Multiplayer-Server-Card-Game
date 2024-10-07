using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Game
{
    public class ServerBunkerAbility : ServerAbilityManager
    {
        public CardData CardDataToProtect { get; set; }

        public ServerBunkerAbility(CardBattleBoardManager cardBattleBoardManager, CardData cardDataUsed, CardData cardDataToProtect) : base(cardBattleBoardManager, cardDataUsed)
        {
            CardBattleBoardManager = cardBattleBoardManager;
            CardDataUsed = cardDataUsed;
            CardDataToProtect = cardDataToProtect;
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
