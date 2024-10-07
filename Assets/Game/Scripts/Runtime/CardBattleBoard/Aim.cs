using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Game
{
    public class Aim : MonoBehaviour
    {
        private RectTransform canvasTransform;
        [SerializeField] private RectTransform mainAimImage;
        [SerializeField] private RectTransform resizeAbleBar;
        [SerializeField] private RectTransform linePivot;
        private Camera mainCamera;
        public void SetAim()
        {
            mainCamera = Camera.main;
            canvasTransform = transform.root as RectTransform;
        }

        public void CalculateAim(Vector3 startPosition, Vector3 endPosition)
        {
            mainAimImage.position = endPosition;
            
            Vector2 aimScreenPointPosition = RectTransformUtility.WorldToScreenPoint(mainCamera, mainAimImage.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasTransform, aimScreenPointPosition, mainCamera, out Vector2 aimLocalPoint);
            
            Vector2 startScreenPointPosition = RectTransformUtility.WorldToScreenPoint(mainCamera, startPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasTransform, startScreenPointPosition, mainCamera, out Vector2 startLocalPoint);
            
            Vector3 direction = startLocalPoint - aimLocalPoint;
            var imageWidth = direction.magnitude - resizeAbleBar.localPosition.z;
            resizeAbleBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, imageWidth);
            var lookRotation = Quaternion.LookRotation(direction.normalized, Vector3.forward);
            linePivot.rotation = lookRotation;
        }
    }
}
