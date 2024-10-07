using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities
{
    public class UrlOpener : MonoBehaviour
    {
        [SerializeField] private string url;

        public void OpenUrl()
        {
            Application.OpenURL(url);
        }
        
        public void OpenUrl(string urlToOpen)
        {
            Application.OpenURL(urlToOpen);
        }
    }
}
