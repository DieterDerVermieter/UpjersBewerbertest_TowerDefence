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
}
