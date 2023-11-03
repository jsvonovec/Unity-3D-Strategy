using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tech : MonoBehaviour
{
    //THIS IS A BASE CLASS FOR ALL THE OTHER TECHS.

    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!DON'T COPY THIS ONE, COPY BASICTECH!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

    //Basic info about a tech that applies to all of them.
    public bool active = false;
    [Space]
    public int ID;
    public string realName;
    public string desc;
    public bool available = true;
    public bool availableToAI;  //Is the AI allowed to research this tech?
    public bool repeatable = false; //Can we have multiple of the same tech at once?
    [Space]
    public Team team;
    public bool initialized = false;

    //MonoBehavior functions don't work in this base function.


    public bool Initialize()
    {
        active = true;

        if (initialized)
            return false;

        StartCoroutine("Startup");

        initialized = true;
        return true;
    }



    public bool StartMatch()
    {
        if (!initialized || !active)
            return false;

        StartCoroutine("StartOfMatch");

        initialized = true;
        return true;
    }
}
