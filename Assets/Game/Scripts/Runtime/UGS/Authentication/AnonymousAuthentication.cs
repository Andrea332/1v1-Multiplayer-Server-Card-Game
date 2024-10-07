using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Steamworks;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class AnonymousAuthentication : MonoBehaviour, IAuthenticationManager
    {
        public UnityEvent OnSuccess { get; set; } = new();
        public UnityEvent<string> OnError { get; set; } = new();
        public bool Enabled => enabled;
        private void Awake()
        {
#if  UNITY_SERVER
            enabled = false;
#elif UNITY_EDITOR
            if (!ParrelSync.ClonesManager.IsClone())
            {
                enabled = false;
            }
#else
            enabled = false;
#endif
        }
        
        public async void Authenticate()
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                var playerName = await AuthenticationService.Instance.GetPlayerNameAsync();

                var testAccountName = "TestAccount";

                if (playerName != testAccountName)
                {
                    await AuthenticationService.Instance.UpdatePlayerNameAsync(testAccountName);
                }

                OnSuccess?.Invoke();
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR
                Debug.LogError(ex);
#endif  
                OnError?.Invoke(ex.Message);
            }
        }
    }
}
