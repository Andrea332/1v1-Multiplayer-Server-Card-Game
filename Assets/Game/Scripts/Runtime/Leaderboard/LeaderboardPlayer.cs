using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Game
{
    public class LeaderboardPlayer : MonoBehaviour
    {
        [SerializeField] private UnityEvent<string>  onLeaderboardPositionSetted, onPlayerSetted, onMatchesCompletedSetted, onWinRatioSetted, onPointsSetted;
        [SerializeField] private Color playerColor;
        [SerializeField] private List<TextMeshProUGUI> texts;
        [SerializeField] private Image bg;
        [SerializeField] private Image medalImage;
        [SerializeField] private GameObject bars;
        [SerializeField] private List<Sprite> positionsMedalsLogo;
        [SerializeField] private List<Color> bgPositionsColors;

        public void SetLeaderboardPosition(int leaderboardPosition)
        {
            if (leaderboardPosition < 4)
            {
                medalImage.sprite = positionsMedalsLogo[leaderboardPosition - 1];
                bg.color = bgPositionsColors[leaderboardPosition - 1];
            }
            else
            {
                medalImage.enabled = false;
            }
            
            onLeaderboardPositionSetted?.Invoke(leaderboardPosition.ToString());
        }

        public void SetName(string playerName)
        {
            onPlayerSetted?.Invoke(playerName);
        }

        public void SetPoints(string producedResources)
        {
            onPointsSetted?.Invoke(producedResources);
        }
        
        public void SetMatchesCompleted(string destroyedTargets)
        {
            onMatchesCompletedSetted?.Invoke(destroyedTargets);
        }
        
        public void SetWinRatio(string destroyedShips)
        {
            onWinRatioSetted?.Invoke(destroyedShips);
        }
        
        public void SetBgState(bool active)
        {
            bg.gameObject.SetActive(active);
        }

        public void SetMedalLogoVisibility(bool visible)
        {
            medalImage.gameObject.SetActive(visible);
        }

        public void SetBars(bool active)
        {
            bars.SetActive(active);
        }

        public void SetTextAlternativeColor()
        {
            foreach (var textMeshProUGUI in texts)
            {
                textMeshProUGUI.fontStyle = FontStyles.Bold;
            }
            
            foreach (var textMeshProUGUI in texts)
            {
                textMeshProUGUI.color = playerColor;
            }
        }
    }
}
