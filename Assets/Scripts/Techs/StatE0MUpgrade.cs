using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatE0MUpgrade : Tech
{
    ///This is supposed to be a technology that you research between games. Sometimes, the AI might be able to use it, too.


    [SerializeField] private float damageChange = 3f;
    [SerializeField] private float rangeChange = 0.3f;
    [SerializeField] private float HPChange = 8f;
    [SerializeField] private float CostChange = 1f;


    //Set tech-specific info here, like name or description.
    private void OnValidate()
    {
        ID = 0;
        realName = "Tougher Clubs";
        desc = "Clubwielder Damage, HP, Range +++ // Cost +";
        availableToAI = true;
        //enabled = active;
        repeatable = true;

        team = transform.parent.GetComponent<Team>();
    }


    //This is called as soon as the tech is enabled or at the start of a game (after Awake())
    private IEnumerator Startup()
    {
        //skip if we're inactive or we've already started up
        //if (initialized || !active)
            //yield break;
            //We want the tech to be releatable, and since Startup() is only called when it first gets researched, we shouldn't have weirdly
            //skyrocketing stats for out clubwielders.

        OnValidate();

        Unit upgradedUnit = team.uc.EnableUnitCustomizationOn(0);

        upgradedUnit.damage += damageChange;
        upgradedUnit.range += rangeChange;
        upgradedUnit.maxhp += HPChange;
        upgradedUnit.cost += CostChange;

        initialized = true;
        yield break;
    }



}
