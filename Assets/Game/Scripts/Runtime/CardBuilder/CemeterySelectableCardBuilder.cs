using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "CemeterySelectableCardBuilder", menuName = "Card Builders/CemeterySelectableCardBuilder")]
    public class CemeterySelectableCardBuilder : BaseCardBuilder
    {
        [SerializeField] private HealthPresenter healthPresenterPrefab;
        [SerializeField] private AttackPresenter attackPresenterPrefab;
        [SerializeField] private CardTypePresenter cardTypePresenterPrefab;
        [SerializeField] private RarityPresenter rarityPresenterPrefab;
        [SerializeField] private CostPresenter costPresenterPrefab;
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
        
        
        public List<BaseCard> BuildCardsWithReturnType(List<BaseCard> cards)
        {
            List<BaseCard> cardsToReturn = new();
            
            for (int i = 0; i < cards.Count; i++)
            {
                //Set battleCard gameobjectName for debugging
                var oldCard = cards[i];
                var selectableCard = Instantiate(cardPrefab);
                
                string cardEconomyId = default;
                if (oldCard.TryGetComponent(out CardEconomyIdComponent cardEconomyIdComponent)) cardEconomyId = cardEconomyIdComponent.Parameter;
                string effectDescription  = default;
                if (oldCard.TryGetComponent(out EffectDescriptionComponent effectDescriptionComponent)) effectDescription = effectDescriptionComponent.Parameter;
                int health = default;
                if (oldCard.TryGetComponent(out HealthComponent healthComponent)) health = healthComponent.Parameter;
                int attack = default;
                if (oldCard.TryGetComponent(out AttackComponent attackComponent)) attack = attackComponent.Parameter;
                string type = default;
                if (oldCard.TryGetComponent(out CardTypeComponent cardTypeComponent)) type = cardTypeComponent.Parameter;
                int cost = default;
                if (oldCard.TryGetComponent(out CostComponent costComponent)) cost = costComponent.Parameter;
                string rarity = default;
                if (oldCard.TryGetComponent(out RarityComponent rarityComponent)) rarity = rarityComponent.Parameter;
                string cardInventoryId = default;
                if (oldCard.TryGetComponent(out CardInventoryIdComponent cardInventoryIdComponent)) cardInventoryId = cardInventoryIdComponent.Parameter;
                List<string> targetType = new();
                if (oldCard.TryGetComponent(out TargetTypesComponent targetTypesComponent)) targetType = targetTypesComponent.Parameter;
                
                //Try set components
                selectableCard.name = oldCard.gameObject.name;
                selectableCard.TrySetCardImageComponent(cardEconomyId,  selectableCard.GetComponentInChildren<CardImagePresenter>());
                selectableCard.TrySetNameComponent(oldCard.gameObject.name, cardNamePresenterPrefab);
                selectableCard.TrySetEffectDescriptionComponent(effectDescription);
                selectableCard.TrySetHealthComponent(health, healthPresenterPrefab);
                selectableCard.TrySetAttackComponent(attack, attackPresenterPrefab);
                selectableCard.TrySetCardTypeComponent(type, cardTypePresenterPrefab);
                selectableCard.TrySetCostComponent(cost, costPresenterPrefab);
                selectableCard.TrySetRarityComponent(rarity, rarityPresenterPrefab);
                selectableCard.TrySetCardEconomyIdComponent(cardEconomyId);
                selectableCard.TrySetInventoryIdComponent(cardInventoryId);
                if (targetType != null)
                {
                    selectableCard.TrySetTargetTypesComponent(targetType.ToList(), targetTypesPresenterPrefab);
                }
                cardsToReturn.Add(selectableCard);
            }

            return cardsToReturn;
        }
       
    }
}
