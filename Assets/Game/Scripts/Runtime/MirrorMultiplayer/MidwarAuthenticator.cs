using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Services.Authentication;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

namespace Game
{
    public class MidwarAuthenticator : NetworkAuthenticator
    {
        public MatchmakingResults MatchmakingResults { get; set; }

        readonly HashSet<NetworkConnection> connectionsPendingDisconnect = new HashSet<NetworkConnection>();

        #region Messages

        public struct MidwarAuthRequestMessage : NetworkMessage
        {
            // use whatever credentials make sense for your game
            // for example, you might want to pass the accessToken if using oauth
            public string playerId;
        }

        public struct MidwarAuthResponseMessage : NetworkMessage
        {
            public byte code;
            public string message;
        }

        #endregion

        #region Server

        /// <summary>
        /// Called on server from StartServer to initialize the Authenticator
        /// <para>Server message handlers should be registered in this method.</para>
        /// </summary>
        public override void OnStartServer()
        {
            // register a handler for the authentication request we expect from client
            NetworkServer.RegisterHandler<MidwarAuthRequestMessage>(OnAuthRequestMessage, false);
        }

        /// <summary>
        /// Called on server from StopServer to reset the Authenticator
        /// <para>Server message handlers should be unregistered in this method.</para>
        /// </summary>
        public override void OnStopServer()
        {
            // unregister the handler for the authentication request
            NetworkServer.UnregisterHandler<MidwarAuthRequestMessage>();
        }

        /// <summary>
        /// Called on server from OnServerConnectInternal when a client needs to authenticate
        /// </summary>
        /// <param name="conn">Connection to client.</param>
        public override void OnServerAuthenticate(NetworkConnectionToClient conn)
        {
            // do nothing...wait for AuthRequestMessage from client
        }

        /// <summary>
        /// Called on server when the client's AuthRequestMessage arrives
        /// </summary>
        /// <param name="conn">Connection to client.</param>
        /// <param name="msg">The message payload</param>
        public void OnAuthRequestMessage(NetworkConnectionToClient conn, MidwarAuthRequestMessage msg)
        {
            //Debug.Log($"Authentication Request: {msg.authUsername} {msg.authPassword}");

            if (connectionsPendingDisconnect.Contains(conn)) return;
            
            var player = MatchmakingResults.MatchProperties.Players.Find(x => x.Id == msg.playerId);
            
            if (player != null)
            {
                conn.authenticationData = player.Id;
                
                MidwarAuthResponseMessage authResponseMessage = new MidwarAuthResponseMessage
                {
                    code = 100,
                    message = "success"
                };
                
                conn.Send(authResponseMessage);

                // Accept the successful authentication
                ServerAccept(conn);
            }
            else
            {
                connectionsPendingDisconnect.Add(conn);

                // create and send msg to client so it knows to disconnect
                MidwarAuthResponseMessage authResponseMessage = new MidwarAuthResponseMessage
                {
                    code = 200,
                    message = "Invalid Credentials"
                };

                conn.Send(authResponseMessage);

                // must set NetworkConnection isAuthenticated = false
                conn.isAuthenticated = false;

                // disconnect the client after 1 second so that response message gets delivered
                StartCoroutine(DelayedDisconnect(conn, 1f));
            }
        }

        IEnumerator DelayedDisconnect(NetworkConnectionToClient conn, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);

            // Reject the unsuccessful authentication
            ServerReject(conn);

            yield return null;

            // remove conn from pending connections
            connectionsPendingDisconnect.Remove(conn);
        }

        #endregion

        #region Client

        /// <summary>
        /// Called on client from StartClient to initialize the Authenticator
        /// <para>Client message handlers should be registered in this method.</para>
        /// </summary>
        public override void OnStartClient()
        {
            // register a handler for the authentication response we expect from server
            NetworkClient.RegisterHandler<MidwarAuthResponseMessage>(OnAuthResponseMessage, false);
        }

        /// <summary>
        /// Called on client from StopClient to reset the Authenticator
        /// <para>Client message handlers should be unregistered in this method.</para>
        /// </summary>
        public override void OnStopClient()
        {
            // unregister the handler for the authentication response
            NetworkClient.UnregisterHandler<MidwarAuthResponseMessage>();
        }

        /// <summary>
        /// Called on client from OnClientConnectInternal when a client needs to authenticate
        /// </summary>
        public override void OnClientAuthenticate()
        {
            MidwarAuthRequestMessage authRequestMessage = new MidwarAuthRequestMessage
            {
                playerId = AuthenticationService.Instance.PlayerId
            };

            NetworkClient.Send(authRequestMessage);
        }

        /// <summary>
        /// Called on client when the server's AuthResponseMessage arrives
        /// </summary>
        /// <param name="msg">The message payload</param>
        public void OnAuthResponseMessage(MidwarAuthResponseMessage msg)
        {
            if (msg.code == 100)
            {
                //Debug.Log($"Authentication Response: {msg.message}");

                // Authentication has been accepted
                ClientAccept();
            }
            else
            {
                Debug.LogError($"Authentication Response: {msg.message}");

                // Authentication has been rejected
                ClientReject();
            }
        }

        #endregion
    }
}
