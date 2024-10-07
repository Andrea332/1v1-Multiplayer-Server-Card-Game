using System;
using Cysharp.Threading.Tasks;
using kcp2k;
using Mirror;
using Mirror.SimpleWeb;
using ScriptableObjectEvents;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class MirrorClientManager : MonoBehaviour
    {
        [SerializeField] private ScriptableObjectEventWithObject startClientEvent;
        [SerializeField] private ScriptableObjectEvent stopClientEvent;
        [SerializeField] private string debugIpAddress;
        [SerializeField] private ushort debugPort;
        [SerializeField] private MidwarNetworkManager midwarNetworkManager;
        [SerializeField] private UnityEvent onClientConnect;
        [SerializeField] private UnityEvent onClientDisconnect;
        
#if !UNITY_SERVER
        
        private void Awake()
        {
            stopClientEvent.onEvent.AddListener(StopClient);
            startClientEvent.onEvent.AddListener(StartClient);
            midwarNetworkManager.onStartClient += OnStartClient;
            midwarNetworkManager.onClientConnect += OnClientConnect;
            midwarNetworkManager.onClientDisconnect += OnClientDisconnect;
            midwarNetworkManager.onStopClient += OnStopClient;
            midwarNetworkManager.onClientError += OnClientError;
            midwarNetworkManager.onClientNotReady += OnClientNotReady;
        }
        
        private void OnDestroy()
        {
            stopClientEvent.onEvent.RemoveListener(StopClient);
            startClientEvent.onEvent.RemoveListener(StartClient);
            midwarNetworkManager.onStartClient -= OnStartClient;
            midwarNetworkManager.onClientConnect -= OnClientConnect;
            midwarNetworkManager.onClientDisconnect -= OnClientDisconnect;
            midwarNetworkManager.onStopClient -= OnStopClient;
            midwarNetworkManager.onClientError -= OnClientError;
            midwarNetworkManager.onClientNotReady -= OnClientNotReady;
        }

#endif
        public void StartClient(string ipAddress, ushort port)
        {
            NetworkManager.singleton.networkAddress = ipAddress;
            ((PortTransport)NetworkManager.singleton.transport).Port = port;
            NetworkManager.singleton.StartClient();
        }
        
        [ContextMenu("StartDebugClient")]
        public void StartDebugClient()
        {
            StartDebugClient(debugIpAddress, debugPort);
        }

        public void StartDebugClient(string ipAddress, ushort port)
        {
            ((PortTransport)NetworkManager.singleton.transport).Port = port;
            NetworkManager.singleton.networkAddress = ipAddress;
            
            NetworkManager.singleton.StartClient();
        }
        
        public void StartClient(object multiplayAssignment)
        {
            MultiplayAssignment currentMultiplayAssignment = (MultiplayAssignment)multiplayAssignment;
            StartClient(currentMultiplayAssignment.Ip, (ushort)currentMultiplayAssignment.Port);
        }

        public void StopClient()
        {
            NetworkManager.singleton.StopClient();
        }

        private void OnClientNotReady()
        {
            
        }

        private void OnClientError(TransportError arg1, string arg2)
        {
            
        }

        private void OnStopClient()
        {
            onClientDisconnect?.Invoke();
        }

        private void OnClientDisconnect()
        {
            
        }

        private void OnClientConnect()
        {
            onClientConnect?.Invoke();
        }

        private void OnStartClient()
        {
         
        }
    }
}
