using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game
{
    public class SlotAvailabilityManager : MonoBehaviour
    {
        [SerializeField] private protected Image slotGlowPrefab;

        [SerializeField] private protected Color cardPlaceable;
        [SerializeField] private protected Color cardNotPlaceable;

        private protected List<Image> slotGlows = new();
        
        public virtual void OnCardDragToFindSlotBegin(BaseCard cardToSet, List<TableBattleSlot> playerTableSlots, List<TableBattleSlot> enemyTableSlots, object parameter = default)
        {
            SetSlotsGlow(cardToSet, playerTableSlots, enemyTableSlots, parameter);
        }

        private protected virtual void SetSlotsGlow(BaseCard cardToSet, List<TableBattleSlot> playerTableSlots, List<TableBattleSlot> enemyTableSlots, object parameter = default)
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

                if (playerTableSlot.CurrentBattleCard == null)
                {
                    var slotCardTypes = playerTableSlot.GetComponents<CardTypeComponent>();
                    var cardType = cardToSet.GetComponent<CardTypeComponent>().Parameter;
                    var typeFound = slotCardTypes.ToList().Find(x => x.Parameter == cardType);

                    if (playerTableSlot.GetComponent<ClientSmokescreenAbilityComponent>() != null)
                    {
                        SetSlotGlowAsNotPlaceable(slotGlow);
                        continue;
                    }

                    if (typeFound != null)
                    {
                        SetSlotGlowAsPlaceable(slotGlow);
                        continue;
                    }
                }
                
                SetSlotGlowAsNotPlaceable(slotGlow);
            }
        }

        public void OnCardDragToFindSlotEnd(BaseCard card)
        {
            for (int i = slotGlows.Count - 1; i >= 0; i--)
            {
                Destroy(slotGlows[i].gameObject);
            }
            
            slotGlows.Clear();
        }

        private protected Image SpawnSlotGlow(Transform parent)
        {
            return Instantiate(slotGlowPrefab, parent, false);
        }

        private protected void SetSlotGlowAsPlaceable(Image slotGlow)
        {
            slotGlow.color = cardPlaceable;
        }
        
        private protected void SetSlotGlowAsNotPlaceable(Image slotGlow)
        {
            slotGlow.color = cardNotPlaceable;
        }
    }
}
