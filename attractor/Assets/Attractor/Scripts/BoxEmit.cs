using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BoxEmit : MonoBehaviour {
    public Targets targets;
    public GameObject particleSystemObject;
    List<GameObject> boxEmitters;
    bool init = false;

    // Use this for initialization
    void Start () {

        
	}
	
	void Update () {
        if (!init) 
        {
            return;
        }
        List<GameObject> targs = targets.getTargets();
        if(targs.Count > boxEmitters.Count)
        {
            int start = targs.Count - boxEmitters.Count;
            for(int i = start; i<targs.Count; i++)
            {
                GameObject newParticleSys = Instantiate(particleSystemObject) as GameObject;
                if (boxEmitters[i] == null)
                {
                    newParticleSys.transform.position = targs[i].transform.position;
                    boxEmitters.Add(newParticleSys);
                } else
                {
                    ParticleSystem.EmissionModule em = newParticleSys.GetComponent<ParticleSystem>().emission;
                    em.enabled = true;
                }

            }
        } else if (targs.Count < boxEmitters.Count)
        {
            int start = boxEmitters.Count - targs.Count;
            for(int i=start;i < boxEmitters.Count; i++)
            {
                ParticleSystem.EmissionModule em = targs[i].GetComponent<ParticleSystem>().emission;
                em.enabled = false;
            }
            targs.RemoveRange(start, targs.Count);
        }


        for(int i=0;i<boxEmitters.Count;i++)
        {
            boxEmitters[i].transform.position = targs[i].transform.position;
        }
    }

    private void OnDisable()
    {
        foreach(GameObject b in boxEmitters)
        {
            Destroy(b);
        }
        boxEmitters.Clear();
    }

    private void OnEnable()
    {
        if (!init)
        {
            init = true;
        }
        createParticleSystems();

    }

    void createParticleSystems()
    {
        boxEmitters = new List<GameObject>();
        List<GameObject> attractors = targets.getTargets();
        foreach (GameObject a in attractors)
        {
            GameObject newParticleSys = Instantiate(particleSystemObject) as GameObject;
            newParticleSys.transform.position = a.transform.position;
            boxEmitters.Add(newParticleSys);
        }
    }
}
