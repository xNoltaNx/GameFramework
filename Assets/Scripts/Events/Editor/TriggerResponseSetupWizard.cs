#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using GameFramework.Events.Templates;
using GameFramework.Events.Triggers;
using GameFramework.Events.Actions;
using GameFramework.Events.Conditions;
using GameFramework.Events.Channels;
using GameFramework.Events.Listeners;

namespace GameFramework.Events.Editor
{
    /// <summary>
    /// Multi-step wizard for setting up trigger/response patterns.
    /// Supports templates and custom configuration for common gameplay scenarios.
    /// </summary>
    public class TriggerResponseSetupWizard : EditorWindow
    {
        private enum WizardStep
        {
            TemplateSelection,
            TriggerSetup,
            EventChannelSetup,
            ConditionSetup,
            ResponseObjectSetup,
            Review,
            Complete
        }
        
        [MenuItem("GameFramework/Events/Complete Interaction Setup Wizard")]
        public static void ShowWindow()
        {
            var window = GetWindow<TriggerResponseSetupWizard>("Complete Interaction Setup");
            window.minSize = new Vector2(750, 700);
        }
        
        // Wizard state
        private WizardStep currentStep = WizardStep.TemplateSelection;
        private Vector2 scrollPosition;
        
        // Template system
        private TriggerResponseTemplate[] availableTemplates;
        private TriggerResponseTemplate selectedTemplate;
        private bool useTemplate = true;
        
        // Configuration data
        private GameObject triggerObject;
        private TriggerConfig triggerConfig = new TriggerConfig();
        private List<EventChannelConfig> eventChannelConfigs = new List<EventChannelConfig>();
        private List<ConditionConfig> conditionConfigs = new List<ConditionConfig>();
        private List<ResponseObjectConfig> responseObjectConfigs = new List<ResponseObjectConfig>();
        private bool requireAllConditions = true;
        private bool canRepeat = true;
        private float cooldownTime = 0f;
        private bool debugMode = false;
        
        // UI state
        private string searchFilter = "";
        private string selectedCategory = "All";
        private bool showAdvancedSettings = false;
        
        private void OnEnable()
        {
            LoadAvailableTemplates();
        }
        
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            DrawStepIndicator();
            DrawHeader();
            
            EditorGUILayout.Space(10);
            
            // Draw step content with colored background
            DrawStepContentWithBackground();
            
            EditorGUILayout.Space(10);
            DrawNavigationButtons();
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawStepContentWithBackground()
        {
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
            switch (currentStep)
            {
                case WizardStep.TemplateSelection:
                    DrawTemplateSelection();
                    break;
                case WizardStep.TriggerSetup:
                    DrawTriggerSetup();
                    break;
                case WizardStep.EventChannelSetup:
                    DrawEventChannelSetup();
                    break;
                case WizardStep.ConditionSetup:
                    DrawConditionSetup();
                    break;
                case WizardStep.ResponseObjectSetup:
                    DrawResponseObjectSetup();
                    break;
                case WizardStep.Review:
                    DrawReview();
                    break;
                case WizardStep.Complete:
                    DrawComplete();
                    break;
            }
            
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
        
        private Color GetCurrentStepBackgroundColor()
        {
            return currentStep switch
            {
                WizardStep.TemplateSelection => new Color(0.9f, 0.3f, 0.3f, 0.05f), // System (Red)
                WizardStep.TriggerSetup => new Color(0.9f, 0.7f, 0.3f, 0.05f),      // Action (Orange)
                WizardStep.EventChannelSetup => new Color(0.3f, 0.7f, 0.9f, 0.05f), // Event (Blue)
                WizardStep.ConditionSetup => new Color(0.9f, 0.7f, 0.3f, 0.05f),    // Action (Orange)
                WizardStep.ResponseObjectSetup => new Color(0.3f, 0.9f, 0.4f, 0.05f), // ObjectControl (Green)
                WizardStep.Review => new Color(0.9f, 0.3f, 0.3f, 0.05f),            // System (Red)
                WizardStep.Complete => new Color(0.9f, 0.3f, 0.3f, 0.05f),          // System (Red)
                _ => Color.gray
            };
        }
        
        private Color GetCurrentStepBorderColor()
        {
            return currentStep switch
            {
                WizardStep.TemplateSelection => new Color(0.9f, 0.3f, 0.3f, 0.3f), // System (Red)
                WizardStep.TriggerSetup => new Color(0.9f, 0.7f, 0.3f, 0.3f),      // Action (Orange)
                WizardStep.EventChannelSetup => new Color(0.3f, 0.7f, 0.9f, 0.3f), // Event (Blue)
                WizardStep.ConditionSetup => new Color(0.9f, 0.7f, 0.3f, 0.3f),    // Action (Orange)
                WizardStep.ResponseObjectSetup => new Color(0.3f, 0.9f, 0.4f, 0.3f), // ObjectControl (Green)
                WizardStep.Review => new Color(0.9f, 0.3f, 0.3f, 0.3f),            // System (Red)
                WizardStep.Complete => new Color(0.9f, 0.3f, 0.3f, 0.3f),          // System (Red)
                _ => Color.white
            };
        }
        
        #region Header and Navigation
        
        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            
            // Main title with wizard icon
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };

            // Current step header with theme colors and icons
            DrawCurrentStepHeader();
            
            string description = currentStep switch
            {
                WizardStep.TemplateSelection => "Choose a template or start from scratch to create complete multi-object interactions.",
                WizardStep.TriggerSetup => "Configure the triggering object that will start the interaction.",
                WizardStep.EventChannelSetup => "Set up event channels that connect triggers to responses.",
                WizardStep.ConditionSetup => "Add optional conditions that must be met for the interaction to occur.",
                WizardStep.ResponseObjectSetup => "Configure multiple objects that will respond to the interaction events.",
                WizardStep.Review => "Review your complete interaction system before creation.",
                WizardStep.Complete => "Setup complete! Your multi-object interaction system has been created.",
                _ => "Configure your complete interaction system."
            };
            
            DrawThemedHelpBox(description);
        }
        
        private void DrawCurrentStepHeader()
        {
            var stepInfo = GetCurrentStepInfo();
            Color borderColor = GetCurrentStepBorderColor();
            
            // Create header rect
            Rect headerRect = GUILayoutUtility.GetRect(0, 28);
            headerRect.x += 10;
            headerRect.width -= 20;
            
            // Draw background
            EditorGUI.DrawRect(headerRect, borderColor);
            
            // Create styles
            GUIStyle iconStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white },
                fontSize = 40,
                fontStyle = FontStyle.Bold
            };
            
            GUIStyle textStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white },
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            
            // Draw icon and text
            float iconWidth = iconStyle.CalcSize(new GUIContent(stepInfo.icon)).x;
            
            Rect iconRect = new Rect(headerRect.x + 8, headerRect.y, iconWidth, headerRect.height);
            EditorGUI.LabelField(iconRect, stepInfo.icon, iconStyle);
            
            Rect textRect = new Rect(iconRect.x + iconWidth, headerRect.y, headerRect.width - iconWidth - 16, headerRect.height);
            EditorGUI.LabelField(textRect, stepInfo.title, textStyle);
            
            EditorGUILayout.Space(5);
        }
        
        private (string icon, string title) GetCurrentStepInfo()
        {
            return currentStep switch
            {
                WizardStep.TemplateSelection => ("📋", "CLGF: TEMPLATE SELECTION"),
                WizardStep.TriggerSetup => ("⚡", "CLGF: TRIGGER CONFIGURATION"),
                WizardStep.EventChannelSetup => ("📡", "CLGF: EVENT CHANNELS"),
                WizardStep.ConditionSetup => ("🔍", "CLGF: TRIGGER CONDITIONS"),
                WizardStep.ResponseObjectSetup => ("🎬", "CLGF: RESPONSE OBJECTS"),
                WizardStep.Review => ("📝", "CLGF: INTERACTION REVIEW"),
                WizardStep.Complete => ("✅", "CLGF: SETUP COMPLETE"),
                _ => ("🔧", "CLGF: WIZARD STEP")
            };
        }
        
        private void DrawThemedHelpBox(string message)
        {
            Color backgroundColor = GetCurrentStepBackgroundColor();
            Color borderColor = GetCurrentStepBorderColor();
            
            // Make colors more opaque for the help box
            backgroundColor.a = 0.15f;
            borderColor.a = 0.8f;
            
            // Get rect for the help box
            GUIContent content = new GUIContent(message);
            GUIStyle helpBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                wordWrap = true,
                fontSize = 12,
                padding = new RectOffset(12, 12, 8, 8),
                normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black }
            };
            
            float height = helpBoxStyle.CalcHeight(content, EditorGUIUtility.currentViewWidth - 20);
            Rect helpBoxRect = GUILayoutUtility.GetRect(content, helpBoxStyle, GUILayout.Height(height));
            
            // Add margin
            helpBoxRect.x += 10;
            helpBoxRect.width -= 20;
            
            // Draw colored background
            EditorGUI.DrawRect(helpBoxRect, backgroundColor);
            
            // Draw border
            float borderWidth = 1f;
            EditorGUI.DrawRect(new Rect(helpBoxRect.x, helpBoxRect.y, helpBoxRect.width, borderWidth), borderColor);
            EditorGUI.DrawRect(new Rect(helpBoxRect.x, helpBoxRect.y + helpBoxRect.height - borderWidth, helpBoxRect.width, borderWidth), borderColor);
            EditorGUI.DrawRect(new Rect(helpBoxRect.x, helpBoxRect.y, borderWidth, helpBoxRect.height), borderColor);
            EditorGUI.DrawRect(new Rect(helpBoxRect.x + helpBoxRect.width - borderWidth, helpBoxRect.y, borderWidth, helpBoxRect.height), borderColor);
            
            // Draw the text
            EditorGUI.LabelField(helpBoxRect, content, helpBoxStyle);
            
            EditorGUILayout.Space(5);
        }
        
        private void DrawStepIndicator()
        {
            EditorGUILayout.Space(5);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            var steps = new[] { "Template", "Trigger", "Events", "Conditions", "Responses", "Review", "Complete" };
            var stepColors = GetStepColors();
            
            for (int i = 0; i < steps.Length; i++)
            {
                bool isActive = (int)currentStep == i;
                bool isCompleted = (int)currentStep > i;
                bool canNavigateTo = CanNavigateToStep((WizardStep)i);
                
                // Get step-specific color based on CLGF themes
                Color stepColor = stepColors[i];
                
                GUIStyle stepStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    fixedWidth = 70,
                    fixedHeight = 25,
                    normal = { textColor = isActive ? Color.white : (canNavigateTo ? Color.white : Color.gray) },
                    fontStyle = isActive ? FontStyle.Bold : FontStyle.Normal
                };
                
                // Set background color based on step type and state
                if (isActive)
                {
                    GUI.backgroundColor = stepColor;
                }
                else if (isCompleted)
                {
                    GUI.backgroundColor = Color.Lerp(stepColor, Color.green, 0.3f);
                }
                else if (canNavigateTo)
                {
                    GUI.backgroundColor = Color.Lerp(stepColor, Color.white, 0.7f);
                }
                else
                {
                    GUI.backgroundColor = Color.gray;
                }
                
                // Make button clickable for navigation
                if (GUILayout.Button(steps[i], stepStyle) && canNavigateTo)
                {
                    NavigateToStep((WizardStep)i);
                }
                
                GUI.backgroundColor = Color.white;
                
                if (i < steps.Length - 1)
                {
                    GUILayout.Label(">", GUILayout.Width(10));
                }
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            // Add legend for colors
            EditorGUILayout.Space(3);
            DrawColorLegend();
        }
        
        private Color[] GetStepColors()
        {
            // Map each step to appropriate CLGF theme colors
            return new Color[]
            {
                new Color(0.9f, 0.3f, 0.3f, 0.8f), // Template - System (Red)
                new Color(0.9f, 0.7f, 0.3f, 0.8f), // Trigger - Action (Orange) 
                new Color(0.3f, 0.7f, 0.9f, 0.8f), // Events - Event (Blue)
                new Color(0.9f, 0.7f, 0.3f, 0.8f), // Conditions - Action (Orange)
                new Color(0.3f, 0.9f, 0.4f, 0.8f), // Responses - ObjectControl (Green)
                new Color(0.9f, 0.3f, 0.3f, 0.8f), // Review - System (Red)
                new Color(0.9f, 0.3f, 0.3f, 0.8f)  // Complete - System (Red)
            };
        }
        
        private void DrawColorLegend()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            var legendStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9
            };
            
            GUILayout.Label("🟠 Triggers", legendStyle, GUILayout.Width(60));
            GUILayout.Label("🔵 Events", legendStyle, GUILayout.Width(50));
            GUILayout.Label("🟢 Actions", legendStyle, GUILayout.Width(50));
            GUILayout.Label("🔴 System", legendStyle, GUILayout.Width(50));
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        
        private bool CanNavigateToStep(WizardStep step)
        {
            // Allow navigation to completed steps and the current step
            return (int)step <= (int)currentStep || HasCompletedStep(step);
        }
        
        private bool HasCompletedStep(WizardStep step)
        {
            // Check if the step has been completed based on required data
            return step switch
            {
                WizardStep.TemplateSelection => true, // Always accessible
                WizardStep.TriggerSetup => !useTemplate || selectedTemplate != null,
                WizardStep.EventChannelSetup => triggerObject != null,
                WizardStep.ConditionSetup => eventChannelConfigs.Count > 0,
                WizardStep.ResponseObjectSetup => true, // Conditions are optional
                WizardStep.Review => responseObjectConfigs.Count > 0,
                WizardStep.Complete => false, // Only accessible after applying setup
                _ => false
            };
        }
        
        private void NavigateToStep(WizardStep step)
        {
            if (CanNavigateToStep(step))
            {
                currentStep = step;
                Repaint();
            }
        }
        
        private void DrawNavigationButtons()
        {
            GUILayout.BeginHorizontal();
            
            // Back button
            GUI.enabled = currentStep > WizardStep.TemplateSelection;
            if (GUILayout.Button("← Back", GUILayout.Height(30)))
            {
                PreviousStep();
            }
            GUI.enabled = true;
            
            GUILayout.FlexibleSpace();
            
            // Next/Finish button
            string buttonText = currentStep == WizardStep.Review ? "Apply Setup" : 
                               currentStep == WizardStep.Complete ? "Close" : "Next →";
            
            bool canProceed = CanProceedToNextStep();
            GUI.enabled = canProceed;
            
            if (GUILayout.Button(buttonText, GUILayout.Height(30), GUILayout.Width(120)))
            {
                if (currentStep == WizardStep.Review)
                {
                    ApplySetup();
                }
                else if (currentStep == WizardStep.Complete)
                {
                    Close();
                }
                else
                {
                    NextStep();
                }
            }
            GUI.enabled = true;
            
            GUILayout.EndHorizontal();
        }
        
        #endregion
        
        #region Step Implementation
        
        private void DrawTemplateSelection()
        {
            EditorGUILayout.LabelField("Template Selection", EditorStyles.boldLabel);
            
            // Template mode toggle
            EditorGUILayout.BeginHorizontal();
            bool newUseTemplate = EditorGUILayout.Toggle("Use Template", useTemplate);
            if (newUseTemplate != useTemplate)
            {
                useTemplate = newUseTemplate;
                if (!useTemplate)
                {
                    selectedTemplate = null;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            if (useTemplate)
            {
                DrawTemplateGallery();
            }
            else
            {
                EditorGUILayout.HelpBox("Manual setup selected. You'll configure each step individually.", MessageType.Info);
            }
        }
        
        private void DrawTemplateGallery()
        {
            if (availableTemplates == null || availableTemplates.Length == 0)
            {
                EditorGUILayout.HelpBox("No templates found. Create some TriggerResponseTemplate assets to get started.", MessageType.Warning);
                return;
            }
            
            // Search and filter
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            searchFilter = EditorGUILayout.TextField("Search:", searchFilter);
            
            var categories = availableTemplates.Select(t => t.Category).Distinct().Prepend("All").ToArray();
            int categoryIndex = Array.IndexOf(categories, selectedCategory);
            if (categoryIndex < 0) categoryIndex = 0;
            
            int newCategoryIndex = EditorGUILayout.Popup("Category:", categoryIndex, categories, GUILayout.Width(120));
            selectedCategory = categories[newCategoryIndex];
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Template grid
            var filteredTemplates = availableTemplates.Where(t => 
                (string.IsNullOrEmpty(searchFilter) || t.TemplateName.ToLower().Contains(searchFilter.ToLower())) &&
                (selectedCategory == "All" || t.Category == selectedCategory)
            ).ToArray();
            
            if (filteredTemplates.Length == 0)
            {
                EditorGUILayout.HelpBox("No templates match your filter criteria.", MessageType.Info);
                return;
            }
            
            int columns = 2;
            int rows = Mathf.CeilToInt((float)filteredTemplates.Length / columns);
            
            for (int row = 0; row < rows; row++)
            {
                EditorGUILayout.BeginHorizontal();
                
                for (int col = 0; col < columns; col++)
                {
                    int index = row * columns + col;
                    if (index >= filteredTemplates.Length) break;
                    
                    var template = filteredTemplates[index];
                    DrawTemplateCard(template);
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        private void DrawTemplateCard(TriggerResponseTemplate template)
        {
            bool isSelected = selectedTemplate == template;
            
            GUIStyle cardStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
            
            if (isSelected)
            {
                GUI.backgroundColor = Color.cyan;
            }
            
            EditorGUILayout.BeginVertical(cardStyle, GUILayout.Width(220), GUILayout.Height(120));
            
            // Template icon and name
            EditorGUILayout.BeginHorizontal();
            if (template.TemplateIcon != null)
            {
                GUILayout.Label(template.TemplateIcon, GUILayout.Width(32), GUILayout.Height(32));
            }
            else
            {
                GUILayout.Label("🎯", new GUIStyle(EditorStyles.label) { fontSize = 24 }, GUILayout.Width(32), GUILayout.Height(32));
            }
            
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(template.TemplateName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Difficulty: {GetDifficultyString(template.Difficulty)}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            // Description
            EditorGUILayout.LabelField(template.Description, EditorStyles.wordWrappedMiniLabel, GUILayout.Height(40));
            
            // Select button
            if (GUILayout.Button(isSelected ? "Selected" : "Select"))
            {
                selectedTemplate = template;
                LoadTemplateConfiguration();
            }
            
            EditorGUILayout.EndVertical();
            
            GUI.backgroundColor = Color.white;
            
            GUILayout.Space(10);
        }
        
        private void DrawTriggerSetup()
        {
            EditorGUILayout.LabelField("Trigger Configuration", EditorStyles.boldLabel);
            
            // Trigger object selection
            triggerObject = (GameObject)EditorGUILayout.ObjectField(
                new GUIContent("Trigger GameObject", "The GameObject that will start the interaction (receives trigger components)"),
                triggerObject, typeof(GameObject), true);
                
            if (triggerObject == null)
            {
                EditorGUILayout.HelpBox("Please select a trigger GameObject to continue.", MessageType.Warning);
                return;
            }
            
            EditorGUILayout.Space(10);
            
            // Trigger type selection
            triggerConfig.triggerType = (TriggerType)EditorGUILayout.EnumPopup("Trigger Type", triggerConfig.triggerType);
            
            EditorGUILayout.Space(5);
            
            // Type-specific settings
            switch (triggerConfig.triggerType)
            {
                case TriggerType.Collision:
                    DrawCollisionTriggerSettings();
                    break;
                case TriggerType.Proximity:
                    DrawProximityTriggerSettings();
                    break;
                case TriggerType.Timer:
                    DrawTimerTriggerSettings();
                    break;
            }
            
            EditorGUILayout.Space(10);
            
            // General settings
            EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
            canRepeat = EditorGUILayout.Toggle("Can Repeat", canRepeat);
            if (canRepeat)
            {
                cooldownTime = EditorGUILayout.FloatField("Cooldown Time", cooldownTime);
            }
            debugMode = EditorGUILayout.Toggle("Debug Mode", debugMode);
        }
        
        private void DrawEventChannelSetup()
        {
            EditorGUILayout.LabelField("Event Channel Configuration", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox("Event channels connect triggers to responses. When a trigger fires, it raises events that listeners respond to.", MessageType.Info);
            
            // List existing event channels
            for (int i = 0; i < eventChannelConfigs.Count; i++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Event Channel {i + 1}", EditorStyles.boldLabel);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    eventChannelConfigs.RemoveAt(i);
                    i--;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    continue;
                }
                EditorGUILayout.EndHorizontal();
                
                eventChannelConfigs[i].eventName = EditorGUILayout.TextField("Event Name", eventChannelConfigs[i].eventName);
                eventChannelConfigs[i].description = EditorGUILayout.TextField("Description", eventChannelConfigs[i].description);
                eventChannelConfigs[i].createNewEvent = EditorGUILayout.Toggle("Create New Event", eventChannelConfigs[i].createNewEvent);
                
                if (!eventChannelConfigs[i].createNewEvent)
                {
                    eventChannelConfigs[i].existingEventPath = EditorGUILayout.TextField("Existing Event Path", eventChannelConfigs[i].existingEventPath);
                }
                
                EditorGUILayout.EndVertical();
            }
            
            // Add event channel button
            if (GUILayout.Button("Add Event Channel"))
            {
                eventChannelConfigs.Add(new EventChannelConfig());
            }
            
            if (eventChannelConfigs.Count == 0)
            {
                EditorGUILayout.HelpBox("Add at least one event channel to connect triggers to responses.", MessageType.Warning);
            }
        }
        
        private void DrawConditionSetup()
        {
            EditorGUILayout.LabelField("Condition Setup (Optional)", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox("Conditions allow you to add additional checks before the trigger fires. Leave empty to always trigger.", MessageType.Info);
            
            if (conditionConfigs.Count > 0)
            {
                requireAllConditions = EditorGUILayout.Toggle("Require All Conditions", requireAllConditions);
                EditorGUILayout.Space(5);
            }
            
            // List existing conditions
            for (int i = 0; i < conditionConfigs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                conditionConfigs[i].conditionType = (ConditionType)EditorGUILayout.EnumPopup($"Condition {i + 1}", conditionConfigs[i].conditionType);
                conditionConfigs[i].invertResult = EditorGUILayout.Toggle("Invert", conditionConfigs[i].invertResult, GUILayout.Width(60));
                
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    conditionConfigs.RemoveAt(i);
                    i--;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            // Add condition button
            if (GUILayout.Button("Add Condition"))
            {
                conditionConfigs.Add(new ConditionConfig());
            }
        }
        
        private void DrawResponseObjectSetup()
        {
            EditorGUILayout.LabelField("Response Object Configuration", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox("Response objects listen to events and perform actions. Create multiple objects for complex interactions.", MessageType.Info);
            
            // List existing response objects
            for (int i = 0; i < responseObjectConfigs.Count; i++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Response Object {i + 1}", EditorStyles.boldLabel);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    responseObjectConfigs.RemoveAt(i);
                    i--;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    continue;
                }
                EditorGUILayout.EndHorizontal();
                
                var responseConfig = responseObjectConfigs[i];
                
                responseConfig.objectName = EditorGUILayout.TextField("Object Name", responseConfig.objectName);
                responseConfig.description = EditorGUILayout.TextField("Description", responseConfig.description);
                responseConfig.createNewObject = EditorGUILayout.Toggle("Create New Object", responseConfig.createNewObject);
                
                if (!responseConfig.createNewObject)
                {
                    responseConfig.targetObjectId = EditorGUILayout.TextField("Target Object ID", responseConfig.targetObjectId);
                }
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Events to Listen For", EditorStyles.boldLabel);
                
                // Event subscriptions
                for (int j = 0; j < responseConfig.listenToEvents.Count; j++)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    // Show available event names from eventChannelConfigs
                    if (eventChannelConfigs.Count > 0)
                    {
                        var eventNames = eventChannelConfigs.ConvertAll(e => e.eventName).ToArray();
                        int currentIndex = Array.IndexOf(eventNames, responseConfig.listenToEvents[j]);
                        if (currentIndex < 0) currentIndex = 0;
                        
                        int newIndex = EditorGUILayout.Popup($"Event {j + 1}", currentIndex, eventNames);
                        if (newIndex >= 0 && newIndex < eventNames.Length)
                        {
                            responseConfig.listenToEvents[j] = eventNames[newIndex];
                        }
                    }
                    else
                    {
                        responseConfig.listenToEvents[j] = EditorGUILayout.TextField($"Event {j + 1}", responseConfig.listenToEvents[j]);
                    }
                    
                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        responseConfig.listenToEvents.RemoveAt(j);
                        j--;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                if (GUILayout.Button("Add Event Listener"))
                {
                    responseConfig.listenToEvents.Add(eventChannelConfigs.Count > 0 ? eventChannelConfigs[0].eventName : "");
                }
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
                
                // Actions for this response object
                for (int j = 0; j < responseConfig.actions.Count; j++)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    responseConfig.actions[j].actionType = (ActionType)EditorGUILayout.EnumPopup($"Action {j + 1}", responseConfig.actions[j].actionType);
                    responseConfig.actions[j].executionDelay = EditorGUILayout.FloatField("Delay", responseConfig.actions[j].executionDelay, GUILayout.Width(80));
                    
                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        responseConfig.actions.RemoveAt(j);
                        j--;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                if (GUILayout.Button("Add Action"))
                {
                    responseConfig.actions.Add(new ActionConfig());
                }
                
                EditorGUILayout.EndVertical();
            }
            
            // Add response object button
            if (GUILayout.Button("Add Response Object"))
            {
                responseObjectConfigs.Add(new ResponseObjectConfig());
            }
            
            if (responseObjectConfigs.Count == 0)
            {
                EditorGUILayout.HelpBox("Add at least one response object to complete the interaction.", MessageType.Warning);
            }
        }
        
        
        private void DrawReview()
        {
            EditorGUILayout.LabelField("Complete Interaction Review", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox("Review your complete interaction system below. Click 'Apply Setup' to create all components and event connections.", MessageType.Info);
            
            // Trigger object
            EditorGUILayout.LabelField("Trigger GameObject:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(triggerObject ? triggerObject.name : "None");
            
            EditorGUILayout.Space(5);
            
            // Trigger configuration
            EditorGUILayout.LabelField("Trigger Configuration:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Type: {triggerConfig.triggerType}");
            EditorGUILayout.LabelField($"Can Repeat: {canRepeat}");
            if (canRepeat && cooldownTime > 0)
            {
                EditorGUILayout.LabelField($"Cooldown: {cooldownTime}s");
            }
            
            EditorGUILayout.Space(5);
            
            // Event channels
            EditorGUILayout.LabelField("Event Channels:", EditorStyles.boldLabel);
            if (eventChannelConfigs.Count == 0)
            {
                EditorGUILayout.LabelField("None");
            }
            else
            {
                foreach (var eventConfig in eventChannelConfigs)
                {
                    EditorGUILayout.LabelField($"• {eventConfig.eventName} ({(eventConfig.createNewEvent ? "new" : "existing")})");
                }
            }
            
            EditorGUILayout.Space(5);
            
            // Conditions
            EditorGUILayout.LabelField("Conditions:", EditorStyles.boldLabel);
            if (conditionConfigs.Count == 0)
            {
                EditorGUILayout.LabelField("None (always trigger)");
            }
            else
            {
                EditorGUILayout.LabelField($"{conditionConfigs.Count} condition(s) - {(requireAllConditions ? "All must be met" : "Any can be met")}");
            }
            
            EditorGUILayout.Space(5);
            
            // Response objects
            EditorGUILayout.LabelField("Response Objects:", EditorStyles.boldLabel);
            if (responseObjectConfigs.Count == 0)
            {
                EditorGUILayout.LabelField("None");
            }
            else
            {
                foreach (var responseConfig in responseObjectConfigs)
                {
                    EditorGUILayout.LabelField($"• {responseConfig.objectName} ({(responseConfig.createNewObject ? "new" : "existing")})");
                    foreach (var eventName in responseConfig.listenToEvents)
                    {
                        EditorGUILayout.LabelField($"  → Listens to: {eventName}");
                    }
                    foreach (var action in responseConfig.actions)
                    {
                        EditorGUILayout.LabelField($"  → Action: {action.actionType}" + (action.executionDelay > 0 ? $" (delay: {action.executionDelay}s)" : ""));
                    }
                }
            }
            
            EditorGUILayout.Space(10);
            
            // Interaction flow visualization
            EditorGUILayout.LabelField("Interaction Flow:", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            
            EditorGUILayout.LabelField($"1. {triggerConfig.triggerType} trigger on '{(triggerObject ? triggerObject.name : "TriggerObject")}'");
            
            if (conditionConfigs.Count > 0)
            {
                EditorGUILayout.LabelField($"2. Check {conditionConfigs.Count} condition(s)");
            }
            
            foreach (var eventConfig in eventChannelConfigs)
            {
                EditorGUILayout.LabelField($"3. Raise '{eventConfig.eventName}' event");
                
                var listeners = responseObjectConfigs.Where(r => r.listenToEvents.Contains(eventConfig.eventName)).ToArray();
                foreach (var listener in listeners)
                {
                    EditorGUILayout.LabelField($"   → {listener.objectName} responds with {listener.actions.Count} action(s)");
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawComplete()
        {
            EditorGUILayout.LabelField("Complete Interaction Setup Complete!", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox("Your complete interaction system has been successfully created with event-driven architecture spanning multiple GameObjects.", MessageType.Info);
            
            if (triggerObject != null)
            {
                EditorGUILayout.Space(10);
                
                EditorGUILayout.LabelField("Created Components:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"• Trigger components on '{triggerObject.name}'");
                
                foreach (var eventConfig in eventChannelConfigs)
                {
                    EditorGUILayout.LabelField($"• GameEvent asset: '{eventConfig.eventName}'");
                }
                
                foreach (var responseConfig in responseObjectConfigs)
                {
                    EditorGUILayout.LabelField($"• Response object: '{responseConfig.objectName}'");
                }
                
                EditorGUILayout.Space(10);
                
                if (GUILayout.Button("Select Trigger GameObject"))
                {
                    Selection.activeGameObject = triggerObject;
                    EditorGUIUtility.PingObject(triggerObject);
                }
                
                if (GUILayout.Button("Create Another Interaction"))
                {
                    ResetWizard();
                }
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private void LoadAvailableTemplates()
        {
            string[] guids = AssetDatabase.FindAssets("t:TriggerResponseTemplate");
            availableTemplates = new TriggerResponseTemplate[guids.Length];
            
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                availableTemplates[i] = AssetDatabase.LoadAssetAtPath<TriggerResponseTemplate>(path);
            }
        }
        
        private void LoadTemplateConfiguration()
        {
            if (selectedTemplate == null) return;
            
            // Load template settings
            triggerConfig = selectedTemplate.TriggerSettings;
            eventChannelConfigs = new List<EventChannelConfig>(selectedTemplate.EventChannels);
            conditionConfigs = new List<ConditionConfig>(selectedTemplate.Conditions);
            responseObjectConfigs = new List<ResponseObjectConfig>(selectedTemplate.ResponseObjects);
            requireAllConditions = selectedTemplate.RequireAllConditions;
            canRepeat = selectedTemplate.CanRepeat;
            cooldownTime = selectedTemplate.CooldownTime;
            debugMode = selectedTemplate.DebugMode;
        }
        
        private string GetDifficultyString(int difficulty)
        {
            return difficulty switch
            {
                1 => "Beginner",
                2 => "Intermediate",
                3 => "Advanced",
                _ => "Unknown"
            };
        }
        
        private void DrawCollisionTriggerSettings()
        {
            triggerConfig.collisionEvent = (CollisionTrigger.TriggerEvent)EditorGUILayout.EnumPopup("Collision Event", triggerConfig.collisionEvent);
            triggerConfig.triggerLayers = EditorGUILayout.MaskField("Trigger Layers", triggerConfig.triggerLayers, UnityEditorInternal.InternalEditorUtility.layers);
            triggerConfig.requiredTag = EditorGUILayout.TagField("Required Tag", triggerConfig.requiredTag);
            triggerConfig.requireRigidbody = EditorGUILayout.Toggle("Require Rigidbody", triggerConfig.requireRigidbody);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Collider Setup", EditorStyles.boldLabel);
            triggerConfig.colliderType = (ColliderType)EditorGUILayout.EnumPopup("Collider Type", triggerConfig.colliderType);
            
            if (triggerObject != null)
            {
                var existingCollider = triggerObject.GetComponent<Collider>();
                if (existingCollider != null)
                {
                    EditorGUILayout.HelpBox($"Trigger object already has a {existingCollider.GetType().Name}. It will be used instead.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox($"A {triggerConfig.colliderType} will be added to the trigger GameObject.", MessageType.Info);
                }
            }
        }
        
        private void DrawProximityTriggerSettings()
        {
            triggerConfig.proximityEvent = (ProximityTrigger.ProximityEvent)EditorGUILayout.EnumPopup("Proximity Event", triggerConfig.proximityEvent);
            triggerConfig.triggerDistance = EditorGUILayout.FloatField("Trigger Distance", triggerConfig.triggerDistance);
            triggerConfig.checkInterval = EditorGUILayout.FloatField("Check Interval", triggerConfig.checkInterval);
            triggerConfig.use3DDistance = EditorGUILayout.Toggle("Use 3D Distance", triggerConfig.use3DDistance);
            triggerConfig.targetMode = (ProximityTrigger.TargetMode)EditorGUILayout.EnumPopup("Target Mode", triggerConfig.targetMode);
            
            if (triggerConfig.targetMode == ProximityTrigger.TargetMode.FindByTag)
            {
                triggerConfig.targetTag = EditorGUILayout.TagField("Target Tag", triggerConfig.targetTag);
            }
        }
        
        private void DrawTimerTriggerSettings()
        {
            triggerConfig.timerDuration = EditorGUILayout.FloatField("Timer Duration", triggerConfig.timerDuration);
            triggerConfig.startOnAwake = EditorGUILayout.Toggle("Start On Awake", triggerConfig.startOnAwake);
            triggerConfig.autoReset = EditorGUILayout.Toggle("Auto Reset", triggerConfig.autoReset);
        }
        
        private void DrawActionTypeSettings(ActionConfig actionConfig)
        {
            // Placeholder for action-specific settings
            // In a full implementation, you'd add specific UI for each action type
            switch (actionConfig.actionType)
            {
                case ActionType.AudioAction:
                    EditorGUILayout.HelpBox("Audio settings: Configure audio clips, volume, pitch, 3D settings.", MessageType.Info);
                    break;
                case ActionType.GameObjectActivateAction:
                    EditorGUILayout.HelpBox("GameObject settings: Configure which objects to activate/deactivate.", MessageType.Info);
                    break;
                case ActionType.MoveAction:
                    EditorGUILayout.HelpBox("Movement settings: Configure target position, duration, easing.", MessageType.Info);
                    break;
                case ActionType.RotateAction:
                    EditorGUILayout.HelpBox("Rotation settings: Configure target rotation, duration, easing.", MessageType.Info);
                    break;
                case ActionType.ScaleAction:
                    EditorGUILayout.HelpBox("Scale settings: Configure target scale, duration, easing.", MessageType.Info);
                    break;
                case ActionType.InstantiateAction:
                    EditorGUILayout.HelpBox("Instantiate settings: Configure prefab, spawn position, rotation.", MessageType.Info);
                    break;
                case ActionType.DestroyAction:
                    EditorGUILayout.HelpBox("Destroy settings: Configure target objects, delay, effects.", MessageType.Info);
                    break;
                case ActionType.ComponentToggleAction:
                    EditorGUILayout.HelpBox("Component settings: Configure which components to enable/disable.", MessageType.Info);
                    break;
                case ActionType.MaterialPropertyAction:
                    EditorGUILayout.HelpBox("Material settings: Configure material properties to modify.", MessageType.Info);
                    break;
                default:
                    EditorGUILayout.HelpBox("Action-specific settings will be configured on the component.", MessageType.Info);
                    break;
            }
        }
        
        private bool CanProceedToNextStep()
        {
            return currentStep switch
            {
                WizardStep.TemplateSelection => !useTemplate || selectedTemplate != null,
                WizardStep.TriggerSetup => triggerObject != null,
                WizardStep.EventChannelSetup => eventChannelConfigs.Count > 0,
                WizardStep.ConditionSetup => true, // Conditions are optional
                WizardStep.ResponseObjectSetup => responseObjectConfigs.Count > 0,
                WizardStep.Review => true,
                WizardStep.Complete => true,
                _ => false
            };
        }
        
        private void NextStep()
        {
            if (currentStep < WizardStep.Complete)
            {
                currentStep++;
            }
        }
        
        private void PreviousStep()
        {
            if (currentStep > WizardStep.TemplateSelection)
            {
                currentStep--;
            }
        }
        
        private void ResetWizard()
        {
            currentStep = WizardStep.TemplateSelection;
            selectedTemplate = null;
            triggerObject = null;
            triggerConfig = new TriggerConfig();
            eventChannelConfigs.Clear();
            conditionConfigs.Clear();
            responseObjectConfigs.Clear();
            requireAllConditions = true;
            canRepeat = true;
            cooldownTime = 0f;
            debugMode = false;
            useTemplate = true;
        }
        
        #endregion
        
        #region Setup Application
        
        private void ApplySetup()
        {
            if (triggerObject == null)
            {
                EditorUtility.DisplayDialog("Error", "No trigger GameObject selected.", "OK");
                return;
            }
            
            try
            {
                // Record undo
                Undo.RecordObject(triggerObject, "Add Complete Interaction System");
                
                // Create event channels first
                CreateEventChannels();
                
                // Create collider if needed for collision triggers
                if (triggerConfig.triggerType == TriggerType.Collision)
                {
                    CreateColliderIfNeeded();
                }
                
                // Create trigger component
                BaseTrigger trigger = CreateTriggerComponent();
                if (trigger == null)
                {
                    EditorUtility.DisplayDialog("Error", "Failed to create trigger component.", "OK");
                    return;
                }
                
                // Configure trigger
                ConfigureTrigger(trigger);
                
                // Create condition components
                CreateConditionComponents();
                
                // Create raise event action on trigger
                CreateRaiseEventActions();
                
                // Create response objects and their components
                CreateResponseObjects();
                
                // Mark scene as dirty
                EditorSceneManager.MarkSceneDirty(triggerObject.scene);
                
                // Move to complete step
                currentStep = WizardStep.Complete;
                
                EditorUtility.DisplayDialog("Success", "Complete interaction system created successfully!", "OK");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to apply setup: {e.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to create interaction system: {e.Message}", "OK");
            }
        }
        
        private BaseTrigger CreateTriggerComponent()
        {
            return triggerConfig.triggerType switch
            {
                TriggerType.Collision => triggerObject.AddComponent<CollisionTrigger>(),
                TriggerType.Proximity => triggerObject.AddComponent<ProximityTrigger>(),
                TriggerType.Timer => triggerObject.AddComponent<TimerTrigger>(),
                _ => null
            };
        }
        
        private void ConfigureTrigger(BaseTrigger trigger)
        {
            // Use reflection to set canRepeat (as it's read-only property)
            var canRepeatField = typeof(BaseTrigger).GetField("canRepeat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            canRepeatField?.SetValue(trigger, canRepeat);
            
            // Use reflection to set cooldown time (as it's protected)
            var cooldownField = typeof(BaseTrigger).GetField("cooldownTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            cooldownField?.SetValue(trigger, cooldownTime);
            
            // Use reflection to set debug mode
            var debugField = typeof(BaseTrigger).GetField("debugMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            debugField?.SetValue(trigger, debugMode);
            
            // Configure type-specific settings
            switch (trigger)
            {
                case CollisionTrigger collisionTrigger:
                    ConfigureCollisionTrigger(collisionTrigger);
                    break;
                case ProximityTrigger proximityTrigger:
                    ConfigureProximityTrigger(proximityTrigger);
                    break;
                case TimerTrigger timerTrigger:
                    ConfigureTimerTrigger(timerTrigger);
                    break;
            }
        }
        
        private void ConfigureCollisionTrigger(CollisionTrigger trigger)
        {
            trigger.SetTriggerEvent(triggerConfig.collisionEvent);
            trigger.SetTriggerLayers(triggerConfig.triggerLayers);
            trigger.SetRequiredTag(triggerConfig.requiredTag);
            trigger.SetRequireRigidbody(triggerConfig.requireRigidbody);
        }
        
        private void ConfigureProximityTrigger(ProximityTrigger trigger)
        {
            trigger.SetProximityEvent(triggerConfig.proximityEvent);
            trigger.SetTriggerDistance(triggerConfig.triggerDistance);
            trigger.SetCheckInterval(triggerConfig.checkInterval);
            
            // Set target mode using reflection (if needed)
            var targetModeField = typeof(ProximityTrigger).GetField("targetMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            targetModeField?.SetValue(trigger, triggerConfig.targetMode);
            
            var use3DField = typeof(ProximityTrigger).GetField("use3DDistance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            use3DField?.SetValue(trigger, triggerConfig.use3DDistance);
            
            var targetTagField = typeof(ProximityTrigger).GetField("targetTag", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            targetTagField?.SetValue(trigger, triggerConfig.targetTag);
        }
        
        private void ConfigureTimerTrigger(TimerTrigger trigger)
        {
            trigger.SetDuration(triggerConfig.timerDuration);
            
            // Set start on awake using reflection
            var startOnAwakeField = typeof(TimerTrigger).GetField("startOnAwake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            startOnAwakeField?.SetValue(trigger, triggerConfig.startOnAwake);
        }
        
        private void CreateColliderIfNeeded()
        {
            // Check if trigger object already has a collider
            if (triggerObject.GetComponent<Collider>() != null)
            {
                return; // Already has a collider
            }
            
            // Add the specified collider type
            switch (triggerConfig.colliderType)
            {
                case ColliderType.BoxCollider:
                    var boxCollider = triggerObject.AddComponent<BoxCollider>();
                    boxCollider.isTrigger = true;
                    break;
                case ColliderType.SphereCollider:
                    var sphereCollider = triggerObject.AddComponent<SphereCollider>();
                    sphereCollider.isTrigger = true;
                    break;
                case ColliderType.CapsuleCollider:
                    var capsuleCollider = triggerObject.AddComponent<CapsuleCollider>();
                    capsuleCollider.isTrigger = true;
                    break;
                case ColliderType.MeshCollider:
                    var meshCollider = triggerObject.AddComponent<MeshCollider>();
                    meshCollider.isTrigger = true;
                    meshCollider.convex = true; // Required for trigger colliders
                    break;
            }
        }
        
        private void CreateConditionComponents()
        {
            foreach (var conditionConfig in conditionConfigs)
            {
                BaseTriggerCondition condition = CreateConditionComponent(conditionConfig);
                if (condition != null)
                {
                    ConfigureConditionComponent(condition, conditionConfig);
                }
            }
        }
        
        private BaseTriggerCondition CreateConditionComponent(ConditionConfig conditionConfig)
        {
            return conditionConfig.conditionType switch
            {
                ConditionType.TagCondition => triggerObject.AddComponent<TagCondition>(),
                ConditionType.LayerCondition => triggerObject.AddComponent<LayerCondition>(),
                ConditionType.DistanceCondition => triggerObject.AddComponent<DistanceCondition>(),
                _ => null
            };
        }
        
        private void ConfigureConditionComponent(BaseTriggerCondition condition, ConditionConfig conditionConfig)
        {
            if (conditionConfig.invertResult)
            {
                condition.SetInvertResult(true);
            }
            
            // Type-specific configuration would go here
            // For a full implementation, you'd deserialize conditionConfig.conditionData
            // and configure the condition accordingly
        }
        
        // Store created event assets for use in other methods
        private System.Collections.Generic.Dictionary<string, GameEvent> createdGameEvents = new System.Collections.Generic.Dictionary<string, GameEvent>();
        
        private void CreateEventChannels()
        {
            createdGameEvents.Clear();
            
            foreach (var eventConfig in eventChannelConfigs)
            {
                GameEvent gameEvent = null;
                
                if (eventConfig.createNewEvent)
                {
                    // Create new GameEvent asset
                    gameEvent = CreateGameEventAsset(eventConfig);
                    if (gameEvent != null)
                    {
                        Debug.Log($"Created GameEvent asset: {eventConfig.eventName} at {AssetDatabase.GetAssetPath(gameEvent)}");
                        createdGameEvents[eventConfig.eventName] = gameEvent;
                    }
                    else
                    {
                        Debug.LogError($"Failed to create GameEvent asset: {eventConfig.eventName}");
                    }
                }
                else
                {
                    // Load existing GameEvent asset
                    gameEvent = LoadExistingGameEvent(eventConfig);
                    if (gameEvent != null)
                    {
                        Debug.Log($"Loaded existing GameEvent: {eventConfig.eventName} from {eventConfig.existingEventPath}");
                        createdGameEvents[eventConfig.eventName] = gameEvent;
                    }
                    else
                    {
                        Debug.LogError($"Failed to load existing GameEvent: {eventConfig.existingEventPath}");
                    }
                }
            }
        }
        
        private GameEvent CreateGameEventAsset(EventChannelConfig eventConfig)
        {
            try
            {
                // Create the GameEvent ScriptableObject
                GameEvent gameEvent = ScriptableObject.CreateInstance<GameEvent>();
                
                // Set the channel name and description
                var channelNameField = typeof(GameEvent).BaseType.GetField("channelName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                channelNameField?.SetValue(gameEvent, eventConfig.eventName);
                
                var descriptionField = typeof(GameEvent).BaseType.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                descriptionField?.SetValue(gameEvent, eventConfig.description);
                
                // Determine asset save path
                string assetPath = GetEventAssetPath(eventConfig.eventName);
                
                // Ensure directory exists
                string directory = System.IO.Path.GetDirectoryName(assetPath);
                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                    AssetDatabase.Refresh();
                }
                
                // Create the asset
                AssetDatabase.CreateAsset(gameEvent, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                return gameEvent;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception creating GameEvent asset '{eventConfig.eventName}': {e.Message}");
                return null;
            }
        }
        
        private GameEvent LoadExistingGameEvent(EventChannelConfig eventConfig)
        {
            try
            {
                if (string.IsNullOrEmpty(eventConfig.existingEventPath))
                {
                    Debug.LogWarning($"No path specified for existing event: {eventConfig.eventName}");
                    return null;
                }
                
                // Load the asset at the specified path
                GameEvent gameEvent = AssetDatabase.LoadAssetAtPath<GameEvent>(eventConfig.existingEventPath);
                
                if (gameEvent == null)
                {
                    Debug.LogWarning($"Could not load GameEvent at path: {eventConfig.existingEventPath}");
                }
                
                return gameEvent;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception loading GameEvent at '{eventConfig.existingEventPath}': {e.Message}");
                return null;
            }
        }
        
        private string GetEventAssetPath(string eventName)
        {
            // Clean the event name for use as filename
            string cleanEventName = eventName.Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
            
            // Create path in Events folder structure
            string basePath = "Assets/Content/Events";
            
            if (triggerObject != null)
            {
                // Try to organize by trigger object name
                string triggerName = triggerObject.name.Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
                basePath = $"{basePath}/{triggerName}";
            }
            
            return $"{basePath}/{cleanEventName}.asset";
        }
        
        private void CreateRaiseEventActions()
        {
            if (triggerObject == null)
            {
                Debug.LogError("Cannot create raise event actions - trigger object is null");
                return;
            }
            
            if (eventChannelConfigs.Count == 0)
            {
                Debug.LogWarning("No event channels configured - no raise actions will be created");
                return;
            }
            
            // Collect all GameEvents that were created/loaded
            var gameEventsToRaise = new System.Collections.Generic.List<GameEvent>();
            
            foreach (var eventConfig in eventChannelConfigs)
            {
                if (createdGameEvents.TryGetValue(eventConfig.eventName, out GameEvent gameEvent))
                {
                    gameEventsToRaise.Add(gameEvent);
                    Debug.Log($"Will configure RaiseGameEventAction for event: {eventConfig.eventName}");
                }
                else
                {
                    Debug.LogError($"GameEvent '{eventConfig.eventName}' not found in created events dictionary");
                }
            }
            
            if (gameEventsToRaise.Count == 0)
            {
                Debug.LogError("No valid GameEvents found to configure in RaiseGameEventAction");
                return;
            }
            
            // Check if trigger object already has a RaiseGameEventAction
            var existingRaiseAction = triggerObject.GetComponent<RaiseGameEventAction>();
            RaiseGameEventAction raiseAction;
            
            if (existingRaiseAction != null)
            {
                raiseAction = existingRaiseAction;
                Debug.Log("Using existing RaiseGameEventAction component");
            }
            else
            {
                raiseAction = triggerObject.AddComponent<RaiseGameEventAction>();
                Debug.Log("Added new RaiseGameEventAction component");
            }
            
            // Configure the RaiseGameEventAction with all the GameEvents
            raiseAction.SetGameEvents(gameEventsToRaise);
            
            // Optional: Configure execution delay if specified in global settings
            if (cooldownTime > 0)
            {
                raiseAction.SetExecutionDelay(0f); // Event raising should be immediate, cooldown is handled by trigger
            }
            
            // Mark the component as dirty for undo/redo
            UnityEditor.EditorUtility.SetDirty(raiseAction);
            
            Debug.Log($"Configured RaiseGameEventAction with {gameEventsToRaise.Count} GameEvent(s)");
        }
        
        private void CreateResponseObjects()
        {
            if (responseObjectConfigs.Count == 0)
            {
                Debug.LogWarning("No response objects configured");
                return;
            }
            
            foreach (var responseConfig in responseObjectConfigs)
            {
                GameObject responseObject = null;
                
                if (responseConfig.createNewObject)
                {
                    // Create new GameObject
                    responseObject = CreateNewResponseObject(responseConfig);
                }
                else
                {
                    // Find existing GameObject
                    responseObject = FindExistingResponseObject(responseConfig);
                    if (responseObject == null)
                    {
                        Debug.LogWarning($"Could not find existing object: {responseConfig.targetObjectId}. Skipping this response object.");
                        continue;
                    }
                }
                
                if (responseObject == null)
                {
                    Debug.LogError($"Failed to get response object for config: {responseConfig.objectName}");
                    continue;
                }
                
                // Create GameEventListeners for each event this object should listen to
                CreateGameEventListeners(responseObject, responseConfig);
                
                // Create action components for this response object
                CreateActionComponents(responseObject, responseConfig);
                
                // Mark object as dirty for undo/redo
                UnityEditor.EditorUtility.SetDirty(responseObject);
                
                Debug.Log($"Successfully configured response object: {responseObject.name}");
            }
        }
        
        private GameObject CreateNewResponseObject(ResponseObjectConfig responseConfig)
        {
            try
            {
                GameObject responseObject = new GameObject(responseConfig.objectName);
                
                // Position it near the trigger object if possible
                if (triggerObject != null)
                {
                    Vector3 offset = new Vector3(2f, 0f, 0f); // Place 2 units to the right of trigger
                    responseObject.transform.position = triggerObject.transform.position + offset;
                }
                
                // Register undo for the creation
                Undo.RegisterCreatedObjectUndo(responseObject, $"Create Response Object: {responseConfig.objectName}");
                
                Debug.Log($"Created new response object: {responseConfig.objectName}");
                return responseObject;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception creating response object '{responseConfig.objectName}': {e.Message}");
                return null;
            }
        }
        
        private GameObject FindExistingResponseObject(ResponseObjectConfig responseConfig)
        {
            try
            {
                // Try finding by exact name first
                GameObject responseObject = GameObject.Find(responseConfig.targetObjectId);
                
                if (responseObject == null)
                {
                    // Try finding by partial name match
                    GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                    foreach (var obj in allObjects)
                    {
                        if (obj.name.Contains(responseConfig.targetObjectId))
                        {
                            responseObject = obj;
                            Debug.Log($"Found object by partial name match: {obj.name}");
                            break;
                        }
                    }
                }
                
                if (responseObject != null)
                {
                    Debug.Log($"Found existing response object: {responseObject.name}");
                }
                
                return responseObject;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception finding existing object '{responseConfig.targetObjectId}': {e.Message}");
                return null;
            }
        }
        
        private void CreateGameEventListeners(GameObject responseObject, ResponseObjectConfig responseConfig)
        {
            foreach (var eventName in responseConfig.listenToEvents)
            {
                if (!createdGameEvents.TryGetValue(eventName, out GameEvent gameEvent))
                {
                    Debug.LogError($"GameEvent '{eventName}' not found for response object '{responseObject.name}'. Skipping listener creation.");
                    continue;
                }
                
                try
                {
                    // Check if there's already a GameEventListener for this event
                    var existingListeners = responseObject.GetComponents<GameEventListener>();
                    GameEventListener existingListener = null;
                    
                    foreach (var listener in existingListeners)
                    {
                        if (listener.GameEvent == gameEvent)
                        {
                            existingListener = listener;
                            break;
                        }
                    }
                    
                    GameEventListener gameEventListener;
                    if (existingListener != null)
                    {
                        gameEventListener = existingListener;
                        Debug.Log($"Using existing GameEventListener for event: {eventName} on {responseObject.name}");
                    }
                    else
                    {
                        gameEventListener = responseObject.AddComponent<GameEventListener>();
                        Debug.Log($"Added new GameEventListener for event: {eventName} on {responseObject.name}");
                    }
                    
                    // Configure the listener
                    gameEventListener.SetGameEvent(gameEvent);
                    
                    // Enable debug mode if global debug is on
                    if (debugMode)
                    {
                        var debugField = typeof(GameEventListener).GetField("debugMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        debugField?.SetValue(gameEventListener, true);
                    }
                    
                    // Mark component as dirty
                    UnityEditor.EditorUtility.SetDirty(gameEventListener);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Exception creating GameEventListener for '{eventName}' on '{responseObject.name}': {e.Message}");
                }
            }
        }
        
        private void CreateActionComponents(GameObject responseObject, ResponseObjectConfig responseConfig)
        {
            foreach (var actionConfig in responseConfig.actions)
            {
                try
                {
                    BaseTriggerAction action = CreateActionComponent(responseObject, actionConfig);
                    if (action != null)
                    {
                        ConfigureActionComponent(action, actionConfig);
                        
                        // Connect action to GameEventListener UnityEvents
                        ConnectActionToEventListeners(responseObject, action, responseConfig);
                        
                        UnityEditor.EditorUtility.SetDirty(action);
                        Debug.Log($"Created and configured {actionConfig.actionType} on {responseObject.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to create action component: {actionConfig.actionType} on {responseObject.name}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Exception creating action '{actionConfig.actionType}' on '{responseObject.name}': {e.Message}");
                }
            }
        }
        
        private void ConnectActionToEventListeners(GameObject responseObject, BaseTriggerAction action, ResponseObjectConfig responseConfig)
        {
            try
            {
                var listeners = responseObject.GetComponents<GameEventListener>();
                
                foreach (var listener in listeners)
                {
                    if (listener.GameEvent == null) continue;
                    
                    // Check if this listener is for an event this response object should listen to
                    string eventName = GetEventNameFromGameEvent(listener.GameEvent);
                    if (responseConfig.listenToEvents.Contains(eventName))
                    {
                        // Connect the action to this listener's UnityEvent
                        var onEventRaised = listener.OnEventRaised;
                        
                        // Add a persistent call to the action's Execute method
                        UnityEditor.Events.UnityEventTools.AddPersistentListener(onEventRaised, action.Execute);
                        Debug.Log($"Connected {action.GetType().Name} to GameEventListener for event: {eventName}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception connecting action to event listeners: {e.Message}");
            }
        }
        
        private string GetEventNameFromGameEvent(GameEvent gameEvent)
        {
            // Try to get the channel name using reflection
            var channelNameField = typeof(GameEvent).BaseType.GetField("channelName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (channelNameField != null)
            {
                return (string)channelNameField.GetValue(gameEvent) ?? gameEvent.name;
            }
            
            return gameEvent.name;
        }
        
        private BaseTriggerAction CreateActionComponent(GameObject targetObj, ActionConfig actionConfig)
        {
            return actionConfig.actionType switch
            {
                ActionType.AudioAction => targetObj.AddComponent<AudioAction>(),
                ActionType.GameObjectActivateAction => targetObj.AddComponent<GameObjectActivateAction>(),
                ActionType.InstantiateAction => targetObj.AddComponent<InstantiateAction>(),
                ActionType.DestroyAction => targetObj.AddComponent<DestroyAction>(),
                ActionType.ComponentToggleAction => targetObj.AddComponent<ComponentToggleAction>(),
                ActionType.MoveAction => targetObj.AddComponent<MoveAction>(),
                ActionType.RotateAction => targetObj.AddComponent<RotateAction>(),
                ActionType.ScaleAction => targetObj.AddComponent<ScaleAction>(),
                ActionType.MaterialPropertyAction => targetObj.AddComponent<MaterialPropertyAction>(),
                // Note: Other action types like LightAction, ParticleAction, etc. would be added as they're implemented
                _ => null
            };
        }
        
        private void ConfigureActionComponent(BaseTriggerAction action, ActionConfig actionConfig)
        {
            if (actionConfig.executionDelay > 0)
            {
                action.SetExecutionDelay(actionConfig.executionDelay);
            }
            
            // Type-specific configuration would go here
            // For a full implementation, you'd deserialize actionConfig.actionData
            // and configure the action accordingly
        }
        
        #endregion
    }
}
#endif