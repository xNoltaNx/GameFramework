using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace GameFramework.Core.Editor
{
    /// <summary>
    /// Utility class providing foldout functionality for Unity Editor windows and inspectors.
    /// Can be used by both EditorWindow and Editor classes.
    /// </summary>
    public static class FoldoutUtility
    {
        /// <summary>
        /// Dictionary to track foldout states across all editor instances.
        /// Key format: "editorType_sectionId" for unique identification.
        /// </summary>
        private static Dictionary<string, bool> globalFoldoutStates = new Dictionary<string, bool>();
        
        /// <summary>
        /// Configuration for foldout sections.
        /// </summary>
        public class FoldoutConfig
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Icon { get; set; }
            public CLGFBaseEditor.CLGFTheme Theme { get; set; }
            public bool DefaultExpanded { get; set; }
            public bool ShowItemCount { get; set; }
            public int ItemCount { get; set; }
            public string Tooltip { get; set; }
            public FoldoutButton[] HeaderButtons { get; set; }
            
            public FoldoutConfig(string id, string title, string icon = "📁", CLGFBaseEditor.CLGFTheme theme = CLGFBaseEditor.CLGFTheme.System, bool defaultExpanded = true)
            {
                Id = id;
                Title = title;
                Icon = icon;
                Theme = theme;
                DefaultExpanded = defaultExpanded;
                ShowItemCount = false;
                ItemCount = 0;
                Tooltip = "";
                HeaderButtons = new FoldoutButton[0];
            }
        }
        
        /// <summary>
        /// Configuration for buttons in foldout headers.
        /// </summary>
        public class FoldoutButton
        {
            public string Text { get; set; }
            public string Tooltip { get; set; }
            public System.Action OnClick { get; set; }
            public Color? BackgroundColor { get; set; }
            public Color? TextColor { get; set; }
            public float Width { get; set; }
            
            public FoldoutButton(string text, System.Action onClick, string tooltip = "", Color? backgroundColor = null, Color? textColor = null, float width = 60f)
            {
                Text = text;
                OnClick = onClick;
                Tooltip = tooltip;
                BackgroundColor = backgroundColor;
                TextColor = textColor;
                Width = width;
            }
        }
        
        /// <summary>
        /// Draws a collapsible foldout section with themed styling.
        /// Returns true if the section is expanded and content should be drawn.
        /// </summary>
        public static bool DrawFoldoutSection(System.Type editorType, FoldoutConfig config, System.Action drawContent)
        {
            return DrawFoldoutSection(editorType, config.Id, config.Title, config.Icon, config.Theme, drawContent, config.DefaultExpanded, config.ShowItemCount, config.ItemCount, config.Tooltip, true, config.HeaderButtons);
        }
        
        /// <summary>
        /// Draws a collapsible foldout section with themed styling.
        /// Returns true if the section is expanded and content should be drawn.
        /// </summary>
        public static bool DrawFoldoutSection(System.Type editorType, string id, string title, string icon = "📁", CLGFBaseEditor.CLGFTheme theme = CLGFBaseEditor.CLGFTheme.System, System.Action drawContent = null, bool defaultExpanded = true, bool showItemCount = false, int itemCount = 0, string tooltip = "", bool drawBackground = true, FoldoutButton[] headerButtons = null)
        {
            // Create unique key for this foldout
            string foldoutKey = $"{editorType.Name}_{id}";
            
            // Get or set initial state
            if (!globalFoldoutStates.ContainsKey(foldoutKey))
                globalFoldoutStates[foldoutKey] = defaultExpanded;
            
            var (background, border, label) = GetCLGFThemeColors(theme);
            
            // Calculate total button width
            float totalButtonWidth = 0f;
            if (headerButtons != null && headerButtons.Length > 0)
            {
                foreach (var button in headerButtons)
                {
                    totalButtonWidth += button.Width + 5f; // 5px spacing between buttons
                }
                totalButtonWidth -= 5f; // Remove spacing after last button
            }
            
            // Create header rect
            Rect headerRect = GUILayoutUtility.GetRect(0, 28);
            headerRect.x += 5;
            headerRect.width -= 10;
            
            // Create clickable area for foldout (excluding button area)
            Rect foldoutClickRect = new Rect(headerRect.x, headerRect.y, headerRect.width - totalButtonWidth - 10f, headerRect.height);
            
            // Handle foldout click events (only in non-button area)
            if (Event.current.type == EventType.MouseDown && foldoutClickRect.Contains(Event.current.mousePosition))
            {
                globalFoldoutStates[foldoutKey] = !globalFoldoutStates[foldoutKey];
                Event.current.Use();
                if (EditorWindow.focusedWindow != null) EditorWindow.focusedWindow.Repaint();
            }
            
            // Draw background with hover effect
            Color headerColor = border;
            if (headerRect.Contains(Event.current.mousePosition))
            {
                headerColor = Color.Lerp(border, Color.white, 0.1f);
            }
            
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(headerRect, headerColor);
            }
            
            // Create styles
            GUIStyle arrowStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(8, 0, 0, 0)
            };
            
            GUIStyle iconStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleLeft
            };
            
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleLeft
            };
            
            GUIStyle countStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 10,
                normal = { textColor = new Color(1f, 1f, 1f, 0.7f) },
                alignment = TextAnchor.MiddleRight
            };
            
            // Draw header content
            bool isExpanded = globalFoldoutStates[foldoutKey];
            string arrow = isExpanded ? "▼" : "▶";
            
            // Draw arrow
            Rect arrowRect = new Rect(headerRect.x, headerRect.y, 20, headerRect.height);
            GUI.Label(arrowRect, arrow, arrowStyle);
            
            // Draw icon
            Rect iconRect = new Rect(headerRect.x + 20, headerRect.y, 25, headerRect.height);
            GUI.Label(iconRect, icon, iconStyle);
            
            // Draw title (adjusted for buttons)
            float titleWidth = headerRect.width - 45 - (showItemCount ? 50 : 0) - totalButtonWidth - 10f;
            Rect titleRect = new Rect(headerRect.x + 45, headerRect.y, titleWidth, headerRect.height);
            string displayTitle = showItemCount ? $"{title}" : title;
            
            // Add tooltip if provided
            if (!string.IsNullOrEmpty(tooltip))
            {
                GUI.Label(titleRect, new GUIContent(displayTitle, tooltip), titleStyle);
            }
            else
            {
                GUI.Label(titleRect, displayTitle, titleStyle);
            }
            
            // Draw item count if requested
            if (showItemCount)
            {
                Rect countRect = new Rect(headerRect.x + headerRect.width - 45 - totalButtonWidth - 10f, headerRect.y, 40, headerRect.height);
                GUI.Label(countRect, $"({itemCount})", countStyle);
            }
            
            // Draw header buttons
            if (headerButtons != null && headerButtons.Length > 0)
            {
                float buttonX = headerRect.x + headerRect.width - totalButtonWidth;
                
                foreach (var button in headerButtons)
                {
                    Rect buttonRect = new Rect(buttonX, headerRect.y + 2, button.Width, headerRect.height - 4);
                    
                    // Create button style
                    GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
                    {
                        fontSize = 10,
                        alignment = TextAnchor.MiddleCenter,
                        padding = new RectOffset(2, 2, 2, 2)
                    };
                    
                    // Apply custom colors if specified
                    if (button.BackgroundColor.HasValue || button.TextColor.HasValue)
                    {
                        Color bgColor = button.BackgroundColor ?? (GUI.skin.button.normal.background.name == "builtin skins/darkskin images/btn" 
                            ? new Color(0.4f, 0.4f, 0.4f, 1f) : new Color(0.8f, 0.8f, 0.8f, 1f));
                        Color txtColor = button.TextColor ?? Color.white;
                        
                        buttonStyle.normal.background = CreateSolidColorTexture(bgColor);
                        buttonStyle.normal.textColor = txtColor;
                        buttonStyle.hover.background = CreateSolidColorTexture(Color.Lerp(bgColor, Color.white, 0.1f));
                        buttonStyle.hover.textColor = txtColor;
                    }
                    
                    // Draw button with tooltip
                    GUIContent buttonContent = string.IsNullOrEmpty(button.Tooltip) 
                        ? new GUIContent(button.Text) 
                        : new GUIContent(button.Text, button.Tooltip);
                    
                    if (GUI.Button(buttonRect, buttonContent, buttonStyle))
                    {
                        button.OnClick?.Invoke();
                    }
                    
                    buttonX += button.Width + 5f;
                }
            }
            
            // Draw content if expanded
            if (isExpanded && drawContent != null)
            {
                EditorGUILayout.Space(2);
                
                // Content area with themed background and proper padding
                EditorGUILayout.BeginVertical();
                var contentRect = EditorGUILayout.BeginVertical();
                EditorGUILayout.Space(8);
                
                // Add horizontal padding for content to prevent border clipping
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(12); // Left padding to prevent text from touching border
                
                EditorGUILayout.BeginVertical();
                // Draw the content
                drawContent.Invoke();
                EditorGUILayout.EndVertical();
                
                GUILayout.Space(12); // Right padding
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(8);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                
                // Draw content background (conditional)
                if (drawBackground && Event.current.type == EventType.Repaint)
                {
                    Rect backgroundRect = new Rect(
                        headerRect.x,
                        headerRect.y + headerRect.height + 2,
                        headerRect.width,
                        contentRect.height + 16
                    );
                    
                    // Light background (reduced alpha for less visual clutter)
                    Color contentBackground = background;
                    contentBackground.a = 0.05f;
                    EditorGUI.DrawRect(backgroundRect, contentBackground);
                    
                    // Subtle border
                    float borderWidth = 1f;
                    Color borderColor = border;
                    borderColor.a = 0.3f;
                    EditorGUI.DrawRect(new Rect(backgroundRect.x, backgroundRect.y, backgroundRect.width, borderWidth), borderColor);
                    EditorGUI.DrawRect(new Rect(backgroundRect.x, backgroundRect.y + backgroundRect.height - borderWidth, backgroundRect.width, borderWidth), borderColor);
                    EditorGUI.DrawRect(new Rect(backgroundRect.x, backgroundRect.y, borderWidth, backgroundRect.height), borderColor);
                    EditorGUI.DrawRect(new Rect(backgroundRect.x + backgroundRect.width - borderWidth, backgroundRect.y, borderWidth, backgroundRect.height), borderColor);
                }
            }
            
            EditorGUILayout.Space(5);
            return isExpanded;
        }
        
        /// <summary>
        /// Draws a simple foldout without themed styling for basic use cases.
        /// </summary>
        public static bool DrawSimpleFoldout(System.Type editorType, string id, string title, System.Action drawContent, bool defaultExpanded = true)
        {
            string foldoutKey = $"{editorType.Name}_{id}";
            
            if (!globalFoldoutStates.ContainsKey(foldoutKey))
                globalFoldoutStates[foldoutKey] = defaultExpanded;
            
            globalFoldoutStates[foldoutKey] = EditorGUILayout.Foldout(globalFoldoutStates[foldoutKey], title, true);
            
            if (globalFoldoutStates[foldoutKey] && drawContent != null)
            {
                EditorGUI.indentLevel++;
                drawContent.Invoke();
                EditorGUI.indentLevel--;
            }
            
            return globalFoldoutStates[foldoutKey];
        }
        
        /// <summary>
        /// Gets the current expansion state of a foldout.
        /// </summary>
        public static bool GetFoldoutState(System.Type editorType, string id)
        {
            string foldoutKey = $"{editorType.Name}_{id}";
            return globalFoldoutStates.ContainsKey(foldoutKey) && globalFoldoutStates[foldoutKey];
        }
        
        /// <summary>
        /// Sets the expansion state of a foldout.
        /// </summary>
        public static void SetFoldoutState(System.Type editorType, string id, bool expanded)
        {
            string foldoutKey = $"{editorType.Name}_{id}";
            globalFoldoutStates[foldoutKey] = expanded;
        }
        
        /// <summary>
        /// Collapses all foldouts for the specified editor type.
        /// </summary>
        public static void CollapseAllFoldouts(System.Type editorType)
        {
            var keysToUpdate = globalFoldoutStates.Keys.Where(k => k.StartsWith(editorType.Name + "_")).ToList();
            foreach (var key in keysToUpdate)
            {
                globalFoldoutStates[key] = false;
            }
        }
        
        /// <summary>
        /// Expands all foldouts for the specified editor type.
        /// </summary>
        public static void ExpandAllFoldouts(System.Type editorType)
        {
            var keysToUpdate = globalFoldoutStates.Keys.Where(k => k.StartsWith(editorType.Name + "_")).ToList();
            foreach (var key in keysToUpdate)
            {
                globalFoldoutStates[key] = true;
            }
        }
        
        /// <summary>
        /// Draws expand/collapse all buttons for the specified editor type.
        /// </summary>
        public static void DrawFoldoutControls(System.Type editorType)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Expand All", GUILayout.Width(80)))
            {
                ExpandAllFoldouts(editorType);
            }
            
            if (GUILayout.Button("Collapse All", GUILayout.Width(80)))
            {
                CollapseAllFoldouts(editorType);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// Gets CLGF theme colors.
        /// </summary>
        public static (Color background, Color border, Color label) GetCLGFThemeColors(CLGFBaseEditor.CLGFTheme theme)
        {
            return theme switch
            {
                CLGFBaseEditor.CLGFTheme.Event => 
                    (new Color(0.3f, 0.7f, 0.9f, 0.05f), new Color(0.3f, 0.7f, 0.9f, 0.8f), new Color(0.2f, 0.6f, 0.8f, 0.8f)),
                CLGFBaseEditor.CLGFTheme.Action => 
                    (new Color(0.3f, 0.9f, 0.4f, 0.05f), new Color(0.3f, 0.9f, 0.4f, 0.8f), new Color(0.2f, 0.7f, 0.3f, 0.8f)),
                CLGFBaseEditor.CLGFTheme.Collision => 
                    (new Color(0.9f, 0.7f, 0.3f, 0.05f), new Color(0.9f, 0.7f, 0.3f, 0.8f), new Color(0.8f, 0.6f, 0.2f, 0.8f)),
                CLGFBaseEditor.CLGFTheme.ObjectControl => 
                    (new Color(0.3f, 0.9f, 0.4f, 0.05f), new Color(0.3f, 0.9f, 0.4f, 0.8f), new Color(0.2f, 0.7f, 0.3f, 0.8f)),
                CLGFBaseEditor.CLGFTheme.Character => 
                    (new Color(0.8f, 0.4f, 0.9f, 0.05f), new Color(0.8f, 0.4f, 0.9f, 0.8f), new Color(0.7f, 0.3f, 0.8f, 0.8f)),
                CLGFBaseEditor.CLGFTheme.Camera => 
                    (new Color(0.4f, 0.9f, 0.8f, 0.05f), new Color(0.4f, 0.9f, 0.8f, 0.8f), new Color(0.3f, 0.8f, 0.7f, 0.8f)),
                CLGFBaseEditor.CLGFTheme.UI => 
                    (new Color(0.9f, 0.5f, 0.7f, 0.05f), new Color(0.9f, 0.5f, 0.7f, 0.8f), new Color(0.8f, 0.4f, 0.6f, 0.8f)),
                CLGFBaseEditor.CLGFTheme.System => 
                    (new Color(0.9f, 0.3f, 0.3f, 0.05f), new Color(0.9f, 0.3f, 0.3f, 0.8f), new Color(0.8f, 0.2f, 0.2f, 0.8f)),
                _ => (Color.gray, Color.white, Color.black)
            };
        }
        
        /// <summary>
        /// Creates a solid color texture for button styling.
        /// </summary>
        private static Texture2D CreateSolidColorTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}