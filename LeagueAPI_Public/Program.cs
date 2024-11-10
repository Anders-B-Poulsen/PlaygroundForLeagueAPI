using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using HttpClient client = new();

await Main();

/*
*
*   Main function
*
*/
async Task Main()
{
    // Gather basic account info
    Account mainAccount = await Setup(client);

    // Gather recent matches from the account
    if(mainAccount.AccountId != "")
    {
        int matchCount = 20;
        Console.WriteLine($"Gathering data from recent {matchCount} matches");
        List<String> recentMatchIDs = await GatherRecentMatchIDs(client, mainAccount, matchCount);
        
        // Gater data from all the recent matches
        List<Match> recentMatches = [];
        foreach (String matchID in recentMatchIDs)
        {
            recentMatches.Add(await GatherSimpleMatchData(client, matchID));
        }

        // Calculate average KDA and Winrate of the user from their recent matches
        GatherAndOutputWinrateAndKDA(client, mainAccount, recentMatches);    
    }
}


/*
*
*   Smaller utility methods.
*
*/

// Create request header
void CreateRequestHeader()
{
    Console.WriteLine("Creating request header");
    client.DefaultRequestHeaders.Clear();
    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36 Edg/127.0.0.0");
    client.DefaultRequestHeaders.Add("Accept-Language", "da");
    client.DefaultRequestHeaders.Add("X-Riot-Token", GetAPIKey());
}

// Call the API and print out the raw JSON in console
static async Task PrintAPIResponseAsJSON(HttpClient client, String endpoint)
{
    // Get key
    string apiKey = GetAPIKey();

    // Request and print JSON
    var json = await client.GetStringAsync(endpoint);
    
    Console.Write(json);
}

// Get API key from file (instead of storing directly in code)
static string GetAPIKey()
{
    StreamReader sr = new StreamReader(@"D:\Bruger\Dokumenter\Projects\APIKeys\Personal Statistics Playground (LeagueAPI)/key.txt");
    return sr.ReadToEnd();
}


/*
*
*   Setup requests and stores basic nessecary account data from the API such as puuid and summoner id (both encrypted).
*   Will require the user to provide name, tag and server of the account they desire to gather data for.
*
*/

async Task<Account> Setup (HttpClient client)
{
    // Get user input for acc name, tag and server
    Console.Write("Provide account name: ");
    var accName = Convert.ToString(Console.ReadLine());
    Console.Write("Provide tag: ");
    var accTag = Convert.ToString(Console.ReadLine());
    Console.Write("Provide server: ");
    var server = Convert.ToString(Console.ReadLine());

    // Terminate if any input is invalid
    try
    {
        if (string.IsNullOrEmpty(accName) || string.IsNullOrEmpty(accTag))
        {
            throw new ArgumentException("Name or tag cannot be empty");
        }
    
        switch (server)
        {
            // I only banter in my own code, don't worry.
            case "NA": case "na": case "Na": case "Mickey Mouse server":
                server = "na1";
                break;

            case "EUNE": case "eune": case "Eune": case "Polish mafia residence": case "Holsterbro":
                server = "eun1";
                break;

            case "EUW": case "euw": case "Euw": case "Cringe L9 breeding ground":
                server = "euw1";
                break;
            
            default:
                throw new ArgumentException("Invalid server provided");
        }

        // Create header for http requests
        CreateRequestHeader();

        // Gather account information
        Account tempAccount1;
        
        try
        {
            Console.WriteLine("Searching for account with provided details.");

            // Request and store a record of the account information (with the limited available data)
            await using Stream stream1 = await client.GetStreamAsync(EndpointList.PuuidByName(accName, accTag));
            var reply = await JsonSerializer.DeserializeAsync<Account>(stream1);
            if(reply is null)
            {
                throw new ArgumentNullException("Reply recieved from API is empty","Reply");
            }
            tempAccount1 = reply;

            // Request and update (create a new record) the account information (now with further data)
            await using Stream stream2 = await client.GetStreamAsync(EndpointList.SummonerInfo(server, reply.Puuid));
            reply = await JsonSerializer.DeserializeAsync<Account>(stream2);
            if(reply is null)
            {
                throw new ArgumentNullException("Reply recieved from API is empty","Reply");
            }
            
            return reply with {Puuid = tempAccount1.Puuid, Name = tempAccount1.Name, Tag = tempAccount1.Tag};
        }
        catch (Exception e)
        {   
            switch (e)
            {
                case HttpRequestException: case ArgumentNullException:
                    Console.Write(e.Message);
                    break;

                default:
                    throw;
            }
            return new Account("","","","","",0,0);
        }   
    }
    catch (ArgumentException e)
    {
        Console.WriteLine(e.Message);
        return new Account("","","","","",0,0);
    }
}


/*
*
*   Gathers data from a match ID. 
*   Returns data as "match" record, keeping the most valuable and overall useful data while discarding the obscure stuff.
*
*/

async Task<Match> GatherSimpleMatchData(HttpClient client, String matchid)
{
    // Request match data for given match ID
    await using Stream stream = await client.GetStreamAsync(EndpointList.MatchData(matchid));
    var match = await JsonSerializer.DeserializeAsync<Match>(stream);
    if(match is null)
    {
        throw new ArgumentNullException("Reply recieved from API is empty","Reply");
    }
    
    return match;
}


/*
*
*   Calculates and outputs general data about winrate and KDA to console.
*   IDEA FOR FUTURE: Consider storing data locally on machine to enable future statistical comparising over longer periods of time.
*
*/

void GatherAndOutputWinrateAndKDA(HttpClient client, Account acc, List<Match> matches)
{
    // Local variables for calculations
    int KA = 0;
    int D = 0;
    int wins = 0;

    Console.WriteLine("Calculating KDA and Winrate.");
    
    // Gather
    foreach (Match m in matches)
    {
        foreach (Participant p in m.Info.Participants)
        {
            if(p.Puuid.Equals(acc.Puuid))
            {
                KA += p.Kills + p.Assists;
                D += p.Deaths;
                if (p.Win) wins++; 
            }
        }    
    }

    string KDA;

    if(D == 0) {KDA = "perfect";} else { KDA = ((float) KA/D).ToString(); }

    Console.WriteLine($"{acc.Name} has had a winrate of {(float) (wins*100/matches.Count)} in their recent games, with a {KDA} KDA.");
}



/*
*
*   Collects match ID from recent matches.
*   Match count is clamped between 0 - 100 to obide by the APIs restrictions.
*
*/

async Task<List<String>> GatherRecentMatchIDs(HttpClient client, Account acc, int count)
{
    // Clamp count within allowed range
    if(count > 100) { count = 100;} else if (count < 0) { count = 0;}

    // Get match IDs from puuid
    try
    {
        // Request and store a record of the account information (with the limited available data)
        await using Stream stream1 = await client.GetStreamAsync(EndpointList.MatchesByPuuid(acc.Puuid, count));
        var reply = await JsonSerializer.DeserializeAsync<List<String>>(stream1);
        if(reply is null)
        {
            throw new ArgumentNullException("Reply recieved from API is empty","Reply");
        }
        return reply;
    }
    catch (Exception e)
    {   
        switch (e)
        {
            case HttpRequestException: case ArgumentNullException:
                Console.Write(e.Message);
                break;

            default:
                throw;
        }
    }
    return [];
}




