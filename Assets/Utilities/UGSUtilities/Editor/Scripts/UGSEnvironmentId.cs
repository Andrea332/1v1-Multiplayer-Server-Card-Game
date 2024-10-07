using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Utilities
{
    [CreateAssetMenu(fileName = "UGSEnvironmentId", menuName = "UGS Utilities/UGSEnvironmentId")]
    public class UGSEnvironmentId : ScriptableObject
    {
        public string environmentId;
    }
}

