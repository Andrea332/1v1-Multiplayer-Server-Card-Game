using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game
{
    public class SlotAvailabilityManagerBunkerAbility : SlotAvailabilityManager
    {
        [SerializeField] private protected BaseItemString tankItemId;
        [SerializeField] private protected BaseItemString aircraftItemId;
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
                
                if (playerTableSlot.CurrentBattleCard != null)
                {
                    var currentCard = playerTableSlot.CurrentBattleCard;
                    var currentCardType = currentCard.GetComponent<CardTypeComponent>().Parameter;
                    if (currentCardType == tankItemId.ItemValue || currentCardType == aircraftItemId.ItemValue)
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
