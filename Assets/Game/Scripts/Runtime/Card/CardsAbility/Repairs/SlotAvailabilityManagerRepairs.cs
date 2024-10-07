using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utilities;

namespace Game
{
    public class SlotAvailabilityManagerRepairs : SlotAvailabilityManagerSupport
    {
        private string cardEconomyId;
        private string cardInventoryId;
        private List<TableBattleSlot> enemyTableSlots;
        private List<SlotSelected> selectedSlots = new();
        public UnityAction<string, string, string> onConfirmSelection;
        private int currentDamageTimes;
        private int damageTimes;
        public GameObject slotSelectedUiPrefab;

        public override void OnCardDragToFindSlotBegin(BaseCard cardToSet, List<TableBattleSlot> playerTableSlots, List<TableBattleSlot> enemyTableSlots, object parameter = default)
        {
            damageTimes = (int)parameter;
            cardEconomyId = cardToSet.GetComponent<CardEconomyIdComponent>().Parameter;
            cardInventoryId = cardToSet.GetComponent<CardInventoryIdComponent>().Parameter;
            this.enemyTableSlots = enemyTableSlots;
            
            SetSlotsGlow(cardToSet, playerTableSlots, enemyTableSlots, parameter);
            
            foreach (var enemyTableSlot in enemyTableSlots)
            {
                enemyTableSlot.onSlotClicked.RemoveAllListeners();
                enemyTableSlot.onSlotClicked.AddListener(OnSlotClicked);
            }
        }

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
                    if (enemyTableSlot.CurrentBattleCard.TryGetComponent(out HealthComponent healthComponent))
                    {
                        SetSlotGlowAsPlaceable(slotGlow);
                        continue;
                    }
                }

                SetSlotGlowAsNotPlaceable(slotGlow);
            }
        }

        private void OnSlotClicked(CardBattleBoardManager cardBattleBoardManager, TableSlot tableSlot, BaseCard baseCard)
        {
            if(baseCard == null) return;
            
            if (baseCard.TryGetComponent(out HealthComponent healthComponent))
            {
                SelectSlot(tableSlot);
            }
        }

        
        private void SelectSlot(TableSlot slot)
        {
            var tempDamageTimes = currentDamageTimes + 1;

            if (tempDamageTimes >= damageTimes)
            {
                SetSlotState(slot);
                ConfirmSlotToDamage();
                return;
            }

            currentDamageTimes = tempDamageTimes;
            
            SetSlotState(slot);
        }
        
        private void SetSlotState(TableSlot slot)
        {
            SlotSelected slotSelectedFound = selectedSlots.Find(slotSelected => slotSelected.SelectedSlot == slot);
            
            if (slotSelectedFound != null)
            {
                slotSelectedFound.DamageTimes++;
                slotSelectedFound.SlotSelectedUi.GetComponentInChildren<TextMeshProUGUI>().SetText(slotSelectedFound.DamageTimes.ToString());
                return;
            }
            
            var slotSelectedUi = Instantiate(slotSelectedUiPrefab, transform, false);
            slotSelectedUi.transform.SetPositionAndRotation(slot.transform.position, new Quaternion());
            slotSelectedFound = new SlotSelected(1, slot, slotSelectedUi);
            slotSelectedUi.GetComponentInChildren<TextMeshProUGUI>().SetText(slotSelectedFound.DamageTimes.ToString());
            selectedSlots.Add(slotSelectedFound);
        }
        
        private void ConfirmSlotToDamage()
        {
            foreach (var enemyTableSlot in enemyTableSlots)
            {
                enemyTableSlot.onSlotClicked.RemoveAllListeners();
            }
            
            List<string> cardInventoryIds = new();
            
            foreach (var slotSelected in selectedSlots)
            {
                var cardInventoryId = slotSelected.SelectedSlot.CurrentBattleCard.GetComponent<CardInventoryIdComponent>().Parameter;
                for (int i = 0; i < slotSelected.DamageTimes; i++)
                {
                    cardInventoryIds.Add(cardInventoryId);
                }
            }

            Repairs_Ability.ServerRepairsParameter serverRepairsParameter = new()
            {
                cardInventoryIds = cardInventoryIds.ToArray()
            };
            
            onConfirmSelection?.Invoke(cardEconomyId, cardInventoryId, JsonUtility.ToJson(serverRepairsParameter));
            OnCardDragToFindSlotEnd(null);
            Destroy(gameObject);
        }
        
        private class SlotSelected
        {
            public SlotSelected(int damageTimes, TableSlot selectedSlot, GameObject slotSelectedUi)
            {
                DamageTimes = damageTimes;
                SelectedSlot = selectedSlot;
                SlotSelectedUi = slotSelectedUi;
            }

            public int DamageTimes { get; set; }
            public TableSlot SelectedSlot { get; }
            public GameObject SlotSelectedUi { get; }
        }
    }
}
