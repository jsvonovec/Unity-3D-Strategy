using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchThing : MonoBehaviour
{
    [HideInInspector]
    public bool isUnit = false;
    [HideInInspector]
    public bool isBase = false;

    [HideInInspector]
    public Unit u = null;
    [HideInInspector]
    public Base b = null;

    public float HP
    {
        get
        {
            if (isUnit)
                return u.hp;

            else if (isBase)
                return b.hp;

            Debug.LogError("We fucked up. A MatchThing is neither a base or a unit.", gameObject);
            return float.NaN;
        }
    }
}
