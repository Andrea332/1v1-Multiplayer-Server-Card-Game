using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game
{
    public class SlotAvailabilityManagerSonarAbility : SlotAvailabilityManagerSupport
    {
        public List<BaseItemString> placeAbleItems;
        private protected override void SetSlotsGlow(BaseCard cardToSet, List<TableBattleSlot> playerTableSlots, List<TableBattleSlot> enemyTableSlots, object parameter = default)
        {
             //Set enemy slot as not valid
            foreach (var enemyTableSlot in enemyTableSlots)
            {
                var slotGlow = SpawnSlotGlow(enemyTableSlot.transform);
                slotGlows.Add(slotGlow);
                SetSlotGlowAsNotPlaceable(slotGlow);
            }

            //Check player's slots
            foreach (var playerTableSlot in playerTableSlots)
            {
                var slotGlow = SpawnSlotGlow(playerTableSlot.transform);
                slotGlows.Add(slotGlow);
                
                //Check if slot is empty
                if (playerTableSlot.CurrentBattleCard == null)
                {
                    SetSlotGlowAsNotPlaceable(slotGlow);
                    continue;
                }
                
                if (playerTableSlot.CurrentSupportCard != null)
                {
                    SetSlotGlowAsNotPlaceable(slotGlow);
                    continue;
                }
                
                var baseItemString = placeAbleItems.Find(x => playerTableSlot.CurrentBattleCard.GetComponent<CardTypeComponent>().Parameter == x.ItemValue);

                if (baseItemString != null)
                {
                    SetSlotGlowAsPlaceable(slotGlow);
                    continue;
                }
                
                SetSlotGlowAsNotPlaceable(slotGlow);
            }
        }
    }
}
