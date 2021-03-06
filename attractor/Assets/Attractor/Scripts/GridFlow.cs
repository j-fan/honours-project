﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *  procedural grid adapted from cat-like coding
 *  https://catlikecoding.com/unity/tutorials/procedural-grid/
 */

[RequireComponent (typeof(MeshFilter),typeof(MeshRenderer))]

public class GridFlow : MonoBehaviour {
    public Targets targets;
    public AudioSource audioSource;
    public int xSize = 10;
    public int ySize = 20;
    public float cellSize = 10;
    public float targetRadius = 10.0f;
    public float noiseScale = 0.07f;
    public float brightness = 0.5f;

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    Vector3 gridOffset;

    public BeatsFFT beatsFFT;

    void Awake () {
        mesh = GetComponent<MeshFilter>().mesh;
	}
	
    void Start()
    {
        gridOffset = GetComponent<Transform>().position;
        //gridOffset = gridOffset + new Vector3(cellSize  , 0, cellSize );  
        updateMesh();
        makeGrid();
    }

    private void OnDisable()
    {
        updateMesh();
    }

    private void OnEnable()
    {
        updateMesh();
        makeGrid();
    }

    void Update () {

        Vector3[] verts = mesh.vertices;
        Color[] colours = new Color[vertices.Length];

        for (int i=0; i < verts.Length; i++)
        {
            Vector3 vert = verts[i];
            //displaces vertices with noise
            verts[i].y = Mathf.PerlinNoise(i * noiseScale, Time.time) * 10;
            //augment the noise with sound
            verts[i].y = verts[i].y * Mathf.Clamp(beatsFFT.runningAvgFreq * 500,0.0f,4.0f);
            //raise region around targets
            foreach (GameObject a in targets.getTargets())
            {
                float distance = Vector3.Distance(a.transform.position, vert);
                float height = 0.0f;
                if (distance < targetRadius)
                {
                    height = (targetRadius - distance + vert.y) * 0.5f;
                    verts[i] = new Vector3(vert.x, height, vert.z);
                } 
                height = verts[i].y - 0.2f;
                if (height < 0.0f)
                {
                    height = 0.0f;
                }
                verts[i] = new Vector3(vert.x, height, vert.z);
                colours[i] = Color.HSVToRGB((float)i / xSize / ySize, 1.0f, brightness);
            }
        }
        mesh.vertices = verts;
        mesh.colors = colours;
		
	}

    private void makeGrid()
    {
        //generate geometry & UVs
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Grid";

        vertices = new Vector3[(xSize + 1) * (ySize + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        Color[] colours = new Color[vertices.Length];

        for (int i = 0, y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                vertices[i] = new Vector3(x*cellSize, 0, y*cellSize) + gridOffset;
                uv[i] = new Vector2((float)x , (float)y );
                colours[i] = Color.HSVToRGB((float)y/ySize, 1.0f,brightness) ;

            }
        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.colors = colours;

        triangles = new int[xSize * ySize * 6];
        for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
        {
            for (int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 5] = vi + xSize + 2;
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

    }
    void updateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

}
