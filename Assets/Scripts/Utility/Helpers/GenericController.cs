using UnityEngine;

/// <summary>
/// Genric controller base class for the controller-data pair.
/// </summary>
/// <typeparam name="TData">The type of the data object</typeparam>
public abstract class GenericController<TData> : MonoBehaviour
{
    public TData Data;
}
