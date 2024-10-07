using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class CounterPresenter : MonoBehaviour
    {
        private int counterNumber;

        [SerializeField] private bool maxLimitOn;
        [SerializeField] private bool minLimitOn;
        [SerializeField] private int maxCounter;
        [SerializeField] private int minCounter;
        [SerializeField] private int defaultAmountToAdd = 1;
        [SerializeField] private int defaultAmountToRemove = 1;
        [SerializeField] private UnityEvent<string> onCounterChanged;
        public void AddAmount(int amount)
        {
            counterNumber += amount;

            CheckMaxLimit();

            CheckMinLimit();

            onCounterChanged?.Invoke(counterNumber.ToString());
        }
        
        public void RemoveAmount(int amount)
        {
            counterNumber -= amount;

            CheckMinLimit();

            CheckMaxLimit();

            onCounterChanged?.Invoke(counterNumber.ToString());
        }

        
        private void CheckMaxLimit()
        {
            if (!maxLimitOn) return;
            
            if (counterNumber > maxCounter)
            {
                counterNumber = maxCounter;
            }
        }

        private void CheckMinLimit()
        {
            if (!minLimitOn) return;
            
            if (counterNumber < minCounter)
            {
                counterNumber = minCounter;
            }
        }

        public void AddDefaultAmount()
        {
            AddAmount(defaultAmountToAdd);
        }
        
        public void RemoveDefaultAmount()
        {
            RemoveAmount(defaultAmountToRemove);
        }

        public void SetAmount(int totalAmount)
        {
            counterNumber = totalAmount;
            
            CheckMinLimit();

            CheckMaxLimit();
            
            onCounterChanged?.Invoke(counterNumber.ToString());
        }
    }
}
