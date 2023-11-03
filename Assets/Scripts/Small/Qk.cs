using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using System.Reflection;

public static class Qk
{


    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~THIS PROJECT ONLY~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    /// <summary>
    /// Finds and returns the GameMaster script in the scene. Throws an error if there isn't exactly one GameMaster.
    /// </summary>
    /// <returns></returns>
    public static GameMaster GM
    {
        get
        {
            //Return the one gameMaster in the scene if we've already found it
            if (GameMaster.def != null)
                return GameMaster.def;

            GameMaster[] gm = Object.FindObjectsOfType<GameMaster>();

            if (gm.Length < 1)
            {
                Debug.LogError("ERROR: Tried to find a GameMaster, but there are none active in the scene.");
                return null;
            }

            else if (gm.Length > 1)
            {
                Debug.LogError("ERROR: There is more than one GameMaster in the scene.", gm[NIU.RandomR(0, gm.Length)]);
            }

            GameMaster.def = gm[0];
            return gm[0];
        }
    }


    /// <summary>
    /// Finds and returns the PlayerControls script in the scene. Throws an error if there isn't exactly one PlayerControls.
    /// </summary>
    /// <returns></returns>
    public static PlayerControls Player
    {
        get
        {
            //Return the default playerControls if we've already found it
            if (PlayerControls.def != null)
                return PlayerControls.def;

            PlayerControls[] pl = Object.FindObjectsOfType<PlayerControls>();

            if (pl.Length < 1)
            {
                Debug.LogError("ERROR: Tried to find a PlayerControls, but there are none active in the scene.");
                return null;
            }

            else if (pl.Length > 1)
            {
                Debug.LogError("ERROR: There is more than one PlayerControls in the scene.", pl[NIU.RandomR(0, pl.Length)]);
            }

            PlayerControls.def = pl[0];
            return pl[0];

        }
    }

    /// <summary>
    /// Finds and returns the MainMenus script in the scene. Throws an error if there isn't exactly one MainMenus.
    /// </summary>
    /// <returns></returns>
    public static MainMenus Menus
    {
        get
        {
            //Return the MainMenus default if we've already found it
            if (MainMenus.def != null)
                return MainMenus.def;

            MainMenus[] mm = Object.FindObjectsOfType<MainMenus>();

            if (mm.Length < 1)
            {
                Debug.LogError("ERROR: Tried to find a MainMenus, but there are none active in the scene.");
                return null;
            }

            else if (mm.Length > 1)
            {
                Debug.LogError("ERROR: There is more than one MainMenus in the scene.", mm[NIU.RandomR(0, mm.Length)]);
            }

            MainMenus.def = mm[0];
            return mm[0];

        }
    }



    public static Campaign Campaign
    {
        get
        {
            //Return the campaign default if we've already found it
            if (Campaign.def != null)
                return Campaign.def;

            Campaign[] cc = Object.FindObjectsOfType<Campaign>();

            if (cc.Length < 1)
            {
                Debug.LogError("ERROR: Tried to find a Campaign, but there are none active in the scene.");
                return null;
            }

            else if (cc.Length > 1)
            {
                Debug.LogError("ERROR: There is more than one Campaign in the scene.", cc[NIU.RandomR(0, cc.Length)]);
            }

            Campaign.def = cc[0];
            return cc[0];

        }
    }



    public static UIWorker UIWorker
    {
        get
        {
            //Return the UIWorker default if we've already found it
            if (UIWorker.def != null)
                return UIWorker.def;

            UIWorker[] ui = Object.FindObjectsOfType<UIWorker>();

            if (ui.Length < 1)
            {
                Debug.LogError("ERROR: Tried to find a UIWorker, but there are none active in the scene.");
                return null;
            }

            else if (ui.Length > 1)
            {
                Debug.LogError("ERROR: There is more than one UIWorker in the scene.", ui[NIU.RandomR(0, ui.Length)]);
            }

            UIWorker.def = ui[0];
            return ui[0];

        }
    }
}
