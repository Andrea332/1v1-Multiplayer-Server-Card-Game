using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    [CreateAssetMenu(fileName = "InfoBoxSO", menuName = "Instatiable/InfoBoxSO")]
    public class InfoBoxSO : Instantiator
    {
        private InfoBox spawnedInfoBox;
        
        public override void SpawnPrefab()
        {
            if (spawnedInfoBox != null) return;
            GameObject infoBoxGameObject = Instantiate(prefabToInstantiate,null);
            spawnedInfoBox = infoBoxGameObject.GetComponent<InfoBox>();
        }
        public void SetConfirmAction(UnityAction onConfirmAction)
        {
            SpawnPrefab();
            spawnedInfoBox.SetConfirmButtonAction(onConfirmAction);
        }
        public void ShowMessage(string message)
        {
            SpawnPrefab();
            spawnedInfoBox.ShowMessage(message);
        }
        
        public void ShowErrorMessage(string message)
        {
            SpawnPrefab();
            spawnedInfoBox.ShowErrorMessage(message);
        }
    
        public void ShowMessageWithTimer(string message)
        {
            SpawnPrefab();
            spawnedInfoBox.ShowMessageWithTimer(message);
        }
    
        public void ShowErrorMessageWithTimer(string message)
        {
            SpawnPrefab();
            spawnedInfoBox.ShowErrorMessageWithTimer(message);
        }

        public void HideMessage()
        { 
            SpawnPrefab();
            spawnedInfoBox.HideMessage();
        }
    }
}
