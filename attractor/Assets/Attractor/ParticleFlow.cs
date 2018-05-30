using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ParticleFlow : MonoBehaviour {

    const int ELECTRIC = 0;
    const int GRAVITY = 1;
    const int SIMPLE = 2;
    public int simType = 0;

    public Gradient particleColourGradient;
    public float forceMultiplier = 0.2f;
    public Material sphereMat;
    float g = 1f;
    float mass = 3f;

    public OSC osc;
    int numTargets = 0; //targets refer to targets detected by orbbec/opencv

    ParticleSystem ps;
    int minAttractors = 5;
    List<GameObject> attractors;

    float alpha = 1.0f; //lowpass filter positions for smooth movement

    // Use this for initialization
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        initAttractors();
        osc.SetAddressHandler("/numPoints", setNumTargets);
        osc.SetAddressHandler("/points", moveTargets);
    }

    void setNumTargets(OscMessage message)
    {
        numTargets = message.GetInt(0);
        //print("num blobs: " + numTargets);
    }

    void moveTargets(OscMessage message)
    {

        //find whether number of targets seen by camera or number of spheres is greater
        int max;
        if (numTargets > attractors.Count)
        {
            max = attractors.Count;
        }
        else
        {
            max = numTargets;

        }

        for (int i=0; i < max; i++) {
            Vector3 newPos = new Vector3(message.GetFloat(i+1) / 20.0f , 0, message.GetFloat(i) / 20.0f );
            Vector3 oldPos = attractors[i].transform.position;
            //dampen for smooth movement
            attractors[i].transform.position = newPos * alpha + (oldPos * (1.0f - alpha));
            //print(message);
        }

    }

    void Update()
    {
        // add variation to particle colour
        ParticleSystem.MainModule main = GetComponent<ParticleSystem>().main;
        main.startColor = particleColourGradient.Evaluate(Random.Range(0f, 1f));
    }

    void LateUpdate()
    {

        //put particles of the system into array & update them to gravity algorithm
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
        ps.GetParticles(particles);

        for (int i = 0; i < particles.Length; i++)
        {
            ParticleSystem.Particle p = particles[i];
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

            Vector3 totalForce;
            if (simType == SIMPLE)
            {
                totalForce = applySimple(particleWorldPosition);
            } else if (simType == GRAVITY)
            {
                totalForce = applyGravity(particleWorldPosition);
            } else if (simType == ELECTRIC)
            {
                totalForce = applyElectric(p);
            } else
            {
                totalForce = applySimple(particleWorldPosition);
            }


            if (simType != GRAVITY)
            {
                p.velocity = totalForce;    //velocity only to visualise field line style
            } else
            {
                p.velocity += totalForce;   //with  acceleration
            }

            particles[i] = p;
        }
        ps.SetParticles(particles, particles.Length); //set updated particles into the system
    }

    Vector3 applySimple(Vector3 particleWorldPosition)
    {
        Vector3 direction = Vector3.zero;
        foreach (GameObject a in attractors)
        {
            direction += (a.transform.position - particleWorldPosition).normalized;
        }
        Vector3 totalForce = ((direction) * forceMultiplier) * Time.deltaTime;
        return totalForce;
    }

    Vector3 applyGravity(Vector3 particleWorldPosition)
    {
        Vector3 direction = Vector3.zero;
        foreach (GameObject a in attractors)
        {
            direction += (a.transform.position - particleWorldPosition).normalized;
        }
        float magnitude = direction.magnitude;
        Mathf.Clamp(magnitude, 5.0f, 10.0f); //eliminate extreme result for very close or very far objects

        float gforce = (g * mass * mass) / direction.magnitude * direction.magnitude;
        Vector3 totalForce = ((direction) * gforce) * Time.deltaTime;
        totalForce = totalForce * forceMultiplier;
        return totalForce;
    }

    Vector3 applyElectric(ParticleSystem.Particle p)
    {
        Vector3 totalForce = Vector3.zero;
        Vector3 force = Vector3.zero;
        int i = 0;
        foreach (GameObject a in attractors)
        {
            float dist = Vector3.Distance(p.position, a.transform.position) * 100000;
            float fieldMag = 99999 / dist * dist;
            Mathf.Clamp(fieldMag, 0.0f, 5.0f);

            //alternate postive and negative charges
            if (i % 2 == 0)
            {
                force.x -= fieldMag * (p.position.x - a.transform.position.x) / dist;
                force.y -= fieldMag * (p.position.y - a.transform.position.y) / dist;
                force.z -= fieldMag * (p.position.z - a.transform.position.z) / dist;
            }
            else
            {
                force.x += fieldMag * (p.position.x - a.transform.position.x) / dist;
                force.y += fieldMag * (p.position.y - a.transform.position.y) / dist;
                force.z += fieldMag * (p.position.z - a.transform.position.z) / dist;
            }

            i++;
        }
        totalForce = force * forceMultiplier;
        return totalForce;
    }

    void initAttractors()
    {
        attractors = new List<GameObject>();
        for (int i = 0; i < minAttractors; i++)
        {
            GameObject newAttractor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            newAttractor.transform.position = new Vector3(0.0f, 0.0f, (minAttractors-i)*3);
            newAttractor.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            newAttractor.GetComponent<Renderer>().material = sphereMat;
            attractors.Add(newAttractor);
        }
    }
}
