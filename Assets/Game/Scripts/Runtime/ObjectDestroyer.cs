using UnityEngine;

namespace Game
{
    public class ObjectDestroyer : MonoBehaviour
    {

        [SerializeField] private float destroyAfterSeconds;
        public void DestroyObject(GameObject gameObjectToDestroy)
        {
            Destroy(gameObjectToDestroy, destroyAfterSeconds);
        }
        
        public void DestroyInstantly(GameObject gameObjectToDestroy)
        {
            Destroy(gameObjectToDestroy);
        }
    }
}
