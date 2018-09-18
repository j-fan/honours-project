using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxEmitNoise : MonoBehaviour {
    ParticleSystem ps;
    float noiseScale = 0.1f;
    float incr = 0.1f;
    public float delay = 0.5f;
    public float animSpeed = 20f;

    public Flowfield flowfield;
    public BeatsFFT beatsFFT;


	// Use this for initialization
	void Start () {
        ps = GetComponent<ParticleSystem>();
    }
	
	// Update is called once per frame
	void LateUpdate () {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
        ps.GetParticles(particles);

        for (int i = 0; i < particles.Length; i++) {
            ParticleSystem.Particle p = particles[i];
            // find global position
            Vector3 particleWorldPosition;
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


            // apply noise field direction vectors

            if (particles[i].startLifetime - particles[i].remainingLifetime > delay)
            {
                Vector3Int particlePos = new Vector3Int(
                    Mathf.FloorToInt(Mathf.Clamp((particleWorldPosition.x / flowfield.cellSize),0,flowfield.gridSize.x-1)),
                    Mathf.FloorToInt(Mathf.Clamp((particleWorldPosition.y / flowfield.cellSize), 0, flowfield.gridSize.y - 1)),
                    Mathf.FloorToInt(Mathf.Clamp((particleWorldPosition.z / flowfield.cellSize), 0, flowfield.gridSize.z - 1))
                    );
                Vector3 flowVector = flowfield.flowFieldDirections[particlePos.x, particlePos.y, particlePos.z] ;
                //Quaternion targetRotation = Quaternion.LookRotation(flowVector.normalized);
                particles[i].velocity = flowVector * animSpeed * beatsFFT.runningAvgFreq;
        
            }


        }

        ps.SetParticles(particles, particles.Length);
    }


}
