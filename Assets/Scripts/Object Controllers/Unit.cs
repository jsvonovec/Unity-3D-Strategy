using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleSheetsToUnity;

public class Unit : MatchThing
{
    // Google Sheets to Unity
    [HideInInspector]
    public string associatedSheet = "1GVXeyWCz0tCjyqE1GWJoayj92rx4a_hu4nQbYmW_PkE";
    [HideInInspector]
    public string associatedWorksheet = "Stats";

    [Header("Descriptors")]
    public string realName;
    public string desc;
    public int ID;
    public bool custom = false;
    public bool dummy
    {
        get
        {
            if (gameObject.name == "Unit Stats" || !CompareTag("Unit"))
            {
                enabled = false;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    //Not actually a live Unit if true - it's on the Unit Stats gameobject.
    public int era = 0;   //When you evolve, you go up an era
    [System.NonSerialized] public int kills = 0;
    public bool jumper = false;  //Jumps at target
    public bool turret = false;
    /// <summary>
    /// Reset attackCD after every kill
    /// </summary>
    public bool comboKills = false; //reset attackCD after a kill
    /// <summary>
    /// Reset attackCD after every kill, but only when first spawned
    /// </summary>
    public bool comboKillsWhenInvuln = false;
    public int level;
    /*{
        get
        {
            //returns which index on killsToLevel we're at
            int i = 0;
            foreach(int l in killsToLevel)
            {
                if (kills < l)
                    break;
                i++;
            }
            //if (i > 0)
                //Debug.Log(name + "is now level " + i);
            return i;
        }
    }*/
    public int PrevKillLevel
    {
        get
        {
            //Returns what level we would be at if we had 1 less kill
            int i = 0;
            foreach (int l in killsToLevel)
            {
                if (kills - 1 < l)
                    break;
                i++;
            }
            return i;
        }
    }
    [Header("Important Stats")]
    public float cost = 50;
    public float bounty = 65;
    public float exp = 50;
    public float damage = 1f;
    public float maxhp = 10f;
    public float speed = 5f;      //force to apply to move
    public float range = 1.5f;    //Melee range = 0.5 ||| Standard Ranged range = ~7-15
    public float attackCD = 0.5f; //This many seconds between attacks
    [Header("Secondary Stats")]
    public float desiredRange = 0f;    //Melee units:  = 0
    public float attackWindUp = 0f;   //When in range, this long until attack
    public float attackSlowSpeed = 0.5f;  //multiply by speed when you're attacking
    public float higherEraDamageMult = 1f;    //For every era higher the target is, we do this much more damage
    public float lowerEraDamageMult = 1f;     //Every era lower applies this again.
    public float buildTime = 1f;
    public float levelUpHeal = 0.5f;
    [Header("Level Up Stat Multipliers")]
    public float lvlDamage = 1.05f;
    public float lvlMaxHp = 1.07f;
    public float lvlAtkRate = 0.97f;
    public float lvlSpeed = 1.1f;
    public float lvlRange = 1.03f;
    public float lvlBounty = 1.5f;
    public float lvlExp = 1.5f;
    public float lvlUnitCount = 2f;
    public float maxLvlLifetime = 10f;
    [Header("Other stats")]
    public Color sphereColor = Color.clear; //Color of sphere (signifies unit type)
    public Sprite icon;    //The graphic that is slightly transparent on the unit card (UI)
    public Color iconColor; //The color of the graphic
    public priority targPriority = priority.close;
    public List<int> killsToLevel;
    public float jumpCD = 5f;
    public float jumpAirTime = 2f;
    public float jumpOvershoot = 0f;
    public float invulnTime = 1f;   //Invulnerable for x seconds after spawning
    public float maxLifetime = 0f;
    public bool targetable = true;
    public float untargetableTime = 0f; //after spawning
    public bool cancelTargetingBaseWhenNewEnemyIsAvailable = true;

    [Header("Misc")]
    [System.NonSerialized] public bool criticalMax = false;    //When at 1 HP at max level; medics shouldn't heal this one, hes a goner
    [System.NonSerialized] public Vector3 turretSpawn;
    [System.NonSerialized] public Base turretBase;
    public bool waitUntilInRange = false;
    [System.NonSerialized] public bool invulnerable = false;

    [System.NonSerialized] public float cAttackCD = 0f;
    private float cJumpCD = 1f;
    [System.NonSerialized] public bool inRange = false;
    private float iniDrag;
    private float timeBetweenChecks = 0.25f;

    [System.NonSerialized] public int teamI = 0;      //0 = player (blue), 1 = enemy (red/purple/orange?)
    [System.NonSerialized] public GameObject target = null;
    public bool targetingBase
    {
        get
        {
            return target != null && target.GetComponent<Base>() != null;
        }
    }
    public bool targetingUnit
    {
        get
        {
            return target != null && target.GetComponent<Unit>() != null;
        }
    }
    [System.NonSerialized] public Base targetBase = null;
    [System.NonSerialized] public Team team;
    public float hp;
    private UnitCustomizer uc;

    private bool initialized = false;
    private GameMaster gm;

    [System.NonSerialized] public GameObject cylinder;
    private Renderer cylR;
    private GameObject hitSph;
    private Renderer hitR;

    private Rigidbody rb;
    private SphereCollider coll;
    private Renderer r;

    /// <summary>
    /// Show custom inspector
    /// </summary>
    public bool showCustomInspector = true;

    // Start is called before the first frame update
    private void Start()
    {
        //Disable this component if we're just here to be stats.
        if (gameObject.name == "Unit Stats" || !CompareTag("Unit") || dummy)
        {
            enabled = false;
        }
        else
        {
            enabled = true;

            /*if (!initialized)
                Initialize();*/
        }
    }

    /*private void Awake()
    {
        if (!initialized)
            Initialize();
    }*/



    //Actually start all processes
    public void Initialize()
    {
        u = this;
        isUnit = true;

        //Disable this component if we're just here to be stats.
        if (gameObject.name == "Unit Stats" || !CompareTag("Unit"))
        {
            enabled = false;
        }
        else
        {
            enabled = true;
        }

        gm = Qk.GM;
        if (dummy || initialized)
            return;


        rb = GetComponent<Rigidbody>();
        coll = GetComponent<SphereCollider>();
        r = GetComponent<Renderer>();

        cylinder = NIU.FindChildNamed(this, "Cylinder", true);
        cylR = cylinder.GetComponent<Renderer>();
        hitSph = NIU.FindChildNamed(this, "Sphere", true);
        hitR = hitSph.GetComponent<Renderer>();

        gm.allUnits.Add(this);        //Add us to all units
        team = gm.allTeams.Find(w => w.number == teamI);

        uc = NIU.FindChildNamed(team, "Unit Stats", nullError: true).GetComponent<UnitCustomizer>();


        //If we have a custom unit possible, update our stats to match that.
        if (uc.IdsOfModdedUnits.Contains(ID))
        {
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(uc.mods.Find(w => w.ID == ID)), this);
            //Debug.Log("Loaded custom stats onto " + name + "!");
        }


        //changing color depending on unit type
        r.material.color = sphereColor;

        //changing cyl color depending on team
        if (teamI <= gm.teamColors.Count)
        {
            cylR.material.color = gm.teamColors[teamI];
        }


        //Enabling and disabling era markers
        GameObject eraIndic = NIU.FindChildNamed(cylinder, "Era Indicators", true);
        for (int i = 0; i < team.eraExps.Count + 1; i++)
        {
            GameObject thisE = NIU.FindChildNamed(eraIndic, "E" + i.ToString(), false);

            if (i <= era)
            {
                thisE?.SetActive(true);
            }
            else
                thisE?.SetActive(false);

        }


        hp = maxhp;
        iniDrag = rb.drag;
        rb.drag = 0f;

        //turrets
        if (turret)
        {
            if (speed == 0f)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                rb.isKinematic = true;
                transform.Rotate(new Vector3(0f, Random.value * 360, 0f));
            }
        }



        StartCoroutine("CheckTargetCycle");
        StartCoroutine("InvulnTimer");
        StartCoroutine(Lifetimer());
        StartCoroutine(UntargetTimer());
        StartCoroutine(CheckGround());

        initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!initialized)
            Initialize();

        if (turret)
        {
            transform.Rotate(Vector3.up, Time.deltaTime * 40f);
        }


        //Take damage if we're at max level
        if (level >= killsToLevel.Count && hp > 1f)
        {
            hp -= (Time.deltaTime / maxLvlLifetime) * maxhp;


            if (hp <= 0 || maxLvlLifetime <= 0f)
            {
                hp = 1f;
                criticalMax = true;
            }
        }

        //Lost health over time if we're at max level
        if (criticalMax)
        {
            hp -= Time.deltaTime / maxLvlLifetime;
        }


        //die
        if (hp <= 0f || transform.position.y < -15f || (turret && (turretBase == null || turretBase.hp <= 0)))
        {
            Die();
            return;
        }











        if (target == null || !CanTarget(target.GetComponent<MatchThing>()))
            FindTarget();
        //~~~~~~~~~~~~~~~~~~~~~~HAS A TARGET~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        if (target != null)
        {
            Debug.DrawLine(transform.position, target.transform.position, gm.teamColors[teamI % gm.teamColors.Count]);

            //find all variables that bases and units have in common
            float distance;
            bool tisbase = target.GetComponent<Base>() != null;
            Unit tunit = null;
            Base tbase = null;
            int tera;
            float tbounty;
            float texp;
            float thp;
            float tmaxhp;

            if (tisbase)
            {
                tbase = target.GetComponent<Base>();
                tera = tbase.team.era;
                tbounty = tbase.bounty;
                texp = tbase.exp;
                thp = tbase.hp;
                tmaxhp = tbase.maxhp;
            }
            else
            {
                tunit = target.GetComponent<Unit>();
                tera = tunit.era;
                tbounty = tunit.bounty;
                texp = tunit.exp;
                thp = tunit.hp;
                tmaxhp = tunit.maxhp;
            }

            distance = Vector3.Distance(transform.position, target.transform.position)
                            - (Mathf.Min(target.transform.lossyScale.x, target.transform.lossyScale.z) / 2)
                            - (Mathf.Min(transform.lossyScale.x, transform.lossyScale.z) / 2);



            //Attack target (in range) (uses total distance between both their transform.positions)
            if (distance < range)
            {


                inRange = true;

                float cDamage = damage;
                // Calculate higher era damage multiplier
                if (higherEraDamageMult != 1f)
                {
                    //Apply multiplier for every era difference there is
                    if (!tisbase && tunit.era > era)
                        cDamage *= Mathf.Pow(higherEraDamageMult, tunit.era - era);
                    else if (tisbase && tbase.team.era > era)
                        cDamage *= Mathf.Pow(higherEraDamageMult, tbase.team.era - era);
                }

                // Calculate lower era damage multiplier
                if (lowerEraDamageMult != 1f)
                {
                    //Apply multiplier for every era difference there is
                    if (!tisbase && tunit.era < era)
                        cDamage *= Mathf.Pow(lowerEraDamageMult, era - tunit.era);
                    else if (tisbase && tbase.team.era < era)
                        cDamage *= Mathf.Pow(lowerEraDamageMult, era - tbase.team.era);
                }


                //Attack & reset cooldown
                if (cAttackCD < 0f)
                {
                    cAttackCD = attackCD;

                    //get exp if its a base
                    if (tisbase)
                        team.Pay(0f, Random.value * 2f * tbase.damageToExpRatio * cDamage * Mathf.Pow(tbase.damageExpEraMod, tbase.team.era) * 2f);

                    //damage them and take credit if we kill them
                    bool killed;
                    if (tisbase)
                        killed = tbase.Damage(cDamage, this);
                    else
                        killed = tunit.Damage(cDamage, this);
                    //Debug.Log(name + " (" + hp + ") =!" + damage + "!=> " + target.name + " (" + target.hp + ")");
                    if (GameMaster.BigDebugLog)
                        Debug.Log(name + " (team " + team.number + ") attacked " + target.name + " for " + damage + " damage. Killed = " + killed + ". Frame: " + Time.frameCount);

                    //We killed them
                    if (killed)
                    {
                        if (comboKills || (comboKillsWhenInvuln && invulnerable)) cAttackCD = 0f;

                        //Count lots of kills for a higher era unit
                        /*if (tera > era)
                        {
                            kills += (tera - era) * 2;
                            maxLvlLifetime += tera - era;
                            lvlBounty *= Mathf.Pow(0.9f, (tera - era));
                            lvlExp *= Mathf.Pow(0.9f, (tera - era));
                            lvlUnitCount *= Mathf.Pow(0.9f, (tera - era));
                        }
                        
                        //Deteriorate if target was lower era
                        else if(tera < era)
                        {
                            //lvlBounty *= Mathf.Pow(1.2f, (era - tera));
                            //lvlExp *= Mathf.Pow(1.2f, (era - tera));

                            LevelUp(Level + 1, false, true);
                        }
                        else*/
                        kills++;

                        //if (tera >= era)
                        {
                            StartCoroutine("DidDamage", target);
                            LevelUp();
                        }

                        team.Pay(tbounty, StandardExpReward(tisbase));
                        FindTarget();
                    }
                    else if (!tisbase && !tunit.invulnerable)
                    {
                        StartCoroutine("DidDamage", target);
                    }

                    //Find new target if we healed them to max
                    if (thp >= tmaxhp && !tisbase && tunit.teamI != teamI)
                    {
                        target = null;
                    }

                    //show graphics
                }

                //count down cooldown ONLY WHEN IN RANGE (so we can wind up again)
                cAttackCD -= Time.deltaTime;
            }
            else
            {
                inRange = false;
                if (waitUntilInRange)
                    target = null;
                //Keep cooldown at windup amount when not in range
                cAttackCD = Mathf.Max(cAttackCD, 0f);   //Gets to 0 or higher...
                cAttackCD = Mathf.Max(attackWindUp, cAttackCD - Time.deltaTime);    //SO we can reverse cooldown until it matches attackWindUp
            }


            //We're still checking if target is null because we could have killed them by now

            //Approach target
            if (target != null && rb.drag != 0)
            {

                //change speed if we're in range
                float cspeed;
                if (inRange)
                    cspeed = speed * attackSlowSpeed * Mathf.Sign(distance - desiredRange);
                else
                    cspeed = speed;

                //Move toward target
                if (rb.velocity.magnitude < Mathf.Abs(cspeed / 10))
                    rb.AddForce(new Vector3(target.transform.position.x - transform.position.x,
                                            0f,
                                            target.transform.position.z - transform.position.z).normalized * cspeed);
            }


            //JUMPERS: Jump toward target
            if (target != null && jumper)
            {
                //jump
                if (cJumpCD <= 0f && !inRange)
                {
                    cJumpCD = jumpCD;

                    //Disable drag for a bit
                    StartCoroutine(DraglessTimer());

                    //Kinematic equations (yikes!)
                    //the horizontal vector
                    Vector3 horiz = new Vector3((target.transform.position.x - transform.position.x),
                                                0f,
                                                (target.transform.position.z - transform.position.z)).normalized * jumpOvershoot;

                    //the force vector (both horizontal and vertical parts)
                    rb.AddForce(new Vector3(horiz.x * (distance / jumpAirTime),
                                            -1f / 2f * Physics.gravity.y * jumpAirTime,
                                            horiz.z * (distance / jumpAirTime)), ForceMode.Impulse);

                }


                cJumpCD -= Time.deltaTime;
            }
        }

    }

    #region Finding a Target - functions of priority and FindTarget()

    public enum priority { useUnits, close, far, @base, low, strong, area, tLow, tStrong, tBase, tArea, anyClose, any }

    public float thisIsOurTargetChance = 0.30f;
    
    /// <summary>
    /// Can prioritize a thing on our team
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    private bool TeamPriority(priority p)
    {
        if (p == priority.useUnits)
            p = targPriority;
        return AnyThingPriority(p) || p == priority.tLow || p == priority.tStrong || p == priority.tBase || p == priority.tArea;
    }

    /// <summary>
    /// Can prioritize a unit
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    private bool UnitPriority(priority p)
    {
        if (p == priority.useUnits)
            p = targPriority;
        return AnyThingPriority(p) || (p != priority.@base && p != priority.tBase);

    }
    /// <summary>
    /// Can prioritize a base
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    private bool BasePriority(priority p)
    {
        if (p == priority.useUnits)
            p = targPriority;
        return AnyThingPriority(p) || p == priority.@base || p == priority.tBase || p == priority.anyClose;

    }

    private bool AnyThingPriority(priority p)
    {
        if (p == priority.useUnits)
            p = targPriority;
        return p == priority.any || p == priority.anyClose;
    }

    /// <summary>
    /// ???
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    ///
    /*
    private bool CanBasePriority(priority p)
    {
        if (p == priority.useUnits)
            p = targPriority;
        return p == priority.any || !TeamPriority(p);

    }
    */
    private bool CanTarget<T>(T thing, priority p = priority.any)
    {
        //UNITS
        //It is a unit....
        if (thing is Unit u)
        {
            //And it's on the team we're looking for
            if ((AnyThingPriority(p) || (u.teamI != teamI && !TeamPriority(p)) || (u.teamI == teamI && TeamPriority(p))) 
                //And it's targetable
                && u.targetable && u.hp > 0)
                return true;
            else
                return false;
        }

        //BASES
        //It is a base...
        else if (thing is Base b)
        {
            //And it's on the team we can target
            if ((AnyThingPriority(p) || (b.team.number != teamI && !TeamPriority(p)) || (b.team.number == teamI && TeamPriority(p))) 
                //And it still has health and its team hasn't lost
                && b.hp > 0 && !b.team.lostCurrentMatch)
                return true;
            else
                return false;
        }

        //error
        else
        {
            Debug.LogError("ERROR: We tried to find out if we can target a " + thing.GetType().ToString() + ". (which isn't a unit or base)");
            return false;
        }
    }


    private GameObject FindTarget(priority p = priority.useUnits, bool reset = false)
    {
        //default to unit's choice
        if (p == priority.useUnits)
            p = targPriority;
        if (targPriority == priority.useUnits)
        {
            Debug.LogError("For some reason our target priority on " + realName + " is useUnits.", this);
            targPriority = priority.close;
        }

        //Cancel if there's no targets
        /*if ((gm.allUnits.Count == team.units.Count && !TeamPriority(p) && UnitPriority(p)) && (gm.allBases.Count == team.bases.Count && CanBasePriority(p)))
        {
            Debug.Log("Everything we can target is apparently on our team, " + team.number + ". Cancelling finding target.");
            return null;
        }*/

        //reset target if requested
        if (reset)
            target = null;

        //set 'best': the best value we've found, and 'goodness': the current quarry's favorability with the current priority. More = better
        float best = float.NaN;
        GameObject bestQuarry = null;
        float goodness = float.NaN;

        //find all possible things that we can target
        List<MatchThing> quarries = new List<MatchThing>();

        //finding units, if that's what we're looking for
        if (UnitPriority(p))
            foreach (Unit u in gm.allUnits)
                if (CanTarget(u, p))
                    quarries.Add(u);
        //Add self if we're a healer
        if (damage < 0)
            quarries.Add(this);

        //finding bases, if that's what we're looking for
        if (BasePriority(p))
            foreach (Base b in gm.allBases)
                if (CanTarget(b, p))
                {
                    quarries.Add(b);
                }

        //Stop if we have no targets possible
        if(quarries.Count == 0)
        {
            //Debug.Log("We have no quarries in FindTarget on " + name + ".", this);
            return null;
        }

        //start with our current target - give them priority. Also remove them if they're not targetable
        if(target != null)
        {
            if (quarries.Contains(target.GetComponent<MatchThing>()))
                quarries.Remove(target.GetComponent<MatchThing>());

            if (CanTarget(target.GetComponent<MatchThing>(), p))
                quarries.Insert(0, target.GetComponent<MatchThing>());
            else
                target = null;
        }


        //Start at a random spot - shuffle
        NIU.Shuffle(ref quarries);



        //Time to find our target!
        foreach(MatchThing q in quarries)
        {
            //Randomly throw away quarries, unless it's the first one (which can be random or our current target)
            if(Random.value < thisIsOurTargetChance || bestQuarry == null)
            {
                //Find goodness based on priority p
                switch (p)
                {
                    //The CLOSEST ENEMY UNIT
                    case priority.close:
                        {
                            //Has to be negative, because a lower distance is better (smaller negative > bigger negative)
                            goodness = -Vector3.Distance(transform.position, q.transform.position);

                            //Make bases a low priority
                            if (q.isBase)
                                goodness -= 1000f;

                            break;
                        }


                    //The LOWEST HEALTH ENEMY UNIT
                    case priority.low:
                        {
                            //Again, negative because lower numbers are better
                            goodness = -q.HP;

                            //Make bases a low priority
                            if (q.isBase)
                                goodness -= 1000f;

                            break;
                        }


                    //The STRONGEST ENEMY UNIT (highest health)
                    case priority.strong:
                        {
                            if (q.isUnit)
                                goodness = q.u.hp;
                            else
                                goodness = -1000f;

                            break;
                        }


                    //The CLOSEST ENEMY BASE
                    case priority.@base:
                        {
                            //Has to be negative, because a lower distance is better (smaller negative > bigger negative)
                            if (q.isBase)
                                goodness = -Vector3.Distance(transform.position, q.transform.position);
                            //If they're not a base, don't care about them. goodness = NaN.

                            break;
                        }


                    //The LOWEST HEALTH FRIENDLY UNIT
                    case priority.tLow:
                        {
                            if (q.isUnit)
                                goodness = -q.HP;
                            //Target ourself if nobody else is around
                            if (q == this)
                                goodness = -1000f;

                            break;
                        }

                    //The CLOSEST THING of ANY TEAM
                    case priority.anyClose:
                        {
                            //Has to be negative, because a lower distance is better (smaller negative > bigger negative)
                            goodness = -Vector3.Distance(transform.position, q.transform.position);
                            //Make us low priority
                            if (q == this)
                                goodness -= 100000f;
                            //Make bases a low priority
                            if (q.isBase)
                                goodness -= 10000f;

                            break;
                        }


                    //ANY THING of ANY TEAM
                    case priority.any:
                        {
                            //Randomly find a unit
                            goodness = Random.value;

                            //Make us low priority
                            if (q == this)
                                goodness -= 100000f;
                            //Make bases a low priority
                            if (q.isBase)
                                goodness -= 10000f;

                            break;
                        }

                }
                
                //Then compare it to the best. If it's better, remember it
                if(!float.IsNaN(goodness) && (goodness > best || float.IsNaN(best)))
                {
                    best = goodness;
                    bestQuarry = q.gameObject;
                }

            }
        }

        if(bestQuarry == null)
        {
            Debug.LogWarning("We got no targets after searching through " + quarries.Count + " of them? (priority: " + p.ToString() + ")", this);
        }

        //Set our target to the best one we've found
        target = bestQuarry;

        //Finally, return our findings
        return bestQuarry;
    }

    #endregion


    public bool Damage(float dmg, Unit attacker = null)
    {
        if (invulnerable) return false;

        hp -= dmg;
        if (hp <= 0f)
        {
            Die(attacker);
            return true;
        }
        else
        {
            StartCoroutine("TookDamage", dmg);
        }
        //Actually getting healed
        if (hp > maxhp)
            hp = maxhp;
        
        return false;
    }


    //Flash red when taking damage
    IEnumerator TookDamage(float dmg)
    {
        

        hitR.enabled = true;
        if(dmg >= 0)
            hitR.material.color = Color.red;
        else
            hitR.material.color = Color.blue;

        yield return new WaitForSeconds((Mathf.Abs(dmg) / maxhp) * 0.5f);

        hitR.enabled = false;
        hitR.material.color = Color.clear;
        yield break;
    }


    //Flash green when dealing damage
    IEnumerator DidDamage(GameObject other)
    {
        float ohp;
        float omaxhp;

        if(other.GetComponent<Base>() == null)
        {
            ohp = other.GetComponent<Unit>().hp;
            omaxhp = other.GetComponent<Unit>().maxhp;
        }
        else
        {
            ohp = other.GetComponent<Base>().hp;
            omaxhp = other.GetComponent<Base>().maxhp;
        }
        //cancel if we're already doing a flash animation
        if (hitR.enabled)
            yield break;

        hitR.enabled = true;
        if (ohp <= 0)
        {
            //gold when we kill
            hitR.material.color = new Color(1f, 1f, 0.2f);
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            //green when we do damage
            if(damage >= 0)
                hitR.material.color = Color.green;
            //cyan when we heal
            else
                hitR.material.color = Color.cyan;

            yield return new WaitForSeconds((Mathf.Abs(damage) / omaxhp) * 0.5f);
        }


        hitR.enabled = false;
        hitR.material.color = Color.clear;
        yield break;
    }



    private void Die(Unit killer = null)
    {
        //turrets can't die
        if (turret && turretBase != null && turretBase.hp > 0f)
            return;

        //remove from lists
        if(team != null)
        {
            gm.ignoreThese.Add(this);
            gm.allUnits.Remove(this);
            team.units.Remove(this);
            team.turrets.Remove(this);
        }
        rb.drag = iniDrag;

        // float away; just died               //(hp + 10200f) / 200f cylinder.transform.position -= new Vector3(0f, Time.deltaTime / 2f, 0f);
        if (hp > -50000f)
        {
            hp = -100000f;
            if(cylinder != null)
            cylinder.AddComponent<Rigidbody>().AddForceAtPosition(new Vector3(Random.value - 0.5f * 8f, 20f, Random.value - 0.5f * 8f),
                                                                  transform.position + new Vector3(Random.value - 0.5f * transform.localScale.x, 0f,
                                                                                                   Random.value - 0.5f * transform.localScale.z),
                                                                  ForceMode.Impulse);

            //If at max level, pay every other team a cut of the bounty
            if (level >= killsToLevel.Count)
                foreach (Team t in gm.matchTeams)
                {
                    if (t != team)
                        t.Pay(bounty / gm.matchTeams.Count, exp / gm.matchTeams.Count);
                }
        }

        //make cylinder fly away depending on damage taken
        if (killer != null && cylinder != null)
        {
            cylinder.GetComponent<Rigidbody>().AddForce((transform.position - killer.transform.position).normalized * (killer.damage / maxhp) * 20f,
                                                            ForceMode.Impulse);
        }
        rb.AddForce(Vector3.up * Physics.gravity.magnitude * 5); // * ((transform.position.y / 2f) + 1f)
        //coll.enabled = false;
        //cylinder.transform.parent = null;
        r.material.color = new Color(r.material.color.r * 1.01f,
                                     r.material.color.g * 1.01f,
                                     r.material.color.b * 1.01f);
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rb.constraints = RigidbodyConstraints.None;
        rb.drag = 0;
        hp -= Time.deltaTime * 100;

        //make sure we don't have any weird big number glitches
        if (transform.position.magnitude > 1000f)
            rb.Sleep();
        if(cylinder != null && cylinder.transform.position.y < -10f)
            Destroy(cylinder);
        if (hp <= -100200f && cylinder == null)
        {
            Destroy(gameObject);
        }
    }

    
    
    IEnumerator CheckTargetCycle()
    {
        while (gm.timeBetweenTargetChecks > 0)
        {
            switch (targPriority)
            {
                case priority.low:
                case priority.anyClose:
                case priority.close:
                    //Reset every so often to find the actual closest unit now that we've moved a bit
                    FindTarget(reset: !inRange);
                    break;

                case priority.useUnits:
                case priority.@base:
                case priority.strong:
                case priority.far:
                    break;

                case priority.tLow:
                    target = null;
                    FindTarget();
                    break;
            }

            yield return new WaitForSeconds(gm.timeBetweenTargetChecks);
        }
    }



    private int LevelUp(int toLevel = -1, bool giveBonuses = true, bool giveDrawbacks = true)
    {
        //actual level is calculated in the Level definition
        if (toLevel == -1)
            toLevel = level;
        else if(toLevel < killsToLevel.Count)        //Force to this level
        {
            kills = killsToLevel[toLevel];
            level = toLevel;
        }
        
        //JUST levelled up
        if(PrevKillLevel != level)
        {
            level++;

            if (giveBonuses)
            {
                //Update stats (good)
                damage *= lvlDamage;
                maxhp *= lvlMaxHp;
                attackCD *= lvlAtkRate;
                speed *= lvlSpeed;
                range *= lvlRange;

                //heal
                hp = Mathf.Min(hp + (maxhp * levelUpHeal), maxhp);
            }

            if (giveDrawbacks)
            {
                //Update stats (bad)
                bounty *= lvlBounty;
                exp *= lvlExp;
            }


        }

        List<GameObject> indicators = new List<GameObject>();

        //add all level indicators (particles, lights, etc.)
        for(int i = 0; i < transform.GetChild(2).childCount; i++)
        {
            indicators.Add(transform.GetChild(2).GetChild(i).gameObject);
        }

        //activate all relevant level objects for each possible level
        for (int j = 0; j <= toLevel; j++)
        {
            foreach (GameObject go in indicators.FindAll(w => w.name == "L" + j.ToString()))
            {
                go.SetActive(true);
                //Debug.Log("Set active L" + j.ToString() + " on " + name);
            }
        }

        //activate glowing light if we're at max level
        if(toLevel >= killsToLevel.Count)
            foreach (GameObject go in indicators.FindAll(w => w.name == "LMAX"))
            {
                go.SetActive(true);
            }



        return toLevel;
    }


    IEnumerator InvulnTimer()
    {
        //Makes us invulnerable for x seconds after spawn
        if(invulnTime <= 0f)
        {
            invulnerable = false;
            yield break;
        }

        invulnerable = true;

        yield return new WaitForSeconds(invulnTime * Mathf.Pow(team.invulnUnitCountFactor, (gm.allUnits.Count / (gm.matchTeams.Count - 1)) - (team.units.Count * 2)));

        invulnerable = false;
    }

    IEnumerator Lifetimer()
    {
        if (maxLifetime <= 0f)
            yield break;

        yield return new WaitForSeconds(maxLifetime);

        Die();
    }

    IEnumerator UntargetTimer()
    {
        if (untargetableTime <= 0f)
            yield break;

        yield return new WaitForSeconds(untargetableTime);

        targetable = true;
    }

    IEnumerator DraglessTimer()
    {
        float time = 0f;
        while (time <= 0.5f)
        {
            rb.drag = 0f;
            yield return new WaitForFixedUpdate();
            time += Time.fixedDeltaTime;
        }
        rb.drag = iniDrag;
    }


    IEnumerator CheckGround()
    {
        while (timeBetweenChecks > 0)
        {
            if (Physics.OverlapSphere(transform.position, coll.transform.lossyScale.y * 1.02f).Length > 1)
                rb.drag = iniDrag;
            else
                rb.drag = 0f;
            yield return new WaitForSeconds(timeBetweenChecks);
        }
    }



    public void BoostStats(float ratio)
    {
        float hpratio = hp / maxhp;

        //cost /= ratio;
        //bounty /= ratio;
        //exp /= ratio;
        attackCD /= ratio; //This many seconds between attacks
        damage *= ratio;
        maxhp *= ratio;
        //speed *= ratio;      //force to apply to move
        //range *= ratio;    //Melee range = 0.5 ||| Standard Ranged range = ~7-15

        //fix health
        hp = maxhp * hpratio;
    }


    private float StandardExpReward(bool tIsBase)
    {

        if (tIsBase)
        {
            Base tbase = target.GetComponent<Base>();
            if (tbase == null)
            {
                Debug.LogWarning("Tried to find EXP reward, but said target is base when it isn't I guess. Target = " + target.name, gameObject);
                return 0f;
            }

            if (tbase.team.era > team.era)
            {
                //Debug.Log("Unit " + name + " earned " + (tbase.exp * Mathf.Pow(team.higherEraCashMult, tbase.team.era - team.era)) + " exp. \n"
                    //+ "(Normal exp amount: " + tbase.exp + ")");
                return tbase.exp * Mathf.Pow(team.higherEraCashMult, tbase.team.era - team.era);
            }
            else
                return tbase.exp;
        }
        else
        {
            Unit tunit = target.GetComponent<Unit>();
            if (tunit == null)
            {
                Debug.LogWarning("Tried to find EXP reward, but said target is NOT a base when it is, I guess. Target = " + target.name, gameObject);
                return 0f;
            }

            if (tunit.team.era > team.era)
            {
                /*Debug.Log("Unit " + name + " earned " + (tunit.exp * Mathf.Pow(team.higherEraCashMult, tunit.team.era - team.era)) + " exp. \n"
                    + "(Normal exp amount: " + tunit.exp + ". Their era: " + tunit.era + ". His name? " + tunit.realName + ".)");*/
                return tunit.exp * Mathf.Pow(team.higherEraCashMult, tunit.team.era - team.era);
            }
            else
                return tunit.exp;
        }
    }



    // To read stat info from Google Sheets
    private void OnValidate()
    {
        //SpreadsheetManager.Read
    }

}
