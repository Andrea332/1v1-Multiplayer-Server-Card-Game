using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game
{
    public class SelectableBattleCard : BaseCard
    {
        [SerializeField] private Image selectedImage;

        public override void OnDrag(PointerEventData eventData) { }

        public override void OnBeginDrag(PointerEventData eventData) { }
       
        public override void OnEndDrag(PointerEventData eventData) { }

        public override void OnPointerEnter(PointerEventData eventData) { }

        public override void OnPointerExit(PointerEventData eventData) { }

        public void SetAsSelected()
        {
            selectedImage.enabled = true;
        }
        
        public void SetAsUnselected()
        {
            selectedImage.enabled = false;
        }

    }
}
