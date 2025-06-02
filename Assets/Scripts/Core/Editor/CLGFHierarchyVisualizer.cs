using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using GameFramework.Events.Triggers;
using GameFramework.Events.Actions;
using GameFramework.Events.Channels;

namespace GameFramework.Core.Editor
{
    /// <summary>
    /// Enhances the Unity Hierarchy window with CLGF component visualization.
    /// Shows emoji icons and colored backgrounds for GameObjects with CLGF components.
    /// </summary>
    [InitializeOnLoad]
    public static class CLGFHierarchyVisualizer
    {
        private static readonly Dictionary<Type, CLGFHierarchyInfo> ComponentTypeMap = new Dictionary<Type, CLGFHierarchyInfo>();
        
        static CLGFHierarchyVisualizer()
        {
            BuildComponentMap();
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        }
        
        private static void BuildComponentMap()
        {
            ComponentTypeMap.Clear();
            
            // Triggers (Orange/Collision theme)
            ComponentTypeMap[typeof(BaseTrigger)] = new CLGFHierarchyInfo("🎯", CLGFBaseEditor.CLGFTheme.Collision);
            ComponentTypeMap[typeof(GameFramework.Events.Triggers.CollisionTrigger)] = new CLGFHierarchyInfo("⚡", CLGFBaseEditor.CLGFTheme.Collision);
            ComponentTypeMap[typeof(GameFramework.Events.Triggers.ProximityTrigger)] = new CLGFHierarchyInfo("⚡", CLGFBaseEditor.CLGFTheme.Collision);
            ComponentTypeMap[typeof(GameFramework.Events.Triggers.TimerTrigger)] = new CLGFHierarchyInfo("⚡", CLGFBaseEditor.CLGFTheme.Collision);
            
            // Actions (Green/ObjectControl theme) 
            ComponentTypeMap[typeof(BaseTriggerAction)] = new CLGFHierarchyInfo("🚀", CLGFBaseEditor.CLGFTheme.ObjectControl);
            ComponentTypeMap[typeof(GameFramework.Events.Actions.MoveAction)] = new CLGFHierarchyInfo("📐", CLGFBaseEditor.CLGFTheme.ObjectControl);
            ComponentTypeMap[typeof(GameFramework.Events.Actions.RotateAction)] = new CLGFHierarchyInfo("🔄", CLGFBaseEditor.CLGFTheme.ObjectControl);
            ComponentTypeMap[typeof(GameFramework.Events.Actions.ScaleAction)] = new CLGFHierarchyInfo("📏", CLGFBaseEditor.CLGFTheme.ObjectControl);
            ComponentTypeMap[typeof(GameFramework.Events.Actions.AudioAction)] = new CLGFHierarchyInfo("🔊", CLGFBaseEditor.CLGFTheme.ObjectControl);
            ComponentTypeMap[typeof(GameFramework.Events.Actions.LightAction)] = new CLGFHierarchyInfo("💡", CLGFBaseEditor.CLGFTheme.ObjectControl);
            ComponentTypeMap[typeof(GameFramework.Events.Actions.GameObjectActivateAction)] = new CLGFHierarchyInfo("👁️", CLGFBaseEditor.CLGFTheme.ObjectControl);
            ComponentTypeMap[typeof(GameFramework.Events.Actions.InstantiateAction)] = new CLGFHierarchyInfo("✨", CLGFBaseEditor.CLGFTheme.ObjectControl);
            ComponentTypeMap[typeof(GameFramework.Events.Actions.DestroyAction)] = new CLGFHierarchyInfo("💥", CLGFBaseEditor.CLGFTheme.ObjectControl);
            ComponentTypeMap[typeof(GameFramework.Events.Actions.ComponentToggleAction)] = new CLGFHierarchyInfo("🔧", CLGFBaseEditor.CLGFTheme.ObjectControl);
            ComponentTypeMap[typeof(GameFramework.Events.Actions.AnimationAction)] = new CLGFHierarchyInfo("🎬", CLGFBaseEditor.CLGFTheme.ObjectControl);
            ComponentTypeMap[typeof(GameFramework.Events.Actions.ParticleAction)] = new CLGFHierarchyInfo("✨", CLGFBaseEditor.CLGFTheme.ObjectControl);
            ComponentTypeMap[typeof(GameFramework.Events.Actions.PhysicsAction)] = new CLGFHierarchyInfo("⚽", CLGFBaseEditor.CLGFTheme.ObjectControl);
            ComponentTypeMap[typeof(GameFramework.Events.Actions.RaiseGameEventAction)] = new CLGFHierarchyInfo("📡", CLGFBaseEditor.CLGFTheme.Event);
            
            // Event Channels (Blue/Event theme)
            ComponentTypeMap[typeof(BaseEventChannel)] = new CLGFHierarchyInfo("📢", CLGFBaseEditor.CLGFTheme.Event);
            ComponentTypeMap[typeof(GameFramework.Events.Channels.GameEvent)] = new CLGFHierarchyInfo("🎧", CLGFBaseEditor.CLGFTheme.Event);
            ComponentTypeMap[typeof(GameFramework.Events.Listeners.GameEventListener)] = new CLGFHierarchyInfo("📻", CLGFBaseEditor.CLGFTheme.Event);
        }
        
        private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            if (!CLGFVisualizationSettings.ShowHierarchyIcons && !CLGFVisualizationSettings.ShowHierarchyBackgrounds)
                return;
                
            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj == null) return;
            
            var clgfInfo = GetCLGFInfo(obj);
            if (clgfInfo == null) return;
            
            // Draw background if enabled
            if (CLGFVisualizationSettings.ShowHierarchyBackgrounds)
            {
                DrawHierarchyBackground(selectionRect, clgfInfo.Theme);
            }
            
            // Draw icon if enabled
            if (CLGFVisualizationSettings.ShowHierarchyIcons)
            {
                DrawHierarchyIcon(selectionRect, clgfInfo.Icon);
            }
        }
        
        private static CLGFHierarchyInfo GetCLGFInfo(GameObject obj)
        {
            // Check for specific component types, prioritizing most specific
            var components = obj.GetComponents<MonoBehaviour>();
            
            CLGFHierarchyInfo bestMatch = null;
            int bestPriority = int.MaxValue;
            
            foreach (var component in components)
            {
                if (component == null) continue;
                
                Type componentType = component.GetType();
                
                // Check exact type match first
                if (ComponentTypeMap.TryGetValue(componentType, out var exactMatch))
                {
                    if (GetTypePriority(componentType) < bestPriority)
                    {
                        bestMatch = exactMatch;
                        bestPriority = GetTypePriority(componentType);
                    }
                }
                else
                {
                    // Check for base type matches
                    foreach (var kvp in ComponentTypeMap)
                    {
                        if (kvp.Key.IsAssignableFrom(componentType))
                        {
                            int priority = GetTypePriority(kvp.Key);
                            if (priority < bestPriority)
                            {
                                bestMatch = kvp.Value;
                                bestPriority = priority;
                            }
                        }
                    }
                }
            }
            
            return bestMatch;
        }
        
        private static int GetTypePriority(Type type)
        {
            // Lower numbers = higher priority
            if (type == typeof(GameFramework.Events.Triggers.CollisionTrigger)) return 1;
            if (type == typeof(GameFramework.Events.Triggers.ProximityTrigger)) return 2;
            if (type == typeof(GameFramework.Events.Triggers.TimerTrigger)) return 3;
            if (type == typeof(BaseTrigger)) return 10;
            
            if (type == typeof(GameFramework.Events.Actions.MoveAction)) return 1;
            if (type == typeof(GameFramework.Events.Actions.RotateAction)) return 2;
            if (type == typeof(GameFramework.Events.Actions.ScaleAction)) return 3;
            if (type == typeof(BaseTriggerAction)) return 10;
            
            if (type == typeof(GameFramework.Events.Channels.GameEvent)) return 1;
            if (type == typeof(GameFramework.Events.Listeners.GameEventListener)) return 2;
            if (type == typeof(BaseEventChannel)) return 10;
            
            return 50; // Default priority for unknown types
        }
        
        private static void DrawHierarchyBackground(Rect rect, CLGFBaseEditor.CLGFTheme theme)
        {
            Color backgroundColor = GetThemeBackgroundColor(theme);
            backgroundColor.a = 0.3f; // Make it more visible in hierarchy
            
            Rect backgroundRect = new Rect(rect.x, rect.y, rect.width, rect.height);
            EditorGUI.DrawRect(backgroundRect, backgroundColor);
        }
        
        private static void DrawHierarchyIcon(Rect rect, string icon)
        {
            GUIStyle iconStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = Color.white }
            };
            
            Rect iconRect = new Rect(rect.x + rect.width - 20, rect.y, 20, rect.height);
            EditorGUI.LabelField(iconRect, icon, iconStyle);
        }
        
        private static Color GetThemeBackgroundColor(CLGFBaseEditor.CLGFTheme theme)
        {
            return theme switch
            {
                CLGFBaseEditor.CLGFTheme.Event => new Color(0.3f, 0.7f, 0.9f, 0.2f),
                CLGFBaseEditor.CLGFTheme.Action => new Color(0.3f, 0.9f, 0.4f, 0.2f),
                CLGFBaseEditor.CLGFTheme.Collision => new Color(0.9f, 0.7f, 0.3f, 0.2f),
                CLGFBaseEditor.CLGFTheme.ObjectControl => new Color(0.3f, 0.9f, 0.4f, 0.2f),
                CLGFBaseEditor.CLGFTheme.Character => new Color(0.8f, 0.4f, 0.9f, 0.2f),
                CLGFBaseEditor.CLGFTheme.Camera => new Color(0.4f, 0.9f, 0.8f, 0.2f),
                CLGFBaseEditor.CLGFTheme.UI => new Color(0.9f, 0.5f, 0.7f, 0.2f),
                CLGFBaseEditor.CLGFTheme.System => new Color(0.9f, 0.3f, 0.3f, 0.2f),
                _ => Color.gray
            };
        }
        
        private class CLGFHierarchyInfo
        {
            public string Icon { get; }
            public CLGFBaseEditor.CLGFTheme Theme { get; }
            
            public CLGFHierarchyInfo(string icon, CLGFBaseEditor.CLGFTheme theme)
            {
                Icon = icon;
                Theme = theme;
            }
        }
    }
}