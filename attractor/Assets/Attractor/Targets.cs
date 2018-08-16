using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targets : MonoBehaviour {

    public OSC osc;
    int numTargets = 0; //targets refer to targets detected by orbbec/opencv
    int currentAttractors = 15;
    List<GameObject> attractors;
    float alpha = 0.1f; //lowpass filter positions for smooth movement, lower number for more smoothing
    public GameObject targetObject;

    void Start () {
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
        //relay this message to maxMSP
        osc.Send(message);

        //find whether number of targets seen by camera or number of spheres is greater
        if (numTargets > attractors.Count)
        {
            currentAttractors = attractors.Count;
        }
        else
        {
            currentAttractors = numTargets;
            for (int i = numTargets; i < attractors.Count; i++)
            {
                //hide offscreen
                attractors[i].transform.position = new Vector3(-100, -100, 100);
            }
        }

        for (int i = 0; i < currentAttractors; i++)
        {
            float x = message.GetFloat(i * 2) / 3f;
            float y = message.GetFloat(i * 2 + 1) / 3f;
            //if(i==0) print(x + " " + y);
            Vector3 newPos = new Vector3(x, 0, y);
            Vector3 oldPos = attractors[i].transform.position;
            //dampen for smooth movement
            attractors[i].transform.position = newPos * alpha + (oldPos * (1.0f - alpha));
        }
    }



    void initAttractors()
    {
        attractors = new List<GameObject>();
        for (int i = 0; i < currentAttractors; i++)
        {
            Vector3 pos = new Vector3(Random.Range(0.0f, 100.0f), 0.0f, Random.Range(0.0f,60.0f));
            Vector3 scale = new Vector3(2.0f, 2.0f, 2.0f);
            GameObject newAttractor = CreateTarget();

            newAttractor.transform.position = pos;
            newAttractor.transform.localScale = scale;
            attractors.Add(newAttractor);
        }
    }
    GameObject CreateTarget()
    {
        GameObject newAttractor;
        if (targetObject == null)
        {
            newAttractor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }
        else
        {
            newAttractor = Instantiate(targetObject) as GameObject;
        }
        return newAttractor;
    }

    public List<GameObject> getTargets()
    {
        return attractors;
    }

    public int getCurrentAttractors()
    {
        return currentAttractors;
    }
}
