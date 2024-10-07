using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game
{
    public class RarityPresenter : ParameterPresenter<string>
    {
        [SerializeField] private Image rarityImage;
        [SerializeField] private SO_ID item_ID;
        [SerializeField] private SO_ID Sprite_ID;
        [SerializeField] private ItemsList rarities;
        public override void SetParameter(string parameter)
        {
            base.SetParameter(parameter);
            BasePropertiesItem rarity = rarities.items.Find(x => x.GetItemValue<string>(item_ID.id) == parameter);
            
            if (rarity == null)
            {
                Debug.LogWarning("Rarity: " + parameter + " Not found in the rarityList");
                return;
            }
            Sprite sprite = rarity.GetItemValue<Sprite>(Sprite_ID.id);
            rarityImage.sprite = sprite;
        }
    }
}
