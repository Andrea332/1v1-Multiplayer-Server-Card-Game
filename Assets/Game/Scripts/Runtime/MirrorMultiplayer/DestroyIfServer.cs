using UnityEngine;

namespace Game
{
    public class DestroyIfServer : MonoBehaviour
    {
#if UNITY_SERVER
    private void Awake()
    {
        Destroy(gameObject);
    }
    
#endif
    }
}
