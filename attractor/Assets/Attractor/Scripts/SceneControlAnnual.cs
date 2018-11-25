using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControlAnnual : MonoBehaviour {

	// Use this for initialization
	void Start () {
        SceneManager.LoadScene("annual", LoadSceneMode.Additive);

    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

    }
}
