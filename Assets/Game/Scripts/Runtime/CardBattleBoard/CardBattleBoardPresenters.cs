using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utilities;

namespace Game
{
    public class CardBattleBoardPresenters : MonoBehaviour
    {
        public ParameterPresenter<string>
            playerNameUI,
            playerStateUI,
            enemyNameUI,
            enemyStateUI;

        public ParameterPresenter<int>
            playerPointsUI,
            playerFuelUI,
            playerMaxFuelUI,
            playerTurnNumber,
            enemyPointsUI,
            enemyFuelUI,
            enemyMaxFuelUI,
            enemyTurnNumber;

        public TimerUI turnTimer;
        public TimerUI matchEndedTurnTimer;
    }
}
