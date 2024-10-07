using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public class DebugLogHelper : MonoBehaviour
    {
        public void Log(float debugFloat)
        {
            Debug.Log(debugFloat);
        }
        public void Log(string debugString)
        {
            Debug.Log(debugString);
        }
        
        public void LogError(string debugString)
        {
            Debug.LogError(debugString);
        }
        
        public void LogWarning(string debugString)
        {
            Debug.LogWarning(debugString);
        }
        
        public void LogBool(bool debugBool)
        {
            Debug.Log(debugBool);
        }
    }
}
