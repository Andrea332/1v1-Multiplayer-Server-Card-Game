using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "CardInfoPresenterSO", menuName = "Instatiable/CardInfoPresenterSO")]
    public class CardInfoPresenterSO : Instantiator
    {
        private CardInfoPresenter cardInfoPresenter;
        public override void SpawnPrefab()
        {
            if (cardInfoPresenter != null) return;
            GameObject spawned = Instantiate(prefabToInstantiate,null);
            cardInfoPresenter = spawned.GetComponent<CardInfoPresenter>();
        }

        public void InizializeCardInfo(BaseCard baseCard)
        {
            SpawnPrefab();
            
            cardInfoPresenter.InizializeCardInfo(baseCard);
        }
    }
}
