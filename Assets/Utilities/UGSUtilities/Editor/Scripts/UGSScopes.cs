using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Utilities
{
    [CreateAssetMenu(fileName = "UGSScopes", menuName = "UGS Utilities/UGSScopes")]
    public class UGSScopes : ScriptableObject
    {
        public List<string> scopes;
    }
}

