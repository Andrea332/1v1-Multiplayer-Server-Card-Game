using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class MatchmakingEvents : MonoBehaviour
    {
        public UnityEvent onMatchmakingStart;
        public UnityEvent<MultiplayAssignment> onMatchFound;
        public UnityEvent onMatchmakingInProgress;
        public UnityEvent<string> onMatchmakingFailed;
        public UnityEvent<string> onMatchmakingTimeout;
        public UnityEvent onMatchmakingCancelled;
        public UnityEvent<int> onDeckSelectedChanged;

        private void Awake()
        {
            MatchmakingSO.onMatchmakingStart += OnMatchmakingStart;
            MatchmakingSO.onMatchFound += OnMatchFound;
            MatchmakingSO.onMatchmakingInProgress += OnMatchmakingInProgress;
            MatchmakingSO.onMatchmakingFailed += OnMatchmakingFailed;
            MatchmakingSO.onMatchmakingTimeout += OnMatchmakingTimeout;
            MatchmakingSO.onMatchmakingCancelled += OnMatchmakingCancelled;
            MatchmakingSO.onDeckSelectedChanged += OnDeckSelectedChanged;
        }

        

        private void OnDestroy()
        {
            MatchmakingSO.onMatchmakingStart -= OnMatchmakingStart;
            MatchmakingSO.onMatchFound -= OnMatchFound;
            MatchmakingSO.onMatchmakingInProgress -= OnMatchmakingInProgress;
            MatchmakingSO.onMatchmakingFailed -= OnMatchmakingFailed;
            MatchmakingSO.onMatchmakingTimeout -= OnMatchmakingTimeout;
            MatchmakingSO.onMatchmakingCancelled -= OnMatchmakingCancelled;
            MatchmakingSO.onDeckSelectedChanged += OnDeckSelectedChanged;
        }

        private void OnMatchmakingStart()
        {
            onMatchmakingStart?.Invoke();
        }
        private void OnMatchFound(MultiplayAssignment multiplayAssignment)
        {
            onMatchFound?.Invoke(multiplayAssignment);
        }
        private void OnMatchmakingInProgress()
        {
            onMatchmakingInProgress?.Invoke();
        }
        private void OnMatchmakingFailed(string obj)
        {
            onMatchmakingFailed?.Invoke(obj);
        }
        private void OnMatchmakingTimeout(string obj)
        {
            onMatchmakingTimeout?.Invoke(obj);
        }
        
        private void OnMatchmakingCancelled()
        {
            onMatchmakingCancelled?.Invoke();
        }
        
        private void OnDeckSelectedChanged(int deckSelected)
        {
            onDeckSelectedChanged?.Invoke(deckSelected);
        }
    }
}
