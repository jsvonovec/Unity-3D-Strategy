using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScreen : MonoBehaviour
{
    public List<GameObject> elements = new List<GameObject>();
    public bool resetParameters = false;

    private bool rawBoolOpen = true;
    public bool open
    {
        get
        {
            return rawBoolOpen;
        }
        set
        {
            //open if we're closed
            if (value == true && !rawBoolOpen)
            {
                Open();
            }
            //Close if we're open
            else if (value == false && rawBoolOpen)
            {
                Close();
            }

            rawBoolOpen = value;
        }
    }

    public MenuScreen(List<GameObject> elems)
    {
        elements = elems;
    }

    public MenuScreen() { }


    public void Open()
    {
        Qk.Menus.CloseAll();
        rawBoolOpen = true;
        gameObject.SetActive(true);

        //Reset menu options if resetParameters is true
        if (resetParameters)
        {
            MainMenus.def.SetDifficulty(0f);
            MainMenus.def.CheckSkirmishButtons();
        }
        //print("Opening this menu: " + name + ". Active: " + isActiveAndEnabled + ". rawBoolOpen = " + rawBoolOpen);
    }

    public void Close()
    {
        rawBoolOpen = false;

        gameObject.SetActive(false);
    }
}
