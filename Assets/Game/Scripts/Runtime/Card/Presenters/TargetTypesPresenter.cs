using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game
{
    public class TargetTypesPresenter : ParameterPresenter<List<string>>
    {
        [SerializeField] private SO_ID item_ID;
        [SerializeField] private SO_ID Sprite_ID;
        [SerializeField] private ItemsList cardTypes;
        [SerializeField] private Image targetTypeImagePrefab;
        [SerializeField] private Transform targetTypeImageParent;
        public override void SetParameter(List<string> parameter)
        {
            base.SetParameter(parameter);
            foreach (var targetTypeId in parameter)
            {
                BasePropertiesItem cardType = cardTypes.items.Find(x => x.GetItemValue<string>(item_ID.id) == targetTypeId);
                
                if (cardType == null)
                {
                    Debug.LogWarning("Card type: " + parameter + " Not found in the CardTypeList");
                    return;
                }

                Sprite sprite = cardType.GetItemValue<Sprite>(Sprite_ID.id);
                Image targetTypeImage = Instantiate(targetTypeImagePrefab, targetTypeImageParent);
                targetTypeImage.name = $"Target: {targetTypeId}";
                targetTypeImage.preserveAspect = true;
                targetTypeImage.sprite = sprite;
            }
        }
    }
}
