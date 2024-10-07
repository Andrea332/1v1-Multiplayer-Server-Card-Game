using UnityEngine;

namespace Game
{
    public class ParallaxEffect : MonoBehaviour
    {
        private float _startingPos, _lengthOfSprite;
        public float AmountOfParallax;
        public Transform parallaxMover;
        public bool repeteAtEnd;

        private void Start()
        {
            _startingPos = transform.localPosition.x;
        
            if(!repeteAtEnd) return;
        
            _lengthOfSprite = GetComponent<SpriteRenderer>().bounds.size.x;
        }

        private void Update()
        {
            Vector3 parallaxMoverPosition = parallaxMover.position;
        
            float Temp = parallaxMoverPosition.x * (1 - AmountOfParallax);
            float Distance = parallaxMoverPosition.x * AmountOfParallax;
        
            transform.localPosition = new Vector3(_startingPos + Distance, 0, 0);
        
            if(!repeteAtEnd) return;
        
            if (Temp > _startingPos + (_lengthOfSprite / 2))
            {
                _startingPos += _lengthOfSprite;
            }
            else if (Temp < _startingPos - (_lengthOfSprite / 2))
            {
                _startingPos -= _lengthOfSprite;
            }
        }
    }
}
