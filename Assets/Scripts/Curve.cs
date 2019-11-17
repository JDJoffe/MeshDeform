using UnityEngine;
using System.Collections;
// get the curve information from customheart script  
public class Curve
{
    public Vector3[] points = new Vector3[3];
    public bool drawCurve = false;
    // paramaters that are set in the customheart script
    public Curve(Vector3 p0, Vector3 p1, Vector3 p2, bool tdraw)
    {
        drawCurve = tdraw;
        points = new Vector3[] { p0, p1, p2 };
        // Draw curve
        if (drawCurve)
        {
            int steps = 10;
            Vector3 start = GetPoint(0f);
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector3 end = GetPoint(t);
                Debug.DrawLine(start, end, Color.cyan, 10, true);
                start = end;
            }
        }
    }
    // values set in customheart displace vertices, float t becomes mathf.sqrt.magnitude
    public Vector3 GetPoint(float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return (oneMinusT * oneMinusT * points[0]) + (2f * oneMinusT * t * points[1]) 
                + (oneMinusT * oneMinusT * points[2]);
    }
}

