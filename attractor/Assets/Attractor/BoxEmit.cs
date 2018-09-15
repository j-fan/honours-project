using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BoxEmit : MonoBehaviour {
    public Targets targets;
    public GameObject particleSystemObject;
    bool init = false;

    // Use this for initialization
    void Start () {

        
	}
	
	// Update is called once per frame
	void Update () {
        if (!init) //targets not ready in start();
        {
            createParticleSystems();
            init = true;
        }
    }

    void createParticleSystems()
    {
        List<GameObject> attractors = targets.getTargets();
        foreach (GameObject a in attractors)
        {
            GameObject newParticleSys = Instantiate(particleSystemObject) as GameObject;
            newParticleSys.transform.position = a.transform.position;


        }
    }
}
