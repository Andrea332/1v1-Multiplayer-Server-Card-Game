using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game
{
    public class TextColorSetter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Color colorToSet;
        
        public void SetTextColor()
        {
            text.color = colorToSet;
        }
    }
}
