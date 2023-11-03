using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSpawn : MonoBehaviour
{

    public List<int> preferredTeam;
    public int actualTeam;
    public Base thisBase;
    public Map map;


    private void OnValidate()
    {
        if (gameObject.activeInHierarchy)
        {
            //Update preferredTeam count
            if(preferredTeam.Count != map.validTeamCounts.Count)
            {
                preferredTeam.Clear();
                preferredTeam.AddRange(new int[map.validTeamCounts.Count]);
            }

        }

    }



    public int OurTeam(int numberOfTeams)
    {
        Map m = FindObjectOfType<Map>();

        return preferredTeam[m.IndexOfTeamCount];
    }
}
