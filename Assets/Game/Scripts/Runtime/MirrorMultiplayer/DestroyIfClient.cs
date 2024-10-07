using UnityEngine;

namespace Game
{
    public class DestroyIfClient : MonoBehaviour
    {
#if !UNITY_SERVER
        private void Awake()
        {
            Destroy(gameObject);
        }
    
#endif
    }
}
