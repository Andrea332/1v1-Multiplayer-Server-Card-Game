using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    [CreateAssetMenu(fileName = "ItemsList", menuName = "Items/Items List", order = 99)]
    public class ItemsList : ScriptableObject
    {
        public List<BasePropertiesItem> items;
        
        //Find BasePropertyItem by passing the itemIdToFind and which id to find
        public BasePropertiesItem FindPropertiesItem(string itemIdType, string itemIdToFind)
        {
            BasePropertiesItem basePropertiesItem = items.Find(x => x.GetItemValue<string>(itemIdType) == itemIdToFind);

            if (basePropertiesItem != null) return basePropertiesItem;
            
            Debug.LogError($"Item with item id -{itemIdToFind}- not found in -{name}- list");
            return null;
        }
    }
}
