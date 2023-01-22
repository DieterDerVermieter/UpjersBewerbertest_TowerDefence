using UnityEngine;
using UnityEditor;

/// <summary>
/// Draws a dropdown for a scriptableObject property.
/// This is useful for the controller-object pair, because you can edit the data from the prefab view.
/// </summary>
[CustomPropertyDrawer(typeof(ExposedScriptableObjectAttribute), true)]
public class ExposedScriptableObjectAttributeDrawer : PropertyDrawer
{
    // Cached scriptable object editor
    private Editor m_editor = null;


    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Draw the property
        EditorGUI.PropertyField(position, property, label, true);

        // Exit, if the scriptableObject is null
        if (property.objectReferenceValue == null)
            return;

        // Draw the foldout arrow
        if (property.objectReferenceValue != null)
        {
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none);
        }

        // Draw the foldout properties
        if (property.isExpanded)
        {
            // Make child fields be indented
            EditorGUI.indentLevel++;

            // Draw object properties
            if (m_editor == null)
                Editor.CreateCachedEditor(property.objectReferenceValue, null, ref m_editor);
            m_editor.OnInspectorGUI();

            // Set indent back to what it was
            EditorGUI.indentLevel--;
        }
    }
}
