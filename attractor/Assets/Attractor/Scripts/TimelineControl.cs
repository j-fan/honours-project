using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public class TimelineControl : MonoBehaviour {

    PlayableDirector playableDirector;
    public bool autoRestartEnabled;
    public float autoRestartDuration = 0; //the amount seconds of silence to trigger restart
    public BeatsFFT beatsFFT;
    float silenceTime = 0;


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

        if (autoRestartEnabled)
        {
            if (beatsFFT.avgFreq < 0.001f)
            {
                silenceTime += Time.deltaTime;
            }
            else
            {
                silenceTime = 0;
            }
            if (silenceTime > autoRestartDuration)
            {
                silenceTime = 0;
                playableDirector.time = 0;
                print("max silence period reached");
            }
        }

    }

    void OnGUI()
    {
        if (autoRestartEnabled)
        {
            GUI.Box(new Rect(10, 10, 150, 100), "Silence Time: " + Mathf.FloorToInt(silenceTime).ToString());
        }

    }
}
