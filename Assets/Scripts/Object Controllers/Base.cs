using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base : MatchThing
{
    [Tooltip("MUST INCLUDE ALL ERAS, INCLUDING E0! Do not remove any of these! It's okay if it's empty, just don't delete!")]
    public List<GameObject> eraIndicators = new List<GameObject>();

    public float spawnDistance;
    public float maxhp = 1250;
    public float damageToMoneyRatio = 0.1f;
    public float damageToExpRatio = 0.1f;
    public float damageMoneyEraMod = 3f;
    public float damageExpEraMod = 3f;
    public float bounty = 200;
    public float exp = 300;
    public float armor = 0f;    //each armor blocks 1 hit

    public int turrets = 0;
    [HideInInspector] public Vector3[] turretSpots = new Vector3[3];
    [SerializeField] private float angleBtwnTurrets = 30f;
    /// <summary>
    /// -1 means no room for turret
    /// </summary>
    public int NextTurretSpot
    {
        get
        {
            //too many turrets
            if (turrets == turretSpots.Length)
                return -1;
            //Start near the middle, spawn turrets further as more are added
            //Odd number of spots
            if (turretSpots.Length % 2 != 0)
                return (turretSpots.Length / 2) + ((-2 * (((turrets) + 1) % 2) + 1) * ((turrets + 1) / 2));
            //Even number of spots
            else
                return (turretSpots.Length / 2) + ((-2 * (((turrets) + 1) % 2) + 1) * ((turrets + 1) / 2)) - 1;
        }
    }

    public float hp;

    private GameMaster gm;
    private Renderer r;
    public Team team;
    public Collider coll;


    // Startup is used at the start of a match
    public void Startup()
    {
        b = this;
        isBase = true;

        gm = Qk.GM;
        r = GetComponent<Renderer>();
        coll = GetComponent<Collider>();

        gm.allBases.Add(this);

        r.material.color = gm.teamColors[team.number % gm.teamColors.Count];

        hp = maxhp;

        //create turret spots
        float centerTheta = Vector2.SignedAngle(Vector2.right, new Vector2(gm.centerOfField.x - transform.position.x, gm.centerOfField.z - transform.position.z));
        float nowTheta = (((turretSpots.Length - 1) * angleBtwnTurrets / 2) + centerTheta) * Mathf.Deg2Rad;
        for(int i = 0; i < turretSpots.Length; i++)
        {
            turretSpots[i] = new Vector3(Mathf.Cos(nowTheta) * transform.lossyScale.x * 0.4f,
                                         transform.lossyScale.y + 0.1f,
                                         Mathf.Sin(nowTheta) * transform.lossyScale.z * 0.4f) 
                            + transform.position;

            nowTheta -= angleBtwnTurrets * Mathf.Deg2Rad;
        }

        //Rotate towards centerOfField

        transform.Rotate(Vector3.up, -Vector2.SignedAngle(Vector2.up, 
            new Vector2(gm.centerOfField.x - transform.position.x, gm.centerOfField.z - transform.position.z).normalized));
    }
    


    // Update is called once per frame
    void Update()
    {
        if (hp <= 0f)
            Die();
    }


    //true if killed, false if still alive. Pay money averaging at damageToMoneyRatio coins per damage point
    public bool Damage(float dmg, Unit killer = null)
    {
        //armor absorbs hit
        if(armor > 0)
        {
            armor--;
            return false;
        }

        hp -= dmg;
        team.Pay(Random.value * 2f * damageToMoneyRatio * dmg * Mathf.Pow(damageMoneyEraMod, team.era), 
               Random.value * 2f * damageToExpRatio * dmg * Mathf.Pow(damageExpEraMod, team.era));
        team.cBuildSkip += dmg * team.buildSkipPerDamage * Mathf.Pow(1.5f, -team.era);
        if(hp <= 0)
        {
            Die();
            return true;
        }
        return false;
    }


    private void Die()
    {
        gm.ignoreThese.Add(this);
        //remove from lists
        gm.allBases.Remove(this);
        team.bases.Remove(this);

        // float away               //(hp + 10200f) / 200f cylinder.transform.position -= new Vector3(0f, Time.deltaTime / 2f, 0f);
        if (hp > -10000f)
        {
            hp = -10000;
        }

        //make cylinder fly away depending on damage taken
        transform.position += new Vector3(0f, -1f * Time.deltaTime, 0f);
        //coll.enabled = false;
        //cylinder.transform.parent = null;
        r.material.color = new Color(r.material.color.r * 1.01f,
                                     r.material.color.g * 1.01f,
                                     r.material.color.b * 1.01f);
        //r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        hp -= Time.deltaTime * 100;
        if (hp <= -10200f && transform.position.y < -2f)
        {
            Destroy(gameObject);
        }
    }



    /// <summary>
    /// Enables era markers on the base. The era property here must be between 0 and 4 inclusive.
    /// </summary>
    /// <param name="era"></param>
    public void SetEraMarkers(int era)
    {
        if (!NIU.Within(era, 0, 4))
            return;

        for(int i = 0; i < 5; i++)
        {
            //This era marker should be enabled.
            if(i <= era && !eraIndicators[i].activeInHierarchy)
            {
                eraIndicators[i].SetActive(true);
            }

            //This era marker should be disabled.
            else if (i > era && eraIndicators[i].activeInHierarchy)
            {
                eraIndicators[i].SetActive(false);
            }
        }

        return;
    }
}
