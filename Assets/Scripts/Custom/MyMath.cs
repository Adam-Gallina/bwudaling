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