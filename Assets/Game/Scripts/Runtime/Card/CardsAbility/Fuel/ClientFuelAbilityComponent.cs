using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ClientFuelAbilityComponent : ClientAbilityManager
    {
        private int FuelToAdd { get; set; }

        public override void AddAbilityUsed()
        {
            base.AddAbilityUsed();
            
        }
    }
}
