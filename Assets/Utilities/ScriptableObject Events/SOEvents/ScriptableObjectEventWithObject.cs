using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ScriptableObjectEvents
{
    [Serializable]
    [CreateAssetMenu(fileName = "ScriptableObjectEventWithObject", menuName = "ScriptableObjectEvents/Scriptable Object Event With object")]
    public class ScriptableObjectEventWithObject : ScriptableObjectEventWithParameter<object>
    {
        
    }
}
