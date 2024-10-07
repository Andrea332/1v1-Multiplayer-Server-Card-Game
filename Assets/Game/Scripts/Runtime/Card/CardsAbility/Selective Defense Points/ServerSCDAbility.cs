using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Game
{
    public class ServerSCDAbility : ServerAbilityManager
    {
        public int PointsToEarn { get; set; }
        public List<BaseItemString> CanBeDamageBy { get; set; }

        public ServerSCDAbility(CardBattleBoardManager cardBattleBoardManager, CardData cardDataUsed, int pointsToEarn, List<BaseItemString> canBeDamageBy) : base(cardBattleBoardManager, cardDataUsed)
        {
            CardBattleBoardManager = cardBattleBoardManager;
            CardDataUsed = cardDataUsed;
            PointsToEarn = pointsToEarn;
            CanBeDamageBy = canBeDamageBy;
        }
    }
}
