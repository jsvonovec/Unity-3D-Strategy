using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitCustomizer : MonoBehaviour
{
    public UnitCustomizer factoryReset = null;


    public List<Unit> mods;

    /*  The way this mods list is supposed to work is as follows: 
     *      - We keep track of which units are being modified
     *      - When we have a unit that wants to change, we create a dummy Unit COMPONENT attached to the gameobject that has their stats
     *      - add that unit's ID to IdsOfModdedUnits
     *      - Finally, if that unit's ID is in the list of modded IDs, then Team will use the modified Unit component instead of default.
     */

    public List<string> baseUnitJsons;
    public List<string> unitJsons;


    Team team = null;
    GameMaster gm
    {
        get
        {
            return Qk.GM;
        }
    }

    public List<int> IdsOfModdedUnits
    {
        get
        {
            List<int> ids = new List<int>();

            foreach(Unit u in mods)
            {
                ids.Add(u.ID);
            }

            return ids;
        }
    }   //IDs of all modded units for easy searching
    private bool initialized = false;

    // Start is called before the first frame update
    void Start()
    {


        if (!initialized)
            Startup();
    }




    public void Startup()
    {
        mods = new List<Unit>();
        team = GetComponent<Team>();

        initialized = true;
    }



    /// <summary>
    /// Create a new custom unit based off of the given ID.
    /// </summary>
    /// <param name="id">The ID of the unit.</param>
    /// <returns></returns>
    public Unit EnableUnitCustomizationOn(int id)
    {
        mods.Sort(new IDSortUnit());

        //When we already have this ID listed as 'modified'
        if (IdsOfModdedUnits.Contains(id))
        {
            //Debug.Log("We already have the CustomizedUnit ID " + id.ToString() + ".", mods.Find(w => w.ID == id));

            if (mods.Find(w => w.ID == id) == null)
            {
                Debug.LogError("We say we're modifying ID " + id.ToString() + ", but that ID isn't in the mods list.");
                return null;
            }
            else
                return mods.Find(w => w.ID == id);
        }




        //New addition
        //Add a component to this object based on the Unit component for the given ID
        Unit newDummy = gameObject.AddComponent<Unit>();
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(gm.everyPossibleUnitInGame[id].GetComponent<Unit>()), newDummy);

        //Set appropriate values for dummy components
        newDummy.custom = true;

        mods.Add(newDummy);
        
        return newDummy;
    }
    /// <summary>
    /// Create a new custom unit based off of the given name.
    /// </summary>
    /// <param name="realName">The name of the unit.</param>
    /// <returns></returns>
    public Unit EnableUnitCustomizationOn(string realName)
    {
        IdsOfModdedUnits.Sort();
        mods.Sort(new IDSortUnit());

        int id = mods.Find(w => w.realName == realName).ID;

        //When we already have this ID listed as 'modified'
        if (IdsOfModdedUnits.Contains(id))
        {
            Debug.LogWarning("We already have the CustomizedUnit ID " + id.ToString() + ".", mods.Find(w => w.ID == id));

            if (mods.Find(w => w.ID == id) == null)
            {
                Debug.LogError("We say we're modifying ID " + id.ToString() + ", but that ID isn't in the mods list.");
                return null;
            }
            else
                return mods.Find(w => w.ID == id);
        }




        //New addition
        //Add a component to this object based on the Unit component for the given ID
        Unit newDummy = gameObject.AddComponent<Unit>();
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(gm.everyPossibleUnitInGame[id].GetComponent<Unit>()), newDummy);

        //Set appropriate values for dummy components
        newDummy.custom = true;

        mods.Add(newDummy);

        return newDummy;
    }



    //Find the final Unit component that will be in the game.
    public Unit findUnit(int id)
    {
        //If we're trying to use this function before we've even begun, start her up
        if (!initialized)
            Startup();

        if (IdsOfModdedUnits.Contains(id))
        {
            return mods.Find(w => w.ID == id);
        }
        return gm.everyPossibleUnitInGame[id].GetComponent<Unit>();
    }
}





public class IDSortGO : IComparer<GameObject>
{
    public int Compare(GameObject x, GameObject y)
    {
        Unit xx = x.GetComponent<Unit>();
        Unit yy = y.GetComponent<Unit>();
        
        // CompareTo() method 
        return xx.ID.CompareTo(yy.ID);

    }
}


public class IDSortUnit : IComparer<Unit>
{
    public int Compare(Unit x, Unit y)
    {
        // CompareTo() method 
        return x.ID.CompareTo(y.ID);

    }
}




