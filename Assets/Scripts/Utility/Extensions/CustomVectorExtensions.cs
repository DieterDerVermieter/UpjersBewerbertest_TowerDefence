using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomVectorExtensions
{
    public static Vector2 RotateDeg(this Vector2 v, float deg) => v.RotateRad(Mathf.Deg2Rad * deg);

    public static Vector2 RotateRad(this Vector2 v, float rad)
    {
        var sin = Mathf.Sin(rad);
        var cos = Mathf.Cos(rad);

        return v.x * new Vector2(cos, sin) + v.y * new Vector2(-sin, cos);
    }


    public static Vector3 RotateRad(this Vector3 v, float rad)
    {
        var sin = Mathf.Sin(rad);
        var cos = Mathf.Cos(rad);

        return v.x * new Vector3(cos, sin) + v.y * new Vector3(-sin, cos) + new Vector3(0, 0, v.z);
    }


    public static float InverseLerp(this Vector3 v, Vector3 a, Vector3 b)
    {
        var ab = b - a;
        var av = v - a;

        return Vector3.Dot(av, ab) / Vector3.Dot(ab, ab);
    }
}
