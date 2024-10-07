using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mirror;
using UnityEngine;

namespace Game
{
    public class CardBattleServerManager : MonoBehaviour
    {
        [SerializeField] private MidwarNetworkManager midwarNetworkManager;
        [SerializeField] private UgsServerManager ugsServerManager;
        [SerializeField] private CardBattleBoardManager cardBattleBoardManagerPrefab;
        private CardBattleBoardManager spawnedCardBattleBoardManager;
        
#if UNITY_SERVER
        
        private void Awake()
        {
            midwarNetworkManager.onStartServer += OnStartServer;
            midwarNetworkManager.onServerConnect += OnServerConnect;
            midwarNetworkManager.onServerDisconnect += OnServerDisconnect;
            midwarNetworkManager.onServerReady += OnServerReady;
            midwarNetworkManager.onServerAddPlayer += OnServerAddPlayer;
            midwarNetworkManager.onServerError += OnServerError;
        }

        private void OnDestroy()
        {
            midwarNetworkManager.onStartServer -= OnStartServer;
            midwarNetworkManager.onServerConnect -= OnServerConnect;
            midwarNetworkManager.onServerDisconnect -= OnServerDisconnect;
            midwarNetworkManager.onServerReady -= OnServerReady;
            midwarNetworkManager.onServerAddPlayer -= OnServerAddPlayer;
            midwarNetworkManager.onServerError -= OnServerError;
        }

        private void OnStartServer()
        {
            InizializeCardBattleMode();
        }
        private void OnServerConnect(NetworkConnectionToClient conn)
        {
            ugsServerManager.AddPlayerToServerQueryHandler();
        }

        private void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            ugsServerManager.RemovePlayerToServerQueryHandler();
            if(spawnedCardBattleBoardManager.matchFinished) return;
            string currentPlayerId = (string)conn.authenticationData;
            spawnedCardBattleBoardManager.OnPlayerDisconnected(currentPlayerId).Forget();
        }

        private void OnServerReady(NetworkConnectionToClient conn)
        {
           
        }

        private void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
           
        }

        private void OnServerError(NetworkConnectionToClient conn, TransportError error, string reason)
        {
          
        }
        
        [Server]
        private async void InizializeCardBattleMode()
        {
            GameObject gameObjectToSpawn = midwarNetworkManager.spawnPrefabs.Find(spawnableGameObject => spawnableGameObject == cardBattleBoardManagerPrefab.gameObject);

            if (gameObjectToSpawn == null)
            {
                Debug.LogError($"GameObject {cardBattleBoardManagerPrefab.gameObject.name} not found in the spawnableGameObjects of the MidwarNetworkManager");
                return;
            }

            spawnedCardBattleBoardManager = Instantiate(cardBattleBoardManagerPrefab);
            await spawnedCardBattleBoardManager.Inizialize(ugsServerManager);
            
            NetworkServer.Spawn(spawnedCardBattleBoardManager.gameObject);
        }
#endif  
    }
}


