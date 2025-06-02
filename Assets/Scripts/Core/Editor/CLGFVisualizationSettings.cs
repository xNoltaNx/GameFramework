using UnityEngine;
using UnityEditor;

namespace GameFramework.Core.Editor
{
    /// <summary>
    /// Editor preferences and settings for CLGF visualization features.
    /// Manages debug toggles and visualization options with persistent storage.
    /// </summary>
    public static class CLGFVisualizationSettings
    {
        // EditorPrefs keys
        private const string HIERARCHY_ICONS_KEY = "CLGF.Visualization.HierarchyIcons";
        private const string HIERARCHY_BACKGROUNDS_KEY = "CLGF.Visualization.HierarchyBackgrounds";
        private const string SCENE_GIZMOS_KEY = "CLGF.Visualization.SceneGizmos";
        private const string CONNECTION_LINES_KEY = "CLGF.Visualization.ConnectionLines";
        private const string ACTION_PREVIEWS_KEY = "CLGF.Visualization.ActionPreviews";
        private const string DEBUG_MODE_KEY = "CLGF.Visualization.DebugMode";
        private const string ALWAYS_SHOW_KEY = "CLGF.Visualization.AlwaysShow";
        private const string GIZMO_SIZE_KEY = "CLGF.Visualization.GizmoSize";
        private const string LINE_THICKNESS_KEY = "CLGF.Visualization.LineThickness";
        private const string SHOW_LISTENERS_KEY = "CLGF.Visualization.ShowListeners";
        private const string MAX_LABEL_DISTANCE_KEY = "CLGF.Visualization.MaxLabelDistance";
        private const string LABEL_FONT_SIZE_KEY = "CLGF.Visualization.LabelFontSize";
        private const string LABEL_PADDING_KEY = "CLGF.Visualization.LabelPadding";
        private const string LABEL_BACKGROUND_ALPHA_KEY = "CLGF.Visualization.LabelBackgroundAlpha";
        private const string LABEL_TEXT_ALPHA_KEY = "CLGF.Visualization.LabelTextAlpha";
        private const string LABEL_DISTANCE_FADE_START_KEY = "CLGF.Visualization.LabelDistanceFadeStart";
        private const string LABEL_DISTANCE_FADE_END_KEY = "CLGF.Visualization.LabelDistanceFadeEnd";
        private const string LABEL_MIN_ALPHA_KEY = "CLGF.Visualization.LabelMinAlpha";
        private const string LABEL_DISTANCE_SCALE_BEYOND_MAX_KEY = "CLGF.Visualization.LabelDistanceScaleBeyondMax";
        private const string LABEL_MIN_FONT_SIZE_KEY = "CLGF.Visualization.LabelMinFontSize";
        private const string LABEL_MAX_FONT_SIZE_KEY = "CLGF.Visualization.LabelMaxFontSize";
        private const string HIERARCHY_BACKGROUND_ALPHA_KEY = "CLGF.Visualization.HierarchyBackgroundAlpha";
        private const string HIERARCHY_ICON_SIZE_KEY = "CLGF.Visualization.HierarchyIconSize";
        
        // Default values
        private const bool DEFAULT_HIERARCHY_ICONS = true;
        private const bool DEFAULT_HIERARCHY_BACKGROUNDS = true;
        private const bool DEFAULT_SCENE_GIZMOS = true;
        private const bool DEFAULT_CONNECTION_LINES = true;
        private const bool DEFAULT_ACTION_PREVIEWS = true;
        private const bool DEFAULT_DEBUG_MODE = false;
        private const bool DEFAULT_ALWAYS_SHOW = false;
        private const float DEFAULT_GIZMO_SIZE = 2.0f;
        private const float DEFAULT_LINE_THICKNESS = 4.0f;
        private const bool DEFAULT_SHOW_LISTENERS = true;
        private const float DEFAULT_MAX_LABEL_DISTANCE = 100f;
        private const float DEFAULT_LABEL_FONT_SIZE = 1.0f;
        private const float DEFAULT_LABEL_PADDING = 1.0f;
        private const float DEFAULT_LABEL_BACKGROUND_ALPHA = 0.85f;
        private const float DEFAULT_LABEL_TEXT_ALPHA = 1.0f;
        private const float DEFAULT_LABEL_DISTANCE_FADE_START = 5f;
        private const float DEFAULT_LABEL_DISTANCE_FADE_END = 100f;
        private const float DEFAULT_LABEL_MIN_ALPHA = 0.25f;
        private const float DEFAULT_LABEL_DISTANCE_SCALE_BEYOND_MAX = 0.01f;
        private const int DEFAULT_LABEL_MIN_FONT_SIZE = 4;
        private const int DEFAULT_LABEL_MAX_FONT_SIZE = 24;
        private const float DEFAULT_HIERARCHY_BACKGROUND_ALPHA = 0.3f;
        private const float DEFAULT_HIERARCHY_ICON_SIZE = 1.0f;
        
        // Cached values
        private static bool? _showHierarchyIcons;
        private static bool? _showHierarchyBackgrounds;
        private static bool? _showSceneGizmos;
        private static bool? _showConnectionLines;
        private static bool? _showActionPreviews;
        private static bool? _debugMode;
        private static bool? _alwaysShow;
        private static float? _gizmoSize;
        private static float? _lineThickness;
        private static bool? _showListeners;
        private static float? _maxLabelDistance;
        private static float? _labelFontSize;
        private static float? _labelPadding;
        private static float? _labelBackgroundAlpha;
        private static float? _labelTextAlpha;
        private static float? _labelDistanceFadeStart;
        private static float? _labelDistanceFadeEnd;
        private static float? _labelMinAlpha;
        private static float? _labelDistanceScaleBeyondMax;
        private static int? _labelMinFontSize;
        private static int? _labelMaxFontSize;
        private static float? _hierarchyBackgroundAlpha;
        private static float? _hierarchyIconSize;
        
        #region Public Properties
        
        /// <summary>
        /// Whether to show emoji icons next to GameObject names in the hierarchy.
        /// </summary>
        public static bool ShowHierarchyIcons
        {
            get => _showHierarchyIcons ??= EditorPrefs.GetBool(HIERARCHY_ICONS_KEY, DEFAULT_HIERARCHY_ICONS);
            set
            {
                _showHierarchyIcons = value;
                EditorPrefs.SetBool(HIERARCHY_ICONS_KEY, value);
                EditorApplication.RepaintHierarchyWindow();
            }
        }
        
        /// <summary>
        /// Whether to show colored backgrounds for GameObjects with CLGF components in the hierarchy.
        /// </summary>
        public static bool ShowHierarchyBackgrounds
        {
            get => _showHierarchyBackgrounds ??= EditorPrefs.GetBool(HIERARCHY_BACKGROUNDS_KEY, DEFAULT_HIERARCHY_BACKGROUNDS);
            set
            {
                _showHierarchyBackgrounds = value;
                EditorPrefs.SetBool(HIERARCHY_BACKGROUNDS_KEY, value);
                EditorApplication.RepaintHierarchyWindow();
            }
        }
        
        /// <summary>
        /// Whether to show 3D scene view gizmos for CLGF components.
        /// </summary>
        public static bool ShowSceneGizmos
        {
            get => _showSceneGizmos ??= EditorPrefs.GetBool(SCENE_GIZMOS_KEY, DEFAULT_SCENE_GIZMOS);
            set
            {
                _showSceneGizmos = value;
                EditorPrefs.SetBool(SCENE_GIZMOS_KEY, value);
                SceneView.RepaintAll();
            }
        }
        
        /// <summary>
        /// Whether to show connection lines between triggers and their target objects.
        /// </summary>
        public static bool ShowConnectionLines
        {
            get => _showConnectionLines ??= EditorPrefs.GetBool(CONNECTION_LINES_KEY, DEFAULT_CONNECTION_LINES);
            set
            {
                _showConnectionLines = value;
                EditorPrefs.SetBool(CONNECTION_LINES_KEY, value);
                SceneView.RepaintAll();
            }
        }
        
        /// <summary>
        /// Whether to show action preview gizmos (target positions, rotations, etc.).
        /// </summary>
        public static bool ShowActionPreviews
        {
            get => _showActionPreviews ??= EditorPrefs.GetBool(ACTION_PREVIEWS_KEY, DEFAULT_ACTION_PREVIEWS);
            set
            {
                _showActionPreviews = value;
                EditorPrefs.SetBool(ACTION_PREVIEWS_KEY, value);
                SceneView.RepaintAll();
            }
        }
        
        /// <summary>
        /// Whether debug mode is enabled (shows additional debug information).
        /// </summary>
        public static bool DebugMode
        {
            get => _debugMode ??= EditorPrefs.GetBool(DEBUG_MODE_KEY, DEFAULT_DEBUG_MODE);
            set
            {
                _debugMode = value;
                EditorPrefs.SetBool(DEBUG_MODE_KEY, value);
                SceneView.RepaintAll();
            }
        }
        
        /// <summary>
        /// Whether to always show all CLGF visualizations regardless of selection.
        /// </summary>
        public static bool AlwaysShow
        {
            get => _alwaysShow ??= EditorPrefs.GetBool(ALWAYS_SHOW_KEY, DEFAULT_ALWAYS_SHOW);
            set
            {
                _alwaysShow = value;
                EditorPrefs.SetBool(ALWAYS_SHOW_KEY, value);
                SceneView.RepaintAll();
            }
        }
        
        /// <summary>
        /// The size multiplier for scene view gizmos.
        /// </summary>
        public static float GizmoSize
        {
            get => _gizmoSize ??= EditorPrefs.GetFloat(GIZMO_SIZE_KEY, DEFAULT_GIZMO_SIZE);
            set
            {
                _gizmoSize = Mathf.Clamp(value, 0.1f, 10.0f);
                EditorPrefs.SetFloat(GIZMO_SIZE_KEY, _gizmoSize.Value);
                SceneView.RepaintAll();
            }
        }
        
        /// <summary>
        /// The thickness of connection lines in the scene view.
        /// </summary>
        public static float LineThickness
        {
            get => _lineThickness ??= EditorPrefs.GetFloat(LINE_THICKNESS_KEY, DEFAULT_LINE_THICKNESS);
            set
            {
                _lineThickness = Mathf.Clamp(value, 0.5f, 10.0f);
                EditorPrefs.SetFloat(LINE_THICKNESS_KEY, _lineThickness.Value);
                SceneView.RepaintAll();
            }
        }
        
        /// <summary>
        /// Whether to always show GameEventListener gizmos in the scene view.
        /// </summary>
        public static bool ShowListeners
        {
            get => _showListeners ??= EditorPrefs.GetBool(SHOW_LISTENERS_KEY, DEFAULT_SHOW_LISTENERS);
            set
            {
                _showListeners = value;
                EditorPrefs.SetBool(SHOW_LISTENERS_KEY, value);
                SceneView.RepaintAll();
            }
        }
        
        /// <summary>
        /// Maximum distance at which labels reach their largest size (beyond this, labels stay small).
        /// </summary>
        public static float MaxLabelDistance
        {
            get => _maxLabelDistance ??= EditorPrefs.GetFloat(MAX_LABEL_DISTANCE_KEY, DEFAULT_MAX_LABEL_DISTANCE);
            set
            {
                _maxLabelDistance = Mathf.Clamp(value, 10f, 500f);
                EditorPrefs.SetFloat(MAX_LABEL_DISTANCE_KEY, _maxLabelDistance.Value);
                SceneView.RepaintAll();
            }
        }
        
        /// <summary>
        /// Font size multiplier for scene view labels.
        /// </summary>
        public static float LabelFontSize
        {
            get => _labelFontSize ??= EditorPrefs.GetFloat(LABEL_FONT_SIZE_KEY, DEFAULT_LABEL_FONT_SIZE);
            set
            {
                _labelFontSize = Mathf.Clamp(value, 0.5f, 3.0f);
                EditorPrefs.SetFloat(LABEL_FONT_SIZE_KEY, _labelFontSize.Value);
                SceneView.RepaintAll();
            }
        }
        
        /// <summary>
        /// Padding multiplier for label backgrounds.
        /// </summary>
        public static float LabelPadding
        {
            get => _labelPadding ??= EditorPrefs.GetFloat(LABEL_PADDING_KEY, DEFAULT_LABEL_PADDING);
            set
            {
                _labelPadding = Mathf.Clamp(value, 0.2f, 3.0f);
                EditorPrefs.SetFloat(LABEL_PADDING_KEY, _labelPadding.Value);
                SceneView.RepaintAll();
            }
        }
        
        /// <summary>
        /// Alpha transparency for label backgrounds.
        /// </summary>
        public static float LabelBackgroundAlpha
        {
            get => _labelBackgroundAlpha ??= EditorPrefs.GetFloat(LABEL_BACKGROUND_ALPHA_KEY, DEFAULT_LABEL_BACKGROUND_ALPHA);
            set
            {
                _labelBackgroundAlpha = Mathf.Clamp01(value);
                EditorPrefs.SetFloat(LABEL_BACKGROUND_ALPHA_KEY, _labelBackgroundAlpha.Value);
                SceneView.RepaintAll();
            }
        }
        
        /// <summary>
        /// Alpha transparency for label text.
        /// </summary>
        public static float LabelTextAlpha
        {
            get => _labelTextAlpha ??= EditorPrefs.GetFloat(LABEL_TEXT_ALPHA_KEY, DEFAULT_LABEL_TEXT_ALPHA);
            set
            {
                _labelTextAlpha = Mathf.Clamp01(value);
                EditorPrefs.SetFloat(LABEL_TEXT_ALPHA_KEY, _labelTextAlpha.Value);
                SceneView.RepaintAll();
            }
        }
        
        /// <summary>
        /// Distance at which labels start fading based on camera distance.
        /// </summary>
        public static float LabelDistanceFadeStart
        {
            get => _labelDistanceFadeStart ??= EditorPrefs.GetFloat(LABEL_DISTANCE_FADE_START_KEY, DEFAULT_LABEL_DISTANCE_FADE_START);
            set
            {
                _labelDistanceFadeStart = Mathf.Clamp(value, 1f, 200f);
                EditorPrefs.SetFloat(LABEL_DISTANCE_FADE_START_KEY, _labelDistanceFadeStart.Value);
                SceneView.RepaintAll();
            }
        }
        
        /// <summary>
        /// Distance at which labels finish fading based on camera distance.
        /// </summary>
        public static float LabelDistanceFadeEnd
        {
            get => _labelDistanceFadeEnd ??= EditorPrefs.GetFloat(LABEL_DISTANCE_FADE_END_KEY, DEFAULT_LABEL_DISTANCE_FADE_END);
            set
            {
                _labelDistanceFadeEnd = Mathf.Clamp(value, 10f, 500f);
                EditorPrefs.SetFloat(LABEL_DISTANCE_FADE_END_KEY, _labelDistanceFadeEnd.Value);
                SceneView.RepaintAll();
            }
        }
        
        /// <summary>
        /// Minimum alpha value for faded labels.
        /// </summary>
        public static float LabelMinAlpha
        {
            get => _labelMinAlpha ??= EditorPrefs.GetFloat(LABEL_MIN_ALPHA_KEY, DEFAULT_LABEL_MIN_ALPHA);
            set
            {
                _labelMinAlpha = Mathf.Clamp01(value);
                EditorPrefs.SetFloat(LABEL_MIN_ALPHA_KEY, _labelMinAlpha.Value);
                SceneView.RepaintAll();
            }
        }
        
        /// <summary>
        /// Scale factor for labels beyond max distance (0.01 = 1% of normal size).
        /// </summary>
        public static float LabelDistanceScaleBeyondMax
        {
            get => _labelDistanceScaleBeyondMax ??= EditorPrefs.GetFloat(LABEL_DISTANCE_SCALE_BEYOND_MAX_KEY, DEFAULT_LABEL_DISTANCE_SCALE_BEYOND_MAX);
            set
            {
                _labelDistanceScaleBeyondMax = Mathf.Clamp(value, 0.01f, 1.0f);
                EditorPrefs.SetFloat(LABEL_DISTANCE_SCALE_BEYOND_MAX_KEY, _labelDistanceScaleBeyondMax.Value);
                SceneView.RepaintAll();
            }
        }
        
        /// <summary>
        /// Minimum font size for labels (in pixels).
        /// </summary>
        public static int LabelMinFontSize
        {
            get => _labelMinFontSize ??= EditorPrefs.GetInt(LABEL_MIN_FONT_SIZE_KEY, DEFAULT_LABEL_MIN_FONT_SIZE);
            set
            {
                _labelMinFontSize = Mathf.Clamp(value, 1, 50);
                EditorPrefs.SetInt(LABEL_MIN_FONT_SIZE_KEY, _labelMinFontSize.Value);
                SceneView.RepaintAll();
            }
        }
        
        /// <summary>
        /// Maximum font size for labels (in pixels).
        /// </summary>
        public static int LabelMaxFontSize
        {
            get => _labelMaxFontSize ??= EditorPrefs.GetInt(LABEL_MAX_FONT_SIZE_KEY, DEFAULT_LABEL_MAX_FONT_SIZE);
            set
            {
                _labelMaxFontSize = Mathf.Clamp(value, 5, 100);
                EditorPrefs.SetInt(LABEL_MAX_FONT_SIZE_KEY, _labelMaxFontSize.Value);
                SceneView.RepaintAll();
            }
        }
        
        /// <summary>
        /// Alpha transparency for hierarchy background colors.
        /// </summary>
        public static float HierarchyBackgroundAlpha
        {
            get => _hierarchyBackgroundAlpha ??= EditorPrefs.GetFloat(HIERARCHY_BACKGROUND_ALPHA_KEY, DEFAULT_HIERARCHY_BACKGROUND_ALPHA);
            set
            {
                _hierarchyBackgroundAlpha = Mathf.Clamp01(value);
                EditorPrefs.SetFloat(HIERARCHY_BACKGROUND_ALPHA_KEY, _hierarchyBackgroundAlpha.Value);
                EditorApplication.RepaintHierarchyWindow();
            }
        }
        
        /// <summary>
        /// Size multiplier for hierarchy icons.
        /// </summary>
        public static float HierarchyIconSize
        {
            get => _hierarchyIconSize ??= EditorPrefs.GetFloat(HIERARCHY_ICON_SIZE_KEY, DEFAULT_HIERARCHY_ICON_SIZE);
            set
            {
                _hierarchyIconSize = Mathf.Clamp(value, 0.5f, 2.0f);
                EditorPrefs.SetFloat(HIERARCHY_ICON_SIZE_KEY, _hierarchyIconSize.Value);
                EditorApplication.RepaintHierarchyWindow();
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Reset all visualization settings to their default values.
        /// </summary>
        public static void ResetToDefaults()
        {
            ShowHierarchyIcons = DEFAULT_HIERARCHY_ICONS;
            ShowHierarchyBackgrounds = DEFAULT_HIERARCHY_BACKGROUNDS;
            ShowSceneGizmos = DEFAULT_SCENE_GIZMOS;
            ShowConnectionLines = DEFAULT_CONNECTION_LINES;
            ShowActionPreviews = DEFAULT_ACTION_PREVIEWS;
            DebugMode = DEFAULT_DEBUG_MODE;
            AlwaysShow = DEFAULT_ALWAYS_SHOW;
            GizmoSize = DEFAULT_GIZMO_SIZE;
            LineThickness = DEFAULT_LINE_THICKNESS;
            ShowListeners = DEFAULT_SHOW_LISTENERS;
            MaxLabelDistance = DEFAULT_MAX_LABEL_DISTANCE;
            LabelFontSize = DEFAULT_LABEL_FONT_SIZE;
            LabelPadding = DEFAULT_LABEL_PADDING;
            LabelBackgroundAlpha = DEFAULT_LABEL_BACKGROUND_ALPHA;
            LabelTextAlpha = DEFAULT_LABEL_TEXT_ALPHA;
            LabelDistanceFadeStart = DEFAULT_LABEL_DISTANCE_FADE_START;
            LabelDistanceFadeEnd = DEFAULT_LABEL_DISTANCE_FADE_END;
            LabelMinAlpha = DEFAULT_LABEL_MIN_ALPHA;
            LabelDistanceScaleBeyondMax = DEFAULT_LABEL_DISTANCE_SCALE_BEYOND_MAX;
            LabelMinFontSize = DEFAULT_LABEL_MIN_FONT_SIZE;
            LabelMaxFontSize = DEFAULT_LABEL_MAX_FONT_SIZE;
            HierarchyBackgroundAlpha = DEFAULT_HIERARCHY_BACKGROUND_ALPHA;
            HierarchyIconSize = DEFAULT_HIERARCHY_ICON_SIZE;
        }
        
        /// <summary>
        /// Enable all visualization features.
        /// </summary>
        public static void EnableAll()
        {
            ShowHierarchyIcons = true;
            ShowHierarchyBackgrounds = true;
            ShowSceneGizmos = true;
            ShowConnectionLines = true;
            ShowActionPreviews = true;
            AlwaysShow = true;
            ShowListeners = true;
        }
        
        /// <summary>
        /// Disable all visualization features.
        /// </summary>
        public static void DisableAll()
        {
            ShowHierarchyIcons = false;
            ShowHierarchyBackgrounds = false;
            ShowSceneGizmos = false;
            ShowConnectionLines = false;
            ShowActionPreviews = false;
            AlwaysShow = false;
            ShowListeners = false;
        }
        
        #endregion
        
        #region Settings Window
        
        /// <summary>
        /// Creates a preferences window for CLGF visualization settings.
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider CreateCLGFVisualizationSettingsProvider()
        {
            var provider = new SettingsProvider("Preferences/CLGF Visualization", SettingsScope.User)
            {
                label = "CLGF Visualization",
                guiHandler = (searchContext) =>
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("CLGF Hierarchy Visualization", EditorStyles.boldLabel);
                    
                    ShowHierarchyIcons = EditorGUILayout.Toggle(
                        new GUIContent("Show Hierarchy Icons", "Display emoji icons next to GameObjects with CLGF components"),
                        ShowHierarchyIcons);
                    
                    ShowHierarchyBackgrounds = EditorGUILayout.Toggle(
                        new GUIContent("Show Hierarchy Backgrounds", "Display colored backgrounds for GameObjects with CLGF components"),
                        ShowHierarchyBackgrounds);
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("CLGF Scene View Visualization", EditorStyles.boldLabel);
                    
                    ShowSceneGizmos = EditorGUILayout.Toggle(
                        new GUIContent("Show Scene Gizmos", "Display 3D gizmos for CLGF components in scene view"),
                        ShowSceneGizmos);
                    
                    ShowConnectionLines = EditorGUILayout.Toggle(
                        new GUIContent("Show Connection Lines", "Display lines connecting triggers to their target objects"),
                        ShowConnectionLines);
                    
                    ShowActionPreviews = EditorGUILayout.Toggle(
                        new GUIContent("Show Action Previews", "Display preview gizmos for action target positions, rotations, etc."),
                        ShowActionPreviews);
                    
                    ShowListeners = EditorGUILayout.Toggle(
                        new GUIContent("Show Listeners", "Always display GameEventListener gizmos in scene view"),
                        ShowListeners);
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Hierarchy Tuning", EditorStyles.boldLabel);
                    
                    HierarchyBackgroundAlpha = EditorGUILayout.Slider(
                        new GUIContent("Background Alpha", "Transparency of hierarchy background colors"),
                        HierarchyBackgroundAlpha, 0f, 1f);
                    
                    HierarchyIconSize = EditorGUILayout.Slider(
                        new GUIContent("Icon Size", "Size multiplier for hierarchy icons"),
                        HierarchyIconSize, 0.5f, 2.0f);
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Label Visual Tuning", EditorStyles.boldLabel);
                    
                    LabelFontSize = EditorGUILayout.Slider(
                        new GUIContent("Font Size", "Font size multiplier for scene view labels"),
                        LabelFontSize, 0.5f, 3.0f);
                    
                    LabelMinFontSize = EditorGUILayout.IntSlider(
                        new GUIContent("Min Font Size", "Minimum font size in pixels (absolute limit)"),
                        LabelMinFontSize, 1, 50);
                    
                    LabelMaxFontSize = EditorGUILayout.IntSlider(
                        new GUIContent("Max Font Size", "Maximum font size in pixels (absolute limit)"),
                        LabelMaxFontSize, 5, 100);
                    
                    LabelPadding = EditorGUILayout.Slider(
                        new GUIContent("Label Padding", "Padding multiplier for label backgrounds"),
                        LabelPadding, 0.2f, 3.0f);
                    
                    LabelBackgroundAlpha = EditorGUILayout.Slider(
                        new GUIContent("Background Alpha", "Transparency of label backgrounds"),
                        LabelBackgroundAlpha, 0f, 1f);
                    
                    LabelTextAlpha = EditorGUILayout.Slider(
                        new GUIContent("Text Alpha", "Transparency of label text"),
                        LabelTextAlpha, 0f, 1f);
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Distance & Fading", EditorStyles.boldLabel);
                    
                    MaxLabelDistance = EditorGUILayout.Slider(
                        new GUIContent("Max Label Distance", "Distance at which labels reach maximum size (beyond this they stay small)"),
                        MaxLabelDistance, 10f, 500f);
                    
                    LabelDistanceScaleBeyondMax = EditorGUILayout.Slider(
                        new GUIContent("Far Distance Scale", "Size scale for labels beyond max distance (0.01 = 1% size)"),
                        LabelDistanceScaleBeyondMax, 0.01f, 1.0f);
                    
                    LabelDistanceFadeStart = EditorGUILayout.Slider(
                        new GUIContent("Fade Start Distance", "Distance at which labels start fading"),
                        LabelDistanceFadeStart, 1f, 200f);
                    
                    LabelDistanceFadeEnd = EditorGUILayout.Slider(
                        new GUIContent("Fade End Distance", "Distance at which labels finish fading"),
                        LabelDistanceFadeEnd, 10f, 500f);
                    
                    LabelMinAlpha = EditorGUILayout.Slider(
                        new GUIContent("Min Fade Alpha", "Minimum alpha for faded labels"),
                        LabelMinAlpha, 0f, 1f);
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Display Mode", EditorStyles.boldLabel);
                    
                    AlwaysShow = EditorGUILayout.Toggle(
                        new GUIContent("Always Show All", "Show all CLGF visualizations regardless of selection (scene overview mode)"),
                        AlwaysShow);
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Debug & Display Settings", EditorStyles.boldLabel);
                    
                    DebugMode = EditorGUILayout.Toggle(
                        new GUIContent("Debug Mode", "Show additional debug information and verbose gizmos"),
                        DebugMode);
                    
                    GizmoSize = EditorGUILayout.Slider(
                        new GUIContent("Gizmo Size", "Size multiplier for scene view gizmos"),
                        GizmoSize, 0.1f, 10.0f);
                    
                    LineThickness = EditorGUILayout.Slider(
                        new GUIContent("Line Thickness", "Thickness of connection lines in scene view"),
                        LineThickness, 0.5f, 10.0f);
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
                    
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Enable All"))
                    {
                        EnableAll();
                    }
                    if (GUILayout.Button("Disable All"))
                    {
                        DisableAll();
                    }
                    if (GUILayout.Button("Reset to Defaults"))
                    {
                        ResetToDefaults();
                    }
                    EditorGUILayout.EndHorizontal();
                },
                keywords = new[] { "CLGF", "visualization", "hierarchy", "gizmos", "triggers", "actions" }
            };
            
            return provider;
        }
        
        #endregion
    }
}