using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game
{
    public class MatchmakingPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI selectedDeckText;
        private DecksData decksData;
        public void SetSelectedDeckInfoBox(int selectedDeck)
        {
            selectedDeckText.text = "Deck selected: " + decksData.decksData[selectedDeck-1].deckName;
        }

        public void SetDecksData(DecksData playerDecksData)
        {
            decksData = playerDecksData;
        }
    }
}
