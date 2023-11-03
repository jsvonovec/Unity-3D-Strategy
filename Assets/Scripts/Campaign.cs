using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Campaign : MonoBehaviour
{
    public float startDiff = -2;
    public float startStatBoostRatio = 1.1f;
    public float startAITechChance = 0.1f;
    public int startLossesAllowed = 2;
    public int minDifficultyToIncreaseEnemies = -3;
    public int maxDifficultyToIncreaseEnemies = 10;

    private float difficulty = -1;   //The difficulty we're starting at
    private float statBoostRatio = 1.1f; //Every match, the AI's stats get boosted this much times
    private float AIChanceToTechPerDifficulty = 0.1f;    //Chance to tech = 1 - ((1 - t)^d). Repeat successes up to [d] techs.
    private int lossesAllowed = 2;
    public float BoostAmount
    {
        get
        {
            if(!thisIsFirstMatch)
                return Mathf.Pow(statBoostRatio, difficulty);
            else
                return Mathf.Pow(statBoostRatio, startDiff);
        }
    }

    public float cAITechChance
    {
        get
        {
            return 1 - Mathf.Pow(1 - AIChanceToTechPerDifficulty, difficulty);
        }
    }
    private int wins
    {
        get
        {
            return gm.allTeams[PlayerControls.playingInt].wins;
        }
    }
    private int losses
    {
        get
        {
            return gm.allTeams[PlayerControls.playingInt].losses;
        }
    }
    private bool thisIsFirstMatch = true;
    [HideInInspector] public bool alreadyLoadedNextMatch = false;

    private PlayerControls p;
    private GameMaster gm;
    public static Campaign def = null;  //This is the one Campaign in the scene. Set in Qk

    private void Awake()
    {

        p = Qk.Player;
        gm = Qk.GM;

        ResetNumbers();
    }


    public void LoadNextMatch()
    {
        if (!thisIsFirstMatch && losses > lossesAllowed)
        {
            LoseCampaign();
            return;
        }


        //The first match of the campaign
        if (thisIsFirstMatch)
            ResetNumbers();




        //only destroy the player if this is the first match
        //But still destroy them all. We have teams waiting to join.
        DestroyAndReplaceTeams(killPlayer: thisIsFirstMatch);

        //clean stuff from previous match
        //Destroy all bases and stuff
        NIU.DestroyAll(ref gm.allUnits);
        NIU.DestroyAll(ref gm.allBases);
        gm.ClearAllTeamsUnitsAndBases();


        //Figure out how many teams should be playing as a function of difficulty
        int teamCount = FindTeamCountFromDifficulty();

        //If this is the first match, then match up teams right now.
        if(thisIsFirstMatch)
        {
            thisIsFirstMatch = false;
            gm.MatchUpTeamsAndMap(teamCount, true, true);
            print("teamcount = " + gm.nextMatch.teams.Count + ", a = " + alreadyLoadedNextMatch);
        }
        //If this isn't the first match, then we SHOULD HAVE MATCHED UP TEAMS ALREADY. Just start.
        
        gm.nextMatch.background = false;

        int i = 0;
        foreach (Team t in gm.nextMatch.teams)
        {
            if (t == null)
            {
                print("NULL at index " + i + "!");
                print("nextMap is " + gm.nextMatch.map.realName + ".");
            }
            else
                //print(i + "'s name is " + t.name);

            i++;
        }
        //print("$$$Here, nextMatch.teams.Count = " + gm.nextMatch.teams.Count);
        gm.StartNewMatchFromPreMatch(gm.nextMatch.teams);

        //Allow AIs to boost their stats with difficulty
        foreach (Team t in gm.matchTeams)
            if (!t.isPlayer)
                t.canBoostStats = true;


        if(thisIsFirstMatch)
            thisIsFirstMatch = false;



        //difficulty++;             //TODO: Difficulty should only go up if you win

    }

    /// <summary>
    /// Returns a number of teams to play with at the campaign's current difficulty.
    /// </summary>
    /// <returns></returns>
    public int FindTeamCountFromDifficulty()
    {
        return FindTeamCountFromDifficulty(difficulty);
    }
    /// <summary>
    /// Returns a number of teams to play with at a given difficulty, using Campaign rules.
    /// </summary>
    /// <param name="diff"></param>
    /// <returns></returns>
    public int FindTeamCountFromDifficulty(float diff)
    {
        int teamCount;

        if (diff <= minDifficultyToIncreaseEnemies)    //low difficulty = lower teams
            teamCount = 2;
        else if (diff >= maxDifficultyToIncreaseEnemies) //high difficulty = high teams
            teamCount = gm.highestTeamCount;
        else
        {
            //An amount of teams that is along the increasing curve between min and max difficulty
            //First, find the floor (ex. if it should be 5.45 teams, first we do 5 teams)
            float exactTeams = Mathf.InverseLerp(minDifficultyToIncreaseEnemies, maxDifficultyToIncreaseEnemies, diff)
                    * (gm.highestTeamCount - 2) + 2;
            teamCount = Mathf.FloorToInt(exactTeams);
            //Then, randomly add 1 depending on remainder (ex. if 5.45, then 45% chance to add 1)
            if (Random.value < exactTeams % 1)
                teamCount++;
        }


        return teamCount;
    }


    public void LoseCampaign()
    {
        gm.BackToMain();
        ResetNumbers();

        thisIsFirstMatch = true;
    }

    private void ResetNumbers()
    {
        difficulty = startDiff;
        statBoostRatio = startStatBoostRatio;
        lossesAllowed = startLossesAllowed;
        AIChanceToTechPerDifficulty = startAITechChance;
    }


    public List<Team> DestroyAndReplaceTeams(int newTeamCount = 0, bool killPlayer = false)
    {
        if (gm == null)
            Awake();
        /*
        if(newTeamCount < 0)
        {
            if(killPlayer)
                newTeamCount = NIU.RandomR(0, gm.highestTeamCount - 2) + 2;
            else
                newTeamCount = NIU.RandomR(0, gm.highestTeamCount - 3) + 2;
        }
        */
        //Replace all AI teams
        gm.DestroyTeamsOfThisMatch(ignorePlayer: !killPlayer);


        if (newTeamCount <= 0)
            return null;

        //int techs;
        //Create new teams and give them techs randomly
        List<Team> newTeams = new List<Team>();
        newTeams.AddRange(gm.CreateNewTeams(newTeamCount));
        foreach (Team t in newTeams)
        {
            t.Initialize();
            
            /*techs = 0;
            while (techs <= difficulty && !t.isPlayer)
                //Roll to see if this AI gets a tech. Keep doing so until either we run out, or techs == difficulty.
                if (Random.value < cAITechChance)
                {
                    int r = t.tm.possibleTechs[NIU.RandomR(0, t.tm.possibleTechs.Count - 1)];
                    Debug.Log("Team " + t.number + " researched tech " + t.tm.techComps[r].realName + "!");

                    t.ResearchTech(r, true);
                    techs++;
                }
                else
                    break;
            */

        }


        GiveTeamsTechs(newTeams, false, difficulty: difficulty, techChance: cAITechChance);

        return newTeams;
    }
    //Chance to tech = 1 - ((1 - t)^d). Repeat successes up to[d] techs.
    /// <summary>
    /// Gives each team a random amount of techs in the list. Each team may learn up to difficulty techs. Chance to tech = 1 - ((1 - t)^d).
    /// </summary>
    /// <param name="teams">The list of teams to be given a random number of randomly-drawn techs.</param>
    /// <param name="difficulty">The given difficulty. Note that a difficulty of 0 or lower prevents teams from getting techs this way.</param>
    /// <param name="techChance">The chance for EACH CHECK to succeed. A failed check will prevent future checks from rolling.</param>
    /// <returns></returns>
    public static int GiveTeamsTechs(List<Team> teams, bool includePlayer, float difficulty = 0f, float techChance = 0.3f)
    {
        int totalTechs = 0;
        int techs = 0;
        //Create new teams and give them techs randomly
        foreach (Team t in teams)
        {
            totalTechs += techs;

            techs = 0;
            while (techs <= difficulty)
                //Roll to see if this AI gets a tech. Keep doing so until either we run out, or techs == difficulty.
                if (Random.value < (1 - Mathf.Pow(1 - techChance, difficulty)) && (includePlayer || t != Qk.Player?.team))
                {
                    int r = t.tm.possibleTechs[NIU.RandomR(0, t.tm.possibleTechs.Count - 1)];
                    Debug.Log("Team " + t.number + " researched tech " + t.tm.techComps[r].realName + "!");

                    t.ResearchTech(r, true);
                    techs++;
                }
                else
                    break;

        }

        return totalTechs;
    }

    public static int GiveTeamTechs(Team team, bool includePlayer = true, float difficulty = 0f, float techChance = 0.3f)
    {
        /*int totalTechs = 0;
        int techs = 0;
        //Create new teams and give them techs randomly
            totalTechs += techs;

            techs = 0;
            while (techs <= difficulty)
                //Roll to see if this AI gets a tech. Keep doing so until either we run out, or techs == difficulty.
                if (Random.value < techChance && (includePlayer || team != Qk.Player?.team))
                {
                    int r = team.tm.possibleTechs[NIU.RandomR(0, team.tm.possibleTechs.Count - 1)];
                    Debug.Log("Team " + team.number + " researched tech " + team.tm.techComps[r].realName + "!");

                    team.ResearchTech(r, true);
                    techs++;
                }
                else
                    break;

        return totalTechs;*/

        List<Team> t = new List<Team>();
        t.Add(team);

        return GiveTeamsTechs(t, includePlayer, difficulty, techChance);
    }

}
