using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "BaseInstantiator", menuName = "Instatiable/BaseInstantiator")]
    public class Instantiator : ScriptableObject
    {
        public GameObject prefabToInstantiate;
        
        public virtual void SpawnPrefab()
        {
            Instantiate(prefabToInstantiate,null);
        }
        public virtual void SpawnAndStartFunction(string functionName, object functionParameter = null)
        {
            GameObject spawnedPrefab = SpawnPrefabAndReturnObject();
            if (functionParameter == null)
            {
                spawnedPrefab.SendMessage(functionName);
                return;
            }
            spawnedPrefab.SendMessage(functionName, functionParameter);
        }
        private protected GameObject SpawnPrefabAndReturnObject()
        {
            return Instantiate(prefabToInstantiate.gameObject,null);
        }
    }
}
