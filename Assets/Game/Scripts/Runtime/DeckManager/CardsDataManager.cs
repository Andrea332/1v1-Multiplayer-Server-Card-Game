using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class CardsDataManager : MonoBehaviour
    {
        private int currentDeckSelected = 1;
        public NetworkedCardData[] playerCardsData;
        public DecksData playerDecksData;
        public List<BaseCard> playerCards;

        public List<BaseCard> PlayerCards
        {
            get => playerCards;
            set
            {
                playerCards = value;
                SortCards(playerCards);
            }
        }
        
        public UnityEvent<int> onDeckSettingsLoaded;
        public UnityEvent<NetworkedCardData[]> onSaveLoaded;
        public UnityEvent onSaveStarted;
        public UnityEvent<string> onSaveError;
        public UnityEvent onSaveCompleted;
        public UnityEvent<string> onDeckName;
        public UnityEvent<List<BaseCard>> onDeckCards;
        public UnityEvent<List<BaseCard>> onUnusedCard;
        private DecksSettingsData decksSettingsData;
        public async void LoadSave()
        {
            DestroySpawnedPlayerCards();
            
            decksSettingsData = await LoadDecksSettings();
            
            onDeckSettingsLoaded?.Invoke(decksSettingsData.maxDeckCardsQuantity);

            playerDecksData = await DecksSaveManager.LoadPlayerDecks();
            
            playerCardsData = await DecksSaveManager.LoadPlayerCards();

            await DecksSaveManager.VerifyDecks(playerDecksData, playerCardsData);
            
            onSaveLoaded?.Invoke(playerCardsData);
        }

        private async Task<DecksSettingsData> LoadDecksSettings()
        {
            EmptyStruct emptyStruct = new EmptyStruct();
            
            RuntimeConfig runtimeConfig = await RemoteConfigService.Instance.FetchConfigsAsync(emptyStruct,emptyStruct);
            
            var decksSettingsJson = runtimeConfig.GetJson(ProjectKeys.REMOTE_CONFIG_MAX_DECKS);

            return JsonUtility.FromJson<DecksSettingsData>(decksSettingsJson);
        }

        private void SortCards(List<BaseCard> playerCards)
        {
            List<BaseCard> currentDeckCards;
            
            List<BaseCard> currentDeckUnusedCards;
            
            DeckData deckData = playerDecksData.decksData.Find(x => x.deckNumber == currentDeckSelected);
            
            onDeckName?.Invoke(deckData.deckName);

            (currentDeckCards, currentDeckUnusedCards) = SortDeckCards(playerCards, deckData);

            onDeckCards?.Invoke(currentDeckCards);
            
            onUnusedCard?.Invoke(currentDeckUnusedCards);
        }

        private (List<BaseCard>, List<BaseCard>) SortDeckCards(List<BaseCard> playerCards, DeckData deckData)
        {
            List<BaseCard> currentDeckCards = new();
            
            List<BaseCard> currentUnusedDeckCards = new();
            
            currentUnusedDeckCards.AddRange(playerCards);

            foreach (var card in playerCards)
            {
                for (int i = 0; i < deckData.cards.Count; i++)
                {
                    if (deckData.cards[i].cardInventoryId == card.GetComponent<CardInventoryIdComponent>().Parameter)
                    {
                        currentDeckCards.Add(card);
                        currentUnusedDeckCards.Remove(card);
                        break;
                    }
                }
            }
            return (currentDeckCards, currentUnusedDeckCards);
        }

        public async void SaveDecks()
        {
            onSaveStarted?.Invoke();

            foreach (var deckData in playerDecksData.decksData)
            {
                if(deckData.cards.Count == 0) continue;
                
                if(deckData.cards.Count == decksSettingsData.maxDeckCardsQuantity) continue;
                
                onSaveError?.Invoke($"There are less than {decksSettingsData.maxDeckCardsQuantity} cards on deck {deckData.deckNumber}");
                onSaveCompleted?.Invoke();
                return;
            }
            
            await DecksSaveManager.SaveDecks(playerDecksData);
            onSaveCompleted?.Invoke();
        }

        public void ChangeDeck(int deckNumber)
        {
            currentDeckSelected = deckNumber;
            SortCards(PlayerCards);
        }

        public void ChangeDeckName(string newName)
        {
            DeckData deckData = playerDecksData.decksData.Find(x => x.deckNumber == currentDeckSelected);
            deckData.deckName = newName;
        }

        public void DestroySpawnedPlayerCards()
        {
            for (int i = PlayerCards.Count - 1; i >= 0; i--)
            {
                Destroy(PlayerCards[i].gameObject);
            }
            PlayerCards.Clear();
        }

        public void AddCardsToCurrentDeck(List<BaseCard> cards)
        {
            foreach (var card in cards)
            {
                AddCardToCurrentDeck(card);
            }
        }

        public void AddCardToCurrentDeck(BaseCard card)
        {
            string cardInventoryId = card.GetComponent<CardInventoryIdComponent>().Parameter;
            
            DeckData deckData = playerDecksData.decksData.Find(x => x.deckNumber == currentDeckSelected);

            DeckCardData deckCardData = deckData.cards.Find(x => x.cardInventoryId == cardInventoryId);

            if (deckCardData != null) return;

            string cardEconomyId = card.GetComponent<CardEconomyIdComponent>().Parameter;
            
            deckCardData = new DeckCardData(cardEconomyId, cardInventoryId);
            
            deckData.cards.Add(deckCardData);
        }

        public void RemoveCardToCurrentDeck(BaseCard card)
        {
            DeckData deckData = playerDecksData.decksData.Find(x => x.deckNumber == currentDeckSelected);

            string cardInventoryId = card.GetComponent<CardInventoryIdComponent>().Parameter;

            DeckCardData deckCardData = deckData.cards.Find(x => x.cardInventoryId == cardInventoryId);
            
            if(deckCardData == null) return;

            deckData.cards.Remove(deckCardData);
        }
    }
}
