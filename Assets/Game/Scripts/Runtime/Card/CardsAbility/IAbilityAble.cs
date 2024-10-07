using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public interface IAbilityAble
    {
        public CardBattleBoardManager CardBattleBoardManager { get; set; }
        public bool AbilityUsed { get; set; }
        public void AddAbilityUsed();
        public void RemoveAbilityUsed();
        public void SubscribeToTurnChanged();
        public void UnsubscribeToTurnChanged();
    }
}
