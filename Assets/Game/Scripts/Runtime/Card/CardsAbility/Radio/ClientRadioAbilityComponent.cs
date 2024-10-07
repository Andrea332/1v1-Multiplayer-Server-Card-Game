using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ClientRadioAbilityComponent : ClientAbilityManager
    {
        
        public override void AddAbilityUsed()
        {
            AbilityUsed = true;
        }

        public override void RemoveAbilityUsed()
        {
            AbilityUsed = false;
            Destroy(this);
        }

        private protected override void OnYourTurnStarted()
        {
            AbilityUsed = false;
        }

        private protected override void OnEnemyTurnStarted()
        {
            AbilityUsed = false;
        }
        
      
    }
}
