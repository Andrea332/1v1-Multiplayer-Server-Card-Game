using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "Same Card Quantity Limiter", menuName = "Deck Rules/Same Card Quantity Limiter")]
    public class SameCardQuantityLimiter : BaseRule
    {
        public override bool IsRuleRespected(BaseCard card, List<BaseCard> deckCardsSpawnedList)
        {
            var currentCardEconomyId = card.GetComponent<CardEconomyIdComponent>().Parameter;
            var currentCardMaxDeckAble = card.GetComponent<MaxDeckableComponent>().Parameter;
            List<BaseCard> sameCards = deckCardsSpawnedList.FindAll(x => x.GetComponent<CardEconomyIdComponent>().Parameter == currentCardEconomyId);
            if (sameCards.Count >= currentCardMaxDeckAble)
            {
                return false;
            }

            return true;
        }
    }
}
