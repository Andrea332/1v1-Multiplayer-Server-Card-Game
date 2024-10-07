using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    [CreateAssetMenu(fileName = "ItemsListContainer", menuName = "Items/Items List Container", order = 99)]
    public class ItemsListContainer : ScriptableObject
    {
        public List<ItemsList> itemsLists;
    }
}
