
// Utility class with static strings for storing URLs
class EndpointList
{
    public static String PuuidByName(String name, String tag)
    {
        return $"https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id/{name}/{tag}";
    }

    public static String SummonerInfo(String server, String puuid)
    {
        return $"https://{server}.api.riotgames.com/lol/summoner/v4/summoners/by-puuid/{puuid}";
    }

    // TODO: Add queue type to arguments. This requires you to find out what their internal queue type representation is (which integer responds to which queue type)
    public static String MatchesByPuuid(String puuid, int count)
    {
        return $"https://europe.api.riotgames.com/lol/match/v5/matches/by-puuid/{puuid}/ids?start=0&count={count}";
    }

    public static String MatchData(String matchid)
    {
        return $"https://europe.api.riotgames.com/lol/match/v5/matches/{matchid}";
    }           
}