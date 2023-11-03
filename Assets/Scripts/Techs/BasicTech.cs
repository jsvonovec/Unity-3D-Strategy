using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTech : Tech
{
    ///This is supposed to be a technology that you research between games. Sometimes, the AI might be able to use it, too.

    //Set tech-specific info here, like name or description.
    private void OnValidate()
    {
        ID = 4;
        realName = "Basic Tech";
        desc = "If this is showing in the final game, Something is Wrong! ☺";
        availableToAI = true;
        repeatable = false;
        available = false;
        //enabled = active;     //Maybe have all techs enabled, but just don't use them if they're not active?

        team = transform.parent.GetComponent<Team>();
    }








    //This should only ever be called ONCE - when the tech is activated.
    private IEnumerator Startup()
    {
        //skip if we're inactive or we've already started up
        if (initialized || !active)
            yield break;


        OnValidate();
        Update();

        /*
         * WHEN TECH IS ACTIVATED CODE
         */

        initialized = true;
        yield break;
    }











    //This one should be called at the BEGINNING OF MATCHES.
    private IEnumerator StartOfMatch()
    {
        //skip if we're inactive. Startup if we haven't
        if (!active)
            yield break;
        if (!initialized)
            Startup();


        /*
         * BEGINNING OF MATCH
         */

        yield break;

    }







    //Put tech function here. Make sure this only looks at other scripts and doesn't edit them unless that's what the tech is for.
    private void Update()
    {
        if (!active)
            return;

        /*
          * EVERY FRAME
          */

    }
}
