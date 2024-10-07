using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ClientAmmunitionAbilityComponent : ClientAbilityManager
    {
        public BattleCard battleCardToAddAttack;
        public int AttackToAdd { get; set; }
        public override void AddAbilityUsed()
        {
            AbilityUsed = true;
            AddAttackAmount(AttackToAdd);
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
        
        private void AddAttackAmount(int attackToAdd)
        {
            if(battleCardToAddAttack.TryGetComponent(out AttackComponent attackComponent))
            {
                attackComponent.Parameter += attackToAdd;
            }
        }
    }
}
