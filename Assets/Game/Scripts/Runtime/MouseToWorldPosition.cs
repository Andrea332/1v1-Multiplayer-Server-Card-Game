using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public class MouseToWorldPosition : MonoBehaviour
    {
        private Camera currentCamera;
        [SerializeField] private bool activeDebug;
        private void Start()
        {
            currentCamera = Camera.main;
        }
    
        void Update()
        {
            Vector3 currentPosition = currentCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            transform.position = currentPosition;
            if(!activeDebug) return;
            Debug.Log("MousePosition: " + currentPosition);
        }
    }
}
