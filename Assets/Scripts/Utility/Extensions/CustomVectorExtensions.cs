using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomVectorExtensions
{
    /// <summary>
    /// Rotate a <see cref="Vector3"/> on the xy-plane.
    /// </summary>
    /// <param name="v">Vector to rotate</param>
    /// <param name="rad">Amount to rotate</param>
    /// <returns>The rotated vector</returns>
    public static Vector3 RotateRad(this Vector3 v, float rad)
    {
        var sin = Mathf.Sin(rad);
        var cos = Mathf.Cos(rad);

        return v.x * new Vector3(cos, sin) + v.y * new Vector3(-sin, cos) + new Vector3(0, 0, v.z);
    }


    /// <summary>
    /// The inverse linear interpolation between two vectors.
    /// </summary>
    /// <param name="v">The target vector</param>
    /// <param name="a">Starting point</param>
    /// <param name="b">End point</param>
    /// <returns>The inverseLerp of v between a and b</returns>
    public static float InverseLerp(this Vector3 v, Vector3 a, Vector3 b)
    {
        var ab = b - a;
        var av = v - a;

        return Vector3.Dot(av, ab) / Vector3.Dot(ab, ab);
    }
}
