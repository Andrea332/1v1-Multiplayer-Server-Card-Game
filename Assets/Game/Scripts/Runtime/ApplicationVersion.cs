using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class ApplicationVersion : MonoBehaviour
    {
        [SerializeField] private UnityEvent<string> onApplicationVersionExposed;

        private void Start()
        {
            ExposeApplicationVersion();
        }

        private void ExposeApplicationVersion()
        {
            onApplicationVersionExposed?.Invoke("v " + Application.version);
        }
    }
}
