using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class EnemyRadarCardsManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private CardBattleBoardManager cardBattleBoardManager;
        [SerializeField] private Transform cardsParent;
        private List<BaseCard> radarCards = new();

        public void ShowEnemyRadarCards()
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            foreach (var fakeCard in cardBattleBoardManager.enemyHandFakeCards)
            {
                var clonedCards = Instantiate(fakeCard, cardsParent, false);
                clonedCards.transform.SetPositionAndRotation(clonedCards.transform.position, new Quaternion());
                radarCards.Add(clonedCards);
            }
        }

        public void HideEnemyRadarCards()
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            for (int i = radarCards.Count - 1; i >= 0; i--)
            {
                Destroy(radarCards[i].gameObject);
            }
            radarCards.Clear();
        }
    }
}
