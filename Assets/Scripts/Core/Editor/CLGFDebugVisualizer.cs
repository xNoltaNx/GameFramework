using UnityEngine;
using UnityEditor;

namespace GameFramework.Core.Editor
{
    /// <summary>
    /// Debug visualizer for testing CLGF gizmo system.
    /// Shows basic gizmos to verify the system is working.
    /// </summary>
    [InitializeOnLoad]
    public static class CLGFDebugVisualizer
    {
        static CLGFDebugVisualizer()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }
        
        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!CLGFVisualizationSettings.ShowSceneGizmos)
                return;
            
            // Draw debug information in the scene view
            if (CLGFVisualizationSettings.DebugMode)
            {
                Handles.BeginGUI();
                
                GUILayout.BeginArea(new Rect(10, 10, 300, 200));
                GUILayout.BeginVertical("box");
                
                GUILayout.Label("CLGF Visualization Debug", EditorStyles.boldLabel);
                GUILayout.Label($"Gizmo Size: {CLGFVisualizationSettings.GizmoSize:F1}");
                GUILayout.Label($"Line Thickness: {CLGFVisualizationSettings.LineThickness:F1}");
                GUILayout.Label($"Scene Gizmos: {CLGFVisualizationSettings.ShowSceneGizmos}");
                GUILayout.Label($"Action Previews: {CLGFVisualizationSettings.ShowActionPreviews}");
                GUILayout.Label($"Connection Lines: {CLGFVisualizationSettings.ShowConnectionLines}");
                
                // Highlight Always Show mode
                if (CLGFVisualizationSettings.AlwaysShow)
                {
                    GUI.backgroundColor = Color.green;
                    GUILayout.Label("🌐 ALWAYS SHOW MODE ACTIVE", EditorStyles.boldLabel);
                    GUI.backgroundColor = Color.white;
                }
                else
                {
                    GUILayout.Label("Always Show: Off");
                }
                
                if (GUILayout.Button("Reset to Defaults"))
                {
                    CLGFVisualizationSettings.ResetToDefaults();
                }
                
                GUILayout.EndVertical();
                GUILayout.EndArea();
                
                Handles.EndGUI();
            }
        }
    }
}