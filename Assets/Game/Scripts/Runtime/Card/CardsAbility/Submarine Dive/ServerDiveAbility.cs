using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ServerDiveAbility : ServerAbilityManager
    {
        public string AbilityOwnerId { get; set; }
        public int CurrentDiveTurns{ get; set; }
        public ServerDiveAbility(CardBattleBoardManager cardBattleBoardManager, CardData cardDataUsed, string abilityOwnerId, int currentDiveTurns) : base(cardBattleBoardManager, cardDataUsed)
        {
            CardBattleBoardManager = cardBattleBoardManager;
            CardDataUsed = cardDataUsed;
            AbilityOwnerId = abilityOwnerId;
            CurrentDiveTurns = currentDiveTurns;
        }
        public override void RemoveAbilityUsed()
        {
            base.RemoveAbilityUsed();
                
            CardDataUsed.targetType = CardDataUsed.defaultTargetType;
        }
        private protected override void OnTurnChanged()
        {
            if(AbilityOwnerId != CardBattleBoardManager.currentTurnOwnerId) return;
                
            CurrentDiveTurns--;
                
            if(CurrentDiveTurns > 0) return;

            RemoveAbilityUsed();
        }
    }
}
