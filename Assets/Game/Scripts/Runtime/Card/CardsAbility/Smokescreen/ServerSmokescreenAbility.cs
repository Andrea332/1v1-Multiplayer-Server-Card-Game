using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ServerSmokescreenAbility : ServerAbilityManager
    {
        public string AbilityOwnerId { get; set; }
        public int CurrentSmokescreenTurns { get; set; }
        public SlotInfo SlotInfoToSmokescreen { get; set; }

        public ServerSmokescreenAbility(CardBattleBoardManager cardBattleBoardManager, CardData cardDataUsed, string abilityOwnerId, int currentSmokescreenTurns, SlotInfo slotInfoToSmokescreen) : base(cardBattleBoardManager, cardDataUsed)
        {
            CardBattleBoardManager = cardBattleBoardManager;
            CardDataUsed = cardDataUsed;
            AbilityOwnerId = abilityOwnerId;
            CurrentSmokescreenTurns = currentSmokescreenTurns;
            SlotInfoToSmokescreen = slotInfoToSmokescreen;
        }
        private protected override void OnTurnChanged()
        {
            if(AbilityOwnerId != CardBattleBoardManager.currentTurnOwnerId) return;
                
            CurrentSmokescreenTurns--;
                
            if(CurrentSmokescreenTurns > 0) return;

            RemoveAbilityUsed();
        }
    }
}
