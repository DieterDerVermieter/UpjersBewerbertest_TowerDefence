using UnityEngine;
using UnityEditor;

/// <summary>
/// Generic editor for a controller-data pair.
/// Draws a Button that generates and links a Data object to the Controller object.
/// </summary>
/// <typeparam name="TController">The type of the controller object</typeparam>
/// <typeparam name="TData">The type of the data object</typeparam>
public class GenericControllerEditor<TController, TData> : Editor where TController : GenericController<TData> where TData : GenericData<TController>
{
    public override void OnInspectorGUI()
    {
        DrawDataButton();
        base.OnInspectorGUI();
    }


    /// <summary>
    /// Draws a Button that generates and links a Data object to the Controller object.
    /// </summary>
    private void DrawDataButton()
    {
        if (GUILayout.Button("Generate Data"))
        {
            // Get the controller object
            var controller = (TController)target;
            if (controller == null)
            {
                Debug.LogError($"{target.name} is not of type {typeof(TController)}.");
                return;
            }

            // Get and adjust the path for the data object
            // The path of the data object will be the same as the controller object
            var path = AssetDatabase.GetAssetPath(target);
            path = System.IO.Path.GetDirectoryName(path) + $"\\{target.name}.asset";

            // Get the data object
            var data = AssetDatabase.LoadAssetAtPath<TData>(path);
            // or generate it
            if (data == null)
            {
                data = CreateInstance<TData>();

                AssetDatabase.CreateAsset(data, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"Generated Data for {controller.name} at {path}.");
            }

            // Link the controller and data objects
            controller.Data = data;
            data.ControllerPrefab = controller;

            // Save the changes
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(data);
        }
    }
}
