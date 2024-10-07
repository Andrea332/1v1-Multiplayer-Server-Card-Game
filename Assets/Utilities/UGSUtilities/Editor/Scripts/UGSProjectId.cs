using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Utilities
{
    [CreateAssetMenu(fileName = "UGSProjectId", menuName = "UGS Utilities/UGSProjectId")]
    public class UGSProjectId : ScriptableObject
    {
        public string projectId;
    }
}

