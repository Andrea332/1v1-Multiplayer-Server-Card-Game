using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class ExposePlayerId : MonoBehaviour
    {
        [SerializeField] private UnityEvent<string> onUgsAccountId;
        private void Start()
        {
            onUgsAccountId?.Invoke(AuthenticationService.Instance.PlayerId);
        }
    }
}
