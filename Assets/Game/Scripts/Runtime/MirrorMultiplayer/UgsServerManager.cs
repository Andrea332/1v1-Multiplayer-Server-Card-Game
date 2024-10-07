using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.Services.Core;
using System.Threading.Tasks;
using Mirror.SimpleWeb;
using Unity.Services.Apis;
using Unity.Services.Matchmaker.Models;

#if UNITY_SERVER
using kcp2k;
using Unity.Services.Multiplay;
#endif

namespace Game
{
    public class UgsServerManager : MonoBehaviour
    {
        private const ushort k_DefaultMaxPlayers = 2;
        private const string k_DefaultServerName = "CardBattleServer";
        private const string k_DefaultGameType = "CardBattle";
        private const string k_DefaultBuildId = "1";
        private const string k_DefaultMap = "MainScene";
        [SerializeField] private MidwarAuthenticator midwarAuthenticator;
        public MatchmakingResults matchmakingResults;
        public MidwarNetworkManager midwarNetworkManager;
      
        
#if UNITY_SERVER

        private IServerQueryHandler serverQueryHandler;
        public IServerClient serverClient;
        
        private void Start()
        {
            InizializeServer();
        }

        private void Update()
        {
            serverQueryHandler?.UpdateServerCheck();
        }

        private async void InizializeServer()
        {
            await UnityServices.InitializeAsync();
            serverClient = await SignInFromServer();
            var serverConfig = MultiplayService.Instance.ServerConfig;
            Debug.Log($"MIDWAR IP Address :{serverConfig.IpAddress}");
            Debug.Log($"MIDWAR Server ID: {serverConfig.ServerId}");
            Debug.Log($"MIDWAR AllocationID: {serverConfig.AllocationId}");
            Debug.Log($"MIDWAR Port: {serverConfig.Port}");
            Debug.Log($"MIDWAR QueryPort: {serverConfig.QueryPort}");
            Debug.Log($"MIDWAR LogDirectory: {serverConfig.ServerLogDirectory}");
            
            var multiplayEventCallbacks = new MultiplayEventCallbacks();
            multiplayEventCallbacks.Allocate += MultiplayEventCallbacksOnAllocate;
            multiplayEventCallbacks.Deallocate += MultiplayEventCallbacksOnDeallocate;
            multiplayEventCallbacks.Error += MultiplayEventCallbacksOnError;
            multiplayEventCallbacks.SubscriptionStateChanged += MultiplayEventCallbacksOnSubscriptionStateChanged;
            await MultiplayService.Instance.SubscribeToServerEventsAsync(multiplayEventCallbacks);
           
            serverQueryHandler = await MultiplayService.Instance.StartServerQueryHandlerAsync(k_DefaultMaxPlayers, k_DefaultServerName, k_DefaultGameType, k_DefaultBuildId, k_DefaultMap);
          
            PortTransport transport =  (PortTransport)NetworkManager.singleton.transport;
         
            transport.Port = serverConfig.Port;
        }

        private async Task<IServerClient> SignInFromServer()
        {
            var serverClient = ApiService.CreateServerClient();
           
            /*var tokenResponse = await serverClient.SignInWithServiceAccount();
            
            if (tokenResponse.IsSuccessful)
            {
                Debug.Log("UGS Sign in success");
            }
            else
            {
                Debug.Log("UGS Sign in error: " + tokenResponse.ErrorType);
            }*/

            if (string.IsNullOrEmpty(serverClient.AccessToken))
            {
                Debug.Log("UGS Sign in error");
            }
            else
            {
                Debug.Log("UGS Sign in success");
            }
            
            return serverClient;
        }
    
        private void MultiplayEventCallbacksOnSubscriptionStateChanged(MultiplayServerSubscriptionState obj)
        {
            switch (obj)
            {
                case MultiplayServerSubscriptionState.Unsubscribed: Debug.Log("MIDWAR The Server Events subscription has been unsubscribed from."); break;
                case MultiplayServerSubscriptionState.Synced: Debug.Log("MIDWAR The Server Events subscription is up to date and active."); break;
                case MultiplayServerSubscriptionState.Unsynced: Debug.Log("MIDWAR The Server Events subscription has fallen out of sync, the subscription tries to automatically recover."); break;
                case MultiplayServerSubscriptionState.Error: Debug.Log("MIDWAR The Server Events subscription has fallen into an errored state and won't recover automatically."); break;
                case MultiplayServerSubscriptionState.Subscribing: Debug.Log("MIDWAR The Server Events subscription is trying to sync."); break;
            }
        }

        private void MultiplayEventCallbacksOnError(MultiplayError obj)
        {
            /*
            Here is where you handle the error.
            This is highly dependent on your game. You can inspect the error by accessing the error.Reason and error.Detail fields.
            You can change on the error.Reason field, log the error, or otherwise handle it as you need to.
            */
            Debug.Log("MIDWAR Error: " + obj.Reason + " | Details: " + obj.Detail);
        }

        private void MultiplayEventCallbacksOnDeallocate(MultiplayDeallocation obj)
        {
            /*
            Here is where you handle the deallocation.
            This is highly dependent on your game, however this would typically be some sort of teardown process.
            You might want to deactivate unnecessary NPCs, log to a file, or perform any other cleanup actions.
            */
            Debug.Log("MIDWAR Deallocated");
        }

        private void MultiplayEventCallbacksOnAllocate(MultiplayAllocation obj)
        {
            /*
            Here is where you handle the allocation.
            This is highly dependent on your game, however this would typically be some sort of setup process.
            Whereby, you spawn NPCs, setup the map, log to a file, or otherwise prepare for players.
            After you the allocation has been handled, you can then call ReadyServerForPlayersAsync()!
             */
            Debug.Log("MIDWAR Allocated");
            OnAllocated();
        }

        private async void OnAllocated()
        {
            matchmakingResults = await MultiplayService.Instance.GetPayloadAllocationFromJsonAs<MatchmakingResults>();
            midwarAuthenticator.MatchmakingResults = matchmakingResults;
            midwarNetworkManager.maxConnections = matchmakingResults.MatchProperties.Players.Count;
            midwarNetworkManager.StartServer();
        }


        public void AddPlayerToServerQueryHandler()
        {
            if(serverQueryHandler.CurrentPlayers >= serverQueryHandler.MaxPlayers) return;
            
            serverQueryHandler.CurrentPlayers++;
        }
        
        public void RemovePlayerToServerQueryHandler()
        {
            if(serverQueryHandler.CurrentPlayers <= 0) return;
            
            serverQueryHandler.CurrentPlayers--;

            if (serverQueryHandler.CurrentPlayers <= 0)
            {
                MultiplayService.Instance.UnreadyServerAsync();
                
                Application.Quit();
            }
        }
#endif

    }
}
