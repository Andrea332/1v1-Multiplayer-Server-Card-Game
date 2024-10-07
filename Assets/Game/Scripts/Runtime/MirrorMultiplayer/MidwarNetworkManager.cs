using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Game
{
    public class MidwarNetworkManager : NetworkManager
    {
        public Action<NetworkConnectionToClient> onServerConnect;
        public Action<NetworkConnectionToClient> onServerDisconnect;
        public Action<NetworkConnectionToClient> onServerReady;
        public Action<NetworkConnectionToClient> onServerAddPlayer;
        public Action<NetworkConnectionToClient,TransportError,string> onServerError;
        public Action onClientConnect;
        public Action onClientDisconnect;
        public Action<TransportError,string> onClientError;
        public Action onClientNotReady;
        public Action onStartServer;
        public Action onStartClient;
        public Action onStopServer;
        public Action onStopClient;
        
        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            Debug.Log("MidwarNetworkManager, OnServerConnect: " + conn.connectionId);
            onServerConnect?.Invoke(conn);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            Debug.Log("MidwarNetworkManager, OnServerDisconnect: " + conn.connectionId);
            onServerDisconnect?.Invoke(conn);
        }

        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
            Debug.Log("MidwarNetworkManager, OnServerReady");
            onServerReady?.Invoke(conn);
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);
            Debug.Log("MidwarNetworkManager, OnServerAddPlayer");
            onServerAddPlayer?.Invoke(conn);
        }

        public override void OnServerError(NetworkConnectionToClient conn, TransportError error, string reason)
        {
            base.OnServerError(conn, error, reason);
            Debug.Log("MidwarNetworkManager, OnServerError");
            onServerError?.Invoke(conn, error, reason);
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            Debug.Log("MidwarNetworkManager, OnClientConnect");
            onClientConnect?.Invoke();
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            Debug.Log("MidwarNetworkManager, OnClientDisconnect");
            onClientDisconnect?.Invoke();
        }

        public override void OnClientError(TransportError error, string reason)
        {
            base.OnClientError(error, reason);
            Debug.Log("MidwarNetworkManager, OnClientError");
            onClientError?.Invoke(error, reason);
        }

        public override void OnClientNotReady()
        {
            base.OnClientNotReady();
            Debug.Log("MidwarNetworkManager, OnClientNotReady");
            onClientNotReady?.Invoke();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("MidwarNetworkManager, OnStartServer");
            onStartServer?.Invoke();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            Debug.Log("MidwarNetworkManager, OnStartClient");
            onStartClient?.Invoke();
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            Debug.Log("MidwarNetworkManager, OnStopServer");
            onStopServer?.Invoke();
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            Debug.Log("MidwarNetworkManager, OnStopClient");
            onStopClient?.Invoke();
        }
    }
}
