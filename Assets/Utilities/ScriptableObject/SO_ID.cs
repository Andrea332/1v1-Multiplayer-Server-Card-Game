using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    [CreateAssetMenu(fileName = "BaseItemScriptableObject", menuName = "Items/ID Object", order = 1)]
    public class SO_ID : ScriptableObject
    {
        public string id;
    }
}
