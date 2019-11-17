using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeartMesh : MonoBehaviour
{
    Mesh oMesh;
    Mesh cMesh;
    MeshFilter oFilter;

    [HideInInspector]
    public int targetIndex;

    [HideInInspector]
    public Vector3 targetVertex;

    [HideInInspector]
    public Vector3[] oVertices;

    [HideInInspector]
    public Vector3[] mVertices;

    [HideInInspector]
    public Vector3[] normals;

    [HideInInspector]
    public bool isMeshReady = false;
    public bool isEditMode = true;
    public bool showTransformHandle = true;
    // selected positions
    public List<int> selectedIndices = new List<int>();
    public float pickSize = 0.01f;

    public float radiusOfEffect = 0.3f;
    public float pullValue = 0.3f;
    public float duration = 1.2f;
    int currentIndex = 0;
    bool isAnimate = false;
    float startTime = 0f;
    float runTime = 0f;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        oFilter = GetComponent<MeshFilter>();
        isMeshReady = false;

        currentIndex = 0;
        // if in editmode in unity
        if (isEditMode)
        {
            oMesh = oFilter.sharedMesh;
            cMesh = new Mesh();
            cMesh.name = "clone";
            cMesh.vertices = oMesh.vertices;
            cMesh.triangles = oMesh.triangles;
            cMesh.normals = oMesh.normals;
            oFilter.mesh = cMesh;

            oVertices = cMesh.vertices;
            normals = cMesh.normals;
            Debug.Log("Init & Cloned");
        }
        // if not in editmode run startdisplacement and get all meshvertiices
        else
        {
            oMesh = oFilter.mesh;
            oVertices = oMesh.vertices;
            normals = oMesh.normals;
            mVertices = new Vector3[oVertices.Length];
            for (int i = 0; i < oVertices.Length; i++)
            {
                mVertices[i] = oVertices[i];
            }
            StartDisplacement();
        }

    }
    // set set vars
    public void StartDisplacement()
    {
        targetVertex = oVertices[selectedIndices[currentIndex]];
        startTime = Time.time;
        isAnimate = true;
    }

   void FixedUpdate()
    {
        if (!isAnimate)
        {
            return;
        }
        //how long you ran it for
        runTime = Time.time - startTime;
        //if runtime is within duration limit get positions of targetvertex and displacevertices around targetvertex
        if (runTime < duration)
        {
            Vector3 targetVertexPos = oFilter.transform.InverseTransformPoint(targetVertex);
            DisplaceVertices(targetVertexPos, pullValue, radiusOfEffect);
        }
        //check if the index is within the num of selected indices if true move on to next vertex 
        else
        {
            currentIndex++;
            if (currentIndex < selectedIndices.Count)
            {
                StartDisplacement();
            }
            // or at the end of the list update omesh with the current mesh and stop the animation
            else
            {
                oMesh = GetComponent<MeshFilter>().mesh;
                isAnimate = false;
                isMeshReady = true;
            }
        }
    }
    // values set in fixed update if the runtime is less than duration
    void DisplaceVertices(Vector3 targetVertexPos, float force, float radius)
    {
        Vector3 currentVertexPos = Vector3.zero;
        float sqrRadius = radius * radius;
        // run through all mvertices
        for (int i = 0; i < mVertices.Length; i++)
        {
            currentVertexPos = mVertices[i];
            // target vertex pos is gotten from fixed update
            // sqrmag = the cur vertex position - the target vertex pos.magnitude
            float sqrMagnitude = (currentVertexPos - targetVertexPos).sqrMagnitude;
            if (sqrMagnitude > sqrRadius)
            {
                continue;               
            }
            // if sqrmag more than sqrrad set gaussfalloff's distance to be mathf.sqrt of sqr mag
            float distance = Mathf.Sqrt(sqrMagnitude);
            // maybe make a enum to swap between the different falloff types??
            float falloff = GaussFalloff(distance, radius);
            Vector3 translate = (currentVertexPos * force) * falloff; //6
            translate.z = 0f;
            Quaternion rotation = Quaternion.Euler(translate);
            Matrix4x4 m = Matrix4x4.TRS(translate, rotation, Vector3.one);
            mVertices[i] = m.MultiplyPoint3x4(currentVertexPos);
        }
        oMesh.vertices = mVertices;
        oMesh.RecalculateNormals();
    }

    public void ClearAllData()
    {
        // clear all values
        selectedIndices = new List<int>();
        targetIndex = 0;
        targetVertex = Vector3.zero;
    }

    public Mesh SaveMesh()
    {
        Mesh nMesh = new Mesh();

        return nMesh;
    }

    #region HELPER FUNCTIONS

    static float LinearFalloff(float dist, float inRadius)
    {
        return Mathf.Clamp01(0.5f + (dist / inRadius) * 0.5f);
    }

    static float GaussFalloff(float dist, float inRadius)
    {
        return Mathf.Clamp01(Mathf.Pow(360, -Mathf.Pow(dist / inRadius, 2.5f) - 0.01f));
    }

    static float NeedleFalloff(float dist, float inRadius)
    {
        return -(dist * dist) / (inRadius * inRadius) + 1.0f;
    }

    #endregion
}
