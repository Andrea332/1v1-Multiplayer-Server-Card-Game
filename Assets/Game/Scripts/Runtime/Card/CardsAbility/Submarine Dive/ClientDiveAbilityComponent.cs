using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;

namespace Game
{
    public class ClientDiveAbilityComponent : ClientAbilityManager
    {
        public DivePresenter divePresenter;
        public int currentDiveTurns;
        public string abilityOwnerId;
        public override void RemoveAbilityUsed()
        {
            Destroy(divePresenter.gameObject);
            base.RemoveAbilityUsed();
        }
        private protected override void OnYourTurnStarted()
        {
            if(abilityOwnerId != AuthenticationService.Instance.PlayerId) return;
                
            RemoveOneTurn();
        }

        private protected override void OnEnemyTurnStarted()
        {
            if(abilityOwnerId == AuthenticationService.Instance.PlayerId) return;

            RemoveOneTurn();
        }
        private void RemoveOneTurn()
        {
            currentDiveTurns--;
                
            divePresenter.SetParameter(currentDiveTurns);
                
            if(currentDiveTurns > 0) return;

            RemoveAbilityUsed();
        }
    }
}
