using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utilities;

namespace Game
{
    public class AirCemeteryManager : MonoBehaviour
    {
        [SerializeField] private CemeterySelectableCardBuilder cardBuilder;
        [SerializeField] private BaseItemString airCraftType;
        [SerializeField] private Transform airCemeteryCardsParent;
        [SerializeField] private TextMeshProUGUI managerMessage;
        [SerializeField] private Button confirmButton;
        private List<SelectableBattleCard> airCemeteryCards = new();
        private List<BaseCard> selectedCards = new();
        private CardBattleBoardManager CardBattleBoardManager { get; set; }
        private string CardEconomyId { get; set; }
        private string CardInventoryId { get; set; }
        private int MaxCardsToSelect { get; set; }
        public UnityAction<string, string, string> onConfirmSelection;

        public void DestroyManager()
        {
            CardBattleBoardManager.onEnemyTurnStarted.RemoveListener(DestroyManager);
            
            Destroy(gameObject);
        }

        public void BuildAirCemeteryCards(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId)
        {
            CardBattleBoardManager = cardBattleBoardManager;
            
            CardBattleBoardManager.onEnemyTurnStarted.AddListener(DestroyManager);
            
            ClearMenu();

            CardEconomyId = cardEconomyId;
            CardInventoryId = cardInventoryId;
            MaxCardsToSelect = MaxSelectableCards(cardBattleBoardManager.playerTableSlots);
            
            managerMessage.SetText($"Select a maximum of {MaxCardsToSelect} aircraft cards");
            
            List<BaseCard> cards = cardBuilder.BuildCardsWithReturnType(cardBattleBoardManager.cemeteryCards);

            foreach (var card in cards)
            {
                card.transform.SetParent(airCemeteryCardsParent,false);
                card.onCardClicked.AddListener(SelectCemeteryCard);
            }
        }

        private int MaxSelectableCards(List<TableBattleSlot> playerTableSlots)
        {
            var useAbleSlots = playerTableSlots.FindAll(tableSlot =>
            {
                if (tableSlot.CurrentBattleCard != null) return false;
                var slotTypes = tableSlot.GetComponents<CardTypeComponent>();
                var aircraftType = slotTypes.ToList().Find(slotType => slotType.Parameter == airCraftType.ItemValue);
                return aircraftType != null;
            });

            return useAbleSlots.Count;
        }

        private void ClearMenu()
        {
            for (int i = airCemeteryCards.Count - 1; i >= 0; i--)
            {
                Destroy(airCemeteryCards[i].gameObject);
                airCemeteryCards.RemoveAt(i);
            }
            
            airCemeteryCards.Clear();
            selectedCards.Clear();
        }
        
        private void SelectCemeteryCard(BaseCard battleCard)
        {
            SetCardState((SelectableBattleCard)battleCard, selectedCards);
        }
        
        private void SetCardState(SelectableBattleCard battleCard, List<BaseCard> cardsSelected)
        {
            BaseCard cardFound = cardsSelected.Find(x=>x == battleCard);

            SelectableBattleCard selectAbleBattleCard;
            if (cardFound != null)
            {
                cardsSelected.Remove(cardFound);
                selectAbleBattleCard = (SelectableBattleCard)cardFound;
                selectAbleBattleCard.SetAsUnselected();
            }
            else
            {
                if(cardsSelected.Count >= MaxCardsToSelect) return;
            
                cardsSelected.Add(battleCard);
                selectAbleBattleCard = battleCard;
                selectAbleBattleCard.SetAsSelected();
            }
            confirmButton.gameObject.SetActive(cardsSelected.Count > 0);
        }
        
        public void ConfirmCardsToResurrect()
        {
            if(selectedCards.Count <= 0) return;
            
            List<string> cardInventoryIds = new();
            
            foreach (var selectedCard in selectedCards)
            {
                cardInventoryIds.Add( selectedCard.GetComponent<CardInventoryIdComponent>().Parameter);
            }

            CemeteryAircraftsToResurrect cemeteryAircraftsToResurrect = new()
            {
                cardInventoryIds = cardInventoryIds.ToArray()
            };
            
            onConfirmSelection?.Invoke(CardEconomyId, CardInventoryId, JsonUtility.ToJson(cemeteryAircraftsToResurrect));
            Destroy(gameObject);
        }
        
        public struct CemeteryAircraftsToResurrect
        {
            public string[] cardInventoryIds;
        }
    }
}
