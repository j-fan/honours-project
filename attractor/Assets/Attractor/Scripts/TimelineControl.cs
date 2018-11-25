using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public class TimelineControl : MonoBehaviour {

    PlayableDirector playableDirector;


	// Use this for initialization
	void Start () {
        playableDirector = GetComponent<PlayableDirector>();
	}
	
	// Update is called once per frame
	void Update () {
        if(playableDirector != null)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                playableDirector.time = 0;
            }
        }

    }
}
