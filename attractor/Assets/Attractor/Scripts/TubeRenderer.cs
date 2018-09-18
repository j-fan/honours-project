using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class TubeRenderer : MonoBehaviour
{
    /*
    TubeRenderer.cs
 
    This script is created by Ray Nothnagel of Last Bastion Games. It is 
    free for use and available on the Unify Wiki.
 
    For other components I've created, see:
    http://lastbastiongames.com/middleware/
 
    (C) 2008 Last Bastion Games

    2018 - adapted to draw multiple tubes
    */

    [Serializable]
    public class TubeVertex
    {
        public Vector3 point = Vector3.zero;
        public float radius = 1.0f;
        public Color color = Color.white;

        public TubeVertex(Vector3 pt, float r, Color c)
        {
            point = pt;
            radius = r;
            color = c;
        }
    }

    public List<List<TubeVertex>> lines;
    public Material material;

    public int crossSegments = 3;
    private Vector3[] crossPoints;
    private int lastCrossSegments;
    public float flatAtDistance = -1;

    private Vector3 lastCameraPosition1;
    private Vector3 lastCameraPosition2;
    public int movePixelsForRebuild = 6;
    public float maxRebuildTime = 0.1f;
    private float lastRebuildTime = 0.00f;

    void Reset()
    {

        lines = new List<List<TubeVertex>>();
        lines.Add(new List<TubeVertex>
        {
            new TubeVertex(Vector3.zero, 1.0f,Color.white),
            new TubeVertex(new Vector3(1,0,0), 1.0f,Color.white),
        });
    }
    void Start()
    {
        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
        mr.material = material;
    }

    void LateUpdate()
    {

        for (int l = 0; l < lines.Count; l++)
        {
            List<TubeVertex> vertices = lines[l];

            if(vertices == null)
            {
                continue;
            }

            if (vertices.Count <= 1)
            {
                GetComponent<Renderer>().enabled = false;
                return;
            }
            GetComponent<Renderer>().enabled = true;

            //rebuild the mesh?
            bool re = false;
            float distFromMainCam;
            if (vertices.Count > 1)
            {
                Vector3 cur1 = Camera.main.WorldToScreenPoint(vertices[0].point);
                distFromMainCam = lastCameraPosition1.z;
                lastCameraPosition1.z = 0;
                Vector3 cur2 = Camera.main.WorldToScreenPoint(vertices[vertices.Count - 1].point);
                lastCameraPosition2.z = 0;

                float distance = (lastCameraPosition1 - cur1).magnitude;
                distance += (lastCameraPosition2 - cur2).magnitude;

                if (distance > movePixelsForRebuild || Time.time - lastRebuildTime > maxRebuildTime)
                {
                    re = true;
                    lastCameraPosition1 = cur1;
                    lastCameraPosition2 = cur2;
                }
            }

            if (re)
            {
                //draw tube

                if (crossSegments != lastCrossSegments)
                {
                    crossPoints = new Vector3[crossSegments];
                    float theta = 2.0f * Mathf.PI / crossSegments;
                    for (int c = 0; c < crossSegments; c++)
                    {
                        crossPoints[c] = new Vector3(Mathf.Cos(theta * c), Mathf.Sin(theta * c), 0);
                    }
                    lastCrossSegments = crossSegments;
                }

                Vector3[] meshVertices = new Vector3[vertices.Count * crossSegments];
                Vector2[] uvs = new Vector2[vertices.Count * crossSegments];
                Color[] colors = new Color[vertices.Count * crossSegments];
                int[] tris = new int[vertices.Count * crossSegments * 6];
                int[] lastVertices = new int[crossSegments];
                int[] theseVertices = new int[crossSegments];
                Quaternion rotation = Quaternion.identity;

                for (int p = 0; p < vertices.Count; p++)
                {
                    if (p < vertices.Count - 1) rotation = Quaternion.FromToRotation(Vector3.forward, vertices[p + 1].point - vertices[p].point);

                    for (int c = 0; c < crossSegments; c++)
                    {
                        int vertexIndex = p * crossSegments + c;
                        meshVertices[vertexIndex] = vertices[p].point + rotation * crossPoints[c] * vertices[p].radius;
                        uvs[vertexIndex] = new Vector2((0.0f + c) / crossSegments, (0.0f + p) / vertices.Count);
                        colors[vertexIndex] = vertices[p].color;

                        //				print(c+" - vertex index "+(p*crossSegments+c) + " is " + meshVertices[p*crossSegments+c]);
                        lastVertices[c] = theseVertices[c];
                        theseVertices[c] = p * crossSegments + c;
                    }
                    //make triangles
                    if (p > 0)
                    {
                        for (int c = 0; c < crossSegments; c++)
                        {
                            int start = (p * crossSegments + c) * 6;
                            tris[start] = lastVertices[c];
                            tris[start + 1] = lastVertices[(c + 1) % crossSegments];
                            tris[start + 2] = theseVertices[c];
                            tris[start + 3] = tris[start + 2];
                            tris[start + 4] = tris[start + 1];
                            tris[start + 5] = theseVertices[(c + 1) % crossSegments];
                            //					print("Triangle: indexes("+tris[start]+", "+tris[start+1]+", "+tris[start+2]+"), ("+tris[start+3]+", "+tris[start+4]+", "+tris[start+5]+")");
                        }
                    }
                }

               
                Mesh mesh = new Mesh();
                mesh.vertices = meshVertices;
                mesh.triangles = tris;
                mesh.RecalculateNormals();
                mesh.uv = uvs;

                Graphics.DrawMesh(mesh,Vector3.zero,Quaternion.identity,material,0);

            }
        }
    }
   
 
}