namespace Game
{
    public static class ProjectKeys
    {
        //Cloud Save
        public const string DECKS_SAVE_CLOUDSAVE_KEY = "Decks";
        public const string MATCHESSTATS_CS_KEY = "MatchesStatistics";
        
        //Cloud Code
        public const string CC_MODULE = "GameModule";
        
        public const string CC_FUNCTION_SAVEDECKS = "SaveDecks";
        public const string CC_FUNCTION_SAVEDECKS_PARAMETER = "decksDatas";
        
        public const string CC_FUNCTION_GETBATTLEDATA = "GetBattleData";
        
        public const string CC_FUNCTION_SAVEMATCHSTATS = "SaveMatchStats";
        public const string CC_FUNCTION_SAVEMATCHSTATS_MATCHSTAT_PARAMETER = "matchesStatistics";
        public const string CC_FUNCTION_SAVEMATCHSTATS_PLAYERID_PARAMETER = "playerId";
        
        //Remote Config
        public const string REMOTE_CONFIG_MAX_DECKS = "DecksSettings";
        
        //Leaderboard
        public const string LEADERBOARD_BATTLEPOINTS = "BattlePoints";
    }
}
