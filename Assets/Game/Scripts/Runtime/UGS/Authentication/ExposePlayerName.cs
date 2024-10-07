using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class ExposePlayerName : MonoBehaviour
    {
        [SerializeField] private UnityEvent<string> onUgsAccountName;
        private void Start()
        {
            onUgsAccountName?.Invoke(AuthenticationService.Instance.PlayerName);
        }
    }
}
