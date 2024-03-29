using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomHeart : MonoBehaviour
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

    // For Editor
    public enum EditType
    {
        AddIndices, RemoveIndices, None
    }

    public EditType editType;

    public bool showTransformHandle = true;
    public List<int> selectedIndices = new List<int>();
    public float pickSize = 0.01f;

    // Deforming settings
    public float radiusofeffect = 0.3f;
    public float pullvalue = 0.3f;

    // Animation settings
    public float duration = 1.2f;
    bool isAnimate = false;
    float starttime = 0f;
    float runtime = 0f;
    int currentIndex = 0;

    // swap between curve types
    public enum CurveType
    {
        Curve1,
        Curve2
    }
    // reference to curve script
    public CurveType curveType;
    Curve curve;
    void Start()
    {
        Init();
    }
    // setup mesh
    public void Init()
    {
        oFilter = GetComponent<MeshFilter>();
        currentIndex = 0;
        // if you are addint indices or removing indices
        if (editType == EditType.AddIndices || editType == EditType.RemoveIndices)
        {
            oMesh = oFilter.sharedMesh;
            cMesh = new Mesh();
            cMesh.name = "clone";
            cMesh.vertices = oMesh.vertices;
            cMesh.triangles = oMesh.triangles;
            cMesh.normals = oMesh.normals;
            oFilter.mesh = cMesh;
            // update local vars...
            oVertices = cMesh.vertices;

            normals = cMesh.normals;
            Debug.Log("Init & Cloned");
        }
        else
        {
            oMesh = oFilter.sharedMesh;
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
    // change displacement
    public void StartDisplacement()
    {
        targetVertex = mVertices[selectedIndices[currentIndex]];
        starttime = Time.time;
        isAnimate = true;
        // check the curvetype enum it is set to in the inspector and perform that curve
        if (curveType == CurveType.Curve1)
        {
            CurveType1();
        }
        else if (curveType == CurveType.Curve2)
        {
            CurveType2();
        }
    }

    void FixedUpdate()
    {
        // dont run if isanimate isnt set to true from start
        if (!isAnimate)
        {
            return;
        }

        runtime = Time.time - starttime;
        // if runtime is less than 1.2f
        if (runtime < duration)
        {
            Vector3 relativePoint = oFilter.transform.InverseTransformPoint(targetVertex);
            // displace vertices pos = mesh point that is targeted position
            DisplaceVertices(relativePoint, pullvalue, radiusofeffect);
        }
         
        else
        {
            // run start displacement
            currentIndex++;
            if (currentIndex < selectedIndices.Count)
            {
                StartDisplacement();
                Debug.Log("next");
            }
            // set the object to be inanimate
            else
            {
                oMesh = GetComponent<MeshFilter>().sharedMesh;
                isAnimate = false;
                Debug.Log("done");
            }
        }
    }

    void DisplaceVertices(Vector3 pos, float force, float radius)
    {
        Vector3 vert = Vector3.zero;
        float sqrRadius = radius * radius;

        for (int i = 0; i < mVertices.Length; i++)
        {
            // sqrmag = current vertice selected - the position of that vertice.sqrmagnitude
            float sqrMagnitude = (mVertices[i] - pos).sqrMagnitude;
            if (sqrMagnitude > sqrRadius)
            {
                // increment i and dont run what is after continue
                continue;
            }
            vert = mVertices[i];

            float distance = Mathf.Sqrt(sqrMagnitude);
            // get curve pos by the distance and multiply y by force to get
            // the value it will increment by
            // v3 to store increment info and apply the transform to mvertices[i]
            // set the increment to be the distance of getpoint in curve * by force
            float increment = curve.GetPoint(distance).y * force; 
            Vector3 translate = (vert * increment) * Time.deltaTime; 
            Quaternion rotation = Quaternion.Euler(translate);
            Matrix4x4 m = Matrix4x4.TRS(translate, rotation, Vector3.one);
            mVertices[i] = m.MultiplyPoint3x4(mVertices[i]);
        }
        oMesh.vertices = mVertices;
        oMesh.RecalculateNormals();
    }
    // clear all selected indices & reset 
    public void ClearAllData()
    {
        selectedIndices = new List<int>();
        targetIndex = 0;
        targetVertex = Vector3.zero;
    }
    // change curve values in curve script
    void CurveType1()
    {
        //  \ (0,1,0)   
        //   \
        //    \ (.5,.5,0)
        //     \
        //      \ (1,0,0)
        // negative linear regression 
        // curve points are coords the line goes to.
        // this line will go diagonally down 
        Vector3[] curvePoints = new Vector3[3];
        // start point at top left
        curvePoints[0] = new Vector3(0, 1, 0);
        // mid point at the middle of graph
        curvePoints[1] = new Vector3(.5f, .5f, 0);
        // end  point at bottom right
        curvePoints[2] = new Vector3(1, 0, 0);
        // tdraw parameter false, previous drawn curve can be seen it tdraw parameter is true
        curve = new Curve(curvePoints[0], curvePoints[1], curvePoints[2], false);
    }
    // change curve values in curve script
    void CurveType2()
    {
        //      _ (.5,1,0)
        //     / \
        //    /   \
        //   /     \
        //  / zero  \ (1,0,0)
        // concave down parabola 
        Vector3[] curvepoints = new Vector3[3];
        // start point on bottom left
        curvepoints[0] = new Vector3(0, 0, 0);
        // mid point at top middle
        curvepoints[1] = new Vector3(0.5f, 1, 0);
        // end point at bottom right
        curvepoints[2] = new Vector3(1, 0, 0);
        curve = new Curve(curvepoints[0], curvepoints[1], curvepoints[2], false); 
    }

    public void ShowNormals()
    {
        for (int i = 0; i < mVertices.Length; i++)
        {
            Debug.DrawLine(transform.TransformPoint(mVertices[i]), transform.TransformPoint(normals[i]), Color.green, 4.0f, false);
        }
    }
}

