using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ScriptableObjectEvents
{
    [Serializable]
    [CreateAssetMenu(fileName = "ScriptableObjectEventWithSO", menuName = "ScriptableObjectEvents/Scriptable Object Event With SO")]
    public class ScriptableObjectEventWithSO : ScriptableObjectEventWithParameter<ScriptableObject>
    {
        
    }
}
