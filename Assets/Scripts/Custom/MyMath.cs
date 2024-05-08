using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyMath
{
    public static Vector2 Rotate(Vector2 v, float ang)
    {
        float sa = Mathf.Sin(Mathf.Deg2Rad * ang);
        float ca = Mathf.Cos(Mathf.Deg2Rad * ang);
        return new Vector2(ca * v.x - sa * v.y,
                           sa * v.x + ca * v.y);
    }
    public static Vector3 RotateAboutY(Vector3 v, float ang)
    {
        float sa = Mathf.Sin(Mathf.Deg2Rad * ang);
        float ca = Mathf.Cos(Mathf.Deg2Rad * ang);
        return new Vector3(ca * v.x - sa * v.z,
                           0,
                           sa * v.x + ca * v.z);
    }


    public static Vector3 SampleBezierPoint(Vector3 c0, Vector3 c1, Vector3 c2, Vector3 c3, float t)
    {
        Vector3 q0 = Vector3.Lerp(c0, c1, t);
        Vector3 q1 = Vector3.Lerp(c1, c2, t);
        Vector3 q2 = Vector3.Lerp(c2, c3, t);

        Vector3 r0 = Vector3.Lerp(q0, q1, t);
        Vector3 r1 = Vector3.Lerp(q1, q2, t);

        return Vector3.Lerp(r0, r1, t);
    }
}

[System.Serializable]
public class RangeI
{
    public int minVal;
    public int maxVal;
    public int RandomVal { get { return Random.Range(minVal, maxVal); } }

    public RangeI(int minVal, int maxVal) { this.minVal = minVal; this.maxVal = maxVal; }

    public int PercentVal(float t) { return minVal + (int)((maxVal - minVal) * t); }
    public float PercentOfRange(int val) { return (float)(val - minVal) / (maxVal - minVal); }
}

[System.Serializable]
public class RangeF
{
    public float minVal;
    public float maxVal;
    public float RandomVal { get { return Random.Range(minVal, maxVal); } }

    public RangeF(float min, float max) { minVal = min; maxVal = max; }

    public float PercentVal(float t) { return minVal + (maxVal - minVal) * t; }
    public float PercentOfRange(float val) { return (val - minVal) / (maxVal - minVal); }
}

public class Bezier
{
    public Vector3 start;
    public Vector3 startConstraint;
    public Vector3 end;
    public Vector3 endConstraint;

    public float length { get; private set; }


    public void MoveEnd(Vector3 end, Vector3? endConstraint = null)
    {
        this.end = end;
        if (endConstraint != null)
            this.endConstraint = (Vector3)endConstraint;
    }

    public Bezier(Vector3 start, Vector3 startConstraint, Vector3 end, Vector3 endConstraint)
    {
        this.start = start;
        this.startConstraint = startConstraint;
        this.end = end;
        this.endConstraint = endConstraint;
    }

    public Vector3 Sample(float t)
    {
        return MyMath.SampleBezierPoint(start, start + startConstraint, end, end + endConstraint, t);
    }

    public float ApproximateLength(int samples)
    {
        length = 0;
        Vector3 lastPos = start;
        for (int i = 1; i < samples; i++)
        {
            Vector3 pos = Sample((float)i / samples);
            length += Vector3.Distance(lastPos, pos);
            lastPos = pos;
        }

        return length;
    }
}