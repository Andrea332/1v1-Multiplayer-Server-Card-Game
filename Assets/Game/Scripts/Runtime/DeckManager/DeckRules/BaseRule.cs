using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public abstract class BaseRule : ScriptableObject
    {
        public string ruleNotRespectedDescription;
        public abstract bool IsRuleRespected(BaseCard card, List<BaseCard> deckCardsSpawnedList);
    }
}
