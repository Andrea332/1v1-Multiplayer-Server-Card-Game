using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;

namespace Game
{
    public class ClientAntiTankAbilityComponent : ClientAbilityManager 
    {
        public BaseItemString antiTankTarget;

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
