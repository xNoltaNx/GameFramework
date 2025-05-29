// DEPRECATED: Legacy camera effect settings editor - replaced with Cinemachine workflow
// This file contained the custom editor for the previous camera effects configuration system
//
// Previous Features:
// - Custom inspector for CameraEffectSettings
// - Preset system UI with apply buttons
// - Toggle between legacy and comprehensive preset modes
// - Property field editors for all camera effect parameters
//
// TODO: Replace with Cinemachine workflow:
// - Use built-in Cinemachine virtual camera inspectors
// - Configure noise settings directly on virtual cameras
// - Set up impulse source configurations
// - Create custom editor tools for virtual camera state management if needed

using UnityEngine;
using UnityEditor;

namespace GameFramework.Camera
{
#if UNITY_EDITOR
    [System.Obsolete("CameraEffectSettingsEditor is deprecated. Use Cinemachine virtual camera inspectors instead.")]
    [CustomEditor(typeof(CameraEffectSettings))]
    public class CameraEffectSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "This camera effects system has been deprecated.\n\n" +
                "Please replace with Cinemachine virtual cameras:\n" +
                "1. Install Cinemachine package\n" +
                "2. Create Cinemachine Brain on main camera\n" +
                "3. Set up virtual cameras for movement states\n" +
                "4. Configure noise profiles for head bob\n" +
                "5. Use impulse sources for camera shake", 
                MessageType.Warning);
                
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Open Cinemachine Documentation"))
            {
                Application.OpenURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@2.8/manual/index.html");
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Legacy Settings (Deprecated)", EditorStyles.boldLabel);
            
            // Still show the default inspector for reference but with warning
            GUI.enabled = false;
            DrawDefaultInspector();
            GUI.enabled = true;
        }
    }
#endif
}