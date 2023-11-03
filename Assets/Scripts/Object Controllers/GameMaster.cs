using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour
{
    public float timeBetweenTargetChecks = 1f;

    public List<Team> allTeams;
    //midmatch important stats
    public List<Color> teamColors;
    public List<Unit> allUnits;
    public List<GameObject> everyPossibleUnitInGame;
    public List<Base> allBases;
    public List<Team> matchTeams;
    public List<int> handledTeams;
    public List<Object> ignoreThese;

    public GameObject coin;
    public GameObject dump;
    public Vector3 centerOfField;
    public GameObject defaultBaseObject;
    private Base defb;
    public Base DefaultBase
    {
        get
        {
            if(defb == null)
                defb = defaultBaseObject.GetComponent<Base>();

            return defb;
        }
    }
    public GameObject defaultTeamObject;
    private Team deft;
    public Team DefaultTeam
    {
        get
        {
            if (deft == null)
                deft = defaultTeamObject.GetComponent<Team>();

            return deft;
        }
    }
    public static GameMaster def = null;    //This is the one GameMaster in the scene. Set in Qk
    public enum MatchState { not, midMatch, afterMatch, preMatch }
    public static MatchState Match_State = MatchState.not;
    public MatchState matchState
    {
        get
        {
            return Match_State;
        }
        set
        {
            Match_State = value;
            //print("Moving to match state: " + value.ToString());
        }
    }

    public List<Team> nonMatchTeams
    {
        get
        {
            List<Team> list = new List<Team>();
            list.AddRange(allTeams);
            list.RemoveAll(w => w.inThisMatch);
            return list;
        }
    }   //All teams not participating in this match

    public List<Map> possibleMaps = new List<Map>();
    /// <summary>
    /// Lit of indexes of maps in the possibleMaps list that we're not going to load again. ex) if 1 is in there, possibleMaps[1] is the map that won't load.
    /// </summary>
    public List<int> alreadyDoneMaps = new List<int>();
    public List<int> numberOfMapsOfSize = new List<int>();
    public Map currentMap;
    /// <summary>
    /// A random map that we haven't played before.
    /// </summary>
    /*public Map RandomMap
    {
        get
        {

            //reset our memory of done maps if we've done them all
            if (alreadyDoneMaps.Count == possibleMaps.Count)
            {
                alreadyDoneMaps.Clear();
                alreadyDoneMaps.Add(possibleMaps.IndexOf(currentMap));
                Debug.Log("Reset the alreadyDoneMaps list.");
            }

            List<int> potential = new List<int>();

            for(int i = 0; i < possibleMaps.Count; i++)
            {
                //Only use maps we haven't done
                if (!alreadyDoneMaps.Contains(i))
                {
                    potential.Add(i);
                }
            }
            //Randomize
            int chosenMap = potential[NIU.RandomR(0, potential.Count - 1)];

            return possibleMaps[chosenMap];

        }
    }*/
    public Map nextMap;
    public Match nextMatch;
    public int highestTeamCount = 0;
    public float backgroundMatchTimeAfterWin = 5f;
    private float bgMatchTimer = 10000f;

    private MainMenus mm;
    private Campaign camp;

    public static bool BigDebugLog = false;


    // Awake is called before EVERYTHING ELSE
    void Awake()
    {
        OnValidate();

        mm = Qk.Menus;
        camp = Qk.Campaign;
        allTeams = new List<Team>();
        allUnits = new List<Unit>();
        allBases = new List<Base>();
        matchTeams = new List<Team>();
        handledTeams = new List<int>();
        centerOfField = Vector3.zero;
        ignoreThese = new List<Object>();
        nextMap = NIU.RandomMember(possibleMaps);

        //Initialize teams
        int i = 0;
        foreach (Team team in FindObjectsOfType<Team>())
        {
            //HandleTeam(i);
            allTeams.Add(team);
            i++;
        }
        allTeams.Sort(new TeamSort());

        //load map
        //ChangeMap(currentMap, false);

        //add bases and find center of field
        foreach(Base b in FindObjectsOfType<Base>())
        {
            //allBases.Add(b);
        }
        UpdateCenterOfField();


        //Figure out how many maps there are of each number of players
        highestTeamCount = 0;
        foreach (Map m in possibleMaps)
            if (Mathf.Max(m.validTeamCounts.ToArray()) > highestTeamCount)
                highestTeamCount = Mathf.Max(m.validTeamCounts.ToArray());
        for(int j = 1; j <= highestTeamCount; j++)
        {
            int amountAtThisSize = 0;
            foreach (Map m in possibleMaps)
                if (m.validTeamCounts.Contains(j))
                    amountAtThisSize++;

            numberOfMapsOfSize.Add(amountAtThisSize);
            //Debug.Log("There is/are " + amountAtThisSize + " map" + NIU.PlurS(amountAtThisSize) + " at size " + j);
        }


        //Not initialization
        //Handling the map that's already loaded in the scene when we hit play
        currentMap = FindObjectOfType<Map>();
        if(currentMap != null)
        {
            Debug.LogWarning("We're starting the match with a pre-loaded map: \"" + currentMap.realName + "\"");
            StartNewMatchFromPreMatch(matchTeams, false);
        }




        //If we're starting at preMatch, then randomly find teams to add
        if(matchState == MatchState.preMatch)
            if (matchTeams.Count < 2)
            {
                Debug.LogWarning("There are less than 2 teams in the upcoming match. Fixing. (Awake())");
                matchTeams.AddRange(NIU.Shuffle(allTeams).GetRange(0, 2));
            }





        //Start a background match in the background
        StartBackgroundMatch(teams: NIU.RandomR(2, highestTeamCount));




        //DEBUG!!!!!!!!!!!!!!! draws a graph that shows teams's exps
        {
            Debug.DrawLine(new Vector3(0f, 48f, 100f), new Vector3(30f, 48f, 100f), Color.black, 5000f);
            Debug.DrawLine(new Vector3(0f, 48f, 100f + (10 * DefaultTeam.eraExps[0] / DefaultTeam.eraExps[3])),
                new Vector3(30f, 48f, 100f + (10 * DefaultTeam.eraExps[0] / DefaultTeam.eraExps[3])), Color.gray, 5000f);
            Debug.DrawLine(new Vector3(0f, 48f, 100f + (10 * DefaultTeam.eraExps[1] / DefaultTeam.eraExps[3])),
                new Vector3(30f, 48f, 100f + (10 * DefaultTeam.eraExps[1] / DefaultTeam.eraExps[3])), Color.gray, 5000f);
            Debug.DrawLine(new Vector3(0f, 48f, 100f + (10 * DefaultTeam.eraExps[2] / DefaultTeam.eraExps[3])),
                new Vector3(30f, 48f, 100f + (10 * DefaultTeam.eraExps[2] / DefaultTeam.eraExps[3])), Color.gray, 5000f);
            Debug.DrawLine(new Vector3(0f, 48f, 110f), new Vector3(30f, 48f, 110f), Color.gray, 5000f);
        }
    }



    private void OnValidate()
    {
        def = this;

        UpdateUnitIDs();

        
    }


    public void UpdateUnitIDs()
    {
        everyPossibleUnitInGame.Sort(new Team.EraSort());

        int i = 0;
        foreach (GameObject go in everyPossibleUnitInGame)
        {
            Unit unit = go.GetComponent<Unit>();
            unit.ID = i;
            i++;
        }
    }

    

    // Update is called once per frame
    void Update()
    {
        //clean up the ignoreThese list
        int i = 0;
        while(i < ignoreThese.Count)
        {
            if (ignoreThese[i] == null)
                ignoreThese.RemoveAt(i);
            else
                i++;
        }

        //PRE MATCH
        if(matchState == MatchState.preMatch)
        {
            if(matchTeams.Count < 2)
            {
                Debug.LogWarning("There are less than 2 teams in the upcoming match. Fixing. (Update())");
                matchTeams.AddRange(NIU.Shuffle(allTeams).GetRange(0, 2));
            }



            //Un-lose teams
            foreach (Team t in matchTeams)
                t.lostCurrentMatch = false;


            if (Time.frameCount % 300 == 299 && currentMap != null)
                matchState = MatchState.midMatch;
        }


        //MID MATCH
        if(matchState == MatchState.midMatch)
        {


            int currentTeamsInGame = 0;
            foreach(Team t in matchTeams)
            {
                if (!t.lostCurrentMatch)
                    currentTeamsInGame++;
            }


            //Load up next match if we're in a campaign and the player lost
            if ((Qk.Player.team?.lostCurrentMatch).GetValueOrDefault(false) && !mm.skirmish && !MatchMaker.current.background)
            {
                MatchUpTeamsAndMap(Qk.Campaign.FindTeamCountFromDifficulty(), true, true);
            }


            //~~~~~~~~~~~~~END OF MATCH~~~~~~~~~~~~~~~~
            if (currentTeamsInGame == 1)
            {
                matchState = MatchState.afterMatch;
                matchTeams.Find(t => !t.lostCurrentMatch).Win();
                MatchUpTeamsAndMap(Qk.Campaign.FindTeamCountFromDifficulty(), !mm.skirmish, !mm.skirmish);
            }
            else if(currentTeamsInGame < 1)
            {
                Debug.LogError("Error: we somehow have 0 teams currently in the game and the match is still going?");
            }


            //moving on from this match after we lost midway through
            if (!MatchMaker.current.background && Input.GetKeyDown(KeyCode.Space) && Qk.Player.team.lostCurrentMatch)
            {
                if (mm.skirmish)
                    BackToMain();
                else
                    camp.LoadNextMatch();
            }

        }

        //AFTER MATCH
        if (matchState == MatchState.afterMatch)
        {

            //If a background match ended, wait a bit then start a new one
            if(MatchMaker.current.background)
            {
                if (bgMatchTimer > backgroundMatchTimeAfterWin)
                    bgMatchTimer = backgroundMatchTimeAfterWin;

                else if (bgMatchTimer <= 0f)
                {
                    Debug.Log("Starting new bgmatch!");
                    StartBackgroundMatch();
                    bgMatchTimer = backgroundMatchTimeAfterWin;
                }

                bgMatchTimer -= Time.deltaTime;


            }

            //next match in campaign
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                if (mm.skirmish)
                    BackToMain();
                else
                    camp.LoadNextMatch();
            }
        }



        /*
        //debug lol
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //StartNewMatchFromPreMatch(matchTeams);

            //Create a match object, then start it          NIU.RandomR(0, allTeams.Count - 2) + 2
            Match nextMatch = MatchMaker.CreateNewMatch(NIU.RandomR(0, allTeams.Count - 2) + 2, true, includePlayer: true);
            nextMatch.StartMatch();
        }
        */

        //debug lol
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            camp.LoseCampaign();
        }
    }


    public Vector3 UpdateCenterOfField()
    {
        //If there's no center of field, just default to origin
        if (allBases.Count == 0)
        {
            centerOfField = Vector3.zero;
            return centerOfField;
        }

        float x = 0f;
        float y = 0f;
        float z = 0f;

        int i = 0;
        foreach(Base b in allBases)
        {
            i++;
            x += b.transform.position.x;
            y += b.transform.position.y;
            z += b.transform.position.z;
        }

        centerOfField = new Vector3(x / i, y / i, z / i);
        Debug.DrawRay(centerOfField, Vector3.up * i, Color.white, 10f);
        return centerOfField;
    }
    
    
    public Map ChangeMap(Map map, int teams, bool dontLoadThisMapAgain = true)
    {
        Map m = FindObjectOfType<Map>();
        if(m != null)
            Destroy(m.gameObject);
        CleanDump();
        IncinerateTheDead();

        currentMap = map;
        //print("we have teams = " + teams);
        currentMap.Load(teams);

        //Add this map to the list of repeats
        if (dontLoadThisMapAgain && possibleMaps.Contains(map))
        {
            alreadyDoneMaps.Add(possibleMaps.FindIndex(w => w.realName == map.realName));
        }


        //Find a next map
        //nextMap = NIU.RandomMember(possibleMaps);

        return nextMap;
    }
    

    public void StartNewMatchFromPreMatch(List<Team> playingTeams = null, bool changeMap = true, Map map = null)
    {
        //Reset flag saying we've already created the next match - we will not have a nextmatch after this fxn
        camp.alreadyLoadedNextMatch = false;

        //Default to 'nextMap' if map == null
        if (map == null)
            map = nextMatch.map;

        //Default to nextMatch.teams if playingTeams is null
        if (playingTeams == null)
        {
            playingTeams = nextMatch.teams;
        }
        matchTeams.Clear();
        //print("At beginning of SNMFPM: playingTeams.Count = " + playingTeams.Count);
        //clean stuff from previous match
        //Destroy all bases and stuff
        NIU.DestroyAll(ref allUnits);
        NIU.DestroyAll(ref allBases);
        ClearAllTeamsUnitsAndBases();
        //Reset all teams
        foreach (Team t in playingTeams)
            t.ResetToStartOfMatchConditions();

        //Update in-this-match related variables
        //matchTeams.Clear();


        //Change the current match to be this one
        MatchMaker.current = nextMatch;

        //Update in-this-match variables.
        foreach (Team t in allTeams)
        {

            //teams in this match
            if (playingTeams.Contains(t))
            {
                t.inThisMatch = true;

                matchTeams.Add(t);
            }
            //teams not in this match
            else
            {
                t.inThisMatch = false;

            }

        }
        
        //Load next map
        if (changeMap)
        {
            //print("We are about to load the map, " + map.realName + ", with " + playingTeams.Count + " teams.");
            ChangeMap(map, playingTeams.Count);
        }
        else
        {
            //print("We are about to load the map in SNMFPM().");
            currentMap.Load(matchTeams.Count);
        }

        //Tell everything the match is starting
        allTeams.Sort(new TeamSort());
        matchTeams.Sort(new TeamSort());

        NIU.BroadcastWorld("STARTPREMATCH");

        matchState = MatchState.preMatch;

        Qk.UIWorker.teamInt = Qk.Player.teamInt;

    }


    public void StartNextMatchFromPreMatch()
    {
        
        //clean stuff from previous match
        //Destroy all bases and stuff
        NIU.DestroyAll(ref allUnits);
        NIU.DestroyAll(ref allBases);
        ClearAllTeamsUnitsAndBases();

        //Reset all teams
        foreach (Team t in nextMatch.teams)
            t.ResetToStartOfMatchConditions();

        //Update in-this-match related variables and find appropriate map
        matchTeams.Clear();

        //Update in-this-match variables.
        foreach (Team t in allTeams)
        {

            //teams in this match
            if (nextMatch.teams.Contains(t))
            {
                t.inThisMatch = true;

                matchTeams.Add(t);
            }
            //teams not in this match
            else
            {
                t.inThisMatch = false;

            }

        }

        //Load next map
        //print(nextMatch.teams.Count);
        ChangeMap(nextMatch.map, nextMatch.teams.Count);

        //Tell everything the match is starting
        allTeams.Sort(new TeamSort());
        matchTeams.Sort(new TeamSort());

        MatchMaker.current = nextMatch;

        NIU.BroadcastWorld("STARTPREMATCH");

        matchState = MatchState.preMatch;

        Qk.UIWorker.teamInt = Qk.Player.teamInt;

    }

    ///Destroys and removes everything in the teams' units and bases lists
    public void ClearAllTeamsUnitsAndBases()
    {
        foreach(Team t in matchTeams)
        {

            ignoreThese.AddRange(t.units);
            ignoreThese.AddRange(t.bases);

            t.units.Clear();
            t.bases.Clear();

        }

    }
    
    ///destroys everything in the dump
    public void CleanDump()
    {
        NIU.DestroyAll(NIU.EveryChildOf(dump));
    }
    
    ///destoys all dead units
    public void IncinerateTheDead()
    {
        //kill dying bases
        List<GameObject> hitlistBASE = new List<GameObject>();

        foreach(Base b in FindObjectsOfType<Base>())
        {
            if (b.hp < 0)
                hitlistBASE.Add(b.gameObject);
        }

        allBases.RemoveAll(w => hitlistBASE.Contains(w.gameObject));

        NIU.DestroyAll(hitlistBASE);


        //kill dying units
        List<GameObject> hitlistUNIT = new List<GameObject>();

        foreach (Unit u in FindObjectsOfType<Unit>())
        {
            if (u.hp < 0 && !u.dummy)
            {
                hitlistUNIT.Add(u.gameObject);
                //hitlistUNIT.Add(u.cylinder);
            }
        }

        allUnits.RemoveAll(w => hitlistUNIT.Contains(w.gameObject));

        NIU.DestroyAll(hitlistUNIT);
    }

    ///figures out which teams are in this match. Should only be called by start-of-match functions.
    public Match MatchUpTeamsAndMap(List<Team> playingTeams)
    {
        //Handle an unworkably small amount of teams
        if (playingTeams.Count < 2)
        {
            Debug.LogWarning("We can't start a match with " + playingTeams.Count + " players! Doing 2 instead.");
            playingTeams.Clear();
            playingTeams.AddRange(NIU.RandomMembers(allTeams, 2));
        }

        


        //Find an appropriate map
        UpdateNextMap(playingTeams.Count);

        nextMatch = new Match(playingTeams, nextMap);

        //Return the next match to play.
        return nextMatch;
    }

    /// <summary>
    /// Matches up teams and map, given only a number of desired teams.
    /// </summary>
    /// <param name="teamCount">Amount of teams to have in the match.</param>
    /// <param name="includePlayer">If true, the player will be included in this match.</param>
    /// <returns></returns>
    public Match MatchUpTeamsAndMap(int teamCount, bool includePlayer = true, bool thisIsTheNextCampaignMatch = false)
    {
        //If this is supposed to be the next match we play for campaign, and we've already loaded one, then skip.
        if (thisIsTheNextCampaignMatch && camp.alreadyLoadedNextMatch)
            return nextMatch;

        camp.alreadyLoadedNextMatch = thisIsTheNextCampaignMatch;


        List<Team> playingTeams = new List<Team>();

        //-1 teams means random
        if (teamCount == -1)
            teamCount = NIU.RandomR(2, highestTeamCount);

        //Handle an unworkably small amount of teams
        if (teamCount < 2)
        {
            Debug.LogWarning("We can't start a match with " + teamCount + " players! Doing 2 instead.");
            teamCount = 2;
        }


        //Create the teams we're gonna use for the next match.
        if (includePlayer)
        {
            //assign a team to player if they don't have one
            if(Qk.Player.team == null)
            {
                playingTeams.AddRange(CreateNewTeams(teamCount));
                playingTeams[0].BecomePlayer();
            }
            //DO NOT DELETE THE PLAYER'S TEAM. ADD THEM TO THE PLAYING TEAMS.
            else
            {
                playingTeams.AddRange(CreateNewTeams(teamCount - 1));
                playingTeams.Add(Qk.Player.team);
            }
            //print("We're including player.");
        }
        else
        {
            playingTeams.AddRange(CreateNewTeams(teamCount));
            //print("We're !!NOT!! including player.");
        }


        //Debug.Log("playingTeams has " + playingTeams.Count + " teams in it. teamCount = " + teamCount + ". includePlayer = " + includePlayer);

        //Debug.Log("playingTeams: " + playingTeams.Count);

        //Set up the next match to load
        nextMatch = new Match(playingTeams, UpdateNextMap(playingTeams.Count));
        camp.alreadyLoadedNextMatch = thisIsTheNextCampaignMatch;
        //Debug.Log("NEXTMATCH: map = " + nextMatch.map.realName + ". playingTeams = " + nextMatch.teams.Count);

        //Return the next match to play.
        return nextMatch;
    }



    /// <summary>
    /// Changes nextMap to be a Map that accomodates the given number of teams.
    /// </summary>
    /// <param name="teamsInMatch">The number of teams to play on the next map.</param>
    /// <returns></returns>
    public Map UpdateNextMap(int teamsInMatch)
    {

        //Find next map


        //If the next map works with this many players, then keep it?
        if (nextMap.validTeamCounts.Contains(teamsInMatch) && !alreadyDoneMaps.Contains(possibleMaps.IndexOf(nextMap)))
        {
            return nextMap;
        }
        //Find maps THAT WE HAVEN'T DONE FIRST that match the current player count
        List<Map> possibleMapsWithThisManyTeams = new List<Map>();

        foreach (Map m in possibleMaps)
        {
            if (m.validTeamCounts.Contains(teamsInMatch) && !alreadyDoneMaps.Contains(possibleMaps.IndexOf(m)))
                possibleMapsWithThisManyTeams.Add(m);
        }

        //If we found none, then resort to maps we've already done
        if (possibleMapsWithThisManyTeams.Count <= 0)
        {
            foreach (Map m in possibleMaps)
            {
                if (m.validTeamCounts.Contains(teamsInMatch))
                    possibleMapsWithThisManyTeams.Add(m);
            }
        }
        //if we STILL found none, then cry
        if (possibleMapsWithThisManyTeams.Count <= 0)
        {
            Debug.LogError("We can't find any maps that can hold " + teamsInMatch + " teams!");
            return nextMap; //TODO: handle this better. Find a valid number of teams and randomly select that many teams from the pool?
        }

        //Randomly find a next map
        NIU.Shuffle(possibleMapsWithThisManyTeams);

        nextMap = possibleMapsWithThisManyTeams[0];


        return nextMap;
    }




    public List<Team> CreateNewTeams(int amount)
    {
        List<Team> results = new List<Team>();

        int currentNumber = 0;
        for(int i = 1; i <= amount; i++)
        {
            while (allTeams.Find(w => w.number == currentNumber))
                currentNumber++;

            Team newTeam = Instantiate(defaultTeamObject, Vector3.zero, NIU.QuaternionAll(0f)).GetComponent<Team>();
            newTeam.number = currentNumber;
            currentNumber++;

            results.Add(newTeam);
            allTeams.Add(newTeam);
        }

        return results;
    }

    /// <summary>
    /// Destroys this many teams.
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="ignorePlayer"></param>
    /// <returns></returns>
    public int DestroyTeams(int amount = -1, bool ignorePlayer = true)
    {
        if (amount < 0)
            amount = allTeams.Count;

        List<GameObject> hitlist = new List<GameObject>();
        for(int i = allTeams.Count - 1; i >= Mathf.Max(allTeams.Count - amount, 0); i--)
        {
            if(!ignorePlayer || !allTeams[i].isPlayer)
                hitlist.Add(allTeams[i].gameObject);
        }
        allTeams.RemoveAll(w => hitlist.Contains(w.gameObject));

        int destroyed = hitlist.Count;
        NIU.DestroyAll(hitlist);

        

        return destroyed;
    }

    public int DestroyTeamsOfThisMatch(bool ignorePlayer = true)
    {
        //Debug.Log("DESTROYING THIS MATCH: map = " + nextMatch.map.realName + ". teamCount = " + nextMatch.teams.Count + ".");

        List<GameObject> hitlist = new List<GameObject>();
        for (int i = MatchMaker.current.teams.Count - 1; i >= 0; i--)
        {
            if (!(ignorePlayer && MatchMaker.current.teams[i].isPlayer))
                hitlist.Add(MatchMaker.current.teams[i].gameObject);
        }
        allTeams.RemoveAll(w => hitlist.Contains(w.gameObject));
        MatchMaker.current.teams.RemoveAll(w => hitlist.Contains(w.gameObject));


        int destroyed = hitlist.Count;
        NIU.DestroyAll(hitlist);


        //Debug.Log("WE HAVE DESTROYED " + destroyed + "!!");
        return destroyed;
    }



    class TeamSort : IComparer<Team>
    {
        public int Compare(Team x, Team y)
        {

            // CompareTo() method 
            return x.number.CompareTo(y.number);

        }
    }




    public void QuitGame()
    {
        Debug.Log("Quitting game.");
        Application.Quit();
#if DEBUG
        UnityEditor.EditorApplication.ExitPlaymode();
#endif
    }


    public void BackToMain()
    {
        /*
        //NOT in match
        matchState = MatchState.not;
        
        //Destroy all teams
        DestroyTeams(-1, false);
        */

        Qk.UIWorker.teamInt = -1;


        //kill the player (if there is one in this match)
        //Start new match with random team number (if the player was in the previous one)
        if (!(Qk.Player?.team?.inThisMatch == null || !Qk.Player.team.inThisMatch))
        {
            Qk.Player.team.Lose();
            StartBackgroundMatch();
        }
        
        //Set this match to background (enables camera automatically rotating)
        MatchMaker.current.background = true;

        //Open title screen
        mm.Open(mm.menuScreens[0]);


        //Tell all other objects that the background state just changed
        NIU.BroadcastWorld("CHECKBACKGROUND");

    }



    public void StartBackgroundMatch(int teams = -1)
    {
        mm.skirmish = true;
        DestroyTeams(ignorePlayer: false);
        Qk.Player.teamInt = PlayerControls.notPlayingInt;
        MatchUpTeamsAndMap(teams, false, false);
        nextMatch.background = true;

        StartNextMatchFromPreMatch();

        //Qk.UIWorker.teamInt = -1;

        /*
        Match match = MatchMaker.CreateNewMatch(teams, includePlayer: false);
        match.background = true;
        Qk.UIWorker.teamInt = -1;
        match.StartMatch();*/
    }
}
