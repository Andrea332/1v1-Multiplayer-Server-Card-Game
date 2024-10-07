using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "Same Card Rarity Limiter", menuName = "Deck Rules/Same Card Rarity Limiter")]
    public class SameCardRarityLimiter : BaseRule
    {
        [SerializeField] private BaseItemString rarity;

        public override bool IsRuleRespected(BaseCard card, List<BaseCard> deckCardsSpawnedList)
        {
            var currentCardRarity = card.GetComponent<RarityComponent>().Parameter;
            if (currentCardRarity == rarity.ItemValue)
            {
                BaseCard qgCard = deckCardsSpawnedList.Find(x => x.GetComponent<RarityComponent>().Parameter == currentCardRarity);
                if (qgCard != null)
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}
