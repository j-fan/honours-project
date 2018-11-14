using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControl : MonoBehaviour {

    public string[] scenesToLoad = { "exhibition pt1","exhibition pt2" };
    public float[] sceneDurations = { 3f, 3f };
    float delay = 0;
    int sceneCount = 0;
    Scene masterScene;

    // Use this for initialization
    void Start () {
        delay = sceneDurations[0];
        SceneManager.LoadScene(scenesToLoad[0], LoadSceneMode.Additive);
        masterScene = SceneManager.GetActiveScene();
    }
	
	// Update is called once per frame
	void Update () {
        delay -= Time.deltaTime;
        if (delay <= 0)
        {
            SceneManager.UnloadSceneAsync(scenesToLoad[sceneCount]);
            GameObject[] objs = masterScene.GetRootGameObjects();
            for(int i = 0; i < objs.Length; i++)
            {
                GameObject go = objs[i];
                if(go.name != "Scene Control")
                {
                    Destroy(go);
                }
            }
            
            sceneCount++;
            sceneCount = sceneCount % scenesToLoad.Length;
            SceneManager.LoadScene(scenesToLoad[sceneCount], LoadSceneMode.Additive);
            delay = sceneDurations[sceneCount];
        }
    }

}
