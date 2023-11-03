using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Team : MonoBehaviour
{
    public Team factoryReset = null;

    public int era = 0;
    public float money = 0;
    public float exp = 0;
    public int startingEra = 0;
    public float startingMoney = 0f;
    public float startingExp = 0f;
    //public float expBankruptCashRatio = 1f;    //When we run out of money, we lose exp and get this much $$$ for every exp lost
    public float inflation
    {
        get
        {
            return Mathf.Pow(eraInflations[era], adjUnitCount);
        }
    }
    public float maxExpMult = 3f;   //Max exp = this times highest era exp needed
    public float turretCostMult = 3f;
    public float buildTimeMod = 1f; //multiplier for build times
    public float buildSkipPerDamage = 0.01f;    //For every point of damage taken on base, skip x seconds of build time per era. (ex era 0 = 1x, era 4 = 3^4x)
    public float invulnUnitCountFactor = 1.1f;  //More invuln for every unit the enemy has on us
    public float basicIncome = 1f;
    public float incomeEraScale = 2.5f;
    public float higherEraCashMult = 2f;    //Gain x^era times exp when killing higher era units

    public bool inThisMatch = true;
    public int number;
    public bool lostCurrentMatch = false;
    public List<Base> bases;
    public List<Unit> units;
    public List<Unit> turrets;
    public List<GameObject> possibleUnitTypes;  //The types of units we can build Throughout the game
    public List<int> possibleIDs
    {
        get
        {
            List<int> ids = new List<int>();

            foreach(GameObject go in possibleUnitTypes)
            {
                ids.Add(go.GetComponent<Unit>().ID);
            }
            ids.Sort();
            return ids;
        }
    }
    public List<GameObject> eraTypes;       //The types of units we can build Now
    public List<Unit> buildQueue;
    public List<Base> baseBuildSpawnQueue;
    private int maxBuildQueue = 5;
    public List<float> amountsPaid;
    public List<Unit> eraUnits
    {
        get
        {
            List<Unit> us = new List<Unit>();
            foreach(GameObject go in eraTypes)
            {
                us.Add(go.GetComponent<Unit>());
            }
            return us;  
        }
    }   //The unit components of available-to-build units
    public List<float> eraExps;
    public List<float> eraInflations;
    public List<float> defPrices;    //prices of eraTypes
    public List<float> actPrices     //prices post inflation
    {
        get
        {
            List<float> t = new List<float>();

            //Updating prices
            if (!lostCurrentMatch)
            {
                int j = 0;
                foreach (int i in defPrices)
                {
                    //non turrets
                    if (!eraTypes[j].GetComponent<Unit>().turret)
                        t.Add(i * inflation);
                    //turrets
                    else
                        t.Add(i * inflation * Mathf.Pow(turretCostMult, turrets.Count));

                    j++;
                }

                return t;
            }
            
            return defPrices;
        }
    }
    public float adjUnitCount
    {
        get
        {
            float count = 0f;

            foreach(Unit u in units)
            {
                //Every level a unit has increases their costs on the team by the square of their level
                count += Mathf.Pow(u.lvlUnitCount, u.level);
            }
            foreach (Unit u in turrets)
            {
                //Every level a unit has increases their costs on the team by the square of their level
                count += Mathf.Pow(u.lvlUnitCount, u.level);
            }

            count += buildQueue.Count;


            return count;
        }
    }   //Adjusted unit count, with levels
    [HideInInspector] public int healerCount = 0;

    public List<Tech> afterMatchResearchableTechs = new List<Tech>();
    public int wins = 0;
    public int losses = 0;
    public bool canPickTech = false;
    public int researchTechCount = 3;
    private bool initialized = false;
    public bool isPlayer = true;
    public bool canBoostStats = false;

    public Base selBase;   //Currently selected base
    public bool readyToBuild = true;
    public bool skipBuild = false;  //When the base gets hit, skip the buildtime
    public float cBuildSkip = 0f;
    private GameMaster gm;
    public Canvas cv;
    private TextMeshPro txm;
    public TechManager tm;
    public UnitCustomizer uc;
    private Campaign camp;

    // Start is called before the first frame updates

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (initialized)
        {
            //Debug.Log("We've already initialized on Team " + number + ".");
            return;
        }

        gm = Qk.GM;
        uc = NIU.FindChildNamed(this, "Unit Stats", nullError: true).GetComponent<UnitCustomizer>();
        tm = NIU.FindChildNamed(this, "Techs").GetComponent<TechManager>();
        camp = Qk.Campaign;
        isPlayer = Qk.Player.teamInt == number;

        if (isPlayer)
            Qk.Player.team = this;

        possibleUnitTypes.Sort(new EraSort());
        
        eraTypes = new List<GameObject>();
        defPrices = new List<float>();
        bases = new List<Base>();
        units = new List<Unit>();
        buildQueue = new List<Unit>();
        baseBuildSpawnQueue = new List<Base>();
        amountsPaid = new List<float>();

        name = "Team " + number.ToString();

        StartCoroutine(CheckEmptiesInLists(1f));

        initialized = true;
    }
    

    public void STARTPREMATCH()
    {
        if (!initialized)
            Start();

        if (!inThisMatch || !gm.allTeams.Contains(this))
        {
            return;
        }

        //Do techs
        tm.StartMatchAll();


        //figure out our lists
        //bases = new List<Base>();

        if (gm == null)
            Debug.Log("gm is null on team " + number, this);

        //bases
        
        foreach (Base b in FindObjectsOfType<Base>())
        {
            if (b.team.number == number && !gm.ignoreThese.Contains(b))
            {
                bases.Add(b);
                //gm.allBases.Add(b);
                //Debug.Log("Adding base. Base count is now " + bases.Count + " on team " + number);
            }
        }

            
        if (bases.Count == 0)
            Debug.LogError("Bases = 0 on " + name + "!");
        else
            selBase = bases[0];
        if (bases[0] == null)
            Debug.LogWarning("We just set selBase to be null, because bases[0] is null.");

        //units
        units = new List<Unit>();
        foreach (Unit u in GetComponentsInChildren<Unit>())
        {
            if (!u.dummy && !gm.ignoreThese.Contains(u))
                units.Add(u);
        }





        //reset stats
        ChangeEra(startingEra);
        money = startingMoney;
        exp = startingExp;
        cBuildSkip = 0f;

        cancelBuild = true;


        //DEBUG
        if (txm == null)
            txm = GetComponentInChildren<TextMeshPro>();
        else
            txm.gameObject.SetActive(true);
        txm.color = Color.white * gm.teamColors[number % gm.teamColors.Count].grayscale + new Color(0,0,0,1);
        cv = GetComponentInChildren<Canvas>();
        cv.transform.SetParent(transform);
        cv.transform.localScale = new Vector3(-0.2f, 0.2f, 0.2f);


        ChangeEra(era);
        if (era > 0)
            exp = eraExps[Mathf.Min(era - 1, eraExps.Count - 1)];
        
    }







    private void OnValidate()
    {
        possibleUnitTypes.Sort(new EraSort());
        //name = "Team " + number.ToString();
    }






    // Update is called once per frame
    void Update()
    {
        //Initialize if we haven't
        if (!initialized)
            Initialize();


        //clean out empty spots in our lists
        //units.RemoveAll(w => w == null);
        //bases.RemoveAll(w => w == null);

        //MIDMATCH
        if (gm.matchState == GameMaster.MatchState.midMatch && inThisMatch)
        {

            

            //basic income
            money += basicIncome * Mathf.Pow(incomeEraScale, era) * Time.deltaTime;

            //If we're out of bases, we lost
            if (bases.Count == 0 && !lostCurrentMatch)
            {
                Lose();
            }


            //Going up an era
            if (eraExps.Count > era && exp >= eraExps[era] && !lostCurrentMatch)
            {
                ChangeEra(era + 1);
            }





            //Update text on base (DEBUG??)
            if (!lostCurrentMatch && selBase != null)
            {
                txm.gameObject.SetActive(true);

                cv.transform.position = selBase.transform.position + (Vector3.up * 8);
                cv.transform.LookAt(FindObjectOfType<Camera>().transform);
                txm.text = "Money: " + money.ToString() + "\nEXP: " + exp.ToString() + "\nEra: " + (era + 1).ToString()
                    + "\n\nHP: " + selBase.hp.ToString();
                //armor pips
                if (selBase.armor > 0)
                {
                    txm.text += " (";
                    for (int i = 0; i < selBase.armor; i++)
                    {
                        if (i % 8 == 0)
                            txm.text += "\n";
                        txm.text += "#";
                    }
                    txm.text += ")";
                }
            }
            else
                txm?.gameObject?.SetActive(false);




        }


        //AFTERMATCH
        else if(gm.matchState == GameMaster.MatchState.afterMatch && inThisMatch)
        {

            

        }
    }


    /// <summary>
    /// Resets soft values to their initial conditions.
    /// </summary>
    public void ResetToStartOfMatchConditions()
    {
        era = startingEra;
        money = startingMoney;
        exp = startingExp;
        skipBuild = false;
        lostCurrentMatch = false;

        buildQueue.Clear();
        baseBuildSpawnQueue.Clear();
        amountsPaid.Clear();
    }



    public void Pay(float cash, float xp = 0)
    {
        if (bases.Count == 0)
            return;

        exp += xp;
        //When exp exceeds max exp, every base gets 1 armor
        if(exp > eraExps[eraExps.Count - 1] * maxExpMult)
        {
            exp -= eraExps[eraExps.Count - 1] * (maxExpMult - 1);

            foreach (Base b in bases)
            {
                b.armor++;
            }
        }

        money += cash;

        
        //spawn coins
        Vector3 originCoins = bases[NIU.RandomR(0, bases.Count - 1)].transform.position;
        float coincounter = 0f;
        int i = 0;
        while (coincounter < cash)
        {
            coincounter += gm.coin.GetComponent<Coin>().moneyPerCoin * Mathf.Pow(3, era);

            GameObject coin = Instantiate(gm.coin, originCoins, transform.rotation, gm.dump.transform);
            coin.GetComponent<Coin>().team = number;

            if (gm.coin.GetComponent<Coin>().moneyPerCoin <= 0)
                break;

            if (i > 10)
                break;
            i++;
        }
    }



    public bool BuyUnit(int index, bool eraIgnore = false)
    {
        //Queue full
        if (buildQueue.Count >= maxBuildQueue)
            return false;

        //cancel if there's no unit to spawn
        if (index >= eraTypes.Count)
        {
            Debug.LogError(number + ": tried to spawn eraTypes of index " + index + " when eraTypes ends at " + eraTypes.Count);
            return false;
        }

        Unit unit = eraTypes[index].GetComponent<Unit>();

        //we're out of money
        if (money < actPrices[index])
        {
            //Debug.Log("poor " + number);
            return false;
        }

        //TURRETS: too many
        if (unit.turret && selBase.turrets >= selBase.turretSpots.Length)
            return false;



        //incur costs
        amountsPaid.Add(unit.cost * inflation);
        money -= actPrices[index];

        buildQueue.Add(unit);
        baseBuildSpawnQueue.Add(selBase);
        StartCoroutine("Build");
        return true;
    }

    
    /// <param name="absolute">Should 'i' be the ID of the tech (true), or just between 0-2 for the available researchable techs (false)?</param>
    public bool ResearchTech(int i, bool absolute = false)
    {
        //Cancel if we can't pick a tech right now
        if((!canPickTech && !absolute) || (!absolute && gm.matchState != GameMaster.MatchState.afterMatch))
        {
            return false;
        }

        //Picking one of the 3
        if (!absolute)
        {
            //out of range
            if (i >= afterMatchResearchableTechs.Count || i < 0)
            {
                Debug.LogWarning("i (" + i + ") is out of range on ResearchTech. (NOT absolute)", this);
                return false;
            }

            Tech tech = afterMatchResearchableTechs[i];
            //In range; add this tech to our list and re-shuffle the available techs
            tm.techs.Add(tech.ID);
            tech.Initialize();

            GetAfterMatchResearchableTechs();
            canPickTech = false;
        }

        //Picking the tech that has this ID
        else
        {
            if(!tm.possibleTechs.Contains(i))
            {
                Debug.LogWarning("i (" + i + ") is not in possibleTechs on ResearchTech. (YES absolute)", this);
                return false;
            }

            //In range; add this tech to our techs
            Tech tech = tm.techComps[i];

            tm.techs.Add(tech.ID);
            tech.Initialize();
        }

        
        return true;
    }


    public void ChangeEra(int e, bool changeExpToStart = false)
    {
        era = e;
        if (changeExpToStart)
        {
            if (era == 0)
                exp = 0;
            else
                exp = eraExps[era];
        }
        
        eraTypes.Clear();
        defPrices.Clear();
        actPrices.Clear();
        CancelBuildQueue();

        for(int i = 0; i < possibleUnitTypes.Count; i++)
        {
            if (uc.findUnit(possibleUnitTypes[i].GetComponent<Unit>().ID).era == era)
            {
                eraTypes.Add(possibleUnitTypes[i]);
                defPrices.Add(uc.findUnit(possibleUnitTypes[i].GetComponent<Unit>().ID).cost);
            }
        }

        foreach(Base b in bases)
        {
            b.SetEraMarkers(e);
        }
    }


    public void UpdateEra()
    {
        ChangeEra(era);
    }


    public void Win()
    {
        lostCurrentMatch = false;
        if(!Qk.Menus.skirmish)
            canPickTech = true;

        //tick up win count
        wins++;

        //Get a new list of techs to research
        GetAfterMatchResearchableTechs();

    }


    public void Lose()
    {
        foreach(Base b in bases)
        {
            b.hp = 0f;
        }

        lostCurrentMatch = true;
        canPickTech = false;

        List<Unit> hitlist = new List<Unit>();
        hitlist.AddRange(units);
        units.Clear();

        foreach (Unit u in hitlist)
        {
            u.Damage(u.hp + 100f);
        }

        losses++;
    }


    public void SelectBase(bool previous = false)
    {
        int index = -1;
        if(bases.Count == 0)
        {
            Debug.LogError("Trying to select the next/previous base when we have none? (next/prev)", gameObject);
            return;
        }
        //Next base
        if (!previous)
        {
            index = bases.FindIndex(w => w == selBase) + 1;
            //Wrap from top to bottom
            if (index >= bases.Count)
                index = 0;
        }
        //Previous base
        else
        {
            index = bases.FindIndex(w => w == selBase) - 1;
            //Wrap from bottom to top
            if (index < 0)
                index = bases.Count - 1;
        }

        //select the base at that index
        selBase = bases[index];
    }
    public void SelectBase(int index)
    {
        //Ignore if the index is negative
        if (index < 0)
            return;

        if (bases.Count == 0)
        {
            Debug.LogError("Trying to select the a base when we have none? (index)", gameObject);
            return;
        }


        //Go to a specific index
        index = index % (bases.Count);

        //select the base at that index
        selBase = bases[index];
    }

    private bool cancelBuild = false;
    private IEnumerator Build()
    {
        if (!readyToBuild)
            yield break;

        if(buildQueue.Count <= 0 || buildQueue[0] == null)
        {
            Debug.LogError("We tried to build a new unit, but the build queue is empty (count = " + buildQueue.Count + "), or the first" +
                "one is null?");
            yield break;
        }

        Base baseToSpawnAt = baseBuildSpawnQueue[0];
        float time = 0f;
        GameObject thingToMake = buildQueue[0].gameObject;
        float timeToBuild = thingToMake.GetComponent<Unit>().buildTime;
        readyToBuild = false;
        cancelBuild = false;

        //LOOP
        while (time < timeToBuild && !cancelBuild && buildQueue.Count != 0)
        {

            //making use of time
            //skip seconds if we take damage
            time += cBuildSkip;
            cBuildSkip = 0f;

            //wait for 1/12 of build time (to tick) 
            if (!lostCurrentMatch && baseToSpawnAt != null)
                baseToSpawnAt.transform.GetChild(0).RotateAround(baseToSpawnAt.transform.position, Vector3.up, 
                                                            (Time.deltaTime / timeToBuild) * 360f);
            else
                break;
            
            yield return new WaitForEndOfFrame();

            time += Time.deltaTime;
        }



        //DONE BUILDING```````

        //reset clock hand
        if(baseToSpawnAt != null)
            baseToSpawnAt.transform.GetChild(0).rotation = NIU.QuaternionAll(0f);

        //cancel
        if (cancelBuild || baseToSpawnAt == null || lostCurrentMatch || buildQueue.Count == 0 
            || (thingToMake.GetComponent<Unit>().turret && baseToSpawnAt.NextTurretSpot == -1))
        {
            readyToBuild = true;
            //addedBounties.RemoveAt(0);
            cancelBuild = false;
            yield break;
        }

        //cancel, but refund
        /*else if(buildQueue.Count == 0)
        {
            money += addedBounties[0];
            yield break;
        }*/



        GameObject created;
        Unit unit;
        //UNITS
        if (!thingToMake.GetComponent<Unit>().turret)
        {
            //create unit
            created = Instantiate(thingToMake, ((gm.centerOfField - baseToSpawnAt.transform.position).normalized * baseToSpawnAt.spawnDistance)
                                                              + baseToSpawnAt.transform.position + new Vector3(Random.value * 2, Random.value * 2, Random.value * 2),
                                      transform.rotation, transform);

            unit = created.GetComponent<Unit>();
            unit.bounty += amountsPaid[0];
        }

        //TURRETS
        else
        {


            //Create turret
            created = Instantiate(thingToMake, baseToSpawnAt.turretSpots[baseToSpawnAt.NextTurretSpot],
                                      transform.rotation, transform);
            unit = created.GetComponent<Unit>();
            baseToSpawnAt.turrets++;
            unit.turretBase = baseToSpawnAt;
        }

        //set unit's metadata
        unit.teamI = number;
        units.Add(unit);
        //initialize them
        unit.Initialize();
        //Boost the unit, if applicable
        if(canBoostStats)
        {
            unit.BoostStats(camp.BoostAmount);
        }
        //AI unit stuff
        if (!isPlayer)
        {
            NIU.FindChildNamed(unit, "Team Light", true).SetActive(false);
        }
        //Player unit stuff
        else
        {
            unit.BroadcastMessage("ChangeColorLight");
        }


        //Debug.Log("Spawning from base " + selBase.name + ", at " + selBase.transform.position);

        amountsPaid.RemoveAt(0);
        buildQueue.RemoveAt(0);
        baseBuildSpawnQueue.RemoveAt(0);

        readyToBuild = true;

        //Build the next one on the queue
        if (buildQueue.Count > 0)
            StartCoroutine("Build");

    }


    private IEnumerator CheckEmptiesInLists(float timeBetweenChecks = -1f)
    {
        
        
        //Default to 1 second between checks
        if (timeBetweenChecks <= 0)
            timeBetweenChecks = 1f;

        yield return new WaitForSeconds((1 / (float)gm.allTeams.Count) * number);

        while (true)
        {
            //DEBUG!!!!!!!!!!!!!!!!!!
            NIU.DebugDot(new Vector3(Time.time % 30f, 50f, 100f + ((exp / (Mathf.Max(eraExps.ToArray()))) * 10f)),
                gm.teamColors[number % gm.teamColors.Count], 0.1f, 30f);
            

            int killed = 0;
            killed += units.RemoveAll(w => w == null);
            killed += bases.RemoveAll(w => w == null);
            killed += turrets.RemoveAll(w => w == null);

            if(killed > 0)
            {
                Debug.Log("Team " + number + " eliminated " + killed + " empt" + NIU.PlurY(killed) + ".");
                gm.allUnits.RemoveAll(w => w == null);
                gm.allBases.RemoveAll(w => w == null);
            }

            yield return new WaitForSeconds(timeBetweenChecks);
        }
    }


    private void GetAfterMatchResearchableTechs()
    {
        afterMatchResearchableTechs = tm.ShuffleResearchDeck().GetRange(0, Mathf.Min(researchTechCount, tm.possibleTechs.Count));
    }


    public void BecomePlayer()
    {
        isPlayer = true;
        GetComponent<AIControls>()?.CheckIfPlayer();
    }


    


    public class EraSort : IComparer<GameObject>
    {
        public int Compare(GameObject x, GameObject y)
        {
            Unit xx = x.GetComponent<Unit>();
            Unit yy = y.GetComponent<Unit>();

            if(xx.era == yy.era)
            {
                return xx.cost.CompareTo(yy.cost);
            }

            // CompareTo() method 
            return xx.era.CompareTo(yy.era);

        }
    }


    public int CancelBuildQueue(int count = -1)
    {
        int amountRemoved = 0;
        //cancel it all if we're out of range
        if (!NIU.Within(count, 0, buildQueue.Count))
        {
            amountRemoved = buildQueue.Count;
            buildQueue.Clear();
            baseBuildSpawnQueue.Clear();
            money += NIU.Sum(amountsPaid);
            amountsPaid.Clear();

            //cancel a build
            cancelBuild = true;
        }

        //cancel only some
        else if(count > 0)
        {
            amountRemoved = count;
            buildQueue.RemoveRange(buildQueue.Count - count, count);
            baseBuildSpawnQueue.RemoveRange(baseBuildSpawnQueue.Count - count, count);
            money += NIU.Sum(amountsPaid.GetRange(amountsPaid.Count - count, count));
            amountsPaid.RemoveRange(amountsPaid.Count - count, count);
        }


        return amountRemoved;
    }


}
