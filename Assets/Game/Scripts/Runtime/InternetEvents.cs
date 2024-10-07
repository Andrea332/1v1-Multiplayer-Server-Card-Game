using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class InternetEvents : MonoBehaviour
    {
        [SerializeField] private UnityEvent onInternetNotAvailable;

        public void OnInternetNotAvailable()
        {
            onInternetNotAvailable?.Invoke();
        }
    }
}
