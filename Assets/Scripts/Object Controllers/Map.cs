using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    //A map is the world that the game is played on. In this case, it's the TERRAIN, BASE SPAWNS, AND NUMBER OF TEAMS.

    public string realName = "Unnamed Map";
    
    public List<int> validTeamCounts;
    public List<BaseSpawn> baseSpawns;
    public int teamCount;

    public int IndexOfTeamCount
    {
        get
        {
            return validTeamCounts.FindIndex(w => w == gm.matchTeams.Count);
        }
    }


    //DEBUG
    public int debugNumberOfPlayers = 2;


    private GameMaster gm;



    
    //Automating mapmaking
    private void OnValidate()
    {

        List<GameObject> childrenOfMap = NIU.EveryChildOf(gameObject);

        int numberOfBaseSpawnsSoFar = 0;
        foreach(GameObject go in FindObjectsOfType<GameObject>())
        {
            /*
            //Assigning things labelled for map use as part of map, and don't mess up things that already are in there
#pragma warning disable
            if (go.CompareTag("Map") && !childrenOfMap.Contains(go))
                    go.transform.SetParent(transform);
#pragma warning restore
*/
            //Sort out basespawns
            if(go.CompareTag("Map") && go.GetComponent<BaseSpawn>() != null)
            {
                numberOfBaseSpawnsSoFar++;
            }

        }

        


        //Don't let minimum players go below 1
        if(validTeamCounts.Count < 1)
        {
            Debug.LogWarning("We need to have at least one possible number of teams on this map " + realName + "!");
            validTeamCounts.Add(2);
        }


        //DEBUG: draw lines for which base is which team's
        foreach(BaseSpawn b in baseSpawns)
        {
            try
            {
                if (!validTeamCounts.Contains(debugNumberOfPlayers))
                    break;

                if (b != null && gameObject.activeInHierarchy)
                    Debug.DrawLine(b.transform.position + new Vector3(3f, 0f, 3f), b.transform.position + new Vector3(3f, 25f, 3f),
                        gm.teamColors[b.preferredTeam[validTeamCounts.FindIndex(w => w == debugNumberOfPlayers)]], 10f);

            }
            catch (UnityException)
            {
                break;
            }
        }
    }






    private void Awake()
    {
        gm = FindObjectOfType<GameMaster>();
    }



    public void Load(int numberOfTeams)
    {

        //Load the actual GameObject if it's not loaded already
        if (!gameObject.activeInHierarchy)
        {
            //Debug.Log("Instantiating a new map.");
            Instantiate(gameObject, Vector3.zero, NIU.QuaternionAll(0f)).GetComponent<Map>().Load(numberOfTeams);
            return;
        }



        teamCount = numberOfTeams;

        //Not a correct number of teams to play on this map
        while (!validTeamCounts.Contains(teamCount))
        {
            Debug.LogError("We're trying to play with " + numberOfTeams + 
                " teams on map " + realName + "! Handling badly now!", this);
            return;
        }

        //Spawn bases on the baseSpawns
        for(int i = 0; i < baseSpawns.Count; i++)
        {
            BaseSpawn bs = baseSpawns[i];
            Transform t = bs.transform;

            //Instantiate the base and set its team as the correct one
            Base b = Instantiate(gm.defaultBaseObject, t.position + (Vector3.up * gm.defaultBaseObject.transform.localScale.y), 
                                    NIU.QuaternionAll(), gm.matchTeams[bs.OurTeam(teamCount)].transform).GetComponent<Base>();

            b.team = gm.matchTeams[bs.OurTeam(teamCount)];
            b.team.Initialize();

            //Debug.Log("Team " + b.team.number + "'s base count is " + b.team.bases.Count);

            b.Startup();
        }
    }
}
