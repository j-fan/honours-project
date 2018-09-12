using System;
using System.Collections.Generic;
using UnityEngine;

public class TubeTrails : MonoBehaviour {
    TubeRenderer tr;
    ParticleSystem ps;
    List<List<TubeRenderer.TubeVertex>> tubes;
    int tubeSteps = 50;
    float tubeWidth = 0.4f;

    // Use this for initialization
    void Start () {
        tr = gameObject.GetComponent<TubeRenderer>();
        if (tr == null)
        {
            tr = gameObject.AddComponent<TubeRenderer>();
        }
        ps = GetComponent<ParticleSystem>();
        tubes = new List<List<TubeRenderer.TubeVertex>>();
        tr.lines = tubes;
        for(int i = 0; i < ps.main.maxParticles; i++) { }
    }
	
	// Update is called once per frame
	void Update () {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
        ps.GetParticles(particles);

        for (int i = 0; i < ps.main.maxParticles; i++)
        {
           
            if (i < ps.particleCount)
            {
                Vector3 particleWorldPosition;
                ParticleSystem.Particle p = particles[i];
               
                if (ps.main.simulationSpace == ParticleSystemSimulationSpace.Local)
                {
                    particleWorldPosition = transform.TransformPoint(p.position);
                }
                else if (ps.main.simulationSpace == ParticleSystemSimulationSpace.Custom)
                {
                    particleWorldPosition = ps.main.customSimulationSpace.TransformPoint(p.position);
                }
                else
                {
                    particleWorldPosition = p.position;
                }
                // update tube vertex locations
                //init list of vertexes that form a trail, if it does not exist
                if (i > tubes.Count-1)
                {
                    tubes.Add(new List<TubeRenderer.TubeVertex>());
                    tubes[i].Add(new TubeRenderer.TubeVertex(particleWorldPosition, tubeWidth, p.GetCurrentColor(ps)));

                }
                //prevent old and new particles joining together on respawn
                else if ((p.startLifetime - p.remainingLifetime < 0.2f))
                {
                    tubes[i].RemoveRange(0,tubes[i].Count);
                    tubes[i].Add(new TubeRenderer.TubeVertex(particleWorldPosition, tubeWidth, p.GetCurrentColor(ps)));
                }
                else
                {
                    if(Vector3.Distance(particleWorldPosition,tubes[i][tubes[i].Count-1].point) > 0.1f)
                    {
                        // remove head and add new point
                        if (tubes[i].Count > tubeSteps)
                        {
                            tubes[i].RemoveAt(0);
                        }
                        tubes[i].Add(new TubeRenderer.TubeVertex(particleWorldPosition, tubeWidth, p.GetCurrentColor(ps)));
                    }


                }
            } else
            {
                // particle does not exist yet (since current particles may be less than max particles)
                if (i <= tubes.Count - 1)
                {
                    tubes[i].RemoveRange(0, tubes[i].Count);
                }
            }

            
        }
        tr.lines = tubes;
  
        

    }
}
