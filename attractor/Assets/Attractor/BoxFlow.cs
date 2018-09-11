using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxFlow : MonoBehaviour
{

    public GameObject boxObj;
    public Targets targets;
    public Material material;
    public int gridX = 18;
    public int gridZ = 12;
    public float spacing = 0.2f;
    public float animScale = 1.0f; 
    public float objScale = 1.0f;
    public bool rotateEnabled = true;
    public bool scaleEnabled = true;
    public float noiseScale = 0.07f; 

    public AudioSource audioSource;
    float[] asamples;
    int numSamples = 128;
    float avgFreq = 0.0f;
    float runningAvgFreq = 0.0f;
    float audioAlpha = 0.1f;
    

    GameObject[][] grid;
    Vector3 centre;
    float objX = 1.0f;
    float objZ = 1.0f;

    // Use this for initialization
    void Start()
    {
        centre = GetComponent<Transform>().position;
        grid = new GameObject[gridX][];
        asamples = new float[numSamples];
        objX *= objScale;
        objZ *= objScale;
        initGrid();
    }


    // Update is called once per frame
    void LateUpdate()
    {
        GetSpectrumAudioSource();
        // foreach(GameObject a in targets.getTargets())
        //{
        //print(a.transform.position);
        //}
        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {
                Vector3 s = grid[x][z].transform.localScale;
                
                if (scaleEnabled)
                {
                    float noiseHeight = Mathf.PerlinNoise(x * z * noiseScale, Time.time);
                    //float height = (asamples[(z * gridX + x) % numSamples] * 100 + 0.1f) * animScale;
                    float heightScale = runningAvgFreq * 4000 * animScale;
                    grid[x][z].transform.localScale = new Vector3(s.x, heightScale*noiseHeight, s.z);
                }
                if (rotateEnabled)
                {
                    float rotate = runningAvgFreq * 360 * animScale;
                    float noiseDirectionX = Mathf.PerlinNoise(x * noiseScale, Time.time);
                    float noiseDirectionZ = Mathf.PerlinNoise(z * noiseScale, Time.time);
                    grid[x][z].transform.Rotate(new Vector3(noiseDirectionX, 0, noiseDirectionZ), rotate);
                }
                
            }
        }
    }

    void GetSpectrumAudioSource()
    {
        audioSource.GetSpectrumData(asamples, 0, FFTWindow.Blackman);
        avgFreq = 0.0f;
        for (int i = 0; i < asamples.Length; i++)
        {
            avgFreq = avgFreq + asamples[i];
        }
        avgFreq = avgFreq * avgFreq;
        avgFreq = avgFreq / asamples.Length;
        runningAvgFreq = (avgFreq * audioAlpha) + ((1 - audioAlpha) * runningAvgFreq);
    }
    void initGrid()
    {
        GameObject newObj;

        if(boxObj != null)
        {
            objX = boxObj.GetComponent<Renderer>().bounds.size.x * objScale;
            objZ = boxObj.GetComponent<Renderer>().bounds.size.z * objScale;
        }
        objX += spacing;
        objZ += spacing;


        for(int x = 0; x < gridX; x++)
        {
            grid[x] = new GameObject[gridZ];
            for(int z=0;z< gridZ; z++)
            {
                if (boxObj == null)
                {
                    newObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                } else
                {
                    newObj = Instantiate(boxObj) as GameObject;
                }
                newObj.GetComponent<Renderer>().material = material;
                float hue = ((float)z + ((float)x * gridZ)) / (gridX*gridZ);
                print(hue);
                newObj.GetComponent<Renderer>().material.color = Color.HSVToRGB(hue,1.0f,1.0f);
                newObj.transform.position = new Vector3(x*objX + centre.x, centre.y, z*objZ+centre.z);
                newObj.transform.localScale = newObj.transform.localScale * objScale;
                if (rotateEnabled && !scaleEnabled)
                {
                    newObj.transform.localScale = newObj.transform.localScale + new Vector3(0,5,0);
                }
                grid[x][z] = newObj;
            }
        }
    }
}
