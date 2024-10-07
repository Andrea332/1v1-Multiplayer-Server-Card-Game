using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game
{
    public class RuleNotRespectedNotificationManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private Animator animator;
        
        public void SendNewNotification(string notificationMessagge)
        {
            notificationText.SetText(notificationMessagge);
            animator.Play("Notification");
        }
    }
}
