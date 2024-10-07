using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Game
{
    public class ClientMineAbilityComponent : ClientAbilityManager
    {
        public BaseItemString mineTarget;
        public int mineDamage;

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
