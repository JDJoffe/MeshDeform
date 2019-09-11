using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
[CustomEditor(typeof(MeshStudy))]
public class MeshInspector : Editor
{
    private MeshStudy mesh;
    private Transform handleTransform;
    private Quaternion handleRotation;
    string triangleIdx;

    void OnSceneGUI()
    {
        mesh = target as MeshStudy;
      //  Debug.Log("Custom editor is running");
        EditMesh();
    }

    void EditMesh()
    {
        handleTransform = mesh.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            ShowPoint(i);
        }
    }

    private void ShowPoint(int index)
    {
        if (mesh.moveVertexPoint)
        {
            //draw dot
            Vector3 point = handleTransform.TransformPoint(mesh.vertices[index]);
            Handles.color = Color.blue;
            //facilitate the dragging action
            point = Handles.FreeMoveHandle(point, handleRotation, mesh.handleSize, Vector3.zero, Handles.DotHandleCap);
            //drag
            if (GUI.changed)
            {
                mesh.DoAction(index,handleTransform.InverseTransformPoint(point));
            }
        }
        else
        {
            //click
        }
    }

    //button on script to run a function pretty handy
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        mesh = target as MeshStudy;

      
        if (GUILayout.Button("Reset"))
        {
            mesh.Reset();
        }
        // For testing Reset function
        if (mesh.isCloned)
        {
            if (GUILayout.Button("Test Edit"))
            {
                mesh.EditMesh();
            }
        }
    }


}
