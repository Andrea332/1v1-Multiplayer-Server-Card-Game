using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game
{
    public class CardInfoStatsPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI statsValue;

        public void SetStats(string value)
        {
            statsValue.text = value;
        }
    }
}
