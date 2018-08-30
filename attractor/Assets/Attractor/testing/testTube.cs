using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testScript : MonoBehaviour {

    TubeRenderer tr;

	// Use this for initialization
	void Start () {
        tr = gameObject.GetComponent<TubeRenderer>();
        if (tr == null)
        {
            tr = gameObject.AddComponent<TubeRenderer>();
        }
	}
	
	// Update is called once per frame
	void Update () {
        TubeRenderer.TubeVertex[][] tubes = new TubeRenderer.TubeVertex[3][];
        tubes[0] = new TubeRenderer.TubeVertex[]{ new TubeRenderer.TubeVertex (new Vector3(0,0,0),3,Color.white),
                                                  new TubeRenderer.TubeVertex (new Vector3(0,40,0),3,Color.white)};

        tubes[1] = new TubeRenderer.TubeVertex[]{ new TubeRenderer.TubeVertex (new Vector3(0,0,0),3,Color.white),
                                                  new TubeRenderer.TubeVertex (new Vector3(0,0,40),3,Color.white)};
        tubes[2] = new TubeRenderer.TubeVertex[]{ new TubeRenderer.TubeVertex (new Vector3(0,0,0),3,Color.white),
                                                  new TubeRenderer.TubeVertex (new Vector3(20,20,0),3,Color.white)};
        tr.lines = tubes;
    }
}
