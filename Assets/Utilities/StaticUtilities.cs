using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Utilities
{
    public static class StaticUtilities
    {
        public static void LogObservableCollection<T>(ObservableCollection<T> sender, string listName) where T : Object
        {
            Debug.Log($"{listName} number: " + sender.Count);

            /*for (int i = 0; i < sender.Count; i++)
            {
                Debug.Log($"{listName}: " +sender[i].name);
            }*/
        }
    }
}
