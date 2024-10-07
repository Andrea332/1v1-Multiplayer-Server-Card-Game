using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ServerRadarAbility : ServerAbilityManager
    {
        private string AbilityOwnerId { get; }
        private int CardsToShow { get; }
        private List<CardData> EnemyHandCardsDatas { get; }

        public ServerRadarAbility(CardBattleBoardManager cardBattleBoardManager, CardData cardDataUsed, string abilityOwnerId ,  List<CardData> enemyHandCardsDatas,  int cardsToShow) : base(cardBattleBoardManager, cardDataUsed)
        {
            CardBattleBoardManager = cardBattleBoardManager;
            CardDataUsed = cardDataUsed;
            AbilityOwnerId = abilityOwnerId;
            EnemyHandCardsDatas = enemyHandCardsDatas;
            CardsToShow = cardsToShow;
        }

        public override void AddAbilityUsed()
        {
            base.AddAbilityUsed();
            FindRandomEnemyHandCardsAndSendToClient();
        }

        public void FindRandomEnemyHandCardsAndSendToClient()
        {
            List<CardData> currentEnemyHandCardsDatas = new ();
            currentEnemyHandCardsDatas.AddRange(EnemyHandCardsDatas);

            List<NetworkedCardData> cardsToShow = new();
            for (int i = 0; i < CardsToShow; i++)
            {
                var randomCardIndex = Random.Range(0, currentEnemyHandCardsDatas.Count);
                cardsToShow.Add(currentEnemyHandCardsDatas[randomCardIndex].ToStruct());
                currentEnemyHandCardsDatas.RemoveAt(randomCardIndex);
            }
            
            Radar_Ability.ClientRadarParameter clientParameter = new()
            {
                abilityOwnerId = AbilityOwnerId,
                abilityCardInventoryId = CardDataUsed.cardInventoryId,
                cardDatasToShow = cardsToShow.ToArray()
            };
           
            CardBattleBoardManager.ClientRpcExecuteCardAbility(JsonUtility.ToJson(clientParameter), CardDataUsed.cardEconomyId);
        }
    }
}
