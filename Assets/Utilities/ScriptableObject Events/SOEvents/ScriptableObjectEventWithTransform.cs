using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ScriptableObjectEvents
{
    [Serializable]
    [CreateAssetMenu(fileName = "ScriptableObjectEventWithTransform", menuName = "ScriptableObjectEvents/Scriptable Object Event With Transform")]
    public class ScriptableObjectEventWithTransform : ScriptableObjectEventWithParameter<Transform>
    {
        
    }
}
