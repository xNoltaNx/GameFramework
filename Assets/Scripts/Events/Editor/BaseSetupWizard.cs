#if UNITY_EDITOR
using System;
using System.Collections.Generic;
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
                
                // Choose colors based on step state
                Color backgroundColor = Color.clear;
                Color textColor = Color.gray;
                
                if (isCurrentStep)
                {
                    var (bg, border, label) = GetCLGFThemeColors(GetStepTheme(step));
                    backgroundColor = border;
                    textColor = Color.white;
                }
                else if (isCompleted)
                {
                    backgroundColor = Color.green;
                    textColor = Color.white;
                }
                else if (canAccess)
                {
                    textColor = Color.black;
                }
                
                // Create button style
                var buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    normal = { background = CreateColorTexture(backgroundColor), textColor = textColor },
                    fontStyle = isCurrentStep ? FontStyle.Bold : FontStyle.Normal
                };
                
                // Draw step button
                if (GUILayout.Button($"{i + 1}. {step.DisplayName}", buttonStyle, GUILayout.Height(25)))
                {
                    if (canAccess)
                    {
                        NavigateToStep(i);
                    }
                }
                
                // Add arrow between steps
                if (i < steps.Length - 1)
                {
                    GUILayout.Label("→", GUILayout.Width(20));
                }
            }
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
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
            
            // Create header rect
            Rect headerRect = GUILayoutUtility.GetRect(0, 28);
            headerRect.x += 10;
            headerRect.width -= 20;
            
            // Draw background
            EditorGUI.DrawRect(headerRect, border);
            
            // Create styles
            GUIStyle iconStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(8, 0, 0, 0)
            };
            
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter
            };
            
            // Draw icon and title
            GUI.Label(new Rect(headerRect.x, headerRect.y, 30, headerRect.height), currentStep.Icon, iconStyle);
            GUI.Label(headerRect, currentStep.DisplayName, titleStyle);
        }
        
        private void DrawStepContentWithBackground()
        {
            var currentStep = GetCurrentStep();
            if (currentStep == null) return;
            
            // Get the background color for current step
            Color backgroundColor = GetCurrentStepBackgroundColor();
            Color borderColor = GetCurrentStepBorderColor();
            
            // Add margin space before content
            EditorGUILayout.Space(8);
            
            // Begin colored background area with proper margins
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10); // Left margin
            
            var contentRect = EditorGUILayout.BeginVertical();
            
            // Add top margin inside the content area
            EditorGUILayout.Space(8);
            
            // Draw the step content
            DrawStepContent(currentStep);
            
            // Add bottom margin inside the content area
            EditorGUILayout.Space(8);
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10); // Right margin
            EditorGUILayout.EndHorizontal();
            
            // Add margin space after content
            EditorGUILayout.Space(8);
            
            // Draw background and border after content to get correct dimensions
            if (Event.current.type == EventType.Repaint)
            {
                // Create background rect that encompasses the entire content area including margins
                Rect backgroundRect = new Rect(
                    5, // Left position with margin
                    contentRect.y - 10, // Top position with margin
                    position.width - 10, // Full width minus left/right margins
                    contentRect.height + 20 // Height plus top/bottom margins
                );
                
                // Draw background
                EditorGUI.DrawRect(backgroundRect, backgroundColor);
                
                // Draw border with proper spacing from content
                float borderWidth = 2f;
                EditorGUI.DrawRect(new Rect(backgroundRect.x, backgroundRect.y, backgroundRect.width, borderWidth), borderColor);
                EditorGUI.DrawRect(new Rect(backgroundRect.x, backgroundRect.y + backgroundRect.height - borderWidth, backgroundRect.width, borderWidth), borderColor);
                EditorGUI.DrawRect(new Rect(backgroundRect.x, backgroundRect.y, borderWidth, backgroundRect.height), borderColor);
                EditorGUI.DrawRect(new Rect(backgroundRect.x + backgroundRect.width - borderWidth, backgroundRect.y, borderWidth, backgroundRect.height), borderColor);
            }
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
        
        #region Theme and Color Management
        
        private (Color background, Color border, Color label) GetCLGFThemeColors(CLGFBaseEditor.CLGFTheme theme)
        {
            return theme switch
            {
                CLGFBaseEditor.CLGFTheme.Event => 
                    (new Color(0.3f, 0.7f, 0.9f, 0.05f), new Color(0.3f, 0.7f, 0.9f, 0.8f), new Color(0.2f, 0.6f, 0.8f, 0.8f)),
                CLGFBaseEditor.CLGFTheme.Action => 
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