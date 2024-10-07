using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ScriptableObjectEvents
{
    [Serializable]
    [CreateAssetMenu(fileName = "ScriptableObjectEventWithBool", menuName = "ScriptableObjectEvents/Scriptable Object Event With Bool")]
    public class ScriptableObjectEventWithBool : ScriptableObjectEventWithParameter<bool>
    {
        
    }
}
