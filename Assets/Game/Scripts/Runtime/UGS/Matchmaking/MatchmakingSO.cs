using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    [CreateAssetMenu(fileName = "MatchmakingSO", menuName = "UGS Commands/MatchmakingSO")]
    public class MatchmakingSO : ScriptableObject
    {
        public static Action onMatchmakingStart;
        public static Action<MultiplayAssignment> onMatchFound;
        public static Action onMatchmakingInProgress;
        public static Action<string> onMatchmakingFailed;
        public static Action<string> onMatchmakingTimeout;
        public static Action onMatchmakingCancelled;
        public static Action<int> onDeckSelectedChanged;
        private static string currentTicketId;
        private static CancellationTokenSource cancellationTokenSource;
        private static string queueName = "CardBattle";
        public static string deckNumberDictionaryName = "DeckNumber";
        private static int deckSelected;

        public static int DeckSelected
        {
            get => deckSelected;
            set
            {
                deckSelected = value;
                onDeckSelectedChanged?.Invoke(deckSelected);
            }
        }

        public static void StartMatchmaking()
        {
            StartMatchmaking(AuthenticationService.Instance.PlayerId, queueName, DeckSelected);
        }
        
        public static async void StartMatchmaking(string playerId, string queueName, int deckNumber)
        {
            try
            {
                onMatchmakingStart?.Invoke();
                
                var players = new List<Player>
                {
                    new (playerId,  new Dictionary<string, object>{{deckNumberDictionaryName,DeckSelected}})
                };

                Dictionary<string, object> attributes = new();
                
                var options = new CreateTicketOptions(queueName, attributes);
                
                var ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, options);
                
                currentTicketId = ticketResponse.Id;
                
                cancellationTokenSource = new CancellationTokenSource();
        
                PoolTicket(currentTicketId, cancellationTokenSource.Token).Forget();
            }
            catch (Exception e)
            {
                onMatchmakingFailed?.Invoke(e.Message);
            }
        }

        public static async void StopMatchMaking()
        {
            if(string.IsNullOrEmpty(currentTicketId)) return;
            
            cancellationTokenSource?.Cancel();
            
            cancellationTokenSource = null;

            try
            {
                await MatchmakerService.Instance.DeleteTicketAsync(currentTicketId);
                
                onMatchmakingCancelled?.Invoke();
            }
            catch (Exception e)
            {
                onMatchmakingFailed?.Invoke(e.Message);
            }
        }
        
        public static async UniTaskVoid PoolTicket(string ticketID, CancellationToken cancellationToken)
        {
            try
            {
                MultiplayAssignment assignment = null;
                bool gotAssignment = false;
                do
                {
                    //Rate limit delay
                    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cancellationToken);

                    // Poll ticket
                    var ticketStatus = await MatchmakerService.Instance.GetTicketAsync(ticketID);
                    if (ticketStatus == null)
                    {
                        continue;
                    }

                    //Convert to platform assignment data (IOneOf conversion)
                    if (ticketStatus.Type == typeof(MultiplayAssignment))
                    {
                        assignment = ticketStatus.Value as MultiplayAssignment;
                    }

                    switch (assignment?.Status)
                    {
                        case MultiplayAssignment.StatusOptions.Found:
                            gotAssignment = true;
                            onMatchFound?.Invoke(assignment);
                            break;
                        case MultiplayAssignment.StatusOptions.InProgress:
                            onMatchmakingInProgress?.Invoke();
                            break;
                        case MultiplayAssignment.StatusOptions.Failed:
                            gotAssignment = true;
                            onMatchmakingFailed?.Invoke("Failed to get ticket status. Error: " + assignment.Message);
                            break;
                        case MultiplayAssignment.StatusOptions.Timeout:
                            gotAssignment = true;
                            onMatchmakingTimeout?.Invoke("Ticket timed out, retry");
                            break;
                        default:
                            throw new InvalidOperationException();
                    }

                } while (!gotAssignment);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                onMatchmakingFailed?.Invoke(e.Message);
            }
        }
    }
}
