using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ScriptableObjectEvents
{
    [Serializable]
    [CreateAssetMenu(fileName = "ScriptableObjectEventWithString", menuName = "ScriptableObjectEvents/Scriptable Object Event With String")]
    public class ScriptableObjectEventWithString : ScriptableObjectEventWithParameter<string>
    {
        
    }
}
