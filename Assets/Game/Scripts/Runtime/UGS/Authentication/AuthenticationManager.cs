using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class AuthenticationManager : MonoBehaviour
    {
        public UnityEvent onSignSuccess;
        public UnityEvent<string> onSignError;
        public UnityEvent<string> onPlayerInfo;

#if !UNITY_SERVER
        private async void Start()
        {
            await InitializeUgsServices();
            Authenticate();
        }
#endif

        private async Task InitializeUgsServices()
        {
            try
            {
                //We MUST set another profile for the cloned instance of the game,
                //because matchmaking will use the same playerID for making the ticket if don't do it, in that case the tickets will not match.
                //With another profile setted the two instance are getted as separeted accounts and can be matched togheter.
                //All this because the signIn session token is saved in the playerPrefs,
                //and using the same profile means that after making the signIn on the two instances
                //the second signIn overwrite the same session token that is used for creating the tickets
                //Using another profile make possible to save two or more different session token and signIn with differnet account on different profiles
#if UNITY_EDITOR
                InitializationOptions initializationOptions = new();
                initializationOptions.SetEnvironmentName("test");
                if (ParrelSync.ClonesManager.IsClone())
                {
                    initializationOptions.SetProfile("clone");
                    await UnityServices.InitializeAsync(initializationOptions);
                }
                else
                {
                    await UnityServices.InitializeAsync();
                }
#else
                InitializationOptions initializationOptions = new();
                initializationOptions.SetEnvironmentName("production");
                await UnityServices.InitializeAsync(initializationOptions);
#endif
            }
            catch (ServicesInitializationException servicesInitializationException)
            {
#if UNITY_EDITOR
                Debug.LogError(servicesInitializationException);
#endif
                onSignError?.Invoke(servicesInitializationException.Message);
            }
        }
        
        private void Authenticate()
        {
            var authenticationManagers = GetComponents<IAuthenticationManager>();
            var authenticationManager = authenticationManagers.ToList().Find(authenticationManager => authenticationManager.Enabled);
            authenticationManager.OnSuccess.AddListener(OnAuthenticationSuccess);
            authenticationManager.OnError.AddListener(OnAuthenticationError);
            authenticationManager.Authenticate();
        }

        private void OnAuthenticationSuccess()
        {
            onSignSuccess?.Invoke();
        }
        
        private void OnAuthenticationError(string error)
        {
            onSignError?.Invoke(error);
        }


        public async void GetPlayerInfo()
        {
            PlayerInfo playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();
            onPlayerInfo?.Invoke(playerInfo.Username);
        }
    }
}

