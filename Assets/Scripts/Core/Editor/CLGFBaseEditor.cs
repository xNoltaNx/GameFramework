using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace GameFramework.Core.Editor
{
    /// <summary>
    /// Universal base editor class for all Claude Code Game Framework (CLGF) components.
    /// Provides consistent styling, colored backgrounds, and CLGF labels across all framework components.
    /// </summary>
    public abstract class CLGFBaseEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Predefined color themes for different component categories.
        /// </summary>
        public enum CLGFTheme
        {
            Event,          // Blue theme for listeners, channels, event components
            Action,         // Green theme for general actions and responses  
            Collision,      // Orange theme for trigger/collision actions specifically
            ObjectControl,  // Green theme for actions that control other GameObjects
            Character,      // Purple theme for character and player components
            Camera,         // Teal theme for camera and view components
            UI,             // Pink theme for UI and inventory components
            System,         // Red theme for managers and core systems
            Custom          // Use custom colors defined in the editor
        }
        
        // Theme color definitions
        private static readonly (Color background, Color border, Color label) EventColors = 
            (new Color(0.3f, 0.7f, 0.9f, 0.05f), new Color(0.3f, 0.7f, 0.9f, 0.8f), new Color(0.2f, 0.6f, 0.8f, 0.8f));
            
        private static readonly (Color background, Color border, Color label) ActionColors = 
            (new Color(0.3f, 0.9f, 0.4f, 0.05f), new Color(0.3f, 0.9f, 0.4f, 0.8f), new Color(0.2f, 0.7f, 0.3f, 0.8f));
            
        private static readonly (Color background, Color border, Color label) CollisionColors = 
            (new Color(0.9f, 0.7f, 0.3f, 0.05f), new Color(0.9f, 0.7f, 0.3f, 0.8f), new Color(0.8f, 0.6f, 0.2f, 0.8f));
            
        private static readonly (Color background, Color border, Color label) ObjectControlColors = 
            (new Color(0.3f, 0.9f, 0.4f, 0.05f), new Color(0.3f, 0.9f, 0.4f, 0.8f), new Color(0.2f, 0.7f, 0.3f, 0.8f));
            
        private static readonly (Color background, Color border, Color label) CharacterColors = 
            (new Color(0.8f, 0.4f, 0.9f, 0.05f), new Color(0.8f, 0.4f, 0.9f, 0.8f), new Color(0.7f, 0.3f, 0.8f, 0.8f));
            
        private static readonly (Color background, Color border, Color label) CameraColors = 
            (new Color(0.4f, 0.9f, 0.8f, 0.05f), new Color(0.4f, 0.9f, 0.8f, 0.8f), new Color(0.3f, 0.8f, 0.7f, 0.8f));
            
        private static readonly (Color background, Color border, Color label) UIColors = 
            (new Color(0.9f, 0.5f, 0.7f, 0.05f), new Color(0.9f, 0.5f, 0.7f, 0.8f), new Color(0.8f, 0.4f, 0.6f, 0.8f));
            
        private static readonly (Color background, Color border, Color label) SystemColors = 
            (new Color(0.9f, 0.3f, 0.3f, 0.05f), new Color(0.9f, 0.3f, 0.3f, 0.8f), new Color(0.8f, 0.2f, 0.2f, 0.8f));
        
        // Override these properties in derived classes
        protected virtual CLGFTheme Theme => CLGFTheme.Custom;
        protected virtual string ComponentIcon => "🔧";
        protected virtual string ComponentName => "CLGF COMPONENT";
        protected virtual int ComponentIconSize => 28; // Icon font size
        
        // Custom colors (used when Theme is Custom)
        protected virtual Color CustomBackgroundColor => Color.gray;
        protected virtual Color CustomBorderColor => Color.white;
        protected virtual Color CustomLabelBackgroundColor => Color.black;
        
        // Computed properties based on theme
        protected Color BackgroundColor
        {
            get
            {
                return Theme switch
                {
                    CLGFTheme.Event => EventColors.background,
                    CLGFTheme.Action => ActionColors.background,
                    CLGFTheme.Collision => CollisionColors.background,
                    CLGFTheme.ObjectControl => ObjectControlColors.background,
                    CLGFTheme.Character => CharacterColors.background,
                    CLGFTheme.Camera => CameraColors.background,
                    CLGFTheme.UI => UIColors.background,
                    CLGFTheme.System => SystemColors.background,
                    CLGFTheme.Custom => CustomBackgroundColor,
                    _ => Color.gray
                };
            }
        }
        
        protected Color BorderColor
        {
            get
            {
                return Theme switch
                {
                    CLGFTheme.Event => EventColors.border,
                    CLGFTheme.Action => ActionColors.border,
                    CLGFTheme.Collision => CollisionColors.border,
                    CLGFTheme.ObjectControl => ObjectControlColors.border,
                    CLGFTheme.Character => CharacterColors.border,
                    CLGFTheme.Camera => CameraColors.border,
                    CLGFTheme.UI => UIColors.border,
                    CLGFTheme.System => SystemColors.border,
                    CLGFTheme.Custom => CustomBorderColor,
                    _ => Color.white
                };
            }
        }
        
        protected Color LabelBackgroundColor
        {
            get
            {
                return Theme switch
                {
                    CLGFTheme.Event => EventColors.label,
                    CLGFTheme.Action => ActionColors.label,
                    CLGFTheme.Collision => CollisionColors.label,
                    CLGFTheme.ObjectControl => ObjectControlColors.label,
                    CLGFTheme.Character => CharacterColors.label,
                    CLGFTheme.Camera => CameraColors.label,
                    CLGFTheme.UI => UIColors.label,
                    CLGFTheme.System => SystemColors.label,
                    CLGFTheme.Custom => CustomLabelBackgroundColor,
                    _ => Color.black
                };
            }
        }
        
        public override void OnInspectorGUI()
        {
            // Draw CLGF label at the top
            DrawCLGFLabel();
            
            // Draw visualization controls for CLGF components
            DrawVisualizationControls();
            
            // Begin background area - we'll draw the background after the content to get accurate height
            Rect backgroundStartRect = GUILayoutUtility.GetRect(0, 0);
            float backgroundStartY = backgroundStartRect.y;
            
            // Draw default inspector
            DrawDefaultInspector();
            
            // Draw colored background now that we know the actual height
            DrawColoredBackgroundActual(backgroundStartY);
        }
        
        protected virtual void DrawCLGFLabel()
        {
            GUILayout.Space(2f);
            
            // Create label rect
            Rect labelRect = GUILayoutUtility.GetRect(0, 24);
            labelRect.x = 5;
            labelRect.width = EditorGUIUtility.currentViewWidth - 10;
            
            // Draw label background
            EditorGUI.DrawRect(labelRect, LabelBackgroundColor);
            
            // Create icon style with tunable size
            GUIStyle iconStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white },
                fontSize = ComponentIconSize,
                fontStyle = FontStyle.Bold
            };
            
            // Create text style 
            GUIStyle textStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white },
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            
            // Draw icon and text separately for better control
            float iconWidth = iconStyle.CalcSize(new GUIContent(ComponentIcon)).x;
            
            // Icon rect
            Rect iconRect = new Rect(labelRect.x + 6, labelRect.y, iconWidth, labelRect.height);
            EditorGUI.LabelField(iconRect, ComponentIcon, iconStyle);
            
            // Text rect (positioned after icon)
            Rect textRect = new Rect(iconRect.x + iconWidth + 4, labelRect.y, labelRect.width - iconWidth - 10, labelRect.height);
            EditorGUI.LabelField(textRect, $"CLGF: {ComponentName}", textStyle);
            
            GUILayout.Space(4f);
        }
        
        protected virtual void DrawColoredBackgroundActual(float startY)
        {
            // Get current position to calculate actual height
            Rect endRect = GUILayoutUtility.GetRect(0, 0);
            float actualHeight = endRect.y - startY + 10f; // Add padding
            
            // Create background rect with actual dimensions
            Rect backgroundRect = new Rect(2, startY - 5f, EditorGUIUtility.currentViewWidth - 4, actualHeight);
            
            // Draw background
            EditorGUI.DrawRect(backgroundRect, BackgroundColor);
            
            // Draw border with proper calculations
            float borderWidth = 2f;
            
            // Top border
            EditorGUI.DrawRect(new Rect(backgroundRect.x, backgroundRect.y, backgroundRect.width, borderWidth), BorderColor);
            // Bottom border  
            EditorGUI.DrawRect(new Rect(backgroundRect.x, backgroundRect.y + backgroundRect.height - borderWidth, backgroundRect.width, borderWidth), BorderColor);
            // Left border
            EditorGUI.DrawRect(new Rect(backgroundRect.x, backgroundRect.y, borderWidth, backgroundRect.height), BorderColor);
            // Right border
            EditorGUI.DrawRect(new Rect(backgroundRect.x + backgroundRect.width - borderWidth, backgroundRect.y, borderWidth, backgroundRect.height), BorderColor);
        }
        
        // Legacy method for backward compatibility - now calls the actual method
        protected virtual void DrawColoredBackground()
        {
            // This method is kept for backward compatibility but shouldn't be used
            // The new OnInspectorGUI uses DrawColoredBackgroundActual instead
        }
        
        protected virtual float GetInspectorHeight()
        {
            // Estimate height based on serialized properties
            SerializedProperty iterator = serializedObject.GetIterator();
            float height = 20f; // Header space
            
            if (iterator.NextVisible(true))
            {
                do
                {
                    height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
                }
                while (iterator.NextVisible(false));
            }
            
            return height + 10f; // Extra padding
        }
        
        #region Foldout System
        
        /// <summary>
        /// Draws a collapsible foldout section with themed styling.
        /// Returns true if the section is expanded and content should be drawn.
        /// </summary>
        protected bool DrawFoldoutSection(FoldoutUtility.FoldoutConfig config, System.Action drawContent)
        {
            return FoldoutUtility.DrawFoldoutSection(GetType(), config, drawContent);
        }
        
        /// <summary>
        /// Draws a collapsible foldout section with themed styling.
        /// Returns true if the section is expanded and content should be drawn.
        /// </summary>
        protected bool DrawFoldoutSection(string id, string title, string icon = "📁", CLGFTheme theme = CLGFTheme.System, System.Action drawContent = null, bool defaultExpanded = true, bool showItemCount = false, int itemCount = 0, string tooltip = "")
        {
            return FoldoutUtility.DrawFoldoutSection(GetType(), id, title, icon, theme, drawContent, defaultExpanded, showItemCount, itemCount, tooltip);
        }
        
        /// <summary>
        /// Draws a simple foldout without themed styling for basic use cases.
        /// </summary>
        protected bool DrawSimpleFoldout(string id, string title, System.Action drawContent, bool defaultExpanded = true)
        {
            return FoldoutUtility.DrawSimpleFoldout(GetType(), id, title, drawContent, defaultExpanded);
        }
        
        /// <summary>
        /// Gets the current expansion state of a foldout.
        /// </summary>
        protected bool GetFoldoutState(string id)
        {
            return FoldoutUtility.GetFoldoutState(GetType(), id);
        }
        
        /// <summary>
        /// Sets the expansion state of a foldout.
        /// </summary>
        protected void SetFoldoutState(string id, bool expanded)
        {
            FoldoutUtility.SetFoldoutState(GetType(), id, expanded);
        }
        
        /// <summary>
        /// Collapses all foldouts for this editor type.
        /// </summary>
        protected void CollapseAllFoldouts()
        {
            FoldoutUtility.CollapseAllFoldouts(GetType());
        }
        
        /// <summary>
        /// Expands all foldouts for this editor type.
        /// </summary>
        protected void ExpandAllFoldouts()
        {
            FoldoutUtility.ExpandAllFoldouts(GetType());
        }
        
        /// <summary>
        /// Draws expand/collapse all buttons for this editor.
        /// </summary>
        protected void DrawFoldoutControls()
        {
            FoldoutUtility.DrawFoldoutControls(GetType());
        }
        
        #endregion
        
        #region Visualization Integration
        
        /// <summary>
        /// Draws visualization controls and information for CLGF components.
        /// </summary>
        protected virtual void DrawVisualizationControls()
        {
            if (!ShouldShowVisualizationControls())
                return;
                
            GUILayout.Space(2f);
            
            // Create a compact visualization controls section
            EditorGUILayout.BeginHorizontal();
            
            // Quick toggle for scene gizmos
            if (GUILayout.Button(new GUIContent("👁️", "Toggle scene view gizmos"), GUILayout.Width(25), GUILayout.Height(20)))
            {
                CLGFVisualizationSettings.ShowSceneGizmos = !CLGFVisualizationSettings.ShowSceneGizmos;
            }
            
            // Quick toggle for connection lines
            if (IsConnectionCapable() && GUILayout.Button(new GUIContent("🔗", "Toggle connection lines"), GUILayout.Width(25), GUILayout.Height(20)))
            {
                CLGFVisualizationSettings.ShowConnectionLines = !CLGFVisualizationSettings.ShowConnectionLines;
            }
            
            // Quick toggle for action previews
            if (IsActionComponent() && GUILayout.Button(new GUIContent("🎯", "Toggle action previews"), GUILayout.Width(25), GUILayout.Height(20)))
            {
                CLGFVisualizationSettings.ShowActionPreviews = !CLGFVisualizationSettings.ShowActionPreviews;
            }
            
            // Always show toggle
            Color originalBgColor = GUI.backgroundColor;
            if (CLGFVisualizationSettings.AlwaysShow)
            {
                GUI.backgroundColor = Color.green;
            }
            
            if (GUILayout.Button(new GUIContent("🌐", "Toggle always show all mode"), GUILayout.Width(25), GUILayout.Height(20)))
            {
                CLGFVisualizationSettings.AlwaysShow = !CLGFVisualizationSettings.AlwaysShow;
            }
            
            GUI.backgroundColor = originalBgColor;
            
            GUILayout.FlexibleSpace();
            
            // Link to full settings
            if (GUILayout.Button(new GUIContent("⚙️", "Open CLGF Visualization Settings"), GUILayout.Width(25), GUILayout.Height(20)))
            {
                SettingsService.OpenUserPreferences("Preferences/CLGF Visualization");
            }
            
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(2f);
        }
        
        /// <summary>
        /// Determines if this component should show visualization controls.
        /// </summary>
        protected virtual bool ShouldShowVisualizationControls()
        {
            return target is MonoBehaviour mb && 
                   (mb is GameFramework.Events.Triggers.BaseTrigger || 
                    mb is GameFramework.Events.Actions.BaseTriggerAction ||
                    mb is GameFramework.Events.Channels.BaseEventChannel);
        }
        
        /// <summary>
        /// Determines if this component can show connection lines.
        /// </summary>
        protected virtual bool IsConnectionCapable()
        {
            return target is GameFramework.Events.Triggers.BaseTrigger;
        }
        
        /// <summary>
        /// Determines if this component is an action that can show previews.
        /// </summary>
        protected virtual bool IsActionComponent()
        {
            return target is GameFramework.Events.Actions.BaseTriggerAction;
        }
        
        #endregion
    }
}