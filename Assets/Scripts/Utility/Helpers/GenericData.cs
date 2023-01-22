using UnityEngine;

/// <summary>
/// Genric data base class for the controller-data pair.
/// </summary>
/// <typeparam name="TData">The type of the controller object</typeparam>
public class GenericData<TController> : ScriptableObject
{
    public TController ControllerPrefab;
}
