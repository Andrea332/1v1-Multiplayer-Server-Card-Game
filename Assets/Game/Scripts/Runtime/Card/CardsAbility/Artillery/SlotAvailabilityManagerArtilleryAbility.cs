using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game
{
    public class SlotAvailabilityManagerArtilleryAbility : SlotAvailabilityManagerSupport
    {
        [SerializeField] private BaseItemString tankItem;

        private protected override void SetSlotsGlow(BaseCard cardToSet, List<TableBattleSlot> playerTableSlots, List<TableBattleSlot> enemyTableSlots, object parameter = default)
        {
            //Set enemy slot as not valid
            foreach (var playerTableSlot in playerTableSlots)
            {
                var slotGlow = SpawnSlotGlow(playerTableSlot.transform);
                slotGlows.Add(slotGlow);
                SetSlotGlowAsNotPlaceable(slotGlow);
            }
            
            //Check player's slots
            foreach (var enemyTableSlot in enemyTableSlots)
            {
                var slotGlow = SpawnSlotGlow(enemyTableSlot.transform);
                slotGlows.Add(slotGlow);
                
                if (enemyTableSlot.CurrentBattleCard != null)
                {
                    var cardType = enemyTableSlot.CurrentBattleCard.GetComponent<CardTypeComponent>().Parameter;

                    if (cardType == tankItem.ItemValue)
                    {
                        SetSlotGlowAsPlaceable(slotGlow);
                        continue;
                    }
                    
                    SetSlotGlowAsNotPlaceable(slotGlow);
                    continue;
                }
                
                SetSlotGlowAsNotPlaceable(slotGlow);
            }
        }
    }
}
