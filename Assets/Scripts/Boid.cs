using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Boid : MonoBehaviour
{
    [Header("Set Dynamically")]
    public Rigidbody rb;
    
    private Neighborhood neighborhood;

    private void Awake()
    {
        neighborhood = GetComponent<Neighborhood>();
        rb = GetComponent<Rigidbody>();
        
        pos = Random.insideUnitSphere * Spawner.S.spawnRadius; //random position
        
        Vector3 vel = Random.insideUnitSphere * Spawner.S.velocity; //random velocity
        rb.velocity = vel;
        
        LookAhead();

        Color randColor = Color.black;
        while (randColor.r + randColor.g + randColor.b < 1.0f)
        {
            randColor = new Color(Random.value, Random.value, Random.value);
        }
        Renderer[] rends = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rends)
        {
            r.material.color = randColor;
        }
        TrailRenderer tRend = GetComponent<TrailRenderer>();
        tRend.material.SetColor("_TintColor", randColor);
    }

    void LookAhead()
    {
        transform.LookAt(pos + rb.velocity); //bird look ahead
    }

    public Vector3 pos
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    private void FixedUpdate()
    {
        Vector3 vel = rb.velocity;
        Spawner spn = Spawner.S;
        
        Vector3 velAvoid = Vector3.zero; // avoidance of close neighbors
        Vector3 tooClosePos = neighborhood.avgClosePos;
        if (tooClosePos != Vector3.zero)
        {
            velAvoid = pos - tooClosePos;
            velAvoid.Normalize();
            velAvoid *= spn.velocity;
        }

        Vector3 velAlign = neighborhood.avgVel; // alignment of close neighbors
        if (velAlign != Vector3.zero)
        {
            velAlign.Normalize(); 
            velAlign *= spn.velocity;
        }

        Vector3 velCenter = neighborhood.avgPos; // center of neighborhood
        if (velCenter != Vector3.zero)
        {
            velCenter -= transform.position;
            velCenter.Normalize();
            velCenter *= spn.velocity;
        }

        Vector3 delta = Attractor.POS - pos; // distance to attractor
        
        bool attracted = (delta.magnitude > spn.attractPushDist); // Check where to move: to Attractor or opposite
        Vector3 velAttract = delta.normalized * spn.velocity;
        
        // Apply all the velocities
        float fdt= Time.fixedDeltaTime;
        if (velAvoid != Vector3.zero)
        {
            vel = Vector3.Lerp(vel, velAvoid, spn.collAvoid * fdt);
        }
        else
        {
            if (velAlign != Vector3.zero)
            {
                vel = Vector3.Lerp(vel, velAlign, spn.velMatching * fdt);
            }

            if (velCenter != Vector3.zero)
            {
                vel = Vector3.Lerp(vel, velAlign, spn.flockCentering * fdt);
            }

            if (velAttract != Vector3.zero)
            {
                if (velAttract != Vector3.zero)
                {
                    if (attracted)
                    {
                        vel = Vector3.Lerp(vel, velAttract, spn.attractPull * fdt);
                    }
                    else
                    {
                        vel = Vector3.Lerp(vel, -velAttract, spn.attractPull * fdt);
                    }
                }
            }
        }
        
        vel = vel.normalized * spn.velocity;
        rb.velocity = vel;
        LookAhead(); //bird look ahead
    }
}
