using System.Collections.Generic;
using UnityEngine;

public class MatchMaker : MonoBehaviour
{
    /*  The purpose of this script is to:
     *      a) Control which teams will face which, most likely with a tournament tracker
     *      b) Handle what the teams that aren't participating in the current match will do
     *          i) This will most likely mean other matches will be 'simulated' - a winner will be randomly selected
     */

    public static Match current = null;

    /// <summary>
    /// Creates a Match object, which stores what teams are fighting. Can start itself with the StartMatch() method.
    /// </summary>
    /// <param name="random">Randomly select teams from gm.allTeams?</param>
    /// <param name="participants">Teams in the match. If it's random, leave as null.</param>
    /// <param name="includePlayer">Should the player be in if it's random?</param>
    public static Match CreateNewMatch(int size = -1, bool random = true, List<Team> participants = null, bool includePlayer = true, Map map = null)
    {
        GameMaster gm = Qk.GM;

        //Just create a new match with the designated participants
        if(participants != null)
        {

            Match m = new Match(participants: participants, map: map);
            current = m;

            return m;
        }


        if (size < 2)
        {
            Debug.LogWarning("We can't start a match with " + size + " players! Doing 2 instead.");
            size = 2;
        }

        //Resize 'size' if it's bigger than the amount of teams we can hold in one map
        size = Mathf.Min(size, gm.highestTeamCount);

        //Randomly find teams
        if (random)
        {
            participants = NIU.Shuffle(gm.allTeams).GetRange(0, size);

            //If player must participate and none are the player yet, then replace one with player
            if (includePlayer && participants.Find(w => w.isPlayer) == null && gm.allTeams.Find(w => w.isPlayer) != null)
            {

                try
                {
                    participants[NIU.RandomR(0, participants.Count - 1)] = gm.allTeams.Find(w => w.isPlayer);
                }
                catch (System.NullReferenceException)
                {
                    Debug.LogWarning("We said we needed to includePlayer, but there is no player.");
                }
            }

            //if we're not supposed to include the player
            else if (!includePlayer)
            {
                PlayerControls.def.teamInt = PlayerControls.notPlayingInt;
            }
        }

        else if (participants == null)
        {
            Debug.LogError("We tried to start a match with a null list of participants, and random is " + random.ToString() + ".");
        }


        //create new Match object
        Match match = new Match(participants: participants, map: map);

        current = match;

        return match;
    }


}









//A general class to describe a match and set-up matches. Should be applicable no matter what metagame we end up going with.
public class Match
{

    public List<Team> teams = new List<Team>();
    public bool background = false;
    public Map map;

    public Team RandomDetermineWinner()
    {
        List<Team> shuffledTeams = NIU.Shuffle(teams);

        for (int i = 1; i < shuffledTeams.Count; i++)
        {
            shuffledTeams[i].Lose();
        }

        shuffledTeams[0].Win();

        return shuffledTeams[0];
    }


    public Match(List<Team> participants, Map map = null)
    {
        teams = participants;
        this.map = map;
    }


    public void StartMatch()
    {
        Qk.GM.StartNewMatchFromPreMatch(teams, true, map: map);
    }

    public void EndMatch()
    {

    }
}


