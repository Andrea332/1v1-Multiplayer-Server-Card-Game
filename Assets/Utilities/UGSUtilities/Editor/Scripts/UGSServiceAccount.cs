using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Utilities
{
    [CreateAssetMenu(fileName = "UGSServiceAccount", menuName = "UGS Utilities/UGSServiceAccount")]
    public class UGSServiceAccount : ScriptableObject
    {
        public string serviceAccountKeyId;
        public string serviceAccountSecretKey;
    }
}

