using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;

namespace Game
{
    public class ClientSmokescreenAbilityComponent : ClientAbilityManager
    {
        public SmokescreenPresenter presenter;
        public int currentSmokescreenTurns;
        public string abilityOwnerId;
        public override void RemoveAbilityUsed()
        {
            Destroy(presenter.gameObject);
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
            currentSmokescreenTurns--;
                
            presenter.SetParameter(currentSmokescreenTurns);
                
            if(currentSmokescreenTurns > 0) return;

            RemoveAbilityUsed();
        }
    }
}
