using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class CemeteryManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;

        public void OpenMenu()
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        public void CloseMenu()
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
