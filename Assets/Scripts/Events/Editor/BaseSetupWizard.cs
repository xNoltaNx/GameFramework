#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using GameFramework.Core.Editor;

namespace GameFramework.Events.Editor
{
    /// <summary>
    /// Base class for setup wizards that provides common functionality like
    /// step navigation, UI theming, and configuration management.
    /// </summary>
    public abstract class BaseSetupWizard<TConfig> : EditorWindow 
        where TConfig : class, new()
    {
        protected TConfig config = new TConfig();
        protected Vector2 scrollPosition;
        protected int currentStepIndex = 0;
        
        // Abstract methods that derived classes must implement
        protected abstract WizardStepInfo[] GetWizardSteps();
        protected abstract void DrawStepContent(WizardStepInfo step);
        protected abstract CLGFBaseEditor.CLGFTheme GetStepTheme(WizardStepInfo step);
        protected abstract bool CanProceedToStep(int stepIndex);
        protected abstract bool CanProceedFromStep(int stepIndex);
        protected abstract void OnWizardComplete();
        
        // Optional virtual methods that can be overridden
        protected virtual void OnWizardStart() { }
        protected virtual void OnStepChanged(int oldStep, int newStep) { }
        protected virtual string GetWizardTitle() => GetType().Name.Replace("Wizard", " Wizard");
        protected virtual Vector2 GetMinWindowSize() => new Vector2(750, 700);
        
        protected void OnEnable()
        {
            minSize = GetMinWindowSize();
            OnWizardStart();
        }
        
        protected void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            DrawStepIndicator();
            DrawHeader();
            
            EditorGUILayout.Space(10);
            
            // Draw step content with themed background
            DrawStepContentWithBackground();
            
            EditorGUILayout.Space(10);
            DrawNavigationButtons();
            
            EditorGUILayout.EndScrollView();
        }
        
        #region Step Management
        
        protected WizardStepInfo GetCurrentStep()
        {
            var steps = GetWizardSteps();
            return currentStepIndex >= 0 && currentStepIndex < steps.Length 
                ? steps[currentStepIndex] 
                : null;
        }
        
        protected bool IsFirstStep() => currentStepIndex == 0;
        protected bool IsLastStep() => currentStepIndex == GetWizardSteps().Length - 1;
        
        protected void NavigateToStep(int stepIndex)
        {
            var steps = GetWizardSteps();
            if (stepIndex >= 0 && stepIndex < steps.Length)
            {
                int oldStep = currentStepIndex;
                currentStepIndex = stepIndex;
                OnStepChanged(oldStep, stepIndex);
            }
        }
        
        protected void NextStep()
        {
            if (!IsLastStep() && CanProceedFromStep(currentStepIndex))
            {
                NavigateToStep(currentStepIndex + 1);
            }
        }
        
        protected void PreviousStep()
        {
            if (!IsFirstStep())
            {
                NavigateToStep(currentStepIndex - 1);
            }
        }
        
        #endregion
        
        #region UI Drawing
        
        private void DrawStepIndicator()
        {
            var steps = GetWizardSteps();
            if (steps.Length <= 1) return;
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            for (int i = 0; i < steps.Length; i++)
            {
                var step = steps[i];
                bool isCurrentStep = i == currentStepIndex;
                bool isCompleted = i < currentStepIndex;
                bool canAccess = CanProceedToStep(i);
                
                // Choose colors based on step state - cleaner rectangular style
                Color backgroundColor;
                Color textColor;
                
                if (isCurrentStep)
                {
                    var (bg, border, label) = GetCLGFThemeColors(GetStepTheme(step));
                    backgroundColor = border; // Use theme color for active step
                    textColor = Color.white;
                }
                else if (isCompleted)
                {
                    // Use darker version of the step's theme color for completed steps
                    var (bg, border, label) = GetCLGFThemeColors(GetStepTheme(step));
                    backgroundColor = Color.Lerp(border, Color.black, 0.4f); // Darker theme color
                    textColor = Color.white;
                }
                else if (canAccess)
                {
                    backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f); // Gray for available
                    textColor = Color.white;
                }
                else
                {
                    backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f); // Dark gray for disabled
                    textColor = new Color(0.6f, 0.6f, 0.6f, 1.0f);
                }
                
                // Create clean rectangular button style
                var buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    normal = { background = CreateColorTexture(backgroundColor), textColor = textColor },
                    hover = { background = CreateColorTexture(Color.Lerp(backgroundColor, Color.black, 0.1f)), textColor = textColor },
                    fontStyle = isCurrentStep ? FontStyle.Bold : FontStyle.Normal,
                    fontSize = 11,
                    alignment = TextAnchor.MiddleCenter,
                    border = new RectOffset(2, 2, 2, 2),
                    margin = new RectOffset(2, 2, 2, 2)
                };
                
                // Draw step button - cleaner rectangular style like old version
                string buttonText = $"{i + 1}. {step.DisplayName.ToUpper()}";
                if (GUILayout.Button(buttonText, buttonStyle, GUILayout.Height(28), GUILayout.MinWidth(120)))
                {
                    if (canAccess)
                    {
                        NavigateToStep(i);
                    }
                }
                
                // No arrows between steps for cleaner look
            }
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(15);
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            
            // Draw current step header with theme colors
            DrawCurrentStepHeader();
            
            // Draw step description
            var currentStep = GetCurrentStep();
            if (currentStep != null && !string.IsNullOrEmpty(currentStep.Description))
            {
                DrawThemedHelpBox(currentStep.Description);
            }
        }
        
        private void DrawCurrentStepHeader()
        {
            var currentStep = GetCurrentStep();
            if (currentStep == null) return;
            
            var theme = GetStepTheme(currentStep);
            var (background, border, label) = GetCLGFThemeColors(theme);
            
            // Create header rect - taller for larger icons
            Rect headerRect = GUILayoutUtility.GetRect(0, 40);
            headerRect.x += 10;
            headerRect.width -= 20;
            
            // Draw background
            EditorGUI.DrawRect(headerRect, border);
            
            // Create styles for larger icons like old styling
            GUIStyle iconStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 32, // Much larger emoji icon like in screenshot
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(8, 0, 0, 0)
            };
            
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18, // Larger title text
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleLeft, // Left align to match screenshot
                padding = new RectOffset(50, 0, 0, 0) // Indent for icon space
            };
            
            // Draw icon and title with CLGF prefix like old styling
            GUI.Label(new Rect(headerRect.x, headerRect.y, 50, headerRect.height), currentStep.Icon, iconStyle);
            GUI.Label(headerRect, $"CLGF: {currentStep.DisplayName.ToUpper()}", titleStyle);
        }
        
        private void DrawStepContentWithBackground()
        {
            var currentStep = GetCurrentStep();
            if (currentStep == null) return;
            
            // Clean content area without background/border like old styling
            EditorGUILayout.Space(15);
            
            // Simple content area with margins but no colored background
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15); // Left margin
            
            EditorGUILayout.BeginVertical();
            
            // Draw the step content
            DrawStepContent(currentStep);
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(15); // Right margin
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
        }
        
        private void DrawNavigationButtons()
        {
            EditorGUILayout.BeginHorizontal();
            
            // Back button
            GUI.enabled = !IsFirstStep();
            if (GUILayout.Button("← Back", GUILayout.Height(30), GUILayout.Width(100)))
            {
                PreviousStep();
            }
            
            GUILayout.FlexibleSpace();
            
            // Cancel button
            GUI.enabled = true;
            if (GUILayout.Button("Cancel", GUILayout.Height(30), GUILayout.Width(100)))
            {
                Close();
            }
            
            // Next/Complete button
            GUI.enabled = CanProceedFromStep(currentStepIndex);
            string buttonText = IsLastStep() ? "Complete" : "Next →";
            if (GUILayout.Button(buttonText, GUILayout.Height(30), GUILayout.Width(100)))
            {
                if (IsLastStep())
                {
                    OnWizardComplete();
                }
                else
                {
                    NextStep();
                }
            }
            
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }
        
        protected void DrawThemedHelpBox(string message)
        {
            var currentStep = GetCurrentStep();
            if (currentStep == null) return;
            
            var theme = GetStepTheme(currentStep);
            var (background, border, label) = GetCLGFThemeColors(theme);
            
            GUIStyle helpBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                normal = { background = CreateColorTexture(background) },
                border = new RectOffset(4, 4, 4, 4),
                margin = new RectOffset(10, 10, 5, 5),
                padding = new RectOffset(10, 10, 8, 8),
                wordWrap = true
            };
            
            EditorGUILayout.LabelField(message, helpBoxStyle);
        }
        
        #endregion
        
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
        protected bool DrawFoldoutSection(string id, string title, string icon = "📁", CLGFBaseEditor.CLGFTheme theme = CLGFBaseEditor.CLGFTheme.System, System.Action drawContent = null, bool defaultExpanded = true, bool showItemCount = false, int itemCount = 0, string tooltip = "", bool drawBackground = true, FoldoutUtility.FoldoutButton[] headerButtons = null)
        {
            return FoldoutUtility.DrawFoldoutSection(GetType(), id, title, icon, theme, drawContent, defaultExpanded, showItemCount, itemCount, tooltip, drawBackground, headerButtons);
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
        /// Collapses all foldouts for this wizard.
        /// </summary>
        protected void CollapseAllFoldouts()
        {
            FoldoutUtility.CollapseAllFoldouts(GetType());
        }
        
        /// <summary>
        /// Expands all foldouts for this wizard.
        /// </summary>
        protected void ExpandAllFoldouts()
        {
            FoldoutUtility.ExpandAllFoldouts(GetType());
        }
        
        /// <summary>
        /// Draws expand/collapse all buttons for this wizard.
        /// </summary>
        protected void DrawFoldoutControls()
        {
            FoldoutUtility.DrawFoldoutControls(GetType());
        }
        
        #endregion
        
        #region Theme and Color Management
        
        private (Color background, Color border, Color label) GetCLGFThemeColors(CLGFBaseEditor.CLGFTheme theme)
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
        
        private Color GetCurrentStepBackgroundColor()
        {
            var currentStep = GetCurrentStep();
            if (currentStep == null) return Color.clear;
            
            var theme = GetStepTheme(currentStep);
            var colors = GetCLGFThemeColors(theme);
            return colors.background;
        }
        
        private Color GetCurrentStepBorderColor()
        {
            var currentStep = GetCurrentStep();
            if (currentStep == null) return Color.clear;
            
            var theme = GetStepTheme(currentStep);
            var colors = GetCLGFThemeColors(theme);
            return colors.border;
        }
        
        private Texture2D CreateColorTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
        
        /// <summary>
        /// Gets ultra-vibrant step colors with hover states and completion status.
        /// </summary>
        protected Color GetUltraVibrantStepColor(WizardStepInfo step, bool isActive, bool isCompleted, bool canNavigateTo, bool isHovering = false)
        {
            // Get the exact CLGF theme color for this step
            var theme = GetStepTheme(step);
            var themeColors = GetCLGFThemeColors(theme);
            
            // Use the exact border color from CLGF theme but force full alpha for maximum vibrancy
            Color baseColor = new Color(
                themeColors.border.r,
                themeColors.border.g, 
                themeColors.border.b,
                1.0f); // Force full alpha for vibrancy
            
            Color finalColor;
            
            if (isActive)
            {
                // Active button: Use exact CLGF border color with full alpha
                finalColor = baseColor;
            }
            else if (isCompleted)
            {
                // Completed: Darker but still recognizable as the theme color
                finalColor = Color.Lerp(baseColor, Color.black, 0.3f);
            }
            else if (canNavigateTo)
            {
                // Available: Much darker version of the theme color
                finalColor = Color.Lerp(baseColor, Color.black, 0.5f);
            }
            else
            {
                // Disabled: Gray
                finalColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
            }
            
            // Apply hover effect: make any button darker when hovered
            if (isHovering && canNavigateTo)
            {
                finalColor = Color.Lerp(finalColor, Color.black, 0.2f);
            }
            
            return finalColor;
        }
        
        /// <summary>
        /// Draws a themed section with colored background and border.
        /// </summary>
        protected void DrawThemedSection(string title, CLGFBaseEditor.CLGFTheme theme, System.Action drawContent, string icon = "📋")
        {
            var (background, border, label) = GetCLGFThemeColors(theme);
            
            // Draw section header
            Rect headerRect = GUILayoutUtility.GetRect(0, 25);
            headerRect.x += 5;
            headerRect.width -= 10;
            
            EditorGUI.DrawRect(headerRect, border);
            
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter
            };
            
            GUI.Label(headerRect, $"{icon} {title}", headerStyle);
            
            // Draw content area with themed background
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(5);
            
            var contentRect = EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(8);
            
            // Draw content
            drawContent?.Invoke();
            
            EditorGUILayout.Space(8);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
            
            // Draw background after content to get correct dimensions
            if (Event.current.type == EventType.Repaint)
            {
                Rect backgroundRect = new Rect(
                    headerRect.x,
                    headerRect.y + headerRect.height,
                    headerRect.width,
                    contentRect.height + 10
                );
                
                EditorGUI.DrawRect(backgroundRect, background);
                
                // Draw border
                float borderWidth = 1f;
                EditorGUI.DrawRect(new Rect(backgroundRect.x, backgroundRect.y, backgroundRect.width, borderWidth), border);
                EditorGUI.DrawRect(new Rect(backgroundRect.x, backgroundRect.y + backgroundRect.height - borderWidth, backgroundRect.width, borderWidth), border);
                EditorGUI.DrawRect(new Rect(backgroundRect.x, backgroundRect.y, borderWidth, backgroundRect.height), border);
                EditorGUI.DrawRect(new Rect(backgroundRect.x + backgroundRect.width - borderWidth, backgroundRect.y, borderWidth, backgroundRect.height), border);
            }
        }
        
        #region Flow Diagram System
        
        /// <summary>
        /// Data structure for flow diagram steps.
        /// </summary>
        public class FlowStep
        {
            public int StepNumber { get; set; }
            public string Icon { get; set; }
            public string Description { get; set; }
            public CLGFBaseEditor.CLGFTheme Theme { get; set; }
            public string[] Details { get; set; }
            public bool IsIndented { get; set; }
            public int IndentLevel { get; set; } // 0 = no indent, 1 = normal indent, 2 = deep indent for children
            
            public FlowStep(int stepNumber, string icon, string description, CLGFBaseEditor.CLGFTheme theme, string[] details = null, bool isIndented = false, int indentLevel = 0)
            {
                StepNumber = stepNumber;
                Icon = icon;
                Description = description;
                Theme = theme;
                Details = details ?? new string[0];
                IsIndented = isIndented;
                IndentLevel = indentLevel;
            }
        }
        
        /// <summary>
        /// Draws a flashy flow diagram showing the process steps with visual flair.
        /// </summary>
        protected void DrawFlowDiagram(List<FlowStep> steps, bool useEnhancedStyling = true)
        {
            if (steps == null || steps.Count == 0) return;
            
            if (useEnhancedStyling)
            {
                DrawEnhancedFlowDiagram(steps);
            }
            else
            {
                DrawBasicFlowDiagram(steps);
            }
        }
        
        private void DrawEnhancedFlowDiagram(List<FlowStep> steps)
        {
            // Create an eye-catching title
            GUIStyle flowTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            
            EditorGUILayout.LabelField("🔄 INTERACTION FLOW DIAGRAM", flowTitleStyle);
            EditorGUILayout.Space(8);
            
            // Create the flow container with blue background styling
            var (backgroundColor, borderColor, _) = GetCLGFThemeColors(CLGFBaseEditor.CLGFTheme.Event); // Blue theme
            backgroundColor.a = 0.15f; // More prominent blue background
            borderColor.a = 0.9f;
            
            Rect sectionStart = GUILayoutUtility.GetRect(0, 0);
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.Space(12);
            
            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                DrawEnhancedFlowStep(step);
                
                // Draw arrow to next step (except for last step)
                if (i < steps.Count - 1)
                {
                    var nextStep = steps[i + 1];
                    DrawEnhancedFlowArrow(step, nextStep);
                }
                
                EditorGUILayout.Space(5);
            }
            
            EditorGUILayout.Space(12);
            EditorGUILayout.EndVertical();
            
            // Draw enhanced background
            if (Event.current.type == EventType.Repaint)
            {
                Rect sectionEnd = GUILayoutUtility.GetLastRect();
                Rect backgroundRect = new Rect(
                    sectionEnd.x,
                    sectionStart.y,
                    sectionEnd.width,
                    sectionEnd.height + (sectionEnd.y - sectionStart.y)
                );
                
                // Draw gradient-like effect with multiple layers
                EditorGUI.DrawRect(backgroundRect, backgroundColor);
                
                // Enhanced border with glow effect
                float borderWidth = 3f;
                EditorGUI.DrawRect(new Rect(backgroundRect.x, backgroundRect.y, backgroundRect.width, borderWidth), borderColor);
                EditorGUI.DrawRect(new Rect(backgroundRect.x, backgroundRect.y + backgroundRect.height - borderWidth, backgroundRect.width, borderWidth), borderColor);
                EditorGUI.DrawRect(new Rect(backgroundRect.x, backgroundRect.y, borderWidth, backgroundRect.height), borderColor);
                EditorGUI.DrawRect(new Rect(backgroundRect.x + backgroundRect.width - borderWidth, backgroundRect.y, borderWidth, backgroundRect.height), borderColor);
                
                // Inner glow
                Color glowColor = borderColor;
                glowColor.a = 0.3f;
                EditorGUI.DrawRect(new Rect(backgroundRect.x + borderWidth, backgroundRect.y + borderWidth, backgroundRect.width - borderWidth * 2, borderWidth), glowColor);
                EditorGUI.DrawRect(new Rect(backgroundRect.x + borderWidth, backgroundRect.y + backgroundRect.height - borderWidth * 2, backgroundRect.width - borderWidth * 2, borderWidth), glowColor);
                EditorGUI.DrawRect(new Rect(backgroundRect.x + borderWidth, backgroundRect.y + borderWidth, borderWidth, backgroundRect.height - borderWidth * 2), glowColor);
                EditorGUI.DrawRect(new Rect(backgroundRect.x + backgroundRect.width - borderWidth * 2, backgroundRect.y + borderWidth, borderWidth, backgroundRect.height - borderWidth * 2), glowColor);
            }
        }
        
        private void DrawBasicFlowDiagram(List<FlowStep> steps)
        {
            EditorGUILayout.Space(10);
            
            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                
                DrawFlowStep(step);
                
                // Draw arrow to next step (except for last step)
                if (i < steps.Count - 1)
                {
                    var nextStep = steps[i + 1];
                    DrawFlowArrow(step.IsIndented, nextStep.IsIndented);
                }
                
                EditorGUILayout.Space(5);
            }
        }
        
        private void DrawEnhancedFlowStep(FlowStep step)
        {
            EditorGUILayout.BeginHorizontal();
            
            // Base indentation with support for multiple levels
            float baseIndent = 16f;
            float additionalIndent = step.IndentLevel * 60f; // 60px per indent level
            // Fallback to old IsIndented for backward compatibility
            if (step.IndentLevel == 0 && step.IsIndented) 
                additionalIndent = 60f;
            GUILayout.Space(baseIndent + additionalIndent);
            
            // Step number in a colored rectangle (like the screenshot)
            var (stepBg, stepBorder, _) = GetCLGFThemeColors(step.Theme);
            stepBg.a = 0.3f;
            stepBorder.a = 1f;
            
            GUIStyle stepNumberStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            
            // Create a vertical group to center the number box with the icon
            EditorGUILayout.BeginVertical();
            GUILayout.Space(10); // Push the number box down to center it with the emoji
            
            // Draw step number rectangle (matching screenshot style)
            Rect numberRect = GUILayoutUtility.GetRect(60, 30); // Wider rectangle like in screenshot
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(numberRect, stepBorder); // Full solid color like screenshot
            }
            EditorGUI.LabelField(numberRect, step.StepNumber.ToString(), stepNumberStyle);
            
            GUILayout.FlexibleSpace(); // Allow the rest to take up space
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(12);
            
            // Icon and description - MUCH BIGGER ICONS (matching original flashy style)
            GUIStyle iconStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 40, // Large icons for visual impact
                alignment = TextAnchor.UpperLeft
            };
            
            GUIStyle descStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.LowerLeft,
                wordWrap = true // Enable text wrapping for long descriptions
            };
            
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(step.Icon, iconStyle, GUILayout.Width(60), GUILayout.Height(50));
            EditorGUILayout.BeginVertical();
            GUILayout.Space(5); // Center align with the larger icon
            // Use ExpandWidth to ensure text has enough space and doesn't clip
            EditorGUILayout.LabelField(step.Description, descStyle, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            // Show details if any (matching original style)
            if (step.Details != null && step.Details.Length > 0)
            {
                GUIStyle detailStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 11,
                    fontStyle = FontStyle.Italic,
                    normal = { textColor = Color.gray }
                };
                
                foreach (var detail in step.Details)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(85); // Align with icon space
                    EditorGUILayout.LabelField(detail, detailStyle);
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUILayout.EndVertical();
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawFlowStep(FlowStep step)
        {
            EditorGUILayout.BeginHorizontal();
            
            // Base indentation
            float baseIndent = 16f;
            float additionalIndent = step.IsIndented ? 60f : 0f; // Extra indent for nested steps
            GUILayout.Space(baseIndent + additionalIndent);
            
            // Step number in a colored circle
            var (stepBg, stepBorder, _) = GetCLGFThemeColors(step.Theme);
            stepBg.a = 0.3f;
            stepBorder.a = 1f;
            
            GUIStyle stepNumberStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            
            // Draw step number circle
            Rect numberRect = GUILayoutUtility.GetRect(30, 30);
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(numberRect, stepBorder);
                EditorGUI.DrawRect(new Rect(numberRect.x + 1, numberRect.y + 1, numberRect.width - 2, numberRect.height - 2), stepBg);
            }
            EditorGUI.LabelField(numberRect, step.StepNumber.ToString(), stepNumberStyle);
            
            GUILayout.Space(12);
            
            // Icon and description with big icons
            GUIStyle iconStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 40, // Large icons for visual impact
                alignment = TextAnchor.UpperLeft
            };
            
            GUIStyle descStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.LowerLeft
            };
            
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(step.Icon, iconStyle, GUILayout.Width(60), GUILayout.Height(50));
            EditorGUILayout.LabelField(step.Description, descStyle, GUILayout.Height(50));
            EditorGUILayout.EndHorizontal();
            
            // Show details if any
            if (step.Details != null && step.Details.Length > 0)
            {
                foreach (var detail in step.Details)
                {
                    GUIStyle detailStyle = new GUIStyle(EditorStyles.label)
                    {
                        fontSize = 11,
                        fontStyle = FontStyle.Normal,
                        normal = { textColor = Color.gray }
                    };
                    EditorGUILayout.LabelField($"  {detail}", detailStyle);
                }
            }
            
            EditorGUILayout.EndVertical();
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawFlowArrow(bool fromIndented, bool toIndented)
        {
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            // Calculate arrow position based on indentation
            float baseIndent = 16f;
            float fromIndent = fromIndented ? 60f : 0f;
            float toIndent = toIndented ? 60f : 0f;
            
            // Position arrow at the step number rectangle center (accounting for centering offset)
            float arrowIndent = baseIndent + fromIndent + 30f; // 30f = half of rectangle width (60px)
            GUILayout.Space(arrowIndent);
            
            // Arrow style
            GUIStyle arrowStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 20,
                normal = { textColor = Color.gray },
                alignment = TextAnchor.MiddleCenter
            };
            
            // Choose arrow based on indentation transition
            string arrowChar;
            if (!fromIndented && toIndented)
            {
                // Going from main flow to indented (child) - use angled arrow
                arrowChar = "↘";
            }
            else if (fromIndented && !toIndented)
            {
                // Going from indented back to main flow - use angled arrow
                arrowChar = "↙";
            }
            else
            {
                // Same indentation level - use straight down arrow
                arrowChar = "↓";
            }
            
            EditorGUILayout.LabelField(arrowChar, arrowStyle, GUILayout.Width(30), GUILayout.Height(20));
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
        }
        
        private void DrawEnhancedFlowArrow(FlowStep fromStep, FlowStep toStep)
        {
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            // Calculate arrow position based on indentation levels
            float baseIndent = 16f;
            float fromIndent = fromStep.IndentLevel * 60f;
            float toIndent = toStep.IndentLevel * 60f;
            
            // Fallback to old IsIndented for backward compatibility
            if (fromStep.IndentLevel == 0 && fromStep.IsIndented) fromIndent = 60f;
            if (toStep.IndentLevel == 0 && toStep.IsIndented) toIndent = 60f;
            
            // Position arrow at the step number rectangle center (accounting for centering offset)
            float arrowIndent = baseIndent + fromIndent + 30f; // 30f = half of rectangle width (60px)
            GUILayout.Space(arrowIndent);
            
            // Arrow style
            GUIStyle arrowStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 20,
                normal = { textColor = Color.gray },
                alignment = TextAnchor.MiddleCenter
            };
            
            // Choose arrow based on indentation transition
            string arrowChar;
            if (fromStep.IndentLevel < toStep.IndentLevel || (!fromStep.IsIndented && toStep.IsIndented))
            {
                // Going from main flow to indented (child) - use angled arrow
                arrowChar = "↘";
            }
            else if (fromStep.IndentLevel > toStep.IndentLevel || (fromStep.IsIndented && !toStep.IsIndented))
            {
                // Going from indented back to main flow - use angled arrow
                arrowChar = "↙";
            }
            else
            {
                // Same indentation level - use straight down arrow
                arrowChar = "↓";
            }
            
            EditorGUILayout.LabelField(arrowChar, arrowStyle, GUILayout.Width(30), GUILayout.Height(20));
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
        }
        
        #endregion
        
        #endregion
    }
    
    /// <summary>
    /// Information about a wizard step.
    /// </summary>
    [Serializable]
    public class WizardStepInfo
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public CLGFBaseEditor.CLGFTheme Theme { get; set; }
        
        public WizardStepInfo(string id, string displayName, string description = "", string icon = "🎯", CLGFBaseEditor.CLGFTheme theme = CLGFBaseEditor.CLGFTheme.System)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            Icon = icon;
            Theme = theme;
        }
    }
}
#endif