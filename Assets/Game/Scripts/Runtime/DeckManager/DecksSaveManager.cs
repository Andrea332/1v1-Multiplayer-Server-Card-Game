using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Services.CloudCode;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.CloudSave.Models.Data.Player;
using Unity.Services.Economy;
using Unity.Services.Economy.Model;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    public static class DecksSaveManager
    {
        public static async UniTask SaveDecks(DecksData decksData)
        {
            Dictionary<string, object> args = new() { { ProjectKeys.CC_FUNCTION_SAVEDECKS_PARAMETER, decksData } };

            await CloudCodeService.Instance.CallModuleEndpointAsync(ProjectKeys.CC_MODULE, ProjectKeys.CC_FUNCTION_SAVEDECKS, args);
        } 
        
        public static async UniTask<NetworkedCardData[]> LoadPlayerCards()
        {
            await EconomyService.Instance.Configuration.SyncConfigurationAsync();

            GetInventoryOptions getInventoryOptions = new()
            {
                ItemsPerFetch = 30
            };

            List<PlayersInventoryItem> playersInventoryItems = new();
            
            var getInventoryResult = await EconomyService.Instance.PlayerInventory.GetInventoryAsync(getInventoryOptions);

            playersInventoryItems.AddRange(getInventoryResult.PlayersInventoryItems);
            
            while (getInventoryResult.HasNext)
            {
                getInventoryResult = await getInventoryResult.GetNextAsync();
                
                playersInventoryItems.AddRange(getInventoryResult.PlayersInventoryItems);
            }
            
            List<NetworkedCardData> networkedCardDatas = new();
            
            for (int i = 0; i < playersInventoryItems.Count; i++)
            {
                InventoryItemDefinition inventoryItemDefinition = playersInventoryItems[i].GetItemDefinition();
                CardData cardData =  inventoryItemDefinition.CustomDataDeserializable.GetAs<CardData>();
                cardData.cardName = inventoryItemDefinition.Name;
                cardData.cardEconomyId = inventoryItemDefinition.Id;
                cardData.cardInventoryId = playersInventoryItems[i].PlayersInventoryItemId;
                networkedCardDatas.Add(cardData.ToStruct());
            }

            return networkedCardDatas.ToArray();
        }
        
        public static async UniTask<DecksData> LoadPlayerDecks()
        {
            HashSet<string> saveKeys = new HashSet<string> {ProjectKeys.DECKS_SAVE_CLOUDSAVE_KEY };

            Dictionary<string, Item> save = await CloudSaveService.Instance.Data.Player.LoadAsync(saveKeys);
            
            EmptyStruct emptyStruct = new EmptyStruct();
         
            RuntimeConfig runtimeConfig = await RemoteConfigService.Instance.FetchConfigsAsync(emptyStruct,emptyStruct);
            
            if (save.ContainsKey(ProjectKeys.DECKS_SAVE_CLOUDSAVE_KEY))
            {
                return save[ProjectKeys.DECKS_SAVE_CLOUDSAVE_KEY].Value.GetAs<DecksData>();
            }
            
            var decksSettingsJson = runtimeConfig.GetJson(ProjectKeys.REMOTE_CONFIG_MAX_DECKS);

            var decksSettingsData = JsonUtility.FromJson<DecksSettingsData>(decksSettingsJson);

            var decksData = CreateDefaultDecksData(decksSettingsData.maxDecksQuantity);

            return decksData;
        }
        
        public static async Task VerifyDecks(DecksData decksData, NetworkedCardData[] cardsDataToVerify)
        {
            bool decksRebuilded = false;
            
            foreach (var deckData in decksData.decksData)
            {
                for (var index = deckData.cards.Count - 1; index >= 0; index--)
                {
                    var deckCardData = deckData.cards[index];
                    NetworkedCardData networkedCardData = cardsDataToVerify.ToList().Find(x => x.cardInventoryId == deckCardData.cardInventoryId);
                    if (string.IsNullOrEmpty(networkedCardData.cardInventoryId))
                    {
                        decksRebuilded = true;
                        deckData.cards.Remove(deckCardData);
                    }
                }
            }

            if(!decksRebuilded) return;
            
            await SaveDecks(decksData);
        }
        private static DecksData CreateDefaultDecksData(int maxDecks)
        {
            List<DeckData> deckDatas = new();

            for (int i = 0; i < maxDecks; i++)
            {
                int deckNumber = i + 1;
                DeckData deckData = new DeckData(deckNumber, $"My Deck {deckNumber}", new List<DeckCardData>());
                deckDatas.Add(deckData);
            }

            return new DecksData(deckDatas);
        }

        public static DeckData CreateDeckSaveData(List<BaseCard> cards, int deckNumber, string deckName)
        {
            List<DeckCardData> deckCardsData = new();

            foreach (var card in cards)
            {
                string cardEconomyId = card.GetComponent<CardEconomyIdComponent>().Parameter;
                string cardInventoryId = card.GetComponent<CardInventoryIdComponent>().Parameter;
                DeckCardData deckCardData = new DeckCardData(cardEconomyId, cardInventoryId);
                deckCardsData.Add(deckCardData);
            }

            return new DeckData(deckNumber, deckName, deckCardsData);
        }
        
        public static NetworkedCardData[] CardDatasToStruct(List<CardData> cardDatas)
        {
            NetworkedCardData[] networkedCardDatas = new NetworkedCardData[cardDatas.Count];

            for (var index = 0; index < cardDatas.Count; index++)
            {
                var tableInfo = cardDatas[index];
                networkedCardDatas.SetValue(tableInfo.ToStruct(), index);
            }

            return networkedCardDatas;
        }
    }
    public struct EmptyStruct { }
    
    public struct DecksSettingsData
    {
        public int maxDecksQuantity;
        public int maxDeckCardsQuantity;
    }
    
    [Serializable]
    public class DecksData
    {
        public List<DeckData> decksData;

        public DecksData(List<DeckData> decksData)
        {
            this.decksData = decksData;
        }
    }

    [Serializable]
    public class DeckData
    {
        public int deckNumber;
        public string deckName;
        public List<DeckCardData> cards;

        public DeckData(int deckNumber, string deckName, List<DeckCardData> cards)
        {
           this.deckNumber = deckNumber;
           this.deckName = deckName;
           this.cards = cards;
        }
    }
    
    [Serializable]
    public class DeckCardData
    {
        public string cardEconomyId;
        public string cardInventoryId;
        
        public DeckCardData(string cardEconomyId, string cardInventoryId)
        {
            this.cardEconomyId = cardEconomyId;
            this.cardInventoryId = cardInventoryId;
        }
    }
}

