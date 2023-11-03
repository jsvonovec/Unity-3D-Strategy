using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechManager : MonoBehaviour
{
    public TechManager factoryReset = null;
    //This script should execute BEFORE all the techs!!!!!!!!


    private GameMaster gm;
    private Team team;


    public List<int> techs;       //Tech scripts rely on THIS VARIABLE, not the other way around! Set this to enable techs.
    public List<int> possibleTechs
    {
        get
        {
            techComps.Sort(new TechSort());

            List<int> pt = new List<int>();

            //First, add EVERY ID...
            foreach(Tech tech in techComps)
            {
                if(TechIsPossibleToResearch(tech))
                    pt.Add(tech.ID);
            }
            /*
            //Then REMOVE ones that aren't applicable anymore
            foreach(int t in techs)
            {
                if (!techComps[t].repeatable)
                    pt.Remove(t);
            }
            */
            return pt;
        }
    }
    /*[HideInInspector]*/
    public List<Tech> techComps;


    // Awake is called before Start(), which all techs use.
    void Awake()
    {
        gm = Qk.GM;
        team = transform.parent.GetComponent<Team>();

        OnValidate();


        
    }

    private void Start()
    {
        team.UpdateEra();
    }


    private void OnValidate()
    {
        techComps = new List<Tech>();

        foreach (Tech c in GetComponents<Tech>())
        {
            if (!c.Equals(this))
            {
                techComps.Add(c);
            }
        }

        if (techs == null)
            techs = new List<int>();

        techs.Sort();

        foreach (Tech t in GetComponents<Tech>())
        {
            if (techs.Contains(t.ID))
            {
                t.active = true;
            }
            else
                t.active = false;   //TODO: For some reason, techs might not get activated as soon as they're researched?
        }
    }



    public List<Tech> researchableTechs
    {
        get
        {
            List<Tech> possibleTechsToResearch = new List<Tech>();

            foreach (Tech t in techComps)
                if (TechIsPossibleToResearch(t))
                    possibleTechsToResearch.Add(t);
            /*
            //Players can use all possible techs
            if (team.isPlayer)
            {
                possibleTechsToResearch.AddRange(techComps);
            }
            //AIs can't use all techs, only those that say AIs can use them
            else
            {
                foreach(Tech t in techComps)
                {
                    if (t.availableToAI)
                        possibleTechsToResearch.Add(t);
                }
            }

            //Unless the tech is repeatable, Don't make it available for research!
            foreach(Tech t in techComps)
            {
                if(!t.repeatable && techs.Contains(t.ID) && possibleTechsToResearch.Contains(t))
                {
                    possibleTechsToResearch.Remove(t);
                }
            }
            */
            return possibleTechsToResearch;
        }
}   //Between matches, these techs are available to have permanently. AIs can't research some, though

    //TODO: Update this function when you make different rarities/qualities of techs. Right now: it's all considered the same way
    public List<Tech> ShuffleResearchDeck()
    {


        return NIU.Shuffle(researchableTechs);

        
    }


    public bool StartMatchAll()
    {
        try
        {
            BroadcastMessage("StartOfMatch", SendMessageOptions.RequireReceiver);
        }
        catch (UnityException)
        {
            Debug.LogWarning("No techs found on this object. Is this intentional?", gameObject);
            return false;
        }

        return true;
    }


    public Tech IDToTech(int id)
    {
        return techComps[id];
    }


    private bool TechIsPossibleToResearch(Tech tech)
    {
        if (!tech.availableToAI)
            if (!team.isPlayer)
                return false;

        if (!tech.repeatable)
            if (techs.Contains(tech.ID))
                return false;


        if (!tech.available)
            return false;

        return true;

    }
}



public class TechSort : IComparer<Tech>
{
    public int Compare(Tech x, Tech y)
    {

        // CompareTo() method 
        return x.ID.CompareTo(y.ID);

    }
}