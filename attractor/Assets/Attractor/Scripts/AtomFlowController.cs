using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtomFlowController : MonoBehaviour {
    public Targets targets;
    public GameObject atomObject;
    public int atomsPerTarget = 10;
    List<List<GameObject>> atoms;
    bool isInit = false;
	// Use this for initialization
	void Start () {
        atoms = new List<List<GameObject>>();
        
	}

    void OnDisable()
    {
        foreach(List<GameObject> target in atoms)
        {
            foreach(GameObject atom in target)
            {
                //print(atom);
                atom.GetComponent<AtomFlow>().enabled = false;
            }
        }
    }

    void OnEnable()
    {
        if (isInit)
        {
            foreach (List<GameObject> target in atoms)
            {
                foreach (GameObject atom in target)
                {
                    atom.GetComponent<AtomFlow>().enabled = true;
                }
            }
        }

    }

    void generateAtoms()
    {
        List<GameObject> attractors = targets.getTargets();
        for (int i = 0; i < 8; i++) //hack to fix instantiation lag for now
        {
            GameObject target = attractors[i];
            List<GameObject> atomsForTarget = new List<GameObject>();
            for(int j = 0; j < atomsPerTarget; j++)
            {
                GameObject newAtom = Instantiate(atomObject) as GameObject;
                newAtom.transform.position = target.transform.position + new Vector3(Random.Range(0,0.1f), Random.Range(0, 0.1f), Random.Range(0, 0.1f));
                float hue = (float)i / targets.getCurrentAttractors();
                newAtom.GetComponent<AtomFlow>().enabled = true;
                newAtom.GetComponent<AtomFlow>().attractor = target;
                atomsForTarget.Add(newAtom);
            }
            atoms.Add(atomsForTarget);
        }
    }

	// Update is called once per frame
	void Update () {
        if (!isInit)
        {
            isInit = true;
            generateAtoms();
        }
	}
}
