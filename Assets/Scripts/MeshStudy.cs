using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MeshStudy : MonoBehaviour
{
    Mesh oMesh;
    Mesh cMesh;
    MeshFilter oMeshFilter;
    int[] triangles;

    //hides in inspector good for complex meshes
   [HideInInspector]
    public Vector3[] vertices;

    [HideInInspector]
    public bool isCloned = false;

    // For Editor
    public float radius = 0.2f;
    public float pull = 0.3f;
    public float handleSize = 0.03f;
    public List<int>[] connectedVertices;
    public List<Vector3[]> allTriangleList;
    public bool moveVertexPoint = true;

    [ExecuteInEditMode]
    void Start()
    {
        InitMesh();
        Reset();
    }
    // call on start
    public void InitMesh()
    {
        oMeshFilter = GetComponent<MeshFilter>();
        oMesh = oMeshFilter.sharedMesh;

        // assign copied mesh to mesh filter
        cMesh = new Mesh();
        cMesh.name = "clone";
        cMesh.vertices = oMesh.vertices;
        cMesh.normals = oMesh.normals;
        cMesh.uv = oMesh.uv;
        oMeshFilter.mesh = cMesh;
       // set var to be the cmesh var
        vertices = cMesh.vertices;
        triangles = cMesh.triangles;
        isCloned = true;
        Debug.Log("init & cloned");

    }

    public void Reset()
    {
        // if cmesh exists reset all it's values
        if (cMesh != null && oMesh != null)
        {
            cMesh.vertices = oMesh.vertices;
            cMesh.triangles = oMesh.triangles;
            cMesh.normals = oMesh.normals;
            cMesh.uv = oMesh.uv;
            oMeshFilter.mesh = cMesh;

            vertices = cMesh.vertices;
            triangles = cMesh.triangles;
        }
    }

    public void GetConnectedVertices()
    {
        connectedVertices = new List<int>[vertices.Length];
    }
    // call function on click
    public void DoAction(int index, Vector3 localPos)
    {
        // specify methods here
        // comment our if you dont want meshes to break
        //PullOneVertex(index, localPos);

        PullSimilarVertices(index, localPos);
    }

    // returns List of int that is related to the targetPt.
    private List<int> FindRelatedVertices(Vector3 targetPt, bool findConnected)
    {
        // list of int
        List<int> relatedVertices = new List<int>();

        int idx = 0;
        Vector3 pos;

        // loop through triangle array of indices
        for (int t = 0; t < triangles.Length; t++)
        {
            // current idx return from tris
            idx = triangles[t];
            // current pos of the vertex
            pos = vertices[idx];
            // if current pos is same as targetPt
            if (pos == targetPt)
            {
                // add to list
                relatedVertices.Add(idx);
                // if find connected vertices
                if (findConnected)
                {
                    // min
                    // - prevent running out of count
                    if (t == 0)
                    {
                        relatedVertices.Add(triangles[t + 1]);
                    }
                    // max 
                    // - prevent runnign out of count
                    if (t == triangles.Length - 1)
                    {
                        relatedVertices.Add(triangles[t - 1]);
                    }
                    // between 1 ~ max-1 
                    // - add idx from triangles before t and after t 
                    if (t > 0 && t < triangles.Length - 1)
                    {
                        relatedVertices.Add(triangles[t - 1]);
                        relatedVertices.Add(triangles[t + 1]);
                    }
                }
            }
        }
        // return compiled list of int
        return relatedVertices;
    }

    public void BuildTriangleList()
    {
    }

    public void ShowTriangle(int idx)
    {
    }

    // Pulling only one vertex pt, results in broken mesh.
    private void PullOneVertex(int index, Vector3 newPos)
    {
        vertices[index] = newPos;
        cMesh.vertices = vertices;
        cMesh.RecalculateNormals();
    }

   
    // pull the vertexes around the v3 point to stop mesh breaking
    private void PullSimilarVertices(int index, Vector3 newPos)
    {
        // get target vertex position
        Vector3 targetVertexPos = vertices[index];
        // return list of vertices that share the same position as target vertex
        List<int> relatedVertices = FindRelatedVertices(targetVertexPos, false);
        // loop through whole list and update their position
        foreach (int i in relatedVertices)
        {
            vertices[i] = newPos;
        }
        // assign updated vertices
        cMesh.vertices = vertices;
        // redraw mesh
        cMesh.RecalculateNormals();
    }

    // To test Reset function
    public void EditMesh()
    {
        vertices[2] = new Vector3(2, 3, 4);
        vertices[3] = new Vector3(1, 2, 4);
        cMesh.vertices = vertices;
        cMesh.RecalculateNormals();
    }


}
