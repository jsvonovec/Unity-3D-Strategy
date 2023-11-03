using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIControls : MonoBehaviour
{

    private float prevMoneyChecked = -420;
    public float Confidence
    {
        get
        {
            //int enemies = gm.allUnits.Count - tm.units.Count;   //How many enemies are on the board?
            float friendlyUnitPercentage = 0f;    
            if(gm.allUnits.Count != 0) friendlyUnitPercentage = (float)team.units.Count / gm.allUnits.Count; //What percent of total units on the board are ours?
            float boardControl = friendlyUnitPercentage * gm.matchTeams.Count;       //How well we're doing with units. 1 = average, 0 = none
            float averageEnemyEra = 0f;
            foreach(Team t in gm.matchTeams)
            {
                if(t != team)
                {
                    averageEnemyEra += t.era + 1;
                }
            }
            averageEnemyEra /= gm.matchTeams.Count - 1;
            float eraAdvantage = Mathf.Pow((team.era+1) / (averageEnemyEra), 2);  //How far behind are we in eras?
            //Debug.Log("iugheiuhrge" + boardControl + " ~~~~~~~ " + tm.units.Count + " / " + gm.allUnits.Count);
            return boardControl * eraAdvantage;        //TODO: Make this equation a little better. Board control is fine for now though
        }
    }
    public float cccccc;

    private List<float> prices;

    private GameMaster gm;
    public Team team;

    // Start is called before the first frame update
    void Start()
    {
        if (team != null) return;

        gm = Qk.GM;
        team = GetComponent<Team>();

        prices = new List<float>();

        //delete this if it's a player
        CheckIfPlayer();

        EraChange();


        StartCoroutine(UpdateTick());
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void ActMoney()
    {
        if (team.lostCurrentMatch || team.bases.Count == 0)
        {
            team.lostCurrentMatch = true;
            return;
        }

        float confidence = Confidence;

        //If we're unconfident (low # of units or are eras behind), buy a unit immediately
        if (confidence < 1)
        {
            BuyUnit(unitArchetype.damageRandom);
            Debug.DrawLine(team.selBase.transform.position, team.selBase.transform.position + (Vector3.up * 1), Color.grey);
        }

        //If we're pretty confident (doing better than average), save a little and buy stronger units
        else if (confidence <= gm.matchTeams.Count * 0.75f)
        {
            //buy a mid-tier unit
            if((team.inflation < 2.5f && team.money >= team.actPrices[1]) || team.money > 3 * team.actPrices[2])
            {
                BuyUnit(unitArchetype.strongAndSupport);
                Debug.DrawLine(team.selBase.transform.position, team.selBase.transform.position + (Vector3.up * 4), Color.yellow);
            }
        }

        //If we're at MAX CONFIDENCE, buy tanks and stuff
        else
        {
            BuyUnit(unitArchetype.finisher);
            Debug.DrawLine(team.selBase.transform.position, team.selBase.transform.position + (Vector3.up * 7), Color.red);
        }
        
        



    }

    //Buy a unit depending on what we want to buy
    enum unitArchetype { random, damageRandom, finisher, safe, strong, support, strongAndSupport }
    private bool BuyUnit(unitArchetype type = unitArchetype.damageRandom)
    {
        int r = NIU.RandomR(0, team.eraTypes.Count - 1);

        switch (type)
        {
            //RANDOM
            case unitArchetype.random:
                return team.BuyUnit(r);


                //Random but only damage (no healers)
            case unitArchetype.damageRandom:
                {
                    //find someone who does damage
                    int i = 0;
                    while (team.eraUnits[r].damage <= 0 && team.actPrices[r] < team.money && i < 13)
                    {
                        r = Mathf.Clamp(Mathf.RoundToInt(Random.value * team.eraTypes.Count - 0.5f), 0, team.eraTypes.Count - 1);
                        i++;
                    }

                    //If we have a fine number of healers, just do random.
                    return team.BuyUnit(r);
                }


                //Buy strongest unit possible that isn't the first one
            case unitArchetype.strong:
                {
                    for (int i = team.eraTypes.Count - 1; i > 0; i--)
                    {
                        if (team.actPrices[i] <= team.money)
                        {
                            return team.BuyUnit(i);
                        }
                    }

                    return false;
                }

                //Buy unit that goes for bases
            case unitArchetype.finisher:
                {
                    int best = -1;
                    float highestCost = 0f;
                    for(int i = 0; i < team.eraTypes.Count; i++)
                    {
                        if(team.eraTypes[i].GetComponent<Unit>().targPriority == Unit.priority.@base && highestCost < team.actPrices[i])
                        {
                            best = i;
                            highestCost = team.actPrices[i];
                        }
                    }

                    //If nothing goes for bases, just build the most expensive one.
                    return team.BuyUnit(team.eraTypes.Count - 1);
                }

                //Healers, turrets and non-targetables
            case unitArchetype.support:
                {
                    //If there's nothing good, randomly select
                    int bestIndex = r;
                    //Find non-targetables, healers, or turrets
                    for (int i = 0; i < team.eraTypes.Count; i++)
                    {
                        Unit unit = team.eraUnits[i];
                        if (!unit.targetable || unit.targPriority == Unit.priority.tLow || unit.turret)
                            bestIndex = i;
                    }

                    return team.BuyUnit(bestIndex);
                }

                //50/50 chance of support or strong
            case unitArchetype.strongAndSupport:
                {
                    if (Random.value < 0.5f)
                        return BuyUnit(unitArchetype.strong);
                    else
                        return BuyUnit(unitArchetype.support);
                }
        }


        return false;
    }



    //When we change eras
    private void EraChange()
    {
        prices.Clear();
        foreach (GameObject go in team.eraTypes)
        {
            Unit u = go.GetComponent<Unit>();
            prices.Add(u.cost);
        }
    }


    //find the best base to spawn units from
    private int FindBaseMostInDanger()
    {
        //No bases
        if(team.bases.Count <= 0)
        {
            //Debug.LogWarning("Tried to select a base when we have none? (AIControls)", this);
            return -1;
        }
        //Exactly one base, no choice
        if(team.bases.Count == 1)
        {
            return 0;
        }
        //No units to worry about
        if(team.units.Count >= gm.allUnits.Count)
        {
            if (team.units.Count > gm.allUnits.Count)
                Debug.LogError("Somehow we have more units than GameMaster knows about? team says " + team.units.Count + ", gm says " + gm.allUnits.Count, team.gameObject);

            return -1;
        }


        //Find the base that has an enemy closest to it
        float closestDistance = -1f;
        int mostInDangerBaseIndex = -1;
        for(int i = 0; i < team.bases.Count; i++)
        {
            Base b = team.bases[i];

            //Each unit has its distance compared
            foreach(Unit u in gm.allUnits)
                if(u.team.number != team.number)
                {
                    float distance = Vector3.Distance(b.transform.position, u.transform.position);
                    
                    if(distance <= closestDistance || closestDistance == -1f)
                    {
                        //If this unit is the closest out of all of them, then the base we're comparing it to is the most in danger
                        mostInDangerBaseIndex = i;
                        closestDistance = distance;
                    }
                }
        }
        //Set team.selBase in a different function
        return mostInDangerBaseIndex;
    }



    [SerializeField] private float tickTime = 0.25f;
    IEnumerator UpdateTick()
    {
        while (true)
        {

            //DEBUG
            cccccc = Confidence;

            //MIDMATCH (and haven't lost)
            if (!team.lostCurrentMatch && gm.matchState == GameMaster.MatchState.midMatch)
            {
                //Select base that's most in danger
                team.SelectBase(FindBaseMostInDanger());


                //Act if you got new money
                if (prevMoneyChecked != team.money && !team.lostCurrentMatch)
                {
                    prevMoneyChecked = team.money;
                    ActMoney();
                }



                //DEBUG

                #region Debug drawn lines into sky to signify confidence level
                if (team.inThisMatch && Confidence < 1 && !team.lostCurrentMatch)
                {
                    Debug.DrawLine(team.selBase.transform.position, team.selBase.transform.position + (Vector3.up * Confidence * 5), Color.cyan, tickTime);
                }

                //If we're pretty confident (doing better than average), save a little and buy stronger units
                else if (team.inThisMatch && Confidence <= gm.matchTeams.Count * 0.75f && !team.lostCurrentMatch)
                {
                    Debug.DrawLine(team.selBase.transform.position, team.selBase.transform.position + (Vector3.up * Confidence * 5), Color.yellow, tickTime);

                }

                //If we're at MAX CONFIDENCE, buy tanks and stuff
                else if (team.inThisMatch && !team.lostCurrentMatch)
                {
                    Debug.DrawLine(team.selBase.transform.position, team.selBase.transform.position + (Vector3.up * Confidence * 5), Color.red, tickTime);
                }
                #endregion

            }

            //AFTERMATCH
            else if (gm.matchState == GameMaster.MatchState.afterMatch)
            {
                //research a tech
                if (team.canPickTech)
                    team.ResearchTech(NIU.RandomR(0, team.afterMatchResearchableTechs.Count - 1));
            }


            yield return new WaitForSeconds(tickTime);

        }
    }


    public bool CheckIfPlayer()
    {
        if (team == null)
            Start();
        if (Qk.Player != null && team.number == Qk.Player.teamInt)
        {
            Destroy(this);
            return true;
        }
        return false;
    }
}
