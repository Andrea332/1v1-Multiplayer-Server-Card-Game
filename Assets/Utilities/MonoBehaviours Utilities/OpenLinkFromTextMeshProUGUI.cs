using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Utilities
{
 
    public class OpenLinkFromTextMeshProUGUI : MonoBehaviour, IPointerClickHandler
    {
        public TextMeshProUGUI text;
        
        public void OnPointerClick(PointerEventData eventData)
        {
          
            //var linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
            
            //var linkId = text.textInfo.linkInfo[linkIndex].GetLinkID();
		
           

            // Let's see that web page!
            Application.OpenURL(text.text);
        }
    }
}
