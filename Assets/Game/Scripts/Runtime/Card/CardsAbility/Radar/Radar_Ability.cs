using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "Radar_Ability", menuName = "Cards/Ability/Radar")]
    public class Radar_Ability : BaseCardAbility
    {
        public int cardsToShow;
        public override void ServerDoAbility(CardBattleBoardManager cardBattleBoardManager, string cardEconomyId, string cardInventoryId, string serverJsonParameter)
        {
            List<TableInfo> tableInfos = cardBattleBoardManager.tableInfos;

            string currentTurnOwnerId = cardBattleBoardManager.currentTurnOwnerId;

            TableInfo playerTableInfo = tableInfos.Find(x => x.playerId == currentTurnOwnerId);

            TableInfo enemyTableInfo = tableInfos.Find(x => x.playerId != currentTurnOwnerId);

            BattleSlotInfo abilitySlotInfo = playerTableInfo.slotInfos.Find(slotInfo => slotInfo.slotSupportCardData?.cardInventoryId == cardInventoryId);

            if (abilitySlotInfo == null)
            {
               string message = $"Table slot not found on server";
               Debug.Log(message);
               cardBattleBoardManager.PingToClient(message);
               return;
            }

            if (abilitySlotInfo.slotCardData == null)
            {
               string message = $"Table slot {abilitySlotInfo.id} doesn't have a card on it";
               Debug.Log(message);
               cardBattleBoardManager.PingToClient(message);
               return;
            }

            if (IsAbilityAlreadyUsed(cardBattleBoardManager, abilitySlotInfo.slotSupportCardData)) return;
          
            var ability = new ServerRadarAbility(
                cardBattleBoardManager, 
                abilitySlotInfo.slotSupportCardData, 
                playerTableInfo.playerId,
                enemyTableInfo.handCardDatas, cardsToShow
            );
           ability.AddAbilityUsed();
        }

        public override void ClientDoAbility(CardBattleBoardManager cardBattleBoardManager, string clientJsonParameter)
        {
            var clientParameter = JsonUtility.FromJson<ClientRadarParameter>(clientJsonParameter);
            
            if (clientParameter.abilityOwnerId == AuthenticationService.Instance.PlayerId)
            {
                SetSupportCard(cardBattleBoardManager, clientParameter);
            }
        }
        
        private void SetSupportCard(CardBattleBoardManager cardBattleBoardManager, ClientRadarParameter clientParameter)
        {
            BaseCard supportCard = cardBattleBoardManager.playerCards.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == clientParameter.abilityCardInventoryId);

            if (!supportCard.gameObject.TryGetComponent(out ClientRadarAbilityComponent abilityComponent))
            {
                abilityComponent = supportCard.gameObject.AddComponent<ClientRadarAbilityComponent>();
            }
      
            abilityComponent.CardBattleBoardManager = cardBattleBoardManager;
            abilityComponent.battleCardToUse = (BattleCard)supportCard;
            abilityComponent.AddAbilityUsed();
            
            List<BaseCard> cards = cardBattleBoardManager.cardBuilder.BuildCardsWithReturnType(clientParameter.cardDatasToShow);
            
            foreach (var card in cards)
            {
                BaseCard fakeCard = cardBattleBoardManager.enemyHandFakeCards[0];
                cardBattleBoardManager.enemyHandFakeCards.RemoveAt(0);
                Destroy(fakeCard.gameObject);
                card.DisableCardInteraction();
                card.GetComponentInChildren<MaxDeckablePresenter>(true).gameObject.SetActive(false);
                card.transform.SetParent(cardBattleBoardManager.enemyCardsPositions);
                cardBattleBoardManager.enemyHandFakeCards.Add(card);
            }
        }
        
        public struct ClientRadarParameter
        {
            public string abilityCardInventoryId;
            public string abilityOwnerId;
            public NetworkedCardData[] cardDatasToShow;
        }
    }
}
