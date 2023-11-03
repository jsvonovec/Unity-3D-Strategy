using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalancedCosts : Tech
{
    ///This is supposed to be a technology that you research between games. Sometimes, the AI might be able to use it, too.
    
    //Set tech-specific info here, like name or description.
    private void OnValidate()
    {
        ID = 2;
        realName = "Long-Run Accounting";
        desc = "The costs of buying units are the same, equal to the average of units available in the era.";
        availableToAI = true;
        //enabled = active;

        team = transform.parent.GetComponent<Team>();
    }


    //This is called as soon as the tech is enabled or at the start of a game (after Awake())
    private IEnumerator Startup()
    {
        //skip if we're inactive or we've already started up
        if (initialized || !active)
            yield break;


        OnValidate();
        Update();

        initialized = true;
        yield break;

    }



    //Put tech function here. Make sure this only looks at other scripts and doesn't edit them unless that's what the tech is for.
    private void Update()
    {
        if (!active)
            return;

        float avgCost = 0f;
        float ccost = 0f;

        int i = 0;
        foreach(float cost in team.defPrices)
        {
            ccost += cost;
            i++;
        }

        avgCost = ccost / i;

        for(int j = 0; j < team.defPrices.Count; j++)
        {
            team.defPrices[j] = avgCost;
        }
    }
}
