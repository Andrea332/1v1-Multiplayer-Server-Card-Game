using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "BattleCardBuilder", menuName = "Card Builders/BattleCardBuilder")]
    public class BattleCardBuilder : BaseCardBuilder
    {
        [SerializeField] private HealthPresenter healthPresenterPrefab;
        [SerializeField] private AttackPresenter attackPresenterPrefab;
        [SerializeField] private CardTypePresenter cardTypePresenterPrefab;
        [SerializeField] private RarityPresenter rarityPresenterPrefab;
        [SerializeField] private CostPresenter costPresenterPrefab;
        [SerializeField] private MaxDeckablePresenter maxDeckablePresenterPrefab;
        [SerializeField] private TargetTypesPresenter targetTypesPresenterPrefab;
        [SerializeField] private CardNamePresenter cardNamePresenterPrefab;
        
        public override BaseCard BuildCardWithReturnType(BaseCard card, NetworkedCardData cardData, bool sendEvent = true)
        {
            //Set battleCard gameobjectName for debugging
            card.name = cardData.cardName;
            //Try set components
            card.TrySetCardImageComponent(cardData.cardEconomyId,  card.GetComponentInChildren<CardImagePresenter>());
            card.TrySetNameComponent(cardData.cardName, cardNamePresenterPrefab);
            card.TrySetEffectDescriptionComponent(cardData.effectDescription);
            card.TrySetHealthComponent(cardData.health, healthPresenterPrefab);
            card.TrySetAttackComponent(cardData.attack, attackPresenterPrefab);
            card.TrySetCardTypeComponent(cardData.type, cardTypePresenterPrefab);
            card.TrySetCostComponent(cardData.cost, costPresenterPrefab);
            card.TrySetRarityComponent(cardData.rarity, rarityPresenterPrefab);
            card.TrySetMaxDeckableComponent(cardData.maxDeckable, maxDeckablePresenterPrefab);
            card.TrySetCardEconomyIdComponent(cardData.cardEconomyId);
            card.TrySetInventoryIdComponent(cardData.cardInventoryId);
            
            if (cardData.targetType != null)
            {
                card.TrySetTargetTypesComponent(cardData.targetType.ToList(), targetTypesPresenterPrefab);
            }
            if (sendEvent)
            {
                List<BaseCard> cards = new() { card };
                onCardsBuilded?.Invoke(cards);
            }

            return card;
        }


       
    }
}
