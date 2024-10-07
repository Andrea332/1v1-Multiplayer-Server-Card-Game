using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class DestroyIfBuild : MonoBehaviour
    {
#if !UNITY_EDITOR
    private void Awake()
    {
        Destroy(gameObject);
    }
#endif
    }
}
