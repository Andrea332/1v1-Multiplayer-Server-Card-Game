using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ClientAbilityManager : MonoBehaviour, IAbilityAble
    {
        public CardBattleBoardManager CardBattleBoardManager { get; set; }
        public BattleCard battleCardToUse;
        public bool AbilityUsed { get; set; }
        public virtual void AddAbilityUsed()
        {
            AbilityUsed = true;
            SubscribeToTurnChanged();
        }
        public virtual void RemoveAbilityUsed()
        {
            AbilityUsed = false;
            UnsubscribeToTurnChanged();
            Destroy(this);
        }
        public virtual void SubscribeToTurnChanged()
        {
            CardBattleBoardManager.onYourTurnStarted.AddListener(OnYourTurnStarted);
            CardBattleBoardManager.onEnemyTurnStarted.AddListener(OnEnemyTurnStarted);
        }
        public virtual void UnsubscribeToTurnChanged()  
        {
            CardBattleBoardManager.onYourTurnStarted.RemoveListener(OnYourTurnStarted);
            CardBattleBoardManager.onEnemyTurnStarted.RemoveListener(OnEnemyTurnStarted);
        }
        private protected virtual void OnYourTurnStarted()
        {
            AbilityUsed = false;
        }
        private protected virtual void OnEnemyTurnStarted()
        {
            AbilityUsed = false;
        }
    }
}
