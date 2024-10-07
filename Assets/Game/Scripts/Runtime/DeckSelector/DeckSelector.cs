using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class DeckSelector : MonoBehaviour
    {
        [SerializeField] private List<DeckEnvelope> cardsEnvelopes;
        [SerializeField] private DeckEnvelope selectedCardEnvelope;
        [SerializeField] private BaseCardBuilder cardBuilder;
        [SerializeField] private UnityEvent onMenuLoaded;
        //private int currentEnvelopeToBuild;
        private NetworkedCardData[] playerNetworkedCardData;
        private DecksData playerDecksData;
        [SerializeField] private UnityEvent<DecksData> onExposeDecksData;
        
        public async void LoadSave()
        {
            playerNetworkedCardData = await DecksSaveManager.LoadPlayerCards();

            playerDecksData = await DecksSaveManager.LoadPlayerDecks();
            
            onExposeDecksData?.Invoke(playerDecksData);
            
            await DecksSaveManager.VerifyDecks(playerDecksData, playerNetworkedCardData);

            SetEnvelopes(playerNetworkedCardData, playerDecksData, cardsEnvelopes);
        }

        public void SetEnvelopes(NetworkedCardData[] playerCardsData,  DecksData currentPlayerDecksData, List<DeckEnvelope> deckEnvelopes)
        {
            int currentEnvelopeToBuild = 0;
            for (var index = 0; index < currentPlayerDecksData.decksData.Count; index++)
            {
                var deckData = currentPlayerDecksData.decksData[index];

                if (deckData.cards.Count == 0)
                {
                    deckEnvelopes[currentEnvelopeToBuild].gameObject.SetActive(false);
                    currentEnvelopeToBuild++;
                    continue;
                }
                
                ToDeck(deckData, playerCardsData, deckEnvelopes[currentEnvelopeToBuild]);
                currentEnvelopeToBuild++;
            }
            onMenuLoaded?.Invoke();
        }

        public void SetEnvelope(int deckNumber)
        {
            selectedCardEnvelope.transform.parent.gameObject.SetActive(true);
            ToDeck(playerDecksData.decksData[deckNumber-1], playerNetworkedCardData, selectedCardEnvelope);
        }

        private void ToDeck(DeckData deckData, NetworkedCardData[] playerCardsData, DeckEnvelope deckEnvelope)
        {
            deckEnvelope.envelopeNameText.text = deckData.deckName;
            
            int numberOfCardsInsideEnvelope = deckEnvelope.cards.Count;
            
            int numberOfEnvelopeCardsToFill = deckData.cards.Count > numberOfCardsInsideEnvelope ? numberOfCardsInsideEnvelope : deckData.cards.Count;

            for (var index = 0; index < numberOfCardsInsideEnvelope; index++)
            {
                if (index > numberOfEnvelopeCardsToFill - 1)
                {
                    deckEnvelope.cards[index].gameObject.SetActive(false);
                    continue;
                }
                
                var deckCardData = deckData.cards[index];
                NetworkedCardData foundedCardData = playerCardsData.ToList().Find(x => x.cardInventoryId == deckCardData.cardInventoryId);
                BattleCard battleCard = (BattleCard)cardBuilder.BuildCardWithReturnType(deckEnvelope.cards[index], foundedCardData, false);
                battleCard.GetComponent<CanvasGroup>().blocksRaycasts = false;
                battleCard.GetComponentInChildren<MaxDeckablePresenter>().gameObject.SetActive(false);
            }
        }
    }
}
