using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;

public record class Account(
    [property: JsonPropertyName("puuid")] string Puuid,
    [property: JsonPropertyName("gameName")] string Name,
    [property: JsonPropertyName("tagLine")] string Tag,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("accountId")] string AccountId,
    [property: JsonPropertyName("profileIconId")] int Icon,
    [property: JsonPropertyName("summonerLevel")] int Level);

public record class Match(
    [property: JsonPropertyName("metadata")] MatchMetadata Metadata,
    [property: JsonPropertyName("info")] MatchInfo Info);

public record class MatchMetadata(
    [property: JsonPropertyName("matchId")] string MatchID);

public record class MatchInfo(
    [property: JsonPropertyName("gameDuration")] int GameDuration,
    [property: JsonPropertyName("gameMode")] string GameMode,
    [property: JsonPropertyName("participants")] List<Participant> Participants);

public record class Participant(
    [property: JsonPropertyName("assists")] int Assists,
    [property: JsonPropertyName("championName")] string Champion,
    [property: JsonPropertyName("deaths")] int Deaths,
    [property: JsonPropertyName("goldEarned")] int GoldEarned,
    [property: JsonPropertyName("kills")] int Kills,
    [property: JsonPropertyName("puuid")] string Puuid,
    [property: JsonPropertyName("spell1Casts")] int CastsQ,
    [property: JsonPropertyName("spell2Casts")] int CastsW,
    [property: JsonPropertyName("spell3Casts")] int CastsE,
    [property: JsonPropertyName("spell4Casts")] int CastsR,
    [property: JsonPropertyName("riotIdName")] string Name,
    [property: JsonPropertyName("riotIdTagline")] string Tag,
    [property: JsonPropertyName("teamPosition")] string Position,
    [property: JsonPropertyName("totalDamageDealt")] int DamageDealt,
    [property: JsonPropertyName("totalDamageTaken")] int DamageTaken,
    [property: JsonPropertyName("totalMinionsKilled")] int MinionsKilled,
    [property: JsonPropertyName("win")] bool Win);
