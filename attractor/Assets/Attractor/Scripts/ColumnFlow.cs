using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColumnFlow : MonoBehaviour {
    public int numCols = 30;
    Mesh mesh;
    List<Vector3> verts;
    List<int> tris;
    List<Rect> rects;
    FastNoise fastnoise;
    public AnimationCurve shutterAnimCurve;
    public float maxWidth = 120f;
    public float maxHeight = 200f;

    const int SHUTTER = 0;
    const int NOISE = 1;
    const int SLIDEY = 2;
    const int RACE = 3;
    const int FALLING = 4;
    public int animationType = SHUTTER;
    public float transparency = 1.0f;
    public float speedModifier = 1.0f;
    public BeatsFFT beatsFFT;

    // Use this for initialization
    void Start () {
        mesh = GetComponent<MeshFilter>().mesh;
        verts = new List<Vector3>();
        tris = new List<int>();
        rects = new List<Rect>();
        fastnoise = new FastNoise();
        initColumns();
    }

    void OnDisable()
    {
        clearMesh();
        updateMesh();
    }
	
	// Update is called once per frame
	void Update () {

        clearMesh();
        switch (animationType)
        {
            case SHUTTER:
                shutterColumns();
                break;
            case NOISE:
                noiseColumns(30f * speedModifier);
                break;
            case SLIDEY:
                slideyColumns(2f * speedModifier);
                break;
            case RACE:
                raceColumns(5f*speedModifier*(beatsFFT.runningAvgFreq*200));
                break;
            case FALLING:
                fallingColumns(3f * speedModifier);
                break;
            default:
                shutterColumns();
                break;
        }
        updateMesh();
        Material mat = GetComponent<Renderer>().material;
        Color oldColor = mat.color;
        Color newColor = new Color(oldColor.r,oldColor.g, oldColor.b, transparency);
        mat.SetColor("_Color", newColor);
    }

    void noiseColumns(float noiseSpeed)
    {
        float zOrigin = transform.position.z;
        for(int i = 0; i < numCols; i++)
        {
            Rect oldRect = rects[i];
            float xOrigin = (fastnoise.GetCellular(oldRect.x * noiseSpeed * Time.time, oldRect.y * noiseSpeed * Time.time)+0.5f) * 100f;
            Rect newRect = new Rect(xOrigin, zOrigin, oldRect.width, oldRect.height); 
            addRectToMesh(newRect);
        }
    }

    void slideyColumns(float noiseSpeed)
    {
        float zOrigin = transform.position.z;
        for (int i = 0; i < numCols; i++)
        {
            Rect oldRect = rects[i];
            float slideAmount = (fastnoise.GetSimplex(oldRect.x * noiseSpeed * Time.time, oldRect.y * noiseSpeed * Time.time)) * 10f;
            Rect newRect = new Rect(oldRect.x + slideAmount, zOrigin, oldRect.width, oldRect.height);
            addRectToMesh(newRect);
        }
    }
    void shutterColumns()
    {
        float duration = 1f;
        for (int i = 0; i < numCols; i++)
        {
            Rect oldRect = rects[i];
            float newWidth = shutterAnimCurve.Evaluate(Time.time/duration) * (maxWidth/numCols);
            float xOrigin = maxWidth / numCols * i;
            Rect newRect = new Rect(xOrigin-newWidth/2, oldRect.y, newWidth, oldRect.height);
            addRectToMesh(newRect);
        }
    }

    void raceColumns(float speed)
    {
        for (int i = 0; i < numCols; i++)
        {
            Rect oldRect = rects[i];
            float newXPos = oldRect.x + fastnoise.GetPerlin(i, oldRect.y) * speed;
            if (newXPos < transform.position.x) { newXPos = transform.position.x+maxWidth; }
            Rect newRect = new Rect(newXPos, oldRect.y, oldRect.width, oldRect.height);
            rects[i] = newRect;
            addRectToMesh(newRect);
        }
    }

    void fallingColumns(float noiseSpeed)
    {
        for (int i = 0; i < numCols; i++)
        {
            Rect oldRect = rects[i];
            float height = (Mathf.Abs(fastnoise.GetSimplex(oldRect.x * noiseSpeed + Time.time, oldRect.y * noiseSpeed * Time.time))) * maxHeight * 2;
            Rect newRect = new Rect(oldRect.x, maxHeight-height, oldRect.width, height);
            addRectToMesh(newRect);
        }
    }

    void initColumns()
    {
        float h = maxHeight;
        float zOrigin = transform.position.z;
        for (int i = 0; i < numCols; i++)
        {
            float w = Random.Range(0.2f, 3f);
            float xOrigin = Random.Range(0, maxWidth);
            Rect newRect = new Rect(xOrigin, zOrigin, w, h);
            rects.Add(newRect);
        }
    }

    void clearMesh()
    {
        mesh.Clear();
        verts.Clear();
        tris.Clear();
    }
    void updateMesh()
    {
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
    }
    void addRectToMesh(Rect r)
    {
        int numVerts = verts.Count;
        verts.Add(new Vector3(r.x, 0, r.y));
        verts.Add(new Vector3(r.x, 0, r.y + r.height));
        verts.Add(new Vector3(r.x + r.width, 0, r.y));
        verts.Add(new Vector3(r.x + r.width, 0, r.y + r.height));

        tris.Add(numVerts + 0);
        tris.Add(numVerts + 1);
        tris.Add(numVerts + 2);
        tris.Add(numVerts + 2);
        tris.Add(numVerts + 1);
        tris.Add(numVerts + 3);
    }
}
