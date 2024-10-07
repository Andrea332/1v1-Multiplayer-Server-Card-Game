using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

namespace Game
{
    public class LeaderboardManager : MonoBehaviour
    {
        [SerializeField] private LeaderboardPlayer leaderboardPersonPrefab;
        [SerializeField] private Transform leaderboardPersonSpawnParent;
        private List<LeaderboardPlayer> spawnedLeaderboardPersons = new();
        private LeaderboardPlayer spawnedCurrentPlayerPerson;
        private LeaderBoardOrder leaderBoardOrderBy;
        private LeaderboardPlayer lastData;
        
        private List<LeaderboardEntry>  battlePointsLeadeboard;
        public void ClearMenu()
        {
            for (int i = spawnedLeaderboardPersons.Count - 1; i >= 0; i--)
            {
                Destroy(spawnedLeaderboardPersons[i].gameObject);
            }
            spawnedLeaderboardPersons.Clear();
            
            if (spawnedCurrentPlayerPerson == null) return;
            
            Destroy(spawnedCurrentPlayerPerson.gameObject);
            spawnedCurrentPlayerPerson = null;
        }

        public async void FillMenu()
        {
            ClearMenu();
            
            var leaderBoardEntry = await GetUgsLeaderboardBattlePoints();
            
            double lastValue = -1;
            int actualPosition = 0;
            for (var index = 0; index < leaderBoardEntry.Count; index++)
            {
                var person = leaderBoardEntry[index];
                double currentValue;

                switch (leaderBoardOrderBy)
                {
                    case LeaderBoardOrder.Points:
                        currentValue = person.Score;
                        break;
                    case LeaderBoardOrder.WinRatio:
                        currentValue = person.Score;
                        break;
                    case LeaderBoardOrder.MatchesCompleted:
                        currentValue = person.Score;
                        break;
                    default:
                        currentValue = -1;
                        break;
                }

                if (currentValue != lastValue)
                {
                    actualPosition++;
                    lastValue = currentValue;
                }
                
                int leaderBoardPosition = actualPosition;
                bool isCurrentPlayer = person.PlayerName == AuthenticationService.Instance.PlayerName;
                var leaderboardPerson = InstantiateLeaderboardPerson(person, leaderBoardPosition, leaderboardPersonSpawnParent, isCurrentPlayer);
                /*if (isCurrentPlayer)
                {
                    spawnedCurrentPlayerPerson = InstantiateLeaderboardPerson(person, leaderBoardPosition, currentPersonSpawnParent, isCurrentPlayer,  true);
                }*/

                spawnedLeaderboardPersons.Add(leaderboardPerson);
            }
        }

        private LeaderboardPlayer InstantiateLeaderboardPerson(LeaderboardEntry entry, int leaderboardPosition, Transform prefabParent, bool isCurrentPlayer, bool alternativeUI = false)
        {
            var leaderboardPerson = Instantiate(leaderboardPersonPrefab, prefabParent);
            leaderboardPerson.SetLeaderboardPosition(leaderboardPosition);
            leaderboardPerson.SetPoints(entry.Score.ToString());
            LeaderboardMetaData leaderboardMetaData = JsonUtility.FromJson<LeaderboardMetaData>(entry.Metadata);
            var totalMatches = leaderboardMetaData.matchesStatistics.matchesDraw + leaderboardMetaData.matchesStatistics.matchesLost + leaderboardMetaData.matchesStatistics.matchesWon;
            leaderboardPerson.SetMatchesCompleted(totalMatches.ToString());
            leaderboardPerson.SetName(entry.PlayerName);
            leaderboardPerson.SetWinRatio((leaderboardMetaData.matchesStatistics.matchesWon/totalMatches).ToString());
            leaderboardPerson.SetBars(isCurrentPlayer);
            if (alternativeUI)
            {
                leaderboardPerson.SetTextAlternativeColor();
                leaderboardPerson.SetBgState(false);
                leaderboardPerson.SetMedalLogoVisibility(false);
                leaderboardPerson.SetBars(false);
            }
            return leaderboardPerson;
        }

        public void OrderBy(int enumNumber)
        {
            leaderBoardOrderBy = (LeaderBoardOrder)enumNumber;
            ClearMenu();
            FillMenu();
        }  
        
        
        public async Task<List<LeaderboardEntry>> GetUgsLeaderboardBattlePoints()
        {
            GetScoresOptions getScoresOptions = new GetScoresOptions()
            {
                Limit = 100,
                IncludeMetadata = true
            };
            var leaderboardScoresPage = await LeaderboardsService.Instance.GetScoresAsync(ProjectKeys.LEADERBOARD_BATTLEPOINTS, getScoresOptions);

            return leaderboardScoresPage.Results;
        }
      
    }
    
 

    public enum LeaderBoardOrder
    {
        Points,
        WinRatio,
        MatchesCompleted
    }
}
