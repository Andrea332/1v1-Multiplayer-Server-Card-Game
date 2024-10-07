using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Utilities
{
    public class TimerUI : MonoBehaviour
    {
        public TextMeshProUGUI timerText;
        [SerializeField] private string timerEndText;
        [SerializeField] private bool showDateTimeUnits = true;
        public UnityEvent onBeforeTimerStart;
        public UnityEvent onTimerStarted;
        public UnityEvent onTimerEnded;

        private DateTime cooldownEnd;
        private CancellationTokenSource cancellationTokenSource;
        
        public UnityEvent onMinuteChange;
        public UnityEvent onHourChange;
        public UnityEvent onDayChange;

        private int previousSecond = -1;
        private int previousMinute = -1;
        private int previousHour = -1;
        private int previousDay = -1;

        public void StartTimerBySeconds(string seconds)
        {
            cancellationTokenSource = new CancellationTokenSource();
            StartCooldownBySeconds(long.Parse(seconds), cancellationTokenSource.Token).Forget();
        }
        public void StartTimerByEndDate(string unixTimestampEndDate)
        {
            cancellationTokenSource = new CancellationTokenSource();
            StartCooldownByEndDate(long.Parse(unixTimestampEndDate), cancellationTokenSource.Token).Forget();
        }
        
        public void StartTimerBySeconds(int seconds)
        {
            cancellationTokenSource = new CancellationTokenSource();
            StartCooldownBySeconds(seconds, cancellationTokenSource.Token).Forget();
        }
        public void StartTimerByEndDate(int unixTimestampEndDate)
        {
            cancellationTokenSource = new CancellationTokenSource();
            StartCooldownByEndDate(unixTimestampEndDate, cancellationTokenSource.Token).Forget();
        }

        public void StartTimerBySeconds(long seconds)
        {
            cancellationTokenSource = new CancellationTokenSource();
            StartCooldownBySeconds(seconds, cancellationTokenSource.Token).Forget();
        }
        public void StartTimerByEndDate(long unixTimestampEndDate)
        {
            cancellationTokenSource = new CancellationTokenSource();
            StartCooldownByEndDate(unixTimestampEndDate, cancellationTokenSource.Token).Forget();
        }
        
        public void StartTimerByEndDate(DateTime endDate)
        {
            cancellationTokenSource = new CancellationTokenSource();
            StartCooldownByEndDate(endDate, cancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid StartCooldownBySeconds(long seconds, CancellationToken cancellationToken)
        {
            cooldownEnd = DateTime.UtcNow.AddSeconds(seconds);
            await TimerTask(cancellationToken);
        }
        private async UniTaskVoid StartCooldownByEndDate(long unixTimestampEndDate, CancellationToken cancellationToken)
        {
            cooldownEnd = UnixTimeStampToDateTime(unixTimestampEndDate);
            await TimerTask(cancellationToken);
        }
        private async UniTaskVoid StartCooldownByEndDate(DateTime endDate, CancellationToken cancellationToken)
        {
            cooldownEnd = endDate;
            await TimerTask(cancellationToken);
        }

        private async Task TimerTask(CancellationToken cancellationToken)
        {
            onBeforeTimerStart?.Invoke();
            
            SetTimerText(string.Empty);
            
            if (DateTime.UtcNow >= cooldownEnd)
            {
                return;
            }

            int millisecondsUntilNextSecond = 1000 - DateTime.UtcNow.Millisecond;
            
            await UniTask.Delay(millisecondsUntilNextSecond, cancellationToken: cancellationToken);

            onTimerStarted?.Invoke();

            while (DateTime.UtcNow < cooldownEnd)
            {
                TimeSpan remainingTime = cooldownEnd - DateTime.UtcNow;
                UpdateTimerText(remainingTime, timerText, showDateTimeUnits);
                CheckTimeChanges(remainingTime);
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cancellationToken);
            }

            SetTimerText(timerEndText);
            onTimerEnded?.Invoke();
        }

        public static void UpdateTimerText(TimeSpan remainingTime, TextMeshProUGUI timerText, bool showDateTimeUnits)
        {
            int actualSeconds = remainingTime.Seconds;
            int actualMinutes = remainingTime.Minutes;

            if (actualSeconds == 60)
            {
                actualMinutes += 1;
                actualSeconds = 0;
            }

            if (remainingTime.TotalDays >= 1)
            {
                timerText.text = string.Format(showDateTimeUnits ? "{0}d {1}h {2}m {3}s" : "{0}:{1}:{2}:{3}", remainingTime.Days, remainingTime.Hours, actualMinutes, actualSeconds);
            }
            else if (remainingTime.TotalHours >= 1)
            {
                timerText.text = string.Format(showDateTimeUnits ? "{0}h {1}m {2}s" : "{0}:{1}:{2}", remainingTime.Hours, actualMinutes, actualSeconds);
            }
            else if (remainingTime.TotalMinutes >= 1)
            {
                timerText.text = string.Format(showDateTimeUnits ? "{0}m {1}s" : "{0}:{1}", actualMinutes, actualSeconds);
            }
            else
            {
                timerText.text = string.Format(showDateTimeUnits ? "{0}s" : "{0}", actualSeconds);
            }
        }
        public static string UpdateTimerText(TimeSpan remainingTime,  bool showDateTimeUnits)
        {
            int actualSeconds = remainingTime.Seconds;
            int actualMinutes = remainingTime.Minutes;

            if (actualSeconds == 60)
            {
                actualMinutes += 1;
                actualSeconds = 0;
            }

            if (remainingTime.TotalDays >= 1)
            {
                return string.Format(showDateTimeUnits ? "{0}d {1}h {2}m {3}s" : "{0}:{1}:{2}:{3}", remainingTime.Days, remainingTime.Hours, actualMinutes, actualSeconds);
            }
            if (remainingTime.TotalHours >= 1)
            {
                return string.Format(showDateTimeUnits ? "{0}h {1}m {2}s" : "{0}:{1}:{2}", remainingTime.Hours, actualMinutes, actualSeconds);
            }
            if (remainingTime.TotalMinutes >= 1)
            {
                return string.Format(showDateTimeUnits ? "{0}m {1}s" : "{0}:{1}", actualMinutes, actualSeconds);
            }

            return string.Format(showDateTimeUnits ? "{0}s" : "{0}", actualSeconds);
        }
        private void CheckTimeChanges(TimeSpan remainingTime)
        {
            if (previousSecond != remainingTime.Seconds)
            {
                previousSecond = remainingTime.Seconds;

                if (previousMinute != remainingTime.Minutes)
                {
                    previousMinute = remainingTime.Minutes;
                    onMinuteChange?.Invoke();

                    if (previousHour != remainingTime.Hours)
                    {
                        previousHour = remainingTime.Hours;
                        onHourChange?.Invoke();

                        if (previousDay != remainingTime.Days)
                        {
                            previousDay = remainingTime.Days;
                            onDayChange?.Invoke();
                        }
                    }
                }
            }
        }

        public void SetTimerText(string textToShow)
        {
            timerText.SetText(textToShow);
        }

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp).UtcDateTime;
            return dateTime;
        }

        private void OnDisable()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
            }
        }
    }
}
