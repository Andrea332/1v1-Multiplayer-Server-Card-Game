using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Steamworks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class SteamAuthentication : MonoBehaviour, IAuthenticationManager
    {
        public UnityEvent OnSuccess { get; set; } = new();
        public UnityEvent<string> OnError { get; set; }= new();
        public bool Enabled => enabled;
        
        private Callback<GetTicketForWebApiResponse_t> m_AuthTicketForWebApiResponseCallback;
        private string m_SessionTicket;
        private string Identity => "unityauthenticationservice";
        

        private void Awake()
        {
#if DISABLESTEAMWORKS || UNITY_SERVER
            enabled = false;
#elif UNITY_EDITOR
            if (ParrelSync.ClonesManager.IsClone())
            {
                enabled = false;
            }
#endif
        }

        
        public void Authenticate()
        {
            try
            {
                // It's not necessary to add event handlers if they are 
                // already hooked up.
                // Callback.Create return value must be assigned to a 
                // member variable to prevent the GC from cleaning it up.
                // Create the callback to receive events when the session ticket
                // is ready to use in the web API.
                // See GetAuthSessionTicket document for details.
                m_AuthTicketForWebApiResponseCallback = Callback<GetTicketForWebApiResponse_t>.Create(OnAuthCallback);
                SteamUser.GetAuthTicketForWebApi(Identity);
            }
            catch (Exception exception)
            {
#if UNITY_EDITOR
                Debug.LogError(exception);
#endif  
                OnError?.Invoke(exception.Message);
            }
        }

        private void OnAuthCallback(GetTicketForWebApiResponse_t callback)
        {
            m_SessionTicket = BitConverter.ToString(callback.m_rgubTicket).Replace("-", string.Empty);
            m_AuthTicketForWebApiResponseCallback.Dispose();
            m_AuthTicketForWebApiResponseCallback = null;
            Debug.Log("Steam Login success. Session Ticket: " + m_SessionTicket);
            
            // Call Unity Authentication SDK to sign in or link with Steam, displayed in the following examples, using the same identity string and the m_SessionTicket.
            SignInWithSteamAsync(m_SessionTicket, Identity).Forget();
        }

        async UniTask SignInWithSteamAsync(string ticket, string identity)
        {
            try
            {
                await AuthenticationService.Instance.SignInWithSteamAsync(ticket, identity);

                var playerName = await AuthenticationService.Instance.GetPlayerNameAsync();

                var steamName = SteamFriends.GetPersonaName();
                
                if (playerName != steamName)
                {
                    await AuthenticationService.Instance.UpdatePlayerNameAsync(steamName);
                }
                
                OnSuccess?.Invoke();
            }
            catch (Exception exception)
            {
#if UNITY_EDITOR
                Debug.LogError(exception);
#endif  
                OnError?.Invoke(exception.Message);
            }
        }
    }
}

