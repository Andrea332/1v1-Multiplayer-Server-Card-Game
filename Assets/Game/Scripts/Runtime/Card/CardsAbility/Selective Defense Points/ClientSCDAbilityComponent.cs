using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Game
{
    public class ClientSCDAbilityComponent : ClientAbilityManager
    {
        public int pointsToEarn;
        public List<BaseItemString> canBeDamageBy;
    }
}
