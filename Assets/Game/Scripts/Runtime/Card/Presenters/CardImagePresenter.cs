using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game
{
    public class CardImagePresenter : ParameterPresenter<string>
    {
        [SerializeField] private Image cardImage;
        [SerializeField] private SO_ID item_ID;
        [SerializeField] private SO_ID Sprite_ID;
        [SerializeField] private ItemsList cardTemplates;
        public override void SetParameter(string parameter)
        {
            base.SetParameter(parameter);
            BasePropertiesItem cardTemplate = cardTemplates.items.Find(x => x.GetItemValue<string>(item_ID.id) == parameter);
            Sprite sprite = cardTemplate.GetItemValue<Sprite>(Sprite_ID.id);
            cardImage.sprite = sprite;
        }
    }
}
