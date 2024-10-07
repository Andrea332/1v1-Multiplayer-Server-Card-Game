using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ServerFuelAbility : ServerAbilityManager
    {
        private string AbilityOwnerId { get; set; }
        private int FuelToAdd { get; set; }
        
        public ServerFuelAbility(CardBattleBoardManager cardBattleBoardManager, CardData cardDataUsed, int fuelToAdd, string abilityOwnerId) : base(cardBattleBoardManager, cardDataUsed)
        {
            CardBattleBoardManager = cardBattleBoardManager;
            CardDataUsed = cardDataUsed;
            AbilityOwnerId = abilityOwnerId;
            FuelToAdd = fuelToAdd;
        }

        public override void AddAbilityUsed()
        {
            base.AddAbilityUsed();
            var abilityOwnerTableInfo = CardBattleBoardManager.tableInfos.Find(tableInfo => tableInfo.playerId == AbilityOwnerId);

            var tempFuel = abilityOwnerTableInfo.currentFuel + FuelToAdd;

            if (tempFuel < abilityOwnerTableInfo.maxFuel)
            {
                abilityOwnerTableInfo.currentFuel = tempFuel;
                return;
            }

            abilityOwnerTableInfo.currentFuel = abilityOwnerTableInfo.maxFuel;
        }
    }
}
