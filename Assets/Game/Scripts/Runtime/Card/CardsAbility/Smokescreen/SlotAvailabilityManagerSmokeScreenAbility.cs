using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game
{
    public class SlotAvailabilityManagerSmokeScreenAbility : SlotAvailabilityManager
    {
        [SerializeField] private protected BaseItemString defenseItemId;
        private protected override void SetSlotsGlow(BaseCard cardToSet, List<TableBattleSlot> playerTableSlots, List<TableBattleSlot> enemyTableSlots, object parameter = default)
        {
            //Set player's slots as not valid
            foreach (var playerTableSlot in playerTableSlots)
            {
                var slotGlow = SpawnSlotGlow(playerTableSlot.transform);
                slotGlows.Add(slotGlow);
                SetSlotGlowAsNotPlaceable(slotGlow);
            }
            
            //Check enemy's slots
            foreach (var enemyTableSlot in enemyTableSlots)
            {
                var slotGlow = SpawnSlotGlow(enemyTableSlot.transform);
                slotGlows.Add(slotGlow);

                var slotTypes = enemyTableSlot.GetComponents<CardTypeComponent>();
                
                if (slotTypes.ToList().Find(x => x.Parameter == defenseItemId.ItemValue) != null)
                {
                    SetSlotGlowAsNotPlaceable(slotGlow);
                    continue;
                }
                
                SetSlotGlowAsPlaceable(slotGlow);
            }
        }
    }
}
