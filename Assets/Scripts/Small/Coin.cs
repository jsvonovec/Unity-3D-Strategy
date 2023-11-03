using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public float moneyPerCoin = 10f;
    public float timeToDespawn = 15f;
    [SerializeField] private float randomHorizontalForce = 5f;
    [SerializeField] private float randomVerticalForce = 30f;
    [SerializeField] private float centerMod = 0.3f;
    [SerializeField] private Renderer rim = null;
    public int team;

    Rigidbody rb;
    //private bool done = false;
    //private float startTime;

    private void Start()
    {
        //startTime = Time.time;

        rb = GetComponent<Rigidbody>();

        rb.detectCollisions = false;

        GameMaster gm = Qk.GM;

        rim.material.color = gm.teamColors[team % gm.teamColors.Count];

        GetComponent<Rigidbody>().AddForceAtPosition((new Vector3(Random.Range(-0.5f,0.5f) * randomHorizontalForce,
                                                                Random.Range(0.5f, 1f) * randomVerticalForce,
                                                                Random.Range(-0.5f, 0.5f) * randomHorizontalForce)
                                                            + new Vector3((gm.centerOfField.x - transform.position.x) * centerMod,
                                                                   0f,
                                                                   (gm.centerOfField.z - transform.position.z) * centerMod)) * rb.mass,
                                                     transform.position + 
                                                        new Vector3(Random.Range(-0.5f, 0.5f) * transform.localScale.x,
                                                                    Random.Range(-0.5f, 0.5f) * transform.localScale.y,
                                                                    Random.Range(-0.5f, 0.5f) * transform.localScale.z),
                                                     ForceMode.Impulse);
    }

    void Update()
    {
       /* if (rb.velocity.y <= 0f && !done)
        {
            rb.detectCollisions = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        if (rb.velocity.magnitude <= 1f)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.isKinematic = true;
            done = true;
        }

        //Fall under floor after certain amt of time
        if (Time.time >= startTime + timeToDespawn)
        {
            rb.isKinematic = false;
            done = false;
            rb.detectCollisions = false;
        }*/


        //If we fall under the floor
        if (transform.position.y < -0.5f)
            Destroy(gameObject);
    }
}
