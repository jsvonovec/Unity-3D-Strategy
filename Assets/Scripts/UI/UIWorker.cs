using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIWorker : MonoBehaviour
{
    GameObject player;
    public int teamInt = 0;
    public GameMaster.MatchState lastKnownMatchState = GameMaster.MatchState.not;
    Team team;
    GameMaster gm;
    TechManager tm;
    UnitCustomizer uc;
    public static UIWorker def = null;

    //~~~~~MATCH UI, DURING THE MATCH~~~~~~~~~~~~~
    GameObject matchUI;

    TextMeshProUGUI money;
    TextMeshProUGUI exp;
    TextMeshProUGUI unitCount;
    TextMeshProUGUI techsList;

    TextMeshProUGUI[] eraNums = new TextMeshProUGUI[6];
    RawImage filledExp;
    RawImage emptyExp;
    RawImage overExp;

    UnitUI[] units = new UnitUI[6];

    GameObject queueText;
    GameObject[] queues = new GameObject[6];
    RawImage[] queueFGs = new RawImage[6];
    TextMeshProUGUI[] queueNames = new TextMeshProUGUI[6];



    //~~~~BETWEEN MATCH UI~~~~~
    GameObject afterMatchUI;

    TextMeshProUGUI winText;
    TextMeshProUGUI lostText;
    TextMeshProUGUI matchWinStreak;
    TextMeshProUGUI winContinue;
    TextMeshProUGUI lostContinue;
    TextMeshProUGUI nextMap;
    List<TechUI> aTechs = new List<TechUI>();    //"availableTechs" - List because there's sometimes less than the max



    // Start is called before the first frame update
    void Start()
    {
        gm = Qk.GM;
        player = gameObject;
        //team = gm.allTeams[teamInt];
        //tm = NIU.FindChildNamed(team, "Techs", true).GetComponent<TechManager>();
        //uc = team.GetComponentInChildren<UnitCustomizer>();

        //Assign all UI elements to a variable

        //GET THE DIFFERENT UIS
        matchUI = NIU.FindChildNamed(this, "During Match UI", true);
        afterMatchUI = NIU.FindChildNamed(this, "Between Match UI", true); 


        //MIDMATCH UI!!!
        int j = 0;
        for(int i = 0; i < matchUI.transform.childCount; i++)
        {
            Transform t = matchUI.transform.GetChild(i);

            //UNIT CARDS
            if (t.name.Contains("Unit") && j < 6)
            {
                units[j] = new UnitUI()
                {
                    root = t.gameObject
                };

                //setting their variables
                for (int k = 0; k < t.childCount; k++)
                {
                    switch (t.GetChild(k).name)
                    {
                        case "TeamColor":
                            units[j].teamBG = t.GetChild(k).GetComponent<RawImage>();
                            break;

                        case "UnitColor":
                            units[j].unitBG = t.GetChild(k).GetComponent<RawImage>();
                            break;

                        case "Icon":
                            units[j].unitIcon = t.GetChild(k).GetComponent<Image>();
                            break;

                        case "Icon Coverer":
                            units[j].iconCoverer = t.GetChild(k).GetComponent<RawImage>();
                            break;

                        case "realName":
                            units[j].tName = t.GetChild(k).GetComponent<TextMeshProUGUI>();
                            break;

                        case "desc":
                            units[j].tDesc = t.GetChild(k).GetComponent<TextMeshProUGUI>();
                            break;

                        case "actPrice":
                            units[j].tActPrice = t.GetChild(k).GetComponent<TextMeshProUGUI>();
                            break;

                        case "defPrice":
                            units[j].tDefPrice = t.GetChild(k).GetComponent<TextMeshProUGUI>();
                            break;

                        case "hotkey":
                            units[j].tButton = t.GetChild(k).GetComponent<TextMeshProUGUI>();
                            break;
                    }
                }

                units[j].root.SetActive(false);

                j++;
            }

            //SIDEBAR (money, exp, unit count)
            else if (t.name == "Player Info")
            {
                for (int k = 0; k < t.childCount; k++)
                {
                    switch (t.GetChild(k).name)
                    {
                        case "money":
                            money = t.GetChild(k).GetComponent<TextMeshProUGUI>();
                            break;

                        case "exp":
                            exp = t.GetChild(k).GetComponent<TextMeshProUGUI>();
                            break;

                        case "unitcount":
                            unitCount = t.GetChild(k).GetComponent<TextMeshProUGUI>();
                            break;
                    }
                }
            }

            //ERA/EXP BAR
            else if (t.name == "EXP Bar")
            {
                int l = 0;
                for (int k = 0; k < t.childCount; k++)
                {
                    switch (t.GetChild(k).name)
                    {
                        case "filledexp":
                            filledExp = t.GetChild(k).GetComponent<RawImage>();
                            break;

                        case "overexp":
                            overExp = t.GetChild(k).GetComponent<RawImage>();
                            break;
                    }

                    if (t.GetChild(k).name.Contains("erat"))
                    {
                        eraNums[l] = t.GetChild(k).GetComponent<TextMeshProUGUI>();
                        l++;
                    }
                }
            }

            //TECHS RESEARCHED
            else if (t.name == "Techs")
            {
                for (int k = 0; k < t.childCount; k++)
                {
                    switch (t.GetChild(k).name)
                    {
                        case "Tech List":
                            techsList = t.GetChild(k).GetComponent<TextMeshProUGUI>();
                            break;
                    }
                }
            }

            //BUILD QUEUE
            else if (t.name == "Build Queue")
            {
                int l = 0;
                for (int k = 0; k < t.childCount; k++)
                {
                    Transform ours = t.GetChild(k);

                    if (ours.name != "Queue")
                    {
                        queues[l] = ours.gameObject;
                        queueFGs[l] = NIU.FindChildNamed(ours, "Foreground").GetComponent<RawImage>();
                        queueNames[l] = ours.GetComponentInChildren<TextMeshProUGUI>();
                        l++;
                    }
                    else
                        queueText = ours.gameObject;
                }
            }
        }



        //AFTERMATCH UI!!!!!!!!!!
        j = 0;
        for (int i = 0; i < afterMatchUI.transform.childCount; i++)
        {
            Transform t = afterMatchUI.transform.GetChild(i);

            //won text
            if(t.name == "Won")
            {
                winText = t.GetComponent<TextMeshProUGUI>();
            }
            //lost text
            else if (t.name == "Lost")
            {
                lostText = t.GetComponent<TextMeshProUGUI>();
            }
            //match streak
            else if (t.name == "MatchStreak")
            {
                matchWinStreak = t.GetComponent<TextMeshProUGUI>();
            }
            //lost, press space to continue text
            else if (t.name == "LostContinue")
            {
                lostContinue = t.GetComponent<TextMeshProUGUI>();
            }
            //lost, press space to continue text
            else if (t.name == "WonContinue")
            {
                winContinue = t.GetComponent<TextMeshProUGUI>();
            }
            //Next map text
            else if (t.name == "NextMap")
            {
                nextMap = t.GetComponent<TextMeshProUGUI>();
            }

            //AVAILABLE TECHS
            else if (t.name == "Researches")
            {
                for(int k = 0; k < t.childCount; k++)
                {
                    Transform tr = t.GetChild(k);

                    TechUI tui = new TechUI()
                    {
                        root = tr.gameObject,
                        tButton = NIU.FindChildNamed(tr, "Button").GetComponent<TextMeshProUGUI>(),
                        tName = NIU.FindChildNamed(tr, "TechName").GetComponent<TextMeshProUGUI>(),
                        tDesc = NIU.FindChildNamed(tr, "TechDesc").GetComponent<TextMeshProUGUI>(),
                        teamBG = NIU.FindChildNamed(tr, "ColoredSquare").GetComponent<RawImage>(),
                        innerBG = NIU.FindChildNamed(tr, "InnerSquare").GetComponent<RawImage>()
                    };

                    aTechs.Add(tui);
                }
            }
        }

            //StartCoroutine("PassiveUpdate");
    }









    // Update is called once per frame
    void Update()
    {


        //DEBUG
        {
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                teamInt = 0;
                UpdateStaticUI();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                teamInt = 1;
                UpdateStaticUI();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                teamInt = 2;
                UpdateStaticUI();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                teamInt = 3;
                UpdateStaticUI();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                teamInt = 4;
                UpdateStaticUI();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                teamInt = 5;
                UpdateStaticUI();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                teamInt = 6;
                UpdateStaticUI();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad7))
            {
                teamInt = 7;
                UpdateStaticUI();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                teamInt = 8;
                UpdateStaticUI();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad9))
            {
                teamInt = 9;
                UpdateStaticUI();
            }
        }




        //Changes of match state: update static UI elements
        if (lastKnownMatchState != gm.matchState)
        {
            UpdateStaticUI();

            lastKnownMatchState = gm.matchState;
        }



        if (teamInt < 0 || gm.matchTeams.Count == 0)
        {
            team = null;
            tm = null;
            uc = null;
            matchUI?.SetActive(false);
            afterMatchUI?.SetActive(false);
            return;
        }




        if (gm.matchState != GameMaster.MatchState.not)
        {
            

            team = gm.matchTeams[teamInt % gm.matchTeams.Count];
            tm = NIU.FindChildNamed(team, "Techs", true).GetComponent<TechManager>();
            uc = team.GetComponentInChildren<UnitCustomizer>();

        }

        //PREMATCH UI
        if (gm.matchState == GameMaster.MatchState.preMatch || gm.matchState == GameMaster.MatchState.not 
                || MatchMaker.current == null || MatchMaker.current.background)
        {
            matchUI?.SetActive(false);
            afterMatchUI?.SetActive(false);

        }

        //MIDMATCH UI
        else if(gm.matchState == GameMaster.MatchState.midMatch && !team.lostCurrentMatch)
        {
            matchUI.SetActive(true);
            //disable other UIs
            afterMatchUI.SetActive(false);
            


            //sidebar
            money.text = "§" + (Mathf.Round(team.money)).ToString() + " <" + (Mathf.Round(team.inflation * 100f) / 100f).ToString() + "x>";

            if (team.era >= team.eraExps.Count)    //max era
                exp.text = "@ " + Mathf.Round(team.exp).ToString() + " (";
            else        //any era other than max
                exp.text = "@ " + Mathf.Round(team.exp).ToString() + "/" + team.eraExps[team.era].ToString() + " (";
            if (team.era <= 0)   //first era
                exp.text += "0)";
            else         //any era other than first
                exp.text += team.eraExps[team.era - 1].ToString() + ")";


            unitCount.text = team.units.Count.ToString() + " // &" + team.turrets.Count.ToString();
            //AI confidence
            if (team.GetComponent<AIControls>() != null)
                unitCount.text += "\nC: " + team.GetComponent<AIControls>().Confidence;

            //era/exp bar
            if (team.era <= 0)
            {
                float scale;
                overExp.transform.localScale = new Vector3(0, 1, 1);
                scale = team.exp / team.eraExps[team.era];
                scale = scale / (team.eraExps.Count);

                filledExp.transform.localScale = new Vector3(scale, 1, 1);
            }
            else if (team.era < (team.eraExps.Count))
            {
                float scale;
                overExp.transform.localScale = new Vector3(0, 1, 1);
                scale = (team.exp - team.eraExps[team.era - 1]) / (team.eraExps[team.era] - team.eraExps[team.era - 1]);
                scale = scale / (team.eraExps.Count);
                scale += ((float)team.era / ((float)team.eraExps.Count));
                filledExp.transform.localScale = new Vector3(scale, 1, 1);
            }
            else
            {
                float scale;
                filledExp.transform.localScale = new Vector3(1, 1, 1);
                scale = (team.exp - team.eraExps[team.era - 1]) / (team.eraExps[team.era - 1] * (team.maxExpMult - 1f));

                overExp.transform.localScale = new Vector3(scale, 1, 1);
            }

            //Unit cards
            int i = 0;
            foreach (UnitUI u in units)
            {
                if (i < team.eraTypes.Count)
                {
                    //team.eraTypes[i].GetComponent<Unit>();
                    u.unit = uc.findUnit(team.eraTypes[i].GetComponent<Unit>().ID);
                    u.root.SetActive(true);
                    u.tName.text = u.unit.realName;
                    u.tDesc.text = "(" + u.unit.desc + ")";
                    u.tDefPrice.text = "(" + Mathf.Round(team.defPrices[i]).ToString() + ")";
                    u.tActPrice.text = Mathf.Round(team.actPrices[i]).ToString();
                    u.tButton.text = (i + 1).ToString();
                    u.unitBG.color = u.unit.sphereColor;
                    u.teamBG.color = gm.teamColors[team.number % gm.teamColors.Count];
                    u.iconCoverer.color = gm.teamColors[team.number % gm.teamColors.Count];
                    u.unitIcon.sprite = u.unit.icon;
                    u.unitIcon.color = u.unit.iconColor;


                    i++;
                }
                else
                {
                    u.root.SetActive(false);
                }
            }

            /*
            //Techs Researched list
            techsList.text = "";
            foreach(int id in tm.techs)
            {
                //List all techs in the techs list on tech manager
                techsList.text += tm.techComps.Find(w => w.ID == id).realName + "\n";
            }*/
            //^^^^^ NOW IN UpdateStaticUI() ^^^^


            //Build Queue
            List<Unit> bq = team.buildQueue;

            queueText.SetActive(bq.Count > 0);

            for (i = 0; i < bq.Count; i++)
            {
                Unit u = bq[i];

                queues[i].SetActive(true);
                queueFGs[i].color = u.sphereColor;
                queueNames[i].text = (team.eraUnits.FindIndex(w => w.realName == u.realName) + 1) + ". " + u.realName;
            }
            while(i < 6)
            {
                queues[i].SetActive(false);
                i++;
                if(i > 100)
                {
                    Debug.LogError("messed up, i > 100");
                    break;
                }
            }
        }



        
        //UPDATE AFTERMATCH UI
        else if(gm.matchState == GameMaster.MatchState.afterMatch || team.lostCurrentMatch)
        {
            afterMatchUI.SetActive(true);
            //disable other UIs
            matchUI.SetActive(false);


            //UI elements that are enabled whether or not we won
            //match streak
            if (team.wins != 0)
            {
                matchWinStreak.gameObject.SetActive(true);
                matchWinStreak.text = "Matches Won: " + team.wins;
            }
            else
                matchWinStreak.gameObject.SetActive(false);



            //WE WON :):):):):):):):):):)
            if (!team.lostCurrentMatch)
            {

                winText.gameObject.SetActive(true);
                lostText.gameObject.SetActive(false);
                lostContinue.gameObject.SetActive(false);

                //Before we pick which tech to research
                if (team.canPickTech)
                {
                    //UPDATING TECH CARDS
                    for(int i = 0; i < team.afterMatchResearchableTechs.Count; i++)
                    {
                        TechUI tui = aTechs[i];
                        Tech tech = team.afterMatchResearchableTechs[i];

                        tui.root.SetActive(true);

                        tui.tName.text = tech.realName;
                        tui.tDesc.text = tech.desc;
                        tui.tButton.text = (i + 1).ToString();
                        //tui.innerBG.color = ???????
                        tui.teamBG.color = gm.teamColors[team.number % gm.teamColors.Count];

                    }


                    winContinue.gameObject.SetActive(false);
                    nextMap.gameObject.SetActive(false);
                }
                //After we pick a tech
                else
                {
                    foreach (TechUI tui in aTechs)
                        tui.root.SetActive(false);
                    winContinue.gameObject.SetActive(true);
                    nextMap.gameObject.SetActive(true);
                    nextMap.text = "Next Map: " + gm.nextMap.realName + "\n---> ---> ---> --->";
                }

            }
            //WE LOST!!!!!!!!!
            else
            {

                winText.gameObject.SetActive(false);
                winContinue.gameObject.SetActive(false);
                lostText.gameObject.SetActive(true);
                lostContinue.gameObject.SetActive(true);
                foreach (TechUI tui in aTechs)
                    tui.root.SetActive(false);
                nextMap.gameObject.SetActive(false);

            }

        }







    }


    public void UpdateStaticUI()
    {
        //print("Updating Static UI.");

        //Update our variables
        if (gm.matchState != GameMaster.MatchState.not && teamInt != -1 
                                                       && NIU.Within(teamInt, 0, gm.matchTeams.Count - 1))
        {
            team = gm.matchTeams[teamInt % gm.matchTeams.Count];
            tm = NIU.FindChildNamed(team, "Techs", true).GetComponent<TechManager>();

        }

        if (tm != null)
        {
            //Techs Researched list
            techsList.text = "";
            foreach (int id in tm.techs)
            {
                //List all techs in the techs list on tech manager
                techsList.text += tm.techComps.Find(w => w.ID == id).realName + "\n";
            }

        }
    }




    class UnitUI
    {
        public string name = default;
        public string desc = default;
        public float defPrice = default;
        public float actPrice = default;
        public char button = default;
        public Color unitColor = default;
        public Color teamColor = default;
        public Image unitIcon = default;
        public Color iconColor = default;

        public TextMeshProUGUI tName = default;
        public TextMeshProUGUI tDesc = default;
        public TextMeshProUGUI tDefPrice = default;
        public TextMeshProUGUI tActPrice = default;
        public TextMeshProUGUI tButton = default;
        public RawImage unitBG = default;
        public RawImage teamBG = default;
        public RawImage iconCoverer = default;

        public GameObject root = default;
        public Unit unit = default;
    }


    class TechUI
    {
        public Color teamColor = default;

        public TextMeshProUGUI tName = default;
        public TextMeshProUGUI tDesc = default;
        public TextMeshProUGUI tButton = default;
        public RawImage innerBG = default;
        public RawImage teamBG = default;

        public GameObject root = default;
        public Tech tech = default;
    }

}
