using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuffleStats : Tech
{
    ///This is supposed to be a technology that you research between games. Sometimes, the AI might be able to use it, too.

    //Set tech-specific info here, like name or description.
    private void OnValidate()
    {
        ID = 3;
        realName = "Hyper-Riffle";
        desc = "Shuffles the stats of ALL your units at the start of every game";
        availableToAI = false;
        //enabled = active;

        team = transform.parent.GetComponent<Team>();
    }


    /*
    [Header("Important Stats")]
    public float cost = 50;
    public float bounty = 65;
    public float exp = 50;
    public float damage = 1f;
    public float maxhp = 10f;
    public float speed = 5f;      //force to apply to move
    public float range = 1.5f;    //Melee range = 0.5 ||| Standard Ranged range = ~7-15
    public float attackCD = 0.5f; //This many seconds between attacks
        public bool jumper = false;  //Jumps at target
    public bool turret = false;
    public int era = 0;   //When you evolve, you go up an era
    public Color sphereColor = Color.clear; //Color of sphere (signifies unit type)
    public priority targPriority = priority.close;
    public string realName;
    public string desc;
    public float jumpAirTime = 2f;
    [Header("Secondary Stats")]
    public float desiredRange = 0f;    //Melee units:  = 0
    public float attackWindUp = 0f;   //When in range, this long until attack
    public float attackSlowSpeed = 0.5f;  //multiply by speed when you're attacking
    public float higherEraDamageMult = 1f;    //For every era higher the target is, we do this much more damage
    public float buildTime = 1f;
    public float levelUpHeal = 0.5f;

    */

    private IEnumerator Startup()
    {
        //skip if we're inactive or we've already started up
        if (initialized || !active)
            yield break;


        OnValidate();

        initialized = true;
        yield break;
    }

    
    public IEnumerator StartOfMatch()
    {
        //skip if we're inactive. Startup if we haven't
        if (!active)
            yield break;
        if (!initialized)
            Startup();

        #region Shuffling the stats (lots of them)
        UnitCustomizer uc = team.GetComponentInChildren<UnitCustomizer>();
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {

            uc.EnableUnitCustomizationOn(i);
        }

        //all the stats we're shuffling
        //I hate this
        //primary
        List<float> cost = new List<float>();
        foreach (GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            cost.Add(u.cost);
        }
        cost = NIU.Shuffle(cost);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.cost = cost[i];
        }

        List<float> bounty = new List<float>();
        foreach(GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            bounty.Add(u.bounty);
        }
        bounty = NIU.Shuffle(bounty);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.bounty = bounty[i];
        }

        List<float> exp = new List<float>();
        foreach (GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            exp.Add(u.exp);
        }
        exp = NIU.Shuffle(exp);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.exp = exp[i];
        }

        List<float> damage = new List<float>();
        foreach (GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            damage.Add(u.damage);
        }
        damage = NIU.Shuffle(damage);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.damage = damage[i];
        }

        List<float> maxhp = new List<float>();
        foreach (GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            maxhp.Add(u.maxhp);
        }
        maxhp = NIU.Shuffle(maxhp);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.maxhp = maxhp[i];
        }

        List<float> speed = new List<float>();
        foreach (GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            speed.Add(u.speed);
        }
        speed = NIU.Shuffle(speed);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.speed = speed[i];
        }

        List<float> range = new List<float>();
        foreach (GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            range.Add(u.range);
        }
        range = NIU.Shuffle(range);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.range = range[i];
        }

        List<float> attackCD = new List<float>();
        foreach (GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            attackCD.Add(u.attackCD);
        }
        attackCD = NIU.Shuffle(attackCD);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.attackCD = attackCD[i];
        }


        //secondary
        List<float> desiredRange = new List<float>();
        foreach (GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            desiredRange.Add(u.desiredRange);
        }
        desiredRange = NIU.Shuffle(desiredRange);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.desiredRange = desiredRange[i];
        }

        List<float> attackWindUp = new List<float>();
        foreach (GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            attackWindUp.Add(u.attackWindUp);
        }
        attackWindUp = NIU.Shuffle(attackWindUp);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.attackWindUp = attackWindUp[i];
        }

        List<float> attackSlowSpeed = new List<float>();
        foreach (GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            attackSlowSpeed.Add(u.attackSlowSpeed);
        }
        attackSlowSpeed = NIU.Shuffle(attackSlowSpeed);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.attackSlowSpeed = attackSlowSpeed[i];
        }

        List<float> higherEraDamageMult = new List<float>();
        foreach (GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            higherEraDamageMult.Add(u.higherEraDamageMult);
        }
        higherEraDamageMult = NIU.Shuffle(higherEraDamageMult);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.higherEraDamageMult = higherEraDamageMult[i];
        }

        List<float> buildTime = new List<float>();
        foreach (GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            buildTime.Add(u.buildTime);
        }
        buildTime = NIU.Shuffle(buildTime);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.buildTime = buildTime[i];
        }

        List<float> levelUpHeal = new List<float>();
        foreach (GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            levelUpHeal.Add(u.levelUpHeal);
        }
        levelUpHeal = NIU.Shuffle(levelUpHeal);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.levelUpHeal = levelUpHeal[i];
        }


        //misc
        List<float> jumpAirTime = new List<float>();
        foreach (GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            jumpAirTime.Add(u.jumpAirTime);
        }
        jumpAirTime = NIU.Shuffle(jumpAirTime);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.jumpAirTime = jumpAirTime[i];
        }

        List<string> realName = new List<string>();
        foreach (GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            realName.Add(u.realName);
        }
        realName = NIU.Shuffle(realName);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.realName = realName[i];
        }

        List<string> uDesc = new List<string>();
        foreach (GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            uDesc.Add(u.desc);
        }
        uDesc = NIU.Shuffle(uDesc);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.desc = uDesc[i];
        }

        List<bool> jumper = new List<bool>();
        foreach (GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            jumper.Add(u.jumper);
        }
        jumper = NIU.Shuffle(jumper);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.jumper = jumper[i];
        }

        List<bool> turret = new List<bool>();
        foreach (GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            turret.Add(u.turret);
        }
        turret = NIU.Shuffle(turret);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.turret = turret[i];
        }

        List<int> era = new List<int>();
        foreach (GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            era.Add(u.era);
        }
        era = NIU.Shuffle(era);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.era = era[i];
        }

        List<Color> sphereColor = new List<Color>();
        foreach (GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            sphereColor.Add(u.sphereColor);
        }
        sphereColor = NIU.Shuffle(sphereColor);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.sphereColor = sphereColor[i];
        }

        List<Unit.priority> targPriority = new List<Unit.priority>();
        foreach (GameObject go in team.possibleUnitTypes)
        {
            Unit u = go.GetComponent<Unit>();

            targPriority.Add(u.targPriority);
        }
        targPriority = NIU.Shuffle(targPriority);
        for (int i = 0; i < team.possibleUnitTypes.Count; i++)
        {
            Unit u = uc.mods[i];

            u.targPriority = targPriority[i];
        }
        #endregion


        yield break;

    }
}
