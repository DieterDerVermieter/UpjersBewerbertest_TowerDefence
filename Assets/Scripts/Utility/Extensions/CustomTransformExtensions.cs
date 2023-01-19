using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomTransformExtensions
{
    public static void ForEachChild(this Transform t, System.Action<Transform> action, bool recursive=false)
    {
        for (int i = 0; i < t.childCount; i++)
        {
            var child = t.GetChild(i);
            action?.Invoke(child);

            if (recursive) child.ForEachChild(action, recursive);
        }
    }

    public static void DestroyChildren(this Transform t)
    {
        t.ForEachChild(child => Object.Destroy(child.gameObject));
    }
}
