using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public abstract class BaseCardAbility : ScriptableObject
    {
        public bool startOnDeployed;
        public virtual void ClientPrepareAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId)
        {
            cardBattleBoardManager.CmdExecuteCardAbility(cardEconomyId, cardInventoryId, string.Empty);
        }

        public virtual void ServerPrepareAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId, string cardOwnerId)
        {
            ClientPrepareAbility(cardBattleBoardManager, cardEconomyId, cardInventoryId);
        }
        public abstract void ServerDoAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId, string serverJsonParameter);
        public abstract void ClientDoAbility(CardBattleBoardManager cardBattleBoardManager, string clientJsonParameter);

        public bool IsAbilityAlreadyUsed(CardBattleBoardManager cardBattleBoardManager, CardData cardDataToCheck)
        {
            if (cardDataToCheck.optionalParameter is not IAbilityAble ability || !ability.AbilityUsed) return false;
            
            string message = "Card Ability already used";
            Debug.Log(message);
            cardBattleBoardManager.PingToClient(message);
            return true;
        }

        
    }
}
