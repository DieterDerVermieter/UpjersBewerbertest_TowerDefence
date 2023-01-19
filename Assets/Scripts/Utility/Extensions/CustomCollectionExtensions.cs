using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomCollectionExtensions
{
    public static void Shuffle(this IList list)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            int j = Random.Range(i, list.Count);

            var tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }

    public static void AddAll<T>(this ICollection<T> collection, ICollection<T> other)
    {
        foreach (var item in other)
        {
            collection.Add(item);
        }
    }
}
