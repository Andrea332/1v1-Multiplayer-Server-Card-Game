using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game
{
    public class TextColorOnOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Color colorOnOver;
        private Color normalColor;
        private void Start()
        {
            normalColor = text.color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            text.color = colorOnOver;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            text.color = normalColor;
        }
        
    }
}
