using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ServerAbilityManager : IAbilityAble
    {
        private protected CardData CardDataUsed { get; set; }
        public CardBattleBoardManager CardBattleBoardManager { get; set; }
        public bool AbilityUsed { get; set; }
        
        public ServerAbilityManager(CardBattleBoardManager cardBattleBoardManager, CardData cardDataUsed)
        {
            CardBattleBoardManager = cardBattleBoardManager;
            CardDataUsed = cardDataUsed;
        }

        public virtual void AddAbilityUsed()
        {
            AbilityUsed = true;
            CardDataUsed.optionalParameter = this;
            SubscribeToTurnChanged();
        }
        public virtual void RemoveAbilityUsed()
        {
            AbilityUsed = false;
            CardDataUsed.optionalParameter = null;
            UnsubscribeToTurnChanged();
        }
        public virtual void SubscribeToTurnChanged()
        {
            CardBattleBoardManager.onTurnChanged.AddListener(OnTurnChanged);
        }
        public virtual void UnsubscribeToTurnChanged()
        {
            CardBattleBoardManager.onTurnChanged.RemoveListener(OnTurnChanged);
        }
        private protected virtual void OnTurnChanged()
        {
            RemoveAbilityUsed();
        }
    }
}
