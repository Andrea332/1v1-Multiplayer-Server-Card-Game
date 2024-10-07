using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace Utilities
{
    [CreateAssetMenu(fileName = "BasePropertiesItem", menuName = "Items/Properties Item", order = 2)]
    public class BasePropertiesItem : ScriptableObject, IItemValueable<BasePropertiesItem>
    {
        public List<PropertyItem> itemProperties;
        
        public T GetItem<T>(string propertyId) where T : ScriptableObject
        {
            PropertyItem propertyItem = itemProperties.Find(x => x.idObject.id == propertyId);

            return (T)propertyItem?.item;
        }
        public T GetItemValue<T>(string propertyId)
        {
            PropertyItem propertyItem = itemProperties.Find(x => x.idObject.id == propertyId);

            if (propertyItem == null)
            {
                return default;
            }

            IItemValueable<T> itemValueable = (IItemValueable<T>)propertyItem.item;
            
            return itemValueable.ItemValue;
        }

        public BasePropertiesItem ItemValue => this;
    }
    [Serializable]
    public class PropertyItem
    {
        public SO_ID idObject;
        public ScriptableObject item;
    }
}
