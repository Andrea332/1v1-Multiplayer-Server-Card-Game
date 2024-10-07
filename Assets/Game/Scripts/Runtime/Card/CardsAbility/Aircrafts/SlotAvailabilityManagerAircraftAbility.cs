using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game
{
    public class SlotAvailabilityManagerAircraftAbility : SlotAvailabilityManager
    {
        private protected override void SetSlotsGlow(BaseCard cardToSet, List<TableBattleSlot> playerTableSlots, List<TableBattleSlot> enemyTableSlots, object parameter = default)
        {
            int slotShift = (int)parameter;
            //Set enemy slot as not valid
            foreach (var enemyTableSlot in enemyTableSlots)
            {
                var slotGlow = SpawnSlotGlow(enemyTableSlot.transform);
                slotGlows.Add(slotGlow);
                SetSlotGlowAsNotPlaceable(slotGlow);
            }
            TableSlot currentCardTableSlot = playerTableSlots.Find(x =>
            {
                if (x.CurrentBattleCard == null) return false;
                        
                return x.CurrentBattleCard.GetComponent<CardInventoryIdComponent>().Parameter == cardToSet.GetComponent<CardInventoryIdComponent>().Parameter;
            });

            var maxMovementShift = currentCardTableSlot.Id + slotShift;
            var minMovementShift = currentCardTableSlot.Id - slotShift;
            //Check player's slots
            foreach (var playerTableSlot in playerTableSlots)
            {
                var slotGlow = SpawnSlotGlow(playerTableSlot.transform);
                slotGlows.Add(slotGlow);
                
                if (playerTableSlot.CurrentBattleCard == null)
                {
                    if (playerTableSlot.Id > maxMovementShift || playerTableSlot.Id < minMovementShift)
                    {
                        SetSlotGlowAsNotPlaceable(slotGlow);
                        continue;
                    }
                    
                    var slotCardTypes = playerTableSlot.GetComponents<CardTypeComponent>();
                    var cardType = cardToSet.GetComponent<CardTypeComponent>().Parameter;
                    var slotFound = slotCardTypes.ToList().Find(x => x.Parameter == cardType);

                    if (slotFound != null)
                    {
                        SetSlotGlowAsPlaceable(slotGlow);
                        continue;
                    }
                }
                
                SetSlotGlowAsNotPlaceable(slotGlow);
            }
        }
    }
}
