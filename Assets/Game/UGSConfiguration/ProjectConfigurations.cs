using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public static class ProjectConfigurations
    {
        public static string ServerProjectId
        {
            get
            {
                #if UNITY_SERVER
                return "3c9d54dd-fa91-4f41-9b1c-9dde6ed524cb";
                #else
                return string.Empty;
                #endif
            }
        }
    }
}