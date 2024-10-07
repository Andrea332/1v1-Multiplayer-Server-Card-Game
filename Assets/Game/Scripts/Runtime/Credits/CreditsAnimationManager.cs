using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class CreditsAnimationManager : MonoBehaviour
    {
        [SerializeField] private RectTransform creditsToMove;
        [SerializeField] private Vector2 startingPosition;
        [SerializeField] private float creditsSpeed;
        [SerializeField] private float animationLength;
        private float animationCurrentLifeTime;
        [SerializeField] private UnityEvent onCreditsEnd;

        private void OnEnable()
        {
            creditsToMove.anchoredPosition = startingPosition;
            animationCurrentLifeTime = 0;
        }

        private void Update()
        {
            creditsToMove.anchoredPosition = new Vector2(creditsToMove.anchoredPosition.x, creditsToMove.anchoredPosition.y + creditsSpeed * Time.deltaTime);

            animationCurrentLifeTime += Time.deltaTime;

            if (animationCurrentLifeTime >= animationLength)
            {
                StopCredits();
            }
        }

        public void StopCredits()
        {
            onCreditsEnd?.Invoke();
        }
    }
}
