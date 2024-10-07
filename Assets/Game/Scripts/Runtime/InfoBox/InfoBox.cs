using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game
{
    public class InfoBox : MonoBehaviour
    {
        [SerializeField] private Button confirmButton;
        [SerializeField] private TextMeshProUGUI textMeshProUGUI;
        [SerializeField] private Color messageColor;
        [SerializeField] private Color errorMessageColor;
        [SerializeField] private int messageTimer;
        public void ShowMessage(string message)
        {
            textMeshProUGUI.color = messageColor;
            textMeshProUGUI.text = message;
            gameObject.SetActive(true);
        }
    
        public void ShowErrorMessage(string message)
        {
            textMeshProUGUI.color = errorMessageColor;
            textMeshProUGUI.text = message;
            gameObject.SetActive(true);
        }
    
        public async void ShowMessageWithTimer(string message)
        {
            textMeshProUGUI.color = messageColor;
            textMeshProUGUI.text = message;
            gameObject.SetActive(true);
            await UniTask.WaitForSeconds(messageTimer);
            gameObject.SetActive(false);
        }
    
        public async void ShowErrorMessageWithTimer(string message)
        {
            textMeshProUGUI.color = errorMessageColor;
            textMeshProUGUI.text = message;
            gameObject.SetActive(true);
            await UniTask.WaitForSeconds(messageTimer);
            gameObject.SetActive(false);
        }

        public void HideMessage()
        { 
            gameObject.SetActive(false);
        }

        public void SetConfirmButtonAction(UnityAction onConfirmAction)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(onConfirmAction);
        }

    }
}
