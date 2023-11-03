using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{

    public int teamInt = 0;
    public static readonly int playingInt = 0;
    public static readonly int notPlayingInt = -1;

    public static bool careAboutPlayerControls = true;
    private bool[] inUnit = new bool[6];
    public bool nextBase = false;
    public bool prevBase = false;
    private float turnInput = 0f;
    private float zoomInput = 0f;
    public float cancelBuildInput = 0f;
    public bool cancelBuildButtonUp = false;
    private bool alreadyCancelled = false;

    [Header("Camera")]
    public float cameraTurnSpeed = 120f;    //degrees/sec at full speed
    public float cameraZoomSpeed = 50f;     //units/sec at full speed
    public float defaultDist = 80f;
    public float maxDist = 90f;         //default dist: 80f
    public float minDist = 20f;
    public float angle = 35f;       //defualt is 35 ish
    public float menuCameraTurnSpeed = 0.3f;    //What % of full speed should it turn on the main menu?

    private GameMaster gm;
    public Team team;
    public static PlayerControls def = null;    //This is the one PlayerControls in the scene. Set in Qk
    private float lastAutoTurnSpeed = 0f;


    // Start is called before the first frame update
    void Start()
    {
        gm = Qk.GM;

        CheckTeam();

        transform.LookAt(gm.centerOfField);



    }

    // Update is called once per frame
    void Update()
    {


        //Only see camera/player inputs when we want it (ie. not during main menu)
        if (careAboutPlayerControls)
        {
            //Get inputs for each purchasable unit
            for (int i = 0; i < inUnit.Length; i++)
            {
                inUnit[i] = Input.GetButtonDown("Unit " + (i + 1).ToString());
            }

            //Camera control
            turnInput = Input.GetAxis("Turn Camera");
            zoomInput = Input.GetAxis("Zoom Camera");

            //Base selection
            nextBase = Input.GetButtonDown("Next Base");
            prevBase = Input.GetButtonDown("Previous Base");

            //cancel build queue
            cancelBuildInput = Input.GetAxis("Cancel Build Queue");
            cancelBuildButtonUp = Input.GetButtonDown("Cancel Build Queue");

            //only do this when we are on a team (not a spectator)
            if (team != null)
            {







                //MIDMATCH
                if(gm.matchState == GameMaster.MatchState.midMatch && !team.lostCurrentMatch)
                {


                    //spawning units
                    for (int i = 0; i < team.eraTypes.Count; i++)
                    {
                        //Spawn on button down
                        if (inUnit[i])
                        {
                            team.BuyUnit(i);
                        }
                    }


                    //switching bases
                    if (nextBase)
                    {
                        team.SelectBase(false);
                    }
                    if (prevBase)
                    {
                        team.SelectBase(true);
                    }



                    //cancelling builds
                    //cancelling all after holding
                    if (cancelBuildInput >= 1 && !alreadyCancelled)
                    {
                        team.CancelBuildQueue();
                        alreadyCancelled = true;
                    }
                    else if(cancelBuildInput < 1)
                        alreadyCancelled = false;


                    if (cancelBuildButtonUp && !alreadyCancelled)
                    {
                        team.CancelBuildQueue(1);
                    }
                }

                //AFTERMATCH
                else if(gm.matchState == GameMaster.MatchState.afterMatch)
                {

                    //research a tech
                    for (int i = 0; i < team.researchTechCount; i++)
                    {
                        if (inUnit[i])
                        {
                            team.ResearchTech(i, false);
                        }
                    }

                }


            

            }


            //Turning camera
            transform.RotateAround(gm.centerOfField, Vector3.up, turnInput * cameraTurnSpeed * Time.deltaTime);

            //zooming camera
            if(zoomInput > 0 && Vector3.Distance(transform.position, gm.centerOfField) > minDist)
                transform.position = Vector3.MoveTowards(transform.position, gm.centerOfField,
                                    Mathf.Min(Vector3.Distance(transform.position, gm.centerOfField) - minDist, zoomInput * Time.deltaTime * cameraZoomSpeed));

            else if (zoomInput < 0 && Vector3.Distance(transform.position, gm.centerOfField) < maxDist)
                transform.position = Vector3.MoveTowards(transform.position, gm.centerOfField,
                                    Mathf.Min(maxDist - Vector3.Distance(transform.position, gm.centerOfField), zoomInput * Time.deltaTime * cameraZoomSpeed));

        }

        else
        {

            //Slowly turn camera on main menu
            if (MatchMaker.current != null && MatchMaker.current.background)
            {
                lastAutoTurnSpeed = Mathf.Lerp(lastAutoTurnSpeed, menuCameraTurnSpeed, 0.025f);
                transform.RotateAround(gm.centerOfField, Vector3.up, lastAutoTurnSpeed * cameraTurnSpeed * Time.deltaTime);
            }
            else
            {
                lastAutoTurnSpeed = 0f;
            }
            
        }

    }



    public void STARTPREMATCH()
    {   
        transform.position = Vector3.back * defaultDist;
        transform.rotation = NIU.QuaternionAll();
        transform.RotateAround(Vector3.zero, Vector3.right, angle);
        if (!MatchMaker.current.background)
        {
            careAboutPlayerControls = true;
        }
        else
        {
            careAboutPlayerControls = false;
        }
    }


    public void CHECKBACKGROUND()
    {
        if (!MatchMaker.current.background)
        {
            careAboutPlayerControls = true;
        }
        else
        {
            careAboutPlayerControls = false;
        }
    }



    public void ChangeTeamColor(bool random = true, Color color = default)
    {

        if (playingInt >= gm.teamColors.Count)
        {
            Debug.LogWarning("We just tried to change our team's color when the player's playingInt is " + playingInt.ToString());
            return;
        }
        if (random)
        {
            //Find a random color that's not too close to the other colors already in the game
            int maxAttempts = 50;
            float proximity = 0.65f;
            for(int attempts = 0; attempts < maxAttempts; attempts++)
            {
                color = NIU.RandomColor();
                if (attempts + 1 >= maxAttempts || !NIU.ColorTooCloseToOthers(color, proximity, gm.teamColors.ToArray()))
                {
                    //print(attempts);
                    break;
                }
            }

        
                

        }

        gm.teamColors[playingInt % gm.teamColors.Count] = color;

    }

    //Random
    public void ChangeTeamColor()
    {

        ChangeTeamColor(random: true);

    }




    public Team CheckTeam()
    {
        if(NIU.Within(teamInt, 0, gm.allTeams.Count - 1))
        {
            team = gm.allTeams[teamInt];
        }
        else
        {
            team = null;
        }

        return team;
    }
}
