using UnityEngine;
using UnityEditor;
using GameFramework.Events.Channels;
using GameFramework.Core.Editor;

namespace GameFramework.Events.Editor
{
    /// <summary>
    /// Custom editor for GameEvent channels with test functionality.
    /// </summary>
    [CustomEditor(typeof(GameEvent))]
    public class GameEventEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.Event;
        protected override string ComponentIcon => "📡";
        protected override string ComponentName => "GAME EVENT";
        
        private GameEvent gameEvent;
        
        private void OnEnable()
        {
            gameEvent = target as GameEvent;
        }
        
        public override void OnInspectorGUI()
        {
            // Draw CLGF label at the top
            DrawCLGFLabel();
            
            // Begin background area
            Rect backgroundStartRect = GUILayoutUtility.GetRect(0, 0);
            float backgroundStartY = backgroundStartRect.y;
            
            // Draw default inspector
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            
            // Runtime controls
            if (Application.isPlaying)
            {
                DrawRuntimeControls();
            }
            else
            {
                DrawEditorControls();
            }
            
            // Draw colored background now that we know the actual height
            DrawColoredBackgroundActual(backgroundStartY);
        }
        
        private void DrawRuntimeControls()
        {
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Raise Event"))
            {
                gameEvent.RaiseEvent();
            }
            
            if (GUILayout.Button("Reset"))
            {
                gameEvent.Reset();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Show runtime statistics
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField("Raised Count", gameEvent.RaisedCount);
            EditorGUILayout.FloatField("Last Raised Time", gameEvent.LastRaisedTime);
            EditorGUILayout.IntField("Subscribers", gameEvent.GetSubscriberCount());
            EditorGUI.EndDisabledGroup();
        }
        
        private void DrawEditorControls()
        {
            EditorGUILayout.LabelField("Editor Controls", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Event can be raised during play mode.", MessageType.Info);
        }
    }
    
    /// <summary>
    /// Custom editor for typed event channels.
    /// </summary>
    [CustomEditor(typeof(IntEventChannel))]
    public class IntEventChannelEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.Event;
        protected override string ComponentIcon => "🔢";
        protected override string ComponentName => "INT EVENT CHANNEL";
        
        private IntEventChannel eventChannel;
        
        private void OnEnable()
        {
            eventChannel = target as IntEventChannel;
        }
        
        public override void OnInspectorGUI()
        {
            // Draw CLGF label at the top
            DrawCLGFLabel();
            
            // Begin background area
            Rect backgroundStartRect = GUILayoutUtility.GetRect(0, 0);
            float backgroundStartY = backgroundStartRect.y;
            
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            
            if (Application.isPlaying)
            {
                DrawRuntimeControls();
            }
            
            // Draw colored background now that we know the actual height
            DrawColoredBackgroundActual(backgroundStartY);
        }
        
        private void DrawRuntimeControls()
        {
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Raise with Test Value"))
            {
                // Use reflection to access private testValue field
                var testValueField = typeof(IntEventChannel).GetField("testValue", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (testValueField != null)
                {
                    int testValue = (int)testValueField.GetValue(eventChannel);
                    eventChannel.RaiseEvent(testValue);
                }
            }
            
            if (GUILayout.Button("Reset"))
            {
                eventChannel.Reset();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Show runtime statistics
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField("Raised Count", eventChannel.RaisedCount);
            EditorGUILayout.FloatField("Last Raised Time", eventChannel.LastRaisedTime);
            EditorGUILayout.TextField("Last Data", eventChannel.LastDataString);
            EditorGUILayout.IntField("Subscribers", eventChannel.GetSubscriberCount());
            EditorGUI.EndDisabledGroup();
        }
    }
    
    /// <summary>
    /// Custom editor for FloatEventChannel.
    /// </summary>
    [CustomEditor(typeof(FloatEventChannel))]
    public class FloatEventChannelEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.Event;
        protected override string ComponentIcon => "🔢";
        protected override string ComponentName => "FLOAT EVENT CHANNEL";
        
        private FloatEventChannel eventChannel;
        
        private void OnEnable()
        {
            eventChannel = target as FloatEventChannel;
        }
        
        public override void OnInspectorGUI()
        {
            // Draw CLGF label at the top
            DrawCLGFLabel();
            
            // Begin background area
            Rect backgroundStartRect = GUILayoutUtility.GetRect(0, 0);
            float backgroundStartY = backgroundStartRect.y;
            
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            
            if (Application.isPlaying)
            {
                DrawRuntimeControls();
            }
            
            // Draw colored background now that we know the actual height
            DrawColoredBackgroundActual(backgroundStartY);
        }
        
        private void DrawRuntimeControls()
        {
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Raise with Test Value"))
            {
                var testValueField = typeof(FloatEventChannel).GetField("testValue", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (testValueField != null)
                {
                    float testValue = (float)testValueField.GetValue(eventChannel);
                    eventChannel.RaiseEvent(testValue);
                }
            }
            
            if (GUILayout.Button("Reset"))
            {
                eventChannel.Reset();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Show runtime statistics
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField("Raised Count", eventChannel.RaisedCount);
            EditorGUILayout.FloatField("Last Raised Time", eventChannel.LastRaisedTime);
            EditorGUILayout.TextField("Last Data", eventChannel.LastDataString);
            EditorGUILayout.IntField("Subscribers", eventChannel.GetSubscriberCount());
            EditorGUI.EndDisabledGroup();
        }
    }
}