using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game
{
    public class CardTypePresenter : ParameterPresenter<string>
    {
        [SerializeField] private Image typeImage;
        [SerializeField] private SO_ID item_ID;
        [SerializeField] private SO_ID Sprite_ID;
        [SerializeField] private ItemsList cardTypes;
        public override void SetParameter(string parameter)
        {
            base.SetParameter(parameter);
            BasePropertiesItem cardType = cardTypes.items.Find(x => x.GetItemValue<string>(item_ID.id) == parameter);
            
            if (cardType == null)
            {
                Debug.LogWarning("Card type: " + parameter + " Not found in the CardTypeList");
                return;
            }

            Sprite sprite = cardType.GetItemValue<Sprite>(Sprite_ID.id);
            typeImage.sprite = sprite;
        }
    }
}
