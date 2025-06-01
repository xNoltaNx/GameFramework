using UnityEngine;
using UnityEditor;
using GameFramework.Events.Triggers;
using GameFramework.Core.Editor;

namespace GameFramework.Events.Editor
{
    /// <summary>
    /// Custom editor for CollisionTrigger with conditional property display.
    /// </summary>
    [CustomEditor(typeof(CollisionTrigger))]
    public class CollisionTriggerEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.Action;
        protected override string ComponentIcon => "⚡";
        protected override string ComponentName => "COLLISION TRIGGER";
        
        private SerializedProperty showTriggerSettingsProp;
        private SerializedProperty showCollisionSettingsProp;
        
        // Trigger Settings
        private SerializedProperty isActiveProp;
        private SerializedProperty canRepeatProp;
        private SerializedProperty cooldownTimeProp;
        private SerializedProperty conditionsProp;
        private SerializedProperty requireAllConditionsProp;
        private SerializedProperty onTriggeredProp;
        private SerializedProperty onEnabledProp;
        private SerializedProperty onDisabledProp;
        private SerializedProperty debugModeProp;
        
        // Collision Settings
        private SerializedProperty triggerEventProp;
        private SerializedProperty triggerLayersProp;
        private SerializedProperty requiredTagProp;
        private SerializedProperty requireRigidbodyProp;
        private SerializedProperty onObjectEnteredProp;
        private SerializedProperty onObjectExitedProp;
        private SerializedProperty onObjectStayingProp;
        
        private void OnEnable()
        {
            // Inspector Display
            showTriggerSettingsProp = serializedObject.FindProperty("showTriggerSettings");
            showCollisionSettingsProp = serializedObject.FindProperty("showCollisionSettings");
            
            // Base Trigger Settings
            isActiveProp = serializedObject.FindProperty("isActive");
            canRepeatProp = serializedObject.FindProperty("canRepeat");
            cooldownTimeProp = serializedObject.FindProperty("cooldownTime");
            conditionsProp = serializedObject.FindProperty("conditions");
            requireAllConditionsProp = serializedObject.FindProperty("requireAllConditions");
            onTriggeredProp = serializedObject.FindProperty("onTriggered");
            onEnabledProp = serializedObject.FindProperty("onEnabled");
            onDisabledProp = serializedObject.FindProperty("onDisabled");
            debugModeProp = serializedObject.FindProperty("debugMode");
            
            // Collision Settings
            triggerEventProp = serializedObject.FindProperty("triggerEvent");
            triggerLayersProp = serializedObject.FindProperty("triggerLayers");
            requiredTagProp = serializedObject.FindProperty("requiredTag");
            requireRigidbodyProp = serializedObject.FindProperty("requireRigidbody");
            onObjectEnteredProp = serializedObject.FindProperty("onObjectEntered");
            onObjectExitedProp = serializedObject.FindProperty("onObjectExited");
            onObjectStayingProp = serializedObject.FindProperty("onObjectStaying");
        }
        
        public override void OnInspectorGUI()
        {
            // Draw CLGF label at the top
            DrawCLGFLabel();
            
            // Begin background area
            Rect backgroundStartRect = GUILayoutUtility.GetRect(0, 0);
            float backgroundStartY = backgroundStartRect.y;
            
            serializedObject.Update();
            
            // Inspector Display toggles
            EditorGUILayout.LabelField("Inspector Display", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(showTriggerSettingsProp, new GUIContent("Show Trigger Settings"));
            EditorGUILayout.PropertyField(showCollisionSettingsProp, new GUIContent("Show Collision Settings"));
            
            EditorGUILayout.Space();
            
            // Trigger Settings Section
            if (showTriggerSettingsProp.boolValue)
            {
                EditorGUILayout.LabelField("Trigger Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(isActiveProp);
                EditorGUILayout.PropertyField(canRepeatProp);
                EditorGUILayout.PropertyField(cooldownTimeProp);
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Conditions", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(conditionsProp);
                EditorGUILayout.PropertyField(requireAllConditionsProp);
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Unity Events", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(onTriggeredProp);
                EditorGUILayout.PropertyField(onEnabledProp);
                EditorGUILayout.PropertyField(onDisabledProp);
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(debugModeProp);
                
                EditorGUILayout.Space();
            }
            
            // Collision Settings Section
            if (showCollisionSettingsProp.boolValue)
            {
                EditorGUILayout.LabelField("Collision Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(triggerEventProp);
                EditorGUILayout.PropertyField(triggerLayersProp);
                EditorGUILayout.PropertyField(requiredTagProp);
                EditorGUILayout.PropertyField(requireRigidbodyProp);
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Collision Events", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("These events fire for their respective collision types regardless of the Trigger Event setting above.", MessageType.Info);
                EditorGUILayout.PropertyField(onObjectEnteredProp);
                EditorGUILayout.PropertyField(onObjectExitedProp);
                EditorGUILayout.PropertyField(onObjectStayingProp);
                
                EditorGUILayout.Space();
            }
            
            // Runtime testing
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Runtime Testing", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Force Trigger"))
                {
                    ((CollisionTrigger)target).ForceFire();
                }
                
                if (GUILayout.Button("Reset Trigger"))
                {
                    ((CollisionTrigger)target).Reset();
                }
                
                EditorGUILayout.EndHorizontal();
                
                // Show runtime status
                CollisionTrigger trigger = (CollisionTrigger)target;
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Toggle("Is Active", trigger.IsActive);
                EditorGUILayout.Toggle("Has Fired", trigger.HasFired);
                EditorGUI.EndDisabledGroup();
            }
            
            serializedObject.ApplyModifiedProperties();
            
            // Draw colored background now that we know the actual height
            DrawColoredBackgroundActual(backgroundStartY);
        }
    }
}