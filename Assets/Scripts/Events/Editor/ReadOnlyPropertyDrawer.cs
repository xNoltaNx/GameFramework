using UnityEngine;
using UnityEditor;
using GameFramework.Core;

namespace GameFramework.Events.Editor
{
    /// <summary>
    /// Property drawer for ReadOnly attribute to make fields read-only in the inspector.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Save the current GUI enabled state
            bool wasEnabled = GUI.enabled;
            
            // Disable the GUI for this property
            GUI.enabled = false;
            
            // Draw the property field
            EditorGUI.PropertyField(position, property, label, true);
            
            // Restore the GUI enabled state
            GUI.enabled = wasEnabled;
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Return the default height for the property
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}