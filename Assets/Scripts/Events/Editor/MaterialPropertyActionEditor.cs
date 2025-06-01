using UnityEngine;
using UnityEditor;
using GameFramework.Events.Actions;
using GameFramework.Core.Editor;
using System.Collections.Generic;
using System.Linq;

namespace GameFramework.Events.Editor
{
    /// <summary>
    /// Custom editor for unified MaterialPropertyAction with smart property dropdowns.
    /// </summary>
    [CustomEditor(typeof(MaterialPropertyAction))]
    public class MaterialPropertyActionEditor : CLGFBaseEditor
    {
        protected override CLGFTheme Theme => CLGFTheme.ObjectControl;
        protected override string ComponentIcon => "🎨";
        protected override string ComponentName => "MATERIAL PROPERTY ACTION";
        
        protected override float GetInspectorHeight()
        {
            // Start with base height calculation
            float height = base.GetInspectorHeight();
            
            // Add extra height for dynamic property changes
            if (propertyChangesProp != null)
            {
                // Add height for "Add Property Change" button
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                
                // Add height for each property change element (estimated)
                for (int i = 0; i < propertyChangesProp.arraySize; i++)
                {
                    height += 120f; // Estimated height per property change element
                }
            }
            
            // Add extra height for runtime controls if playing
            if (Application.isPlaying)
            {
                height += 60f; // Runtime controls height
            }
            
            // Add some extra padding for safety
            height += 40f;
            
            return height;
        }
        
        private MaterialPropertyAction materialAction;
        private SerializedProperty targetRendererProp;
        private SerializedProperty targetMaterialProp;
        private SerializedProperty materialIndexProp;
        private SerializedProperty useSharedMaterialProp;
        private SerializedProperty durationProp;
        private SerializedProperty animateSequentiallyProp;
        private SerializedProperty sequentialDelayProp;
        private SerializedProperty propertyChangesProp;
        
        private Dictionary<string, string[]> cachedProperties = new Dictionary<string, string[]>();
        private Dictionary<string, string[]> cachedPropertyDisplayNames = new Dictionary<string, string[]>();
        private Material lastCachedMaterial;
        
        private void OnEnable()
        {
            materialAction = target as MaterialPropertyAction;
            
            targetRendererProp = serializedObject.FindProperty("targetRenderer");
            targetMaterialProp = serializedObject.FindProperty("targetMaterial");
            materialIndexProp = serializedObject.FindProperty("materialIndex");
            useSharedMaterialProp = serializedObject.FindProperty("useSharedMaterial");
            durationProp = serializedObject.FindProperty("duration");
            animateSequentiallyProp = serializedObject.FindProperty("animateSequentially");
            sequentialDelayProp = serializedObject.FindProperty("sequentialDelay");
            propertyChangesProp = serializedObject.FindProperty("propertyChanges");
            
            RefreshPropertyCache();
        }
        
        public override void OnInspectorGUI()
        {
            // Draw CLGF label at the top
            DrawCLGFLabel();
            
            // Begin background area
            Rect backgroundStartRect = GUILayoutUtility.GetRect(0, 0);
            float backgroundStartY = backgroundStartRect.y;
            
            serializedObject.Update();
            
            // Draw default properties except propertyChanges
            DrawPropertiesExcluding(serializedObject, "propertyChanges");
            
            EditorGUILayout.Space();
            
            // Check if material changed and refresh cache
            Material currentMaterial = GetCurrentMaterial();
            if (currentMaterial != lastCachedMaterial)
            {
                RefreshPropertyCache();
                lastCachedMaterial = currentMaterial;
            }
            
            // Draw property changes list
            DrawPropertyChangesList();
            
            EditorGUILayout.Space();
            
            // Runtime testing
            if (Application.isPlaying)
            {
                DrawRuntimeControls();
            }
            
            serializedObject.ApplyModifiedProperties();
            
            // Draw colored background now that we know the actual height
            DrawColoredBackgroundActual(backgroundStartY);
        }
        
        private void DrawPropertyChangesList()
        {
            EditorGUILayout.LabelField("Property Changes", EditorStyles.boldLabel);
            
            if (GetCurrentMaterial() == null)
            {
                EditorGUILayout.HelpBox("Select a target renderer or material to configure property changes.", MessageType.Info);
                return;
            }
            
            // Add new property button
            if (GUILayout.Button("Add Property Change"))
            {
                propertyChangesProp.arraySize++;
                var newElement = propertyChangesProp.GetArrayElementAtIndex(propertyChangesProp.arraySize - 1);
                ResetPropertyChangeToDefaults(newElement);
            }
            
            // Draw each property change
            for (int i = 0; i < propertyChangesProp.arraySize; i++)
            {
                var element = propertyChangesProp.GetArrayElementAtIndex(i);
                DrawPropertyChange(element, i);
            }
        }
        
        private void DrawPropertyChange(SerializedProperty element, int index)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Header with remove button
            EditorGUILayout.BeginHorizontal();
            var propertyTypeProp = element.FindPropertyRelative("propertyType");
            var propertyNameProp = element.FindPropertyRelative("propertyName");
            
            EditorGUILayout.LabelField($"Property {index + 1}: {propertyNameProp.stringValue} ({propertyTypeProp.enumDisplayNames[propertyTypeProp.enumValueIndex]})", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                propertyChangesProp.DeleteArrayElementAtIndex(index);
                return;
            }
            EditorGUILayout.EndHorizontal();
            
            // Property type
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(propertyTypeProp);
            bool typeChanged = EditorGUI.EndChangeCheck();
            
            // Property name dropdown
            DrawPropertyNameDropdown(element, typeChanged);
            
            // Type-specific settings
            var propertyType = (MaterialPropertyAction.PropertyType)propertyTypeProp.enumValueIndex;
            DrawTypeSpecificSettings(element, propertyType);
            
            // Animation settings
            var animateFromCurrentProp = element.FindPropertyRelative("animateFromCurrent");
            var easingSettingsProp = element.FindPropertyRelative("easingSettings");
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animation Settings", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(animateFromCurrentProp);
            EditorGUILayout.PropertyField(easingSettingsProp);
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawPropertyNameDropdown(SerializedProperty element, bool typeChanged)
        {
            var propertyTypeProp = element.FindPropertyRelative("propertyType");
            var propertyNameProp = element.FindPropertyRelative("propertyName");
            var propertyType = (MaterialPropertyAction.PropertyType)propertyTypeProp.enumValueIndex;
            
            string typeKey = propertyType.ToString();
            
            if (!cachedProperties.ContainsKey(typeKey) || cachedProperties[typeKey].Length == 0)
            {
                EditorGUILayout.HelpBox($"No {propertyType} properties found on the selected material.", MessageType.Warning);
                EditorGUILayout.PropertyField(propertyNameProp);
                return;
            }
            
            string[] properties = cachedProperties[typeKey];
            string[] displayNames = cachedPropertyDisplayNames[typeKey];
            
            // Find current selection
            int selectedIndex = System.Array.IndexOf(properties, propertyNameProp.stringValue);
            
            // If type changed or property not found, reset to first option
            if (typeChanged || selectedIndex < 0)
            {
                selectedIndex = 0;
                propertyNameProp.stringValue = properties[0];
            }
            
            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUILayout.Popup("Property Name", selectedIndex, displayNames);
            
            if (EditorGUI.EndChangeCheck() && selectedIndex >= 0 && selectedIndex < properties.Length)
            {
                propertyNameProp.stringValue = properties[selectedIndex];
            }
            
            // Show property info
            if (selectedIndex >= 0 && selectedIndex < properties.Length)
            {
                ShowPropertyInfo(properties[selectedIndex]);
            }
        }
        
        private void DrawTypeSpecificSettings(SerializedProperty element, MaterialPropertyAction.PropertyType propertyType)
        {
            var animateFromCurrentProp = element.FindPropertyRelative("animateFromCurrent");
            bool animateFromCurrent = animateFromCurrentProp.boolValue;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"{propertyType} Settings", EditorStyles.miniBoldLabel);
            
            switch (propertyType)
            {
                case MaterialPropertyAction.PropertyType.Float:
                    if (!animateFromCurrent)
                    {
                        EditorGUILayout.PropertyField(element.FindPropertyRelative("floatStartValue"));
                    }
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("floatTargetValue"));
                    break;
                    
                case MaterialPropertyAction.PropertyType.Color:
                    if (!animateFromCurrent)
                    {
                        EditorGUILayout.PropertyField(element.FindPropertyRelative("colorStartValue"));
                    }
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("colorTargetValue"));
                    break;
                    
                case MaterialPropertyAction.PropertyType.Vector:
                    if (!animateFromCurrent)
                    {
                        EditorGUILayout.PropertyField(element.FindPropertyRelative("vectorStartValue"));
                    }
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("vectorTargetValue"));
                    break;
                    
                case MaterialPropertyAction.PropertyType.Texture:
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("targetTexture"));
                    
                    var animateOffsetProp = element.FindPropertyRelative("animateOffset");
                    EditorGUILayout.PropertyField(animateOffsetProp);
                    if (animateOffsetProp.boolValue)
                    {
                        if (!animateFromCurrent)
                        {
                            EditorGUILayout.PropertyField(element.FindPropertyRelative("offsetStartValue"));
                        }
                        EditorGUILayout.PropertyField(element.FindPropertyRelative("offsetTargetValue"));
                    }
                    
                    var animateScaleProp = element.FindPropertyRelative("animateScale");
                    EditorGUILayout.PropertyField(animateScaleProp);
                    if (animateScaleProp.boolValue)
                    {
                        if (!animateFromCurrent)
                        {
                            EditorGUILayout.PropertyField(element.FindPropertyRelative("scaleStartValue"));
                        }
                        EditorGUILayout.PropertyField(element.FindPropertyRelative("scaleTargetValue"));
                    }
                    break;
            }
        }
        
        private void RefreshPropertyCache()
        {
            cachedProperties.Clear();
            cachedPropertyDisplayNames.Clear();
            
            Material material = GetCurrentMaterial();
            if (material == null) return;
            
            Shader shader = material.shader;
            int propertyCount = ShaderUtil.GetPropertyCount(shader);
            
            var floatProperties = new List<(string name, string displayName)>();
            var colorProperties = new List<(string name, string displayName)>();
            var vectorProperties = new List<(string name, string displayName)>();
            var textureProperties = new List<(string name, string displayName)>();
            
            for (int i = 0; i < propertyCount; i++)
            {
                string propName = ShaderUtil.GetPropertyName(shader, i);
                string description = ShaderUtil.GetPropertyDescription(shader, i);
                ShaderUtil.ShaderPropertyType propType = ShaderUtil.GetPropertyType(shader, i);
                
                string displayName = string.IsNullOrEmpty(description) ? propName : $"{propName} ({description})";
                displayName += $" [{propType}]";
                
                switch (propType)
                {
                    case ShaderUtil.ShaderPropertyType.Float:
                    case ShaderUtil.ShaderPropertyType.Range:
                        floatProperties.Add((propName, displayName));
                        break;
                    case ShaderUtil.ShaderPropertyType.Color:
                        colorProperties.Add((propName, displayName));
                        break;
                    case ShaderUtil.ShaderPropertyType.Vector:
                        vectorProperties.Add((propName, displayName));
                        break;
                    case ShaderUtil.ShaderPropertyType.TexEnv:
                        textureProperties.Add((propName, displayName));
                        break;
                }
            }
            
            cachedProperties["Float"] = floatProperties.Select(p => p.name).ToArray();
            cachedPropertyDisplayNames["Float"] = floatProperties.Select(p => p.displayName).ToArray();
            
            cachedProperties["Color"] = colorProperties.Select(p => p.name).ToArray();
            cachedPropertyDisplayNames["Color"] = colorProperties.Select(p => p.displayName).ToArray();
            
            cachedProperties["Vector"] = vectorProperties.Select(p => p.name).ToArray();
            cachedPropertyDisplayNames["Vector"] = vectorProperties.Select(p => p.displayName).ToArray();
            
            cachedProperties["Texture"] = textureProperties.Select(p => p.name).ToArray();
            cachedPropertyDisplayNames["Texture"] = textureProperties.Select(p => p.displayName).ToArray();
        }
        
        private Material GetCurrentMaterial()
        {
            if (targetMaterialProp.objectReferenceValue != null)
            {
                return targetMaterialProp.objectReferenceValue as Material;
            }
            
            Renderer renderer = targetRendererProp.objectReferenceValue as Renderer;
            if (renderer != null && renderer.sharedMaterials.Length > materialIndexProp.intValue)
            {
                return renderer.sharedMaterials[materialIndexProp.intValue];
            }
            
            return null;
        }
        
        private void ShowPropertyInfo(string propertyName)
        {
            Material material = GetCurrentMaterial();
            if (material == null || string.IsNullOrEmpty(propertyName)) return;
            
            int propertyID = Shader.PropertyToID(propertyName);
            if (!material.HasProperty(propertyID)) return;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Current Value", EditorStyles.miniBoldLabel);
            
            // Find property type
            Shader shader = material.shader;
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                if (ShaderUtil.GetPropertyName(shader, i) == propertyName)
                {
                    var propType = ShaderUtil.GetPropertyType(shader, i);
                    
                    switch (propType)
                    {
                        case ShaderUtil.ShaderPropertyType.Float:
                        case ShaderUtil.ShaderPropertyType.Range:
                            float floatValue = material.GetFloat(propertyID);
                            EditorGUILayout.LabelField("Float Value:", floatValue.ToString("F3"));
                            break;
                            
                        case ShaderUtil.ShaderPropertyType.Color:
                            Color colorValue = material.GetColor(propertyID);
                            EditorGUILayout.LabelField("Color Value:", colorValue.ToString());
                            break;
                            
                        case ShaderUtil.ShaderPropertyType.Vector:
                            Vector4 vectorValue = material.GetVector(propertyID);
                            EditorGUILayout.LabelField("Vector Value:", vectorValue.ToString());
                            break;
                            
                        case ShaderUtil.ShaderPropertyType.TexEnv:
                            Texture textureValue = material.GetTexture(propertyID);
                            EditorGUILayout.LabelField("Texture:", textureValue ? textureValue.name : "None");
                            break;
                    }
                    break;
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void ResetPropertyChangeToDefaults(SerializedProperty element)
        {
            element.FindPropertyRelative("propertyType").enumValueIndex = 0;
            element.FindPropertyRelative("propertyName").stringValue = "_Color";
            element.FindPropertyRelative("animateFromCurrent").boolValue = true;
        }
        
        private void DrawRuntimeControls()
        {
            EditorGUILayout.LabelField("Runtime Testing", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Execute Action"))
            {
                materialAction.Execute();
            }
            
            if (GUILayout.Button("Stop Action"))
            {
                materialAction.Stop();
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
}