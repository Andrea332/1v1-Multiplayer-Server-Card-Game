using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ClientBunkerAbilityComponent : ClientAbilityManager
    {
        public BattleCard battleCardToProtect;
        public override void AddAbilityUsed()
        {
            AbilityUsed = true;
        }

        public override void RemoveAbilityUsed()
        {
            AbilityUsed = false;
            Destroy(this);
        }
    }
}
