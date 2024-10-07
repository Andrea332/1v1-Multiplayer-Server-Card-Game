using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public interface IAuthenticationManager
    {
        public void Authenticate();

        public bool Enabled { get; }
        public UnityEvent OnSuccess { get; }
        public UnityEvent<string> OnError { get; }
    }
}
