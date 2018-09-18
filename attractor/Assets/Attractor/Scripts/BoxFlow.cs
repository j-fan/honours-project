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
    public float targetRadius = 10.0f;

    public BeatsFFT beatsFFT;

    GameObject[][] grid;
    Vector3 centre; // parent empty location
    float objX = 1.0f;
    float objZ = 1.0f;


    // Use this for initialization
    void Start()
    {
        centre = GetComponent<Transform>().position;
        grid = new GameObject[gridX][];
        objX *= objScale;
        objZ *= objScale;
        initGrid();
    }

    private void OnDisable()
    {
        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {
                Destroy(grid[x][z]);
            }
        }
    }

    private void OnEnable()
    {
        if (grid == null)
        {
            return;
        }
        initGrid();
    }


    // Update is called once per frame
    void LateUpdate()
    {
        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {

                Vector3 s = grid[x][z].transform.localScale;
                //animate scale
                if (scaleEnabled)
                {     
                    float noiseHeight = Mathf.PerlinNoise(x * noiseScale * Time.time, z * noiseScale * Time.time);
                    float heightScale = beatsFFT.runningAvgFreq * 2000 * animScale;
                    grid[x][z].transform.localScale = new Vector3(s.x, Mathf.Abs(heightScale*noiseHeight), s.z);
                }
                //animate rotation
                if (rotateEnabled)
                {
                    float rotate = beatsFFT.runningAvgFreq * 360 * animScale;
                    float noiseDirectionX = Mathf.PerlinNoise(x * noiseScale, Time.time);
                    float noiseDirectionZ = Mathf.PerlinNoise(z * noiseScale, Time.time);
                    grid[x][z].transform.Rotate(new Vector3(noiseDirectionX, 0, noiseDirectionZ), rotate);
                }
                // flatten radius around targets
                foreach (GameObject a in targets.getTargets())
                {
                    float distance = Vector3.Distance(a.transform.position, grid[x][z].transform.position);
                    if(distance < targetRadius)
                    {
                        float distanceRatio = distance / targetRadius;
                        //Vector3 oldScale = grid[x][z].transform.localScale;
                        Quaternion oldRotation = grid[x][z].transform.rotation;
                        if (scaleEnabled)
                            grid[x][z].transform.localScale = Vector3.Lerp(new Vector3(s.x, 10, s.z), new Vector3(s.x, 1, s.z), distanceRatio*distanceRatio);
                        if (rotateEnabled)
                            grid[x][z].transform.rotation = Quaternion.Lerp(Quaternion.identity, oldRotation, distanceRatio);
                        break;
                    }
                }

                //allow changes to box cell scale
                grid[x][z].transform.localScale = new Vector3(objScale, grid[x][z].transform.localScale.y, objScale);
            }
        }
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
                newObj.GetComponent<Renderer>().material.color = Color.HSVToRGB(hue,1.0f,0.3f);
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
