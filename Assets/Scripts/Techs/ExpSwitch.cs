using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpSwitch : Tech
{
    ///This is supposed to be a technology that you research between games. Sometimes, the AI might be able to use it, too.



    //THIS TECH SPECIFICALLY
    //Set tech-specific info here, like name or description.
    private void OnValidate()
    {
        ID = 1;
        realName = "Exp Reverse";
        desc = "Gain experience only when your units die. Gain none when your units score kills.";
        availableToAI = true;
        //enabled = active;

        team = transform.parent.GetComponent<Team>();
    }

    private int knownLivingUnits;
    private List<Unit> deadUnitsAccountedFor;
    //Compare these every frame. If one from Known is missing from Current, add the one that's missing. 
    //(ignore ones that are missing from Known; it just means one just spawned)
    private float prevExp;
    private int prevKills;
    private int totalKills
    {
        get
        {
            int kills = 0;
            foreach(Unit u in team.units)
            {
                kills += u.kills;
            }

            return kills;
        }
    }
    //If totalKills goes up that means we killed something, and we should revert our exp back to before we got the kill.


    private IEnumerator Startup()
    {
        //skip if we're inactive or we've already started up
        if (initialized || !active)
        {
            yield break;
        }

        deadUnitsAccountedFor = new List<Unit>();

        OnValidate();
        Update();

        initialized = true;
        yield break;
    }



    //This tech makes it so that you only gain experience when your units are killed and none when enemies are killed.
    private void Update()
    {
        if (!active)
            return;

        //Remove destroyed dead guys from our list
        List<Unit> duaf = new List<Unit>();
        duaf.AddRange(deadUnitsAccountedFor);
        foreach(Unit u in duaf)
        {
            if (u == null)
                deadUnitsAccountedFor.Remove(u);
        }

        //Cancel experience
        if (totalKills != prevKills && team.exp > prevExp)
        {
            team.exp = prevExp;
        }

        //Add experience if we lose a dude
        if(knownLivingUnits != team.units.Count)
        {
            foreach(Unit u in FindObjectsOfType<Unit>())
            {
                if(u.team == team && u.hp <= 0 && !deadUnitsAccountedFor.Contains(u) && u.enabled)
                {
                    team.exp += u.exp;
                    deadUnitsAccountedFor.Add(u);
                }
            }
        }


        //Update our knowledge
        prevExp = team.exp;
        prevKills = totalKills;
        knownLivingUnits = team.units.Count;
    }
}
