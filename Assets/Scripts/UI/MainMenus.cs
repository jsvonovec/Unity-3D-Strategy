using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenus : MonoBehaviour
{
    private PlayerControls p;
    private GameMaster gm;
    public static MainMenus def = null;    //This is the one MainMenus in the scene. Set in Qk

    public List<MenuScreen> menuScreens;
    public MenuScreen startingScreen;

    public bool skirmish = false;
    public float skirmishTechBoostChance = 0.6f;
    public Toggle[] skirmishDifficultyButtons = new Toggle[4];
    public bool skirmishTechBoostAI = true;
    public bool skirmishTechBoostPlayer = true;
    public bool skirmishStatBoostAI = true;
    public bool skirmishStatBoostPlayer = false;
    public int nextMatchTeamCount = 2;
    public bool includePlayer
    {
        get
        {
            return !includePlayerToggle.isOn;
        }
    }
    public Toggle includePlayerToggle;


    void Awake()
    {
        gm = Qk.GM;
        p = Qk.Player;

        menuScreens.Clear();
        //Add every MenuScreen that is a child of this
        MenuScreen comp;
        foreach(GameObject go in NIU.EveryChildOf(gameObject))
        {
            if(go.TryGetComponent(out comp))
            {
                menuScreens.Add(comp);
            }
        }


        startingScreen.Open();
    }

    private void Update()
    {
        if (startingScreen.open && Input.anyKey)
            startingScreen.elements.Find(w => w.name == "Button").GetComponent<Button>().onClick.Invoke();
    }

    public int CloseAll()
    {
        int amount = 0;
        foreach(MenuScreen screen in menuScreens)
        {
            if (screen.open)
            {
                amount++;
                screen.Close();
            }
        }

        return amount;
    }

    public bool Open(MenuScreen screen)
    {
        if (screen.open)
            return false;

        screen.Open();
        return true;
    }
    public bool Open(GameObject screen)
    {
        MenuScreen menuscreen = menuScreens.Find(w => w.elements.Contains(screen));

        if (menuscreen == null)
            if (!screen.TryGetComponent(out menuscreen))
                return false;

        menuscreen.Open();
        return true;
    }


    public void STARTPREMATCH()
    {
        if(!MatchMaker.current.background)
            CloseAll();
    }


    //Changes the player's team number to 0 and deletes the AI control on that team if Player's team is invalid.
    /*public void ForcePlayerToPlay()
    {
        //Force the player to have a team if they don't already
        if(!NIU.Within(p.teamInt, 0, gm.allTeams.Count))
        {
            if(gm.allTeams.Count <= 0)
            {
                Debug.LogError("We don't have any teams at all????????");
                return;
            }
            Debug.Log("Player was not participating and clicked Skirmish - putting them on Team 0.");
            p.ChangeTeam(0);
        }
    }*/


    public void SetSkirmish(bool value)
    {
        skirmish = value;
    }
    /// <summary>
    /// Update our internal values for what the difficulty buttons represent
    /// </summary>
    public void CheckSkirmishButtons()
    {
        skirmishTechBoostAI = skirmishDifficultyButtons[0].isOn;
        skirmishTechBoostPlayer = skirmishDifficultyButtons[1].isOn;
        skirmishStatBoostAI = skirmishDifficultyButtons[2].isOn;
        skirmishStatBoostPlayer = skirmishDifficultyButtons[3].isOn;
    }
    public void SetTeamCount(Slider slider)
    {
        nextMatchTeamCount = (int)slider.value;
        //print("Team count is set to " + nextMatchTeamCount.ToString() + "!");
    }



    public void StartSkirmish()
    {
        if(!skirmish)
        {
            Debug.LogWarning("Just tried to start a skirmish even though skirmish is set to false?");
            return;
        }
        if (!includePlayer)
            Qk.Player.teamInt = PlayerControls.notPlayingInt;
        else
            PlayerControls.def.teamInt = PlayerControls.playingInt;

        Qk.Campaign.DestroyAndReplaceTeams(nextMatchTeamCount, true);

        Match match = MatchMaker.CreateNewMatch(nextMatchTeamCount, true, includePlayer: includePlayer);
        match.StartMatch();


        //Give Techs to certain teams (or all, or none)
        if (skirmishTechBoostAI || (skirmishTechBoostAI && !includePlayer))
            Campaign.GiveTeamsTechs(gm.matchTeams, skirmishTechBoostPlayer, Qk.Campaign.startDiff, skirmishTechBoostChance);
        else if (skirmishTechBoostPlayer)
                Campaign.GiveTeamTechs(Qk.Player.team, true, techChance: skirmishTechBoostChance);


        //Give Stat Boosts to certain teams
        if(skirmishStatBoostAI || skirmishStatBoostPlayer)
            foreach(Team t in gm.matchTeams)
            {
                if (skirmishStatBoostPlayer && t.isPlayer)
                {
                    t.canBoostStats = true;
                    if (!skirmishStatBoostAI)
                        break;

                    continue;
                }

                if (skirmishStatBoostAI && !t.isPlayer)
                {
                    t.canBoostStats = true;

                    continue;
                }

                t.canBoostStats = false;
            }
    }


    public void StartCampaign()
    {
        if (skirmish)
        {
            Debug.LogWarning("Just tried to start a campaign even though skirmish is set to true?");
            return;
        }

        Qk.Player.teamInt = PlayerControls.playingInt;

        Qk.Campaign.LoadNextMatch();
    }

    //Used in Inspector for sliders
    public void SetDifficulty(Slider slider)
    {
        Qk.Campaign.startDiff = slider.value;
    }

    public void SetDifficulty(float value)
    {
        Qk.Campaign.startDiff = value;
    }
}
