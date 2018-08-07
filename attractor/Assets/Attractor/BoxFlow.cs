using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxFlow : MonoBehaviour
{

    public GameObject drawObj;
    public Targets targets;

    // Use this for initialization
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {
        foreach(GameObject a in targets.getTargets())
        {
            print(a.transform.position);
        }
    }

}
