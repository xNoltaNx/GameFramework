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
using GameFramework.Core.Editor;

namespace GameFramework.Events.Editor
{
    /// <summary>
    /// Configuration class for the trigger response setup wizard.
    /// </summary>
    [System.Serializable]
    public class TriggerResponseConfig
    {
        // This can be expanded later if needed for shared configuration
    }
    
    /// <summary>
    /// Multi-step wizard for setting up trigger/response patterns.
    /// Supports templates and custom configuration for common gameplay scenarios.
    /// </summary>
    public class TriggerResponseSetupWizard : BaseSetupWizard<TriggerResponseConfig>
    {
        private enum WizardStep
        {
            ProjectSetup,
            TemplateSelection,
            TriggerSetup,
            EventChannelSetup,
            ConditionSetup,
            ResponseObjectSetup,
            Review,
            Complete
        }
        
        private enum CreationMode
        {
            SceneObjects,
            Prefabs
        }
        
        [MenuItem("GameFramework/Events/Complete Interaction Setup Wizard")]
        public static void ShowWindow()
        {
            var window = GetWindow<TriggerResponseSetupWizard>("Complete Interaction Setup");
            window.minSize = new Vector2(750, 750);
        }
        
        
        // Template system
        private TriggerResponseTemplate[] availableTemplates;
        private TriggerResponseTemplate selectedTemplate;
        private bool useTemplate = true;
        
        // Configuration data
        private GameObject triggerObject;
        private bool createNewTriggerObject = false;
        private string newTriggerObjectName = "New Trigger";
        private TriggerConfig triggerConfig = new TriggerConfig();
        private List<EventChannelConfig> eventChannelConfigs = new List<EventChannelConfig>();
        private List<ConditionConfig> conditionConfigs = new List<ConditionConfig>();
        private List<ResponseObjectConfig> responseObjectConfigs = new List<ResponseObjectConfig>();
        private List<System.Action> pendingActions = new List<System.Action>();
        private bool requireAllConditions = true;
        private bool canRepeat = true;
        private float cooldownTime = 0f;
        private bool debugMode = false;
        
        // Project setup configuration
        private string projectName = "NewInteractionProject";
        private CreationMode creationMode = CreationMode.SceneObjects;
        private bool useExistingProjectFolder = false;
        private string selectedProjectFolderPath = "";
        private string[] existingProjectFolders = new string[0];
        
        // UI state
        private string searchFilter = "";
        private string selectedCategory = "All";
        private bool showAdvancedSettings = false;
        
        // Removed old foldout states - now using FoldoutUtility
        
        #region BaseSetupWizard Implementation
        
        protected override WizardStepInfo[] GetWizardSteps()
        {
            return new WizardStepInfo[]
            {
                new WizardStepInfo("ProjectSetup", "Project Setup", "Configure the project and creation mode for your interaction system.", "🏗️", CLGFBaseEditor.CLGFTheme.System),
                new WizardStepInfo("TemplateSelection", "Template Selection", "Choose a template or start from scratch to create complete multi-object interactions.", "📋", CLGFBaseEditor.CLGFTheme.System),
                new WizardStepInfo("TriggerSetup", "Trigger Setup", "Configure the triggering object that will start the interaction.", "⚡", CLGFBaseEditor.CLGFTheme.Action),
                new WizardStepInfo("EventChannelSetup", "Event Channels", "Set up event channels that connect triggers to responses.", "📡", CLGFBaseEditor.CLGFTheme.Event),
                new WizardStepInfo("ConditionSetup", "Conditions", "Add optional conditions that must be met for the interaction to occur.", "🔍", CLGFBaseEditor.CLGFTheme.Action),
                new WizardStepInfo("ResponseObjectSetup", "Response Objects", "Configure multiple objects that will respond to the interaction events.", "🎬", CLGFBaseEditor.CLGFTheme.ObjectControl),
                new WizardStepInfo("Review", "Review", "Review your complete interaction system before creation.", "📝", CLGFBaseEditor.CLGFTheme.Character),
                new WizardStepInfo("Complete", "Complete", "Setup complete! Your multi-object interaction system has been created.", "✅", CLGFBaseEditor.CLGFTheme.System)
            };
        }
        
        protected override void DrawStepContent(WizardStepInfo step)
        {
            switch (step.Id)
            {
                case "ProjectSetup":
                    DrawProjectSetupStep();
                    break;
                case "TemplateSelection":
                    DrawTemplateSelectionStep();
                    break;
                case "TriggerSetup":
                    DrawTriggerSetupStep();
                    break;
                case "EventChannelSetup":
                    DrawEventChannelSetupStep();
                    break;
                case "ConditionSetup":
                    DrawConditionSetupStep();
                    break;
                case "ResponseObjectSetup":
                    DrawResponseObjectSetupStep();
                    break;
                case "Review":
                    DrawReviewStep();
                    break;
                case "Complete":
                    DrawCompletionStep();
                    break;
            }
        }
        
        protected override CLGFBaseEditor.CLGFTheme GetStepTheme(WizardStepInfo step)
        {
            return step.Theme;
        }
        
        protected override bool CanProceedToStep(int stepIndex)
        {
            var steps = GetWizardSteps();
            if (stepIndex >= steps.Length) return false;
            
            return stepIndex switch
            {
                0 => true, // Project Setup always accessible
                1 => true, // Template Selection always accessible after project setup
                2 => true, // Trigger Setup accessible after template selection
                3 => HasValidTriggerConfiguration(), // Event Channel Setup requires valid trigger
                4 => eventChannelConfigs.Count > 0, // Condition Setup requires event channels
                5 => eventChannelConfigs.Count > 0, // Response Object Setup requires event channels
                6 => HasValidSetupConfiguration(), // Review requires valid configuration
                7 => HasValidSetupConfiguration(), // Complete requires valid configuration
                _ => false
            };
        }
        
        protected override bool CanProceedFromStep(int stepIndex)
        {
            return stepIndex switch
            {
                0 => !string.IsNullOrEmpty(projectName?.Trim()), // Project Setup requires project name
                1 => selectedTemplate != null || !useTemplate, // Template Selection requires selection or manual mode
                2 => HasValidTriggerConfiguration(), // Trigger Setup requires valid trigger
                3 => eventChannelConfigs.Count > 0, // Event Channel Setup requires at least one channel
                4 => true, // Condition Setup is optional
                5 => responseObjectConfigs.Count > 0, // Response Object Setup requires at least one response
                6 => HasValidSetupConfiguration(), // Review requires valid configuration
                7 => true, // Complete step can always proceed (closes wizard)
                _ => false
            };
        }
        
        protected override void OnWizardComplete()
        {
            CompleteSetup();
        }
        
        protected override void OnWizardStart()
        {
            LoadAvailableTemplates();
            LoadExistingProjectFolders();
        }
        
        protected override string GetWizardTitle()
        {
            return "Complete Interaction Setup Wizard";
        }
        
        protected override Vector2 GetMinWindowSize()
        {
            return new Vector2(750, 750);
        }
        
        
        private WizardStep GetCurrentWizardStep()
        {
            var stepId = GetCurrentStep()?.Id ?? "ProjectSetup";
            return stepId switch
            {
                "ProjectSetup" => WizardStep.ProjectSetup,
                "TemplateSelection" => WizardStep.TemplateSelection,
                "TriggerSetup" => WizardStep.TriggerSetup,
                "EventChannelSetup" => WizardStep.EventChannelSetup,
                "ConditionSetup" => WizardStep.ConditionSetup,
                "ResponseObjectSetup" => WizardStep.ResponseObjectSetup,
                "Review" => WizardStep.Review,
                "Complete" => WizardStep.Complete,
                _ => WizardStep.ProjectSetup
            };
        }
        
        #endregion
        
        #region Legacy Methods (Updated to use BaseSetupWizard)
        private void DrawProjectSetupStep()
        {
            DrawProjectSetup();
        }
        private void DrawTemplateSelectionStep()
        {
            DrawTemplateSelection();
        }
        private void DrawTriggerSetupStep()
        {
            DrawTriggerSetup();
        }
        
        private void DrawEventChannelSetupStep()
        {
            DrawEventChannelSetup();
        }
        
        private void DrawConditionSetupStep()
        {
            DrawConditionSetup();
        }
        
        private void DrawResponseObjectSetupStep()
        {
            DrawResponseObjectSetup();
        }
        
        private void DrawReviewStep()
        {
            DrawReview();
        }
        
        private void DrawCompletionStep()
        {
            DrawComplete();
        }
        private void DrawCustomStepIndicator()
        {
            EditorGUILayout.Space(5);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            var steps = new[] { "Project Setup", "Template", "Trigger", "Events", "Conditions", "Responses", "Review", "Complete" };
            var stepColors = GetStepColors();
            
            for (int i = 0; i < steps.Length; i++)
            {
                bool isActive = currentStepIndex == i;
                bool isCompleted = currentStepIndex > i;
                bool canNavigateTo = CanProceedToStep(i);
                
                // Get step-specific color based on CLGF themes
                Color stepColor = stepColors[i];
                
                // Scale up current step button
                float buttonWidth = isActive ? 140 : 90;
                float buttonHeight = isActive ? 35 : 25;
                
                // Draw colored background rect directly for maximum vibrancy
                Rect buttonRect = GUILayoutUtility.GetRect(buttonWidth, buttonHeight);
                
                // Check if mouse is hovering over this button
                bool isHovering = buttonRect.Contains(Event.current.mousePosition) && canNavigateTo;
                
                // Get ultra-vibrant color for this step with hover state
                Color buttonColor = GetUltraVibrantStepColor((WizardStep)i, isActive, isCompleted, canNavigateTo, isHovering);
                EditorGUI.DrawRect(buttonRect, buttonColor);
                
                // Create transparent button style for text/click handling
                GUIStyle stepStyle = new GUIStyle(EditorStyles.label)
                {
                    fixedWidth = buttonWidth,
                    fixedHeight = buttonHeight,
                    normal = { textColor = Color.white },
                    fontStyle = isActive ? FontStyle.Bold : FontStyle.Normal,
                    fontSize = isActive ? 16 : 12,
                    alignment = TextAnchor.MiddleCenter
                };
                
                // Draw text label over the colored rect
                GUI.Label(buttonRect, steps[i], stepStyle);
                
                // Handle click detection
                if (Event.current.type == EventType.MouseDown && buttonRect.Contains(Event.current.mousePosition) && canNavigateTo)
                {
                    NavigateToStep(i);
                    Event.current.Use();
                }
                
                // Repaint on hover state changes for smooth visual feedback
                if (isHovering)
                {
                    Repaint();
                }
                
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
        
        private Color GetUltraVibrantStepColor(WizardStep step, bool isActive, bool isCompleted, bool canNavigateTo, bool isHovering = false)
        {
            // Get the exact CLGF theme color for this step
            var theme = GetStepThemeForEnum(step);
            var themeColors = GetCLGFThemeColors(theme);
            
            // Use the exact border color from CLGF theme but force full alpha for maximum vibrancy
            Color baseColor = new Color(
                themeColors.borderColor.r,
                themeColors.borderColor.g, 
                themeColors.borderColor.b,
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
        
        private Color[] GetStepColors()
        {
            // Map each step to vibrant CLGF theme colors using centralized color system
            var stepThemes = new CLGFBaseEditor.CLGFTheme[]
            {
                CLGFBaseEditor.CLGFTheme.UI,            // Project Setup
                CLGFBaseEditor.CLGFTheme.System,        // Template
                CLGFBaseEditor.CLGFTheme.Action,        // Trigger  
                CLGFBaseEditor.CLGFTheme.Event,         // Events
                CLGFBaseEditor.CLGFTheme.Action,        // Conditions
                CLGFBaseEditor.CLGFTheme.ObjectControl, // Responses
                CLGFBaseEditor.CLGFTheme.Character,     // Review (keep purple for visual distinction)
                CLGFBaseEditor.CLGFTheme.System         // Complete
            };
            
            var colors = new Color[stepThemes.Length];
            for (int i = 0; i < stepThemes.Length; i++)
            {
                var themeColors = GetCLGFThemeColors(stepThemes[i]);
                // Create vibrant version by increasing saturation and opacity
                var baseColor = themeColors.borderColor; // Use border color as base for vibrancy
                colors[i] = new Color(
                    Mathf.Min(1.0f, baseColor.r * 1.2f), 
                    Mathf.Min(1.0f, baseColor.g * 1.2f), 
                    Mathf.Min(1.0f, baseColor.b * 1.2f), 
                    0.9f); // High opacity for navigation buttons
            }
            
            return colors;
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
            return CanProceedToStep((int)step);
        }
        
        // Removed HasCompletedStep - now using BaseSetupWizard's CanProceedToStep
        
        private void NavigateToStep(WizardStep step)
        {
            if (CanNavigateToStep(step))
            {
                NavigateToStep((int)step);
                Repaint();
            }
        }
        
        private void DrawNavigationButtons()
        {
            GUILayout.BeginHorizontal();
            
            // Back button
            GUI.enabled = currentStepIndex > 1; // Template Selection is index 1
            if (GUILayout.Button("← Back", GUILayout.Height(30)))
            {
                PreviousStep();
            }
            GUI.enabled = true;
            
            GUILayout.FlexibleSpace();
            
            // Next/Finish button
            string buttonText = currentStepIndex == 6 ? "Apply Setup" : 
                               currentStepIndex == 7 ? "Close" : "Next →";
            
            bool canProceed = CanProceedToNextStep();
            GUI.enabled = canProceed;
            
            if (GUILayout.Button(buttonText, GUILayout.Height(30), GUILayout.Width(120)))
            {
                if (currentStepIndex == 6) // Review step
                {
                    ApplySetup();
                }
                else if (currentStepIndex == 7) // Complete step
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
        
        private void DrawProjectSetup()
        {
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16
            };
            EditorGUILayout.LabelField("🎯 Project Setup", titleStyle);
            
            EditorGUILayout.Space(10); // Extra buffer
            
            DrawThemedHelpBox("Configure your project settings to organize assets and choose how objects are created.");
            
            // Creation Mode Selection
            EditorGUILayout.LabelField("Creation Mode", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Object Type:", GUILayout.Width(80));

            if (GUILayout.Toggle(creationMode == CreationMode.SceneObjects, "🌍 Scene Objects", EditorStyles.miniButtonLeft, GUILayout.Width(120)))
            {
                creationMode = CreationMode.SceneObjects;
            }

            if (GUILayout.Toggle(creationMode == CreationMode.Prefabs, "📦 Prefabs", EditorStyles.miniButtonRight, GUILayout.Width(100)))
            {
                creationMode = CreationMode.Prefabs;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(3);
            
            // Mode description
            if (creationMode == CreationMode.SceneObjects)
            {
                EditorGUILayout.LabelField("🌍 Creating objects directly in the scene", EditorStyles.miniLabel);
                EditorGUILayout.HelpBox("Response objects will be created as GameObjects in the current scene.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.LabelField("📦 Creating prefab assets in project", EditorStyles.miniLabel);
                EditorGUILayout.HelpBox("Response objects will be created as prefab assets in the project folder structure.", MessageType.Info);
            }
            
            EditorGUILayout.Space(15);
            
            // Project Organization
            EditorGUILayout.LabelField("Project Organization", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);
            
            // Project folder mode selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Project Mode:", GUILayout.Width(80));

            if (GUILayout.Toggle(!useExistingProjectFolder, "✨ Create New", EditorStyles.miniButtonLeft, GUILayout.Width(100)))
            {
                if (useExistingProjectFolder)
                {
                    useExistingProjectFolder = false;
                    selectedProjectFolderPath = "";
                }
            }

            if (GUILayout.Toggle(useExistingProjectFolder, "📂 Use Existing", EditorStyles.miniButtonRight, GUILayout.Width(100)))
            {
                if (!useExistingProjectFolder)
                {
                    useExistingProjectFolder = true;
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(3);
            
            // Project-specific UI
            if (!useExistingProjectFolder)
            {
                EditorGUILayout.LabelField("✨ Creating New Project", EditorStyles.miniLabel);
                
                projectName = EditorGUILayout.TextField(
                    new GUIContent("Project Name", "Name for the new project folder (will contain Events, Triggers, etc.)"),
                    projectName);
                    
                if (string.IsNullOrEmpty(projectName.Trim()))
                {
                    EditorGUILayout.HelpBox("Please enter a project name to continue.", MessageType.Warning);
                }
                else
                {
                    var cleanProjectName = projectName.Trim();
                    EditorGUILayout.HelpBox($"Assets will be organized as: Events/{cleanProjectName}/[EventName], etc.", MessageType.Info);
                }
            }
            else
            {
                EditorGUILayout.LabelField("📂 Use Existing Project", EditorStyles.miniLabel);
                
                if (existingProjectFolders.Length > 0)
                {
                    int selectedIndex = System.Array.IndexOf(existingProjectFolders, selectedProjectFolderPath);
                    if (selectedIndex < 0) selectedIndex = 0;
                    
                    selectedIndex = EditorGUILayout.Popup(
                        new GUIContent("Existing Project", "Select an existing project folder"),
                        selectedIndex, existingProjectFolders);
                        
                    if (selectedIndex >= 0 && selectedIndex < existingProjectFolders.Length)
                    {
                        selectedProjectFolderPath = existingProjectFolders[selectedIndex];
                    }
                    
                    if (!string.IsNullOrEmpty(selectedProjectFolderPath))
                    {
                        EditorGUILayout.HelpBox($"Using existing project: {selectedProjectFolderPath}", MessageType.Info);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No existing project folders found. Switch to 'Create New' to create your first project.", MessageType.Warning);
                }
            }
        }
        
        private void DrawTemplateSelection()
        {
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16
            };
            EditorGUILayout.LabelField("📋 Template Selection", titleStyle);

            EditorGUILayout.Space(10); // Extra buffer

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
                GUILayout.Label("📋", new GUIStyle(EditorStyles.label) { fontSize = 24 }, GUILayout.Width(32), GUILayout.Height(32));
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
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16
            };
            EditorGUILayout.LabelField("⚡ Trigger Configuration", titleStyle);
            
            EditorGUILayout.Space(5);
            DrawFoldoutControls();
            EditorGUILayout.Space(5);

            // Basic trigger setup foldout
            DrawFoldoutSection("trigger_basic", "Basic Trigger Setup", "⚡", CLGFBaseEditor.CLGFTheme.Action, () =>
            {
                // Trigger mode selection - matching events/responses visual pattern
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Trigger Mode:", GUILayout.Width(80));

                if (GUILayout.Toggle(createNewTriggerObject, "✨ Create New", EditorStyles.miniButtonLeft, GUILayout.Width(100)))
                {
                    if (!createNewTriggerObject)
                    {
                        createNewTriggerObject = true;
                        triggerObject = null;
                    }
                }

                if (GUILayout.Toggle(!createNewTriggerObject, "📂 Use Existing", EditorStyles.miniButtonRight, GUILayout.Width(100)))
                {
                    if (createNewTriggerObject)
                    {
                        createNewTriggerObject = false;
                    }
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(3);

                // Mode-specific UI with mini labels
                if (createNewTriggerObject)
                {
                    EditorGUILayout.LabelField("✨ Creating New GameObject", EditorStyles.miniLabel);
                    
                    newTriggerObjectName = EditorGUILayout.TextField(
                        new GUIContent("GameObject Name", "Name for the new trigger GameObject"),
                        newTriggerObjectName);
                        
                    if (string.IsNullOrEmpty(newTriggerObjectName.Trim()))
                    {
                        EditorGUILayout.HelpBox("Please enter a name for the new GameObject.", MessageType.Warning);
                        return;
                    }
                    
                    // Show a helpful note about what will be created
                    EditorGUILayout.HelpBox($"A new GameObject named '{newTriggerObjectName}' will be created in the scene and configured as your trigger.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.LabelField("📂 Use Existing GameObject", EditorStyles.miniLabel);
                    
                    triggerObject = (GameObject)EditorGUILayout.ObjectField(
                        new GUIContent("Trigger GameObject", "The GameObject that will start the interaction (receives trigger components)"),
                        triggerObject, typeof(GameObject), true);
                        
                    if (triggerObject == null)
                    {
                        EditorGUILayout.HelpBox("Please select a trigger GameObject to continue.", MessageType.Warning);
                        return;
                    }
                }
                
                EditorGUILayout.Space(10);
                
                // Trigger type selection
                triggerConfig.triggerType = (TriggerType)EditorGUILayout.EnumPopup("Trigger Type", triggerConfig.triggerType);
            }, true, false, 0, "Configure the basic trigger GameObject and type");

            // Type-specific settings foldout
            string triggerTypeIcon = triggerConfig.triggerType switch
            {
                TriggerType.Collision => "💥",
                TriggerType.Proximity => "📡",
                TriggerType.Timer => "⏰",
                _ => "⚙️"
            };
            
            DrawFoldoutSection("trigger_settings", $"{triggerConfig.triggerType} Settings", triggerTypeIcon, CLGFBaseEditor.CLGFTheme.Action, () =>
            {
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
            }, true, false, 0, $"Configure {triggerConfig.triggerType.ToString().ToLower()} trigger parameters");

            // General settings foldout
            DrawFoldoutSection("trigger_general", "General Settings", "⚙️", CLGFBaseEditor.CLGFTheme.System, () =>
            {
                canRepeat = EditorGUILayout.Toggle("Can Repeat", canRepeat);
                if (canRepeat)
                {
                    cooldownTime = EditorGUILayout.FloatField("Cooldown Time", cooldownTime);
                }
                debugMode = EditorGUILayout.Toggle("Debug Mode", debugMode);
            }, false, false, 0, "Configure repetition and debug settings");
        }
        
        private void DrawEventChannelSetup()
        {
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16
            };
            EditorGUILayout.LabelField("📡 Event Channel Configuration", titleStyle);
            
            EditorGUILayout.Space(5);
            DrawFoldoutControls();
            EditorGUILayout.Space(5);

            DrawThemedHelpBox("Event channels connect triggers to responses. When a trigger fires, it raises events that listeners respond to.");
            
            // Event channels overview foldout with header buttons
            var eventChannelHeaderButtons = new FoldoutUtility.FoldoutButton[]
            {
                new FoldoutUtility.FoldoutButton("➕ Add", () => {
                    eventChannelConfigs.Add(new EventChannelConfig());
                }, "Add a new event channel", null, null, 80f)
            };
            
            DrawFoldoutSection("event_channels", "Event Channels", "📡", CLGFBaseEditor.CLGFTheme.Event, () =>
            {
                // List existing event channels
                for (int i = 0; i < eventChannelConfigs.Count; i++)
                {
                    var eventConfig = eventChannelConfigs[i];
                    string eventDisplayName = GetEventChannelDisplayName(eventConfig);
                    string channelTitle = string.IsNullOrEmpty(eventDisplayName) ? 
                        $"Event Channel {i + 1}" : 
                        $"{eventDisplayName}";
                    
                    // Individual event channel foldout with header buttons
                    var individualChannelButtons = new FoldoutUtility.FoldoutButton[]
                    {
                        new FoldoutUtility.FoldoutButton("✕", () => {
                            // Store action to execute after the loop to avoid modifying collection during iteration
                            pendingActions.Add(() => {
                                eventChannelConfigs.RemoveAt(i);
                            });
                        }, "Remove this event channel", new Color(0.8f, 0.3f, 0.3f, 0.8f), Color.white, 30f)
                    };
                    
                    DrawFoldoutSection($"event_channel_{i}", channelTitle, "🔵", CLGFBaseEditor.CLGFTheme.Event, () =>
                    {
                        EditorGUILayout.Space(5);
                        
                        // Event selection mode
                        DrawEventSelectionMode(eventChannelConfigs[i]);
                    }, true, false, 0, $"Configure event channel settings", true, individualChannelButtons);
                }
            }, true, true, eventChannelConfigs.Count, "Manage event channels that connect triggers to responses", true, eventChannelHeaderButtons);
            
            // Execute pending actions after the loop to avoid modifying collection during iteration
            foreach (var action in pendingActions)
            {
                action.Invoke();
            }
            pendingActions.Clear();
            
            if (eventChannelConfigs.Count == 0)
            {
                DrawThemedHelpBox("Add at least one event channel to connect triggers to responses.");
            }
        }
        
        private void DrawEventSelectionMode(EventChannelConfig eventConfig)
        {
            // Simple mode selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Event Mode:", GUILayout.Width(80));
            
            if (GUILayout.Toggle(eventConfig.createNewEvent, "✨ Create New", EditorStyles.miniButtonLeft, GUILayout.Width(100)))
            {
                if (!eventConfig.createNewEvent)
                {
                    eventConfig.createNewEvent = true;
                    eventConfig.gameEventAsset = null;
                }
            }
            
            if (GUILayout.Toggle(!eventConfig.createNewEvent, "📂 Use Existing", EditorStyles.miniButtonRight, GUILayout.Width(100)))
            {
                if (eventConfig.createNewEvent)
                {
                    eventConfig.createNewEvent = false;
                }
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(3);
            
            // Mode-specific UI
            if (eventConfig.createNewEvent)
            {
                DrawCreateNewEventMode(eventConfig);
            }
            else
            {
                DrawUseExistingEventMode(eventConfig);
            }
        }
        
        private void DrawCreateNewEventMode(EventChannelConfig eventConfig)
        {
            EditorGUILayout.LabelField("✨ Creating New Event", EditorStyles.miniLabel);
            eventConfig.eventName = EditorGUILayout.TextField("Event Name", eventConfig.eventName);
            eventConfig.description = EditorGUILayout.TextField("Description", eventConfig.description);
            
            if (!string.IsNullOrEmpty(eventConfig.eventName))
            {
                string previewPath = GetEventAssetPath(eventConfig.eventName);
                EditorGUILayout.LabelField("Will create at:", previewPath, EditorStyles.miniLabel);
            }
        }
        
        private void DrawUseExistingEventMode(EventChannelConfig eventConfig)
        {
            EditorGUILayout.LabelField("📂 Use Existing Event", EditorStyles.miniLabel);
            
            // Unity's ObjectField for GameEvent selection
            GameEvent newGameEvent = (GameEvent)EditorGUILayout.ObjectField(
                "GameEvent Asset", 
                eventConfig.gameEventAsset, 
                typeof(GameEvent), 
                false);
            
            if (newGameEvent != eventConfig.gameEventAsset)
            {
                eventConfig.gameEventAsset = newGameEvent;
                
                // Auto-populate fields from selected asset
                if (newGameEvent != null)
                {
                    eventConfig.eventName = newGameEvent.ChannelName;
                    eventConfig.description = newGameEvent.Description;
                    eventConfig.existingEventPath = AssetDatabase.GetAssetPath(newGameEvent);
                }
                else
                {
                    eventConfig.eventName = "";
                    eventConfig.description = "";
                    eventConfig.existingEventPath = "";
                }
            }
            
            // Show event info if selected
            if (eventConfig.gameEventAsset != null)
            {
                EditorGUILayout.Space(3);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("Selected Event Info:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Name:", eventConfig.gameEventAsset.ChannelName);
                if (!string.IsNullOrEmpty(eventConfig.gameEventAsset.Description))
                {
                    EditorGUILayout.LabelField("Description:", eventConfig.gameEventAsset.Description, EditorStyles.wordWrappedLabel);
                }
                EditorGUILayout.LabelField("Path:", AssetDatabase.GetAssetPath(eventConfig.gameEventAsset), EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
            }
        }
        
        private void DrawConditionSetup()
        {
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16
            };
            EditorGUILayout.LabelField("🔍 Condition Setup (Optional)", titleStyle);
            
            EditorGUILayout.Space(5);
            DrawFoldoutControls();
            EditorGUILayout.Space(5);
            
            DrawThemedHelpBox("Conditions allow you to add additional checks before the trigger fires. Leave empty to always trigger.");
            
            // Conditions overview foldout with header buttons
            var conditionHeaderButtons = new FoldoutUtility.FoldoutButton[]
            {
                new FoldoutUtility.FoldoutButton("➕ Add", () => {
                    conditionConfigs.Add(new ConditionConfig());
                }, "Add a new condition", null, null, 80f)
            };
            
            DrawFoldoutSection("conditions", "Conditions", "🔍", CLGFBaseEditor.CLGFTheme.System, () =>
            {
                if (conditionConfigs.Count > 0)
                {
                    requireAllConditions = EditorGUILayout.Toggle("🔗 Require All Conditions", requireAllConditions);
                    EditorGUILayout.Space(8);
                }
                
                // List existing conditions
                for (int i = 0; i < conditionConfigs.Count; i++)
                {
                    // Individual condition foldout with header buttons
                    var individualConditionButtons = new FoldoutUtility.FoldoutButton[]
                    {
                        new FoldoutUtility.FoldoutButton("✕", () => {
                            // Store action to execute after the loop to avoid modifying collection during iteration
                            pendingActions.Add(() => {
                                conditionConfigs.RemoveAt(i);
                            });
                        }, "Remove this condition", new Color(0.8f, 0.3f, 0.3f, 0.8f), Color.white, 30f)
                    };
                    
                    DrawFoldoutSection($"condition_{i}", $"Condition {i + 1} ({conditionConfigs[i].conditionType})", "🔍", CLGFBaseEditor.CLGFTheme.System, () =>
                    {
                        EditorGUILayout.Space(5);
                        
                        conditionConfigs[i].conditionType = (ConditionType)EditorGUILayout.EnumPopup("Condition Type", conditionConfigs[i].conditionType);
                        conditionConfigs[i].invertResult = EditorGUILayout.Toggle("Invert Result", conditionConfigs[i].invertResult);
                        
                        // Add condition-specific settings here in the future
                        EditorGUILayout.HelpBox("Additional condition-specific settings will be added here.", MessageType.Info);
                        
                    }, false, false, 0, $"Configure {conditionConfigs[i].conditionType.ToString().ToLower()} condition", true, individualConditionButtons);
                }
            }, false, true, conditionConfigs.Count, "Optional conditions that must be met for the trigger to fire", true, conditionHeaderButtons);
            
            // Execute pending actions after the loop to avoid modifying collection during iteration
            foreach (var action in pendingActions)
            {
                action.Invoke();
            }
            pendingActions.Clear();
        }
        
        private void DrawResponseObjectSetup()
        {
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16
            };
            EditorGUILayout.LabelField("🎬 Response Object Configuration", titleStyle);
            
            EditorGUILayout.Space(5);
            DrawFoldoutControls();
            EditorGUILayout.Space(5);

            DrawThemedHelpBox("Response objects listen to events and perform actions. Create multiple objects for complex interactions.");
            
            // Response objects overview foldout
            DrawFoldoutSection("response_objects", "Response Objects", "🎬", CLGFBaseEditor.CLGFTheme.ObjectControl, () =>
            {
                // List existing response objects
                for (int i = 0; i < responseObjectConfigs.Count; i++)
                {
                    var responseConfig = responseObjectConfigs[i];
                    string objectDisplayName = GetResponseObjectDisplayName(responseConfig);
                    
                    // Choose icon based on object type and first action
                    string icon;
                    if (responseConfig.isParentObject)
                    {
                        icon = "🏗️"; // Parent container icon
                    }
                    else if (responseConfig.actions != null && responseConfig.actions.Count > 0)
                    {
                        // Use first action's icon
                        icon = GetActionIcon(responseConfig.actions[0].ActionId);
                    }
                    else
                    {
                        // Default icon for objects without actions
                        icon = responseConfig.isChildObject ? "🔧" : "🎬";
                    }
                                 
                    string objectTitle = string.IsNullOrEmpty(objectDisplayName) ? 
                        $"Response Object {i + 1}" : 
                        $"{objectDisplayName}";
                    
                    // Add indentation for child objects
                    bool isChildLayout = responseConfig.isChildObject;
                    if (isChildLayout)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(20); // Indent child objects
                        EditorGUILayout.BeginVertical();
                    }
                    
                    try
                    {
                        // Individual response object foldout with child container styling
                        if (responseConfig.isChildObject)
                        {
                            DrawChildObjectContainer(() => {
                                DrawResponseObjectFoldouts(responseConfig, i, icon, objectTitle);
                            });
                        }
                        else
                        {
                            DrawResponseObjectFoldouts(responseConfig, i, icon, objectTitle);
                        }
                    }
                    finally
                    {
                        // Always close indentation for child objects to prevent layout errors
                        if (isChildLayout)
                        {
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    
                    EditorGUILayout.Space(8); // Buffer between response objects
                }
                
                EditorGUILayout.Space(10);
                
                // Add response object buttons
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("➕ Add Response Object", GUILayout.Height(25)))
                {
                    responseObjectConfigs.Add(new ResponseObjectConfig());
                }
                if (GUILayout.Button("🏗️ Add Parent Container", GUILayout.Height(25)))
                {
                    var parentConfig = new ResponseObjectConfig();
                    parentConfig.isParentObject = true;
                    parentConfig.objectName = "Parent Container";
                    responseObjectConfigs.Add(parentConfig);
                }
                
                // Find last parent container to add child to
                var lastParent = responseObjectConfigs.LastOrDefault(r => r.isParentObject);
                GUI.enabled = lastParent != null; // Only enable if there's a parent available
                if (GUILayout.Button("👶 Add Child to Parent", GUILayout.Height(25)))
                {
                    var childConfig = new ResponseObjectConfig();
                    childConfig.isChildObject = true;
                    childConfig.parentObjectName = lastParent.objectName;
                    childConfig.objectName = $"{lastParent.objectName}_Child";
                    responseObjectConfigs.Add(childConfig);
                }
                GUI.enabled = true; // Re-enable GUI
                
                EditorGUILayout.EndHorizontal();
            }, true, true, responseObjectConfigs.Count, "Manage response objects that react to events", false); // No background to reduce visual clutter
            
            // Execute pending actions after the loop to avoid modifying collection during iteration
            foreach (var action in pendingActions)
            {
                action.Invoke();
            }
            pendingActions.Clear();
        }
        
        private void DrawResponseObjectFoldouts(ResponseObjectConfig responseConfig, int index, string icon, string objectTitle)
        {
            // Main response object foldout (no background to reduce visual clutter)
            // Use red for parent containers, custom darker green for individual response objects
            if (responseConfig.isParentObject)
            {
                var headerTheme = CLGFBaseEditor.CLGFTheme.System;
                
                // Create header buttons for parent objects
                var headerButtons = new FoldoutUtility.FoldoutButton[]
                {
                    new FoldoutUtility.FoldoutButton("👶 Add", () => {
                        // Store action to execute after the loop to avoid modifying collection during iteration
                        pendingActions.Add(() => {
                            var childConfig = new ResponseObjectConfig();
                            childConfig.isChildObject = true;
                            childConfig.parentObjectName = responseConfig.objectName;
                            childConfig.objectName = $"{responseConfig.objectName}_Child";
                            responseObjectConfigs.Add(childConfig);
                        });
                    }, "Add a child object to this parent", null, null, 80f),
                    
                    new FoldoutUtility.FoldoutButton("✕", () => {
                        // Store action to execute after the loop to avoid modifying collection during iteration
                        pendingActions.Add(() => {
                            responseObjectConfigs.RemoveAt(index);
                        });
                    }, "Remove this response object", new Color(0.8f, 0.3f, 0.3f, 0.8f), Color.white, 30f)
                };
                
                DrawFoldoutSection($"response_object_{index}", objectTitle, icon, headerTheme, () =>
                {
                    DrawResponseObjectContent(responseConfig, index);
                }, true, false, 0, $"Configure response object settings", false, headerButtons); // No background to reduce visual clutter
            }
            else
            {
                // Custom darker green foldout for individual response objects with header buttons
                var headerButtons = new FoldoutUtility.FoldoutButton[]
                {
                    new FoldoutUtility.FoldoutButton("✕", () => {
                        // Store action to execute after the loop to avoid modifying collection during iteration
                        pendingActions.Add(() => {
                            responseObjectConfigs.RemoveAt(index);
                        });
                    }, "Remove this response object", new Color(0.8f, 0.3f, 0.3f, 0.8f), Color.white, 30f)
                };
                
                DrawDarkerGreenFoldoutSection($"response_object_{index}", objectTitle, icon, () =>
                {
                    DrawResponseObjectContent(responseConfig, index);
                }, headerButtons);
            }
        }
        
        private void DrawResponseObjectContent(ResponseObjectConfig responseConfig, int index)
        {
                // All objects now have their action buttons in headers for consistency
                EditorGUILayout.Space(5);
                
                // Object selection mode foldout
                DrawFoldoutSection($"response_object_{index}_selection", "Object Selection", "📂", CLGFBaseEditor.CLGFTheme.UI, () =>
                {
                    DrawResponseObjectSelectionMode(responseConfig);
                }, true, false, 0, "Choose how to create or reference the GameObject");
                
                // Hierarchy settings foldout
                int childCount = responseConfig.childObjects?.Count ?? 0;
                DrawFoldoutSection($"response_object_{index}_hierarchy", "Hierarchy Settings", "🏗️", CLGFBaseEditor.CLGFTheme.System, () =>
                {
                    DrawHierarchySettings(responseConfig);
                }, false, true, childCount, "Configure parent-child relationships");
                
                // Event subscriptions foldout
                int eventCount = responseConfig.listenToEvents?.Count ?? 0;
                DrawFoldoutSection($"response_object_{index}_events", "Event Subscriptions", "🔵", CLGFBaseEditor.CLGFTheme.Event, () =>
                {
                    DrawEventSubscriptions(responseConfig);
                }, true, true, eventCount, "Configure which events this object listens to");
                
                // Actions foldout
                int actionCount = responseConfig.actions?.Count ?? 0;
                DrawFoldoutSection($"response_object_{index}_actions", "Actions", "🎬", CLGFBaseEditor.CLGFTheme.ObjectControl, () =>
                {
                    DrawResponseActions(responseConfig);
                }, true, true, actionCount, "Configure actions that execute when events are received");
        }
        
        private void DrawDarkerGreenFoldoutSection(string id, string title, string icon, System.Action drawContent, FoldoutUtility.FoldoutButton[] headerButtons = null)
        {
            // Create darker green colors for individual response objects
            Color darkBackgroundColor = new Color(0.15f, 0.5f, 0.2f, 0.15f); // Darker green background
            Color darkBorderColor = new Color(0.27f, 0.69f, 0.27f, 0.82f); // Much darker green border
            
            // Use the foldout utility with custom darker green theme
            // Since we can't pass custom colors directly, we'll draw this manually with the same style
            string foldoutKey = $"{GetType().Name}_{id}";
            
            // Get or set initial state - use the same logic as FoldoutUtility
            if (!EditorPrefs.HasKey($"{GetType().Name}_{id}"))
            {
                EditorPrefs.SetBool($"{GetType().Name}_{id}", true); // Default to expanded
            }
            bool isExpanded = EditorPrefs.GetBool($"{GetType().Name}_{id}", true);
            
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
                isExpanded = !isExpanded;
                EditorPrefs.SetBool($"{GetType().Name}_{id}", isExpanded);
                Event.current.Use();
                if (EditorWindow.focusedWindow != null) EditorWindow.focusedWindow.Repaint();
            }
            
            // Draw background with darker green and hover effect
            Color headerColor = darkBorderColor;
            if (headerRect.Contains(Event.current.mousePosition))
            {
                headerColor = Color.Lerp(darkBorderColor, Color.white, 0.1f);
            }
            
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(headerRect, headerColor);
            }
            
            // Create styles
            GUIStyle arrowStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(8, 0, 0, 0)
            };
            
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(55, 0, 0, 0) // Increased padding for icon + arrow space
            };
            
            GUIStyle iconStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16, // Larger icon size to match regular foldouts
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(0, 0, 0, 0)
            };
            
            // Draw arrow, icon and title (adjusted for buttons)
            string arrow = isExpanded ? "▼" : "▶";
            GUI.Label(new Rect(headerRect.x, headerRect.y, 25, headerRect.height), arrow, arrowStyle);
            GUI.Label(new Rect(headerRect.x + 25, headerRect.y, 25, headerRect.height), icon, iconStyle); // Better positioning and size
            
            // Draw title (adjusted for buttons)
            float titleWidth = headerRect.width - 55 - totalButtonWidth - 10f;
            Rect titleRect = new Rect(headerRect.x + 55, headerRect.y, titleWidth, headerRect.height);
            GUI.Label(titleRect, title, titleStyle);
            
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
                        Color bgColor = button.BackgroundColor ?? GUI.skin.button.normal.background.name == "builtin skins/darkskin images/btn" 
                            ? new Color(0.4f, 0.4f, 0.4f, 1f) : new Color(0.8f, 0.8f, 0.8f, 1f);
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
            
            // Draw content if expanded (no background for individual response objects)
            if (isExpanded && drawContent != null)
            {
                EditorGUILayout.Space(2);
                
                // Simple content area without background
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space(8);
                
                // Add horizontal padding for content to prevent border clipping
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(12); // Left padding
                
                EditorGUILayout.BeginVertical();
                // Draw the content
                drawContent.Invoke();
                EditorGUILayout.EndVertical();
                
                GUILayout.Space(12); // Right padding
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(8);
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Space(5);
        }
        
        /// <summary>
        /// Creates a solid color texture for button styling.
        /// </summary>
        private Texture2D CreateSolidColorTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
        
        private void DrawChildObjectContainer(System.Action drawContent)
        {
            // Get darker green theme colors for child objects
            var (backgroundColor, borderColor) = GetCLGFThemeColors(CLGFBaseEditor.CLGFTheme.ObjectControl);
            backgroundColor.a = 0.00f;
            borderColor = Color.Lerp(borderColor, Color.black, 0.3f); // Darker border
            
            Rect sectionStart = GUILayoutUtility.GetRect(0, 0);
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.Space(8);
            
            // Draw content
            drawContent?.Invoke();
            
            EditorGUILayout.Space(8);
            EditorGUILayout.EndVertical();
            
            // Draw enhanced background for child objects
            if (Event.current.type == EventType.Repaint)
            {
                Rect sectionEnd = GUILayoutUtility.GetLastRect();
                Rect backgroundRect = new Rect(
                    sectionStart.x + 5, // Account for margin
                    sectionStart.y,
                    sectionEnd.width - 10, // Account for left/right margins
                    sectionEnd.height + (sectionEnd.y - sectionStart.y)
                );
                
                // Draw darker background
                EditorGUI.DrawRect(backgroundRect, backgroundColor);
                
                // Draw darker border with proper bounds
                float borderWidth = 2f;
                // Top border
                EditorGUI.DrawRect(new Rect(backgroundRect.x, backgroundRect.y, backgroundRect.width, borderWidth), borderColor);
                // Bottom border
                EditorGUI.DrawRect(new Rect(backgroundRect.x, backgroundRect.y + backgroundRect.height - borderWidth, backgroundRect.width, borderWidth), borderColor);
                // Left border
                EditorGUI.DrawRect(new Rect(backgroundRect.x, backgroundRect.y, borderWidth, backgroundRect.height), borderColor);
                // Right border - ensure it's within bounds
                EditorGUI.DrawRect(new Rect(backgroundRect.x + backgroundRect.width - borderWidth, backgroundRect.y, borderWidth, backgroundRect.height), borderColor);
            }
        }
        
        private void CompleteResponseObjectSetup()
        {
            // Add response object button
            if (GUILayout.Button("➕ Add Response Object", GUILayout.Height(25)))
            {
                responseObjectConfigs.Add(new ResponseObjectConfig());
            }
            
            if (responseObjectConfigs.Count == 0)
            {
                DrawThemedHelpBox("Add at least one response object to complete the interaction.");
            }
        }
        
        private void DrawResponseObjectSelectionMode(ResponseObjectConfig responseConfig)
        {
            // Simple mode selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Object Mode:", GUILayout.Width(80));
            
            if (GUILayout.Toggle(responseConfig.createNewObject, "✨ Create New", EditorStyles.miniButtonLeft, GUILayout.Width(100)))
            {
                if (!responseConfig.createNewObject)
                {
                    responseConfig.createNewObject = true;
                    responseConfig.targetGameObject = null;
                }
            }
            
            if (GUILayout.Toggle(!responseConfig.createNewObject, "📂 Use Existing", EditorStyles.miniButtonRight, GUILayout.Width(100)))
            {
                if (responseConfig.createNewObject)
                {
                    responseConfig.createNewObject = false;
                }
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(3);
            
            // Mode-specific UI
            if (responseConfig.createNewObject)
            {
                DrawCreateNewObjectMode(responseConfig);
            }
            else
            {
                DrawUseExistingObjectMode(responseConfig);
            }
        }
        
        private void DrawCreateNewObjectMode(ResponseObjectConfig responseConfig)
        {
            EditorGUILayout.LabelField("✨ Creating New GameObject", EditorStyles.miniLabel);
            responseConfig.objectName = EditorGUILayout.TextField("Object Name", responseConfig.objectName);
            responseConfig.description = EditorGUILayout.TextField("Description", responseConfig.description);
            
            if (!string.IsNullOrEmpty(responseConfig.objectName))
            {
                EditorGUILayout.LabelField("Will create new GameObject in scene", EditorStyles.miniLabel);
            }
        }
        
        private void DrawUseExistingObjectMode(ResponseObjectConfig responseConfig)
        {
            EditorGUILayout.LabelField("📂 Use Existing GameObject", EditorStyles.miniLabel);
            
            // Unity's ObjectField for GameObject selection
            GameObject newTargetObject = (GameObject)EditorGUILayout.ObjectField(
                "Target GameObject", 
                responseConfig.targetGameObject, 
                typeof(GameObject), 
                true); // Allow scene objects
            
            if (newTargetObject != responseConfig.targetGameObject)
            {
                responseConfig.targetGameObject = newTargetObject;
                
                // Auto-populate fields from selected object
                if (newTargetObject != null)
                {
                    responseConfig.objectName = newTargetObject.name;
                    responseConfig.targetObjectId = newTargetObject.name; // Legacy compatibility
                }
                else
                {
                    responseConfig.objectName = "";
                    responseConfig.targetObjectId = "";
                }
            }
            
            // Show object info if selected
            if (responseConfig.targetGameObject != null)
            {
                EditorGUILayout.Space(3);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("Selected GameObject Info:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Name:", responseConfig.targetGameObject.name);
                EditorGUILayout.LabelField("Scene:", responseConfig.targetGameObject.scene.name);
                
                // Show existing components
                var components = responseConfig.targetGameObject.GetComponents<Component>();
                if (components.Length > 1) // Ignore Transform
                {
                    EditorGUILayout.LabelField("Components:", EditorStyles.miniLabel);
                    foreach (var comp in components)
                    {
                        if (comp != null && !(comp is Transform))
                        {
                            EditorGUILayout.LabelField($"  • {comp.GetType().Name}", EditorStyles.miniLabel);
                        }
                    }
                }
                
                EditorGUILayout.EndVertical();
            }
        }
        
        private void DrawEventSubscriptions(ResponseObjectConfig responseConfig)
        {
            GUIStyle subsectionStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14
            };
            EditorGUILayout.LabelField("📡 Events to Listen For", subsectionStyle);
            
            // Event subscriptions - support both string events and GameEvent assets
            DrawEventSubscriptionsList(responseConfig);
            
            if (GUILayout.Button("➕ Add Event Listener", GUILayout.Height(20)))
            {
                responseConfig.listenToEvents.Add(eventChannelConfigs.Count > 0 ? eventChannelConfigs[0].eventName : "");
                responseConfig.gameEventAssets.Add(null);
            }
        }
        
        private void DrawEventSubscriptionsList(ResponseObjectConfig responseConfig)
        {
            // Ensure lists are synchronized
            while (responseConfig.gameEventAssets.Count < responseConfig.listenToEvents.Count)
                responseConfig.gameEventAssets.Add(null);
            while (responseConfig.listenToEvents.Count < responseConfig.gameEventAssets.Count)
                responseConfig.listenToEvents.Add("");
            
            // Draw each event subscription
            for (int j = 0; j < responseConfig.listenToEvents.Count; j++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();
                
                // Title and delete button
                EditorGUILayout.LabelField($"🔵 Event Listener {j + 1}", EditorStyles.boldLabel);
                if (GUILayout.Button("✕", GUILayout.Width(25)))
                {
                    responseConfig.listenToEvents.RemoveAt(j);
                    if (j < responseConfig.gameEventAssets.Count)
                        responseConfig.gameEventAssets.RemoveAt(j);
                    j--;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    continue;
                }
                
                EditorGUILayout.EndHorizontal();
                
                // Always use wizard events since event creation is controlled in Event Channels step
                EditorGUILayout.Space(5);
                DrawWizardEventSelection(responseConfig, j);
                
                // Show current event name for clarity
                if (!string.IsNullOrEmpty(responseConfig.listenToEvents[j]))
                {
                    EditorGUILayout.Space(3);
                    EditorGUILayout.LabelField($"Listening to: '{responseConfig.listenToEvents[j]}'", EditorStyles.helpBox);
                }
                
                EditorGUILayout.Space(5);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }
        
        private void DrawWizardEventSelection(ResponseObjectConfig responseConfig, int index)
        {
            if (eventChannelConfigs.Count > 0)
            {
                EditorGUILayout.LabelField("Select from wizard events:", EditorStyles.miniLabel);
                
                var eventNames = eventChannelConfigs.ConvertAll(e => e.eventName).ToArray();
                int currentIndex = Array.IndexOf(eventNames, responseConfig.listenToEvents[index]);
                
                int newIndex = EditorGUILayout.Popup("Wizard Event:", currentIndex, eventNames);
                if (newIndex >= 0 && newIndex < eventNames.Length && newIndex != currentIndex)
                {
                    responseConfig.listenToEvents[index] = eventNames[newIndex];
                    if (index < responseConfig.gameEventAssets.Count)
                        responseConfig.gameEventAssets[index] = null; // Clear asset when using wizard event
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No wizard events configured. Switch to Manual mode to select events.", MessageType.Info);
            }
        }
        
        private void DrawManualEventSelection(ResponseObjectConfig responseConfig, int index)
        {
            EditorGUILayout.LabelField("Manual event selection:", EditorStyles.miniLabel);
            
            // GameEvent ScriptableObject selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("GameEvent Asset:", GUILayout.Width(100));
            
            var previousAsset = index < responseConfig.gameEventAssets.Count ? responseConfig.gameEventAssets[index] : null;
            var newAsset = EditorGUILayout.ObjectField(previousAsset, typeof(GameFramework.Events.Channels.GameEvent), false) as GameFramework.Events.Channels.GameEvent;
            
            if (newAsset != previousAsset)
            {
                if (index >= responseConfig.gameEventAssets.Count)
                    responseConfig.gameEventAssets.Add(newAsset);
                else
                    responseConfig.gameEventAssets[index] = newAsset;
                
                // Update string event name when asset is selected
                if (newAsset != null)
                    responseConfig.listenToEvents[index] = newAsset.ChannelName;
                else if (previousAsset != null)
                    responseConfig.listenToEvents[index] = ""; // Clear when asset removed
            }
            
            EditorGUILayout.EndHorizontal();
            
            // OR separator
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("— OR —", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space(3);
            
            // Text input fallback
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Event Name:", GUILayout.Width(100));
            string newEventName = EditorGUILayout.TextField(responseConfig.listenToEvents[index]);
            if (newEventName != responseConfig.listenToEvents[index])
            {
                responseConfig.listenToEvents[index] = newEventName;
                if (index < responseConfig.gameEventAssets.Count)
                    responseConfig.gameEventAssets[index] = null; // Clear asset when manually typing
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawResponseActions(ResponseObjectConfig responseConfig)
        {
            GUIStyle subsectionStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14
            };
            EditorGUILayout.LabelField("🎬 Actions", subsectionStyle);
            
            // Actions for this response object
            for (int j = 0; j < responseConfig.actions.Count; j++)
            {
                EditorGUILayout.BeginHorizontal();
                
                // Get action-specific icon
                string actionIcon = GetActionIcon(responseConfig.actions[j].ActionId);
                
                // Use ActionDiscoveryService to populate dropdown
                var allActions = ActionDiscoveryService.GetSortedActions();
                var actionNames = allActions.Select(a => $"{a.Icon} {a.DisplayName}").ToArray();
                var actionIds = allActions.Select(a => a.ActionId).ToArray();
                
                int currentIndex = Array.IndexOf(actionIds, responseConfig.actions[j].ActionId);
                if (currentIndex == -1) currentIndex = 0; // Default to first action if not found
                
                int newIndex = EditorGUILayout.Popup($"Action {j + 1}", currentIndex, actionNames);
                if (newIndex != currentIndex && newIndex >= 0 && newIndex < actionIds.Length)
                {
                    responseConfig.actions[j].ActionId = actionIds[newIndex];
                }
                
                responseConfig.actions[j].executionDelay = EditorGUILayout.FloatField("Delay", responseConfig.actions[j].executionDelay, GUILayout.Width(80));
                
                if (GUILayout.Button("✕", GUILayout.Width(25)))
                {
                    responseConfig.actions.RemoveAt(j);
                    j--;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("➕ Add Action", GUILayout.Height(20)))
            {
                responseConfig.actions.Add(new ActionConfig { ActionId = "audio-action" });
            }
        }
        
        private string GetActionIcon(string actionId)
        {
            var actionDef = ActionDiscoveryService.GetAction(actionId);
            return actionDef?.Icon ?? "🎯";
        }
        
        
        private string GetResponseObjectDisplayName(ResponseObjectConfig responseConfig)
        {
            // Priority order: targetGameObject.name > objectName > targetObjectId
            if (responseConfig.targetGameObject != null)
            {
                return responseConfig.targetGameObject.name;
            }
            else if (!string.IsNullOrEmpty(responseConfig.objectName))
            {
                return responseConfig.objectName;
            }
            else if (!string.IsNullOrEmpty(responseConfig.targetObjectId))
            {
                return responseConfig.targetObjectId;
            }
            
            return ""; // No name available
        }
        
        private string GetEventChannelDisplayName(EventChannelConfig eventConfig)
        {
            // Priority order: gameEventAsset.ChannelName > eventName
            if (eventConfig.gameEventAsset != null)
            {
                return eventConfig.gameEventAsset.ChannelName;
            }
            else if (!string.IsNullOrEmpty(eventConfig.eventName))
            {
                return eventConfig.eventName;
            }
            
            return ""; // No name available
        }
        
        private void DrawHierarchySettings(ResponseObjectConfig responseConfig)
        {
            GUIStyle subsectionStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14
            };
            EditorGUILayout.LabelField("🏗️ Hierarchy Settings", subsectionStyle);
            
            EditorGUILayout.BeginHorizontal();
            
            // Parent object toggle
            bool wasParent = responseConfig.isParentObject;
            responseConfig.isParentObject = EditorGUILayout.Toggle("Parent Container", responseConfig.isParentObject);
            
            // If changed from parent to non-parent, clear child objects
            if (wasParent && !responseConfig.isParentObject)
            {
                responseConfig.childObjects.Clear();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Child object selection
            if (!responseConfig.isParentObject)
            {
                EditorGUILayout.BeginHorizontal();
                responseConfig.isChildObject = EditorGUILayout.Toggle("Child Object", responseConfig.isChildObject);
                
                if (responseConfig.isChildObject)
                {
                    // Show dropdown of available parent objects
                    var parentObjects = responseObjectConfigs.Where(r => r.isParentObject && r != responseConfig).ToList();
                    if (parentObjects.Count > 0)
                    {
                        var parentNames = parentObjects.Select(p => p.objectName).ToArray();
                        int currentIndex = Array.IndexOf(parentNames, responseConfig.parentObjectName);
                        if (currentIndex < 0) currentIndex = 0;
                        
                        int newIndex = EditorGUILayout.Popup("Parent", currentIndex, parentNames);
                        if (newIndex >= 0 && newIndex < parentNames.Length)
                        {
                            responseConfig.parentObjectName = parentNames[newIndex];
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Parent", "No parent objects available");
                        responseConfig.isChildObject = false;
                    }
                }
                else
                {
                    responseConfig.parentObjectName = "";
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            // Show child objects for parent containers
            if (responseConfig.isParentObject)
            {
                EditorGUILayout.Space(5);
                
                var childObjects = responseObjectConfigs.Where(r => r.isChildObject && r.parentObjectName == responseConfig.objectName).ToList();
                
                if (childObjects.Count > 0)
                {
                    EditorGUILayout.LabelField($"Child Objects ({childObjects.Count}):", EditorStyles.miniLabel);
                    EditorGUI.indentLevel++;
                    foreach (var child in childObjects)
                    {
                        EditorGUILayout.LabelField($"• {child.objectName}", EditorStyles.miniLabel);
                    }
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.LabelField("No child objects yet", EditorStyles.miniLabel);
                }
                
                // Quick add child button
                if (GUILayout.Button("➕ Add Child Object", GUILayout.Height(20)))
                {
                    var childConfig = new ResponseObjectConfig
                    {
                        objectName = $"{responseConfig.objectName}_Child{childObjects.Count + 1}",
                        isChildObject = true,
                        parentObjectName = responseConfig.objectName,
                        createNewObject = true
                    };
                    
                    // Add a default event subscription (inherit from parent or use first available)
                    if (responseConfig.listenToEvents.Count > 0)
                    {
                        childConfig.listenToEvents.Add(responseConfig.listenToEvents[0]);
                    }
                    else if (eventChannelConfigs.Count > 0)
                    {
                        childConfig.listenToEvents.Add(eventChannelConfigs[0].eventName);
                    }
                    
                    responseObjectConfigs.Add(childConfig);
                }
            }
            
            // Help text
            if (responseConfig.isParentObject)
            {
                EditorGUILayout.HelpBox("Parent containers organize child objects. They typically have minimal actions themselves.", MessageType.Info);
            }
            else if (responseConfig.isChildObject)
            {
                EditorGUILayout.HelpBox($"This object will be created as a child of '{responseConfig.parentObjectName}'.", MessageType.Info);
            }
        }
        
        // CLGF Theme enum for consistency with the base editor
        private (Color backgroundColor, Color borderColor) GetCLGFThemeColors(CLGFBaseEditor.CLGFTheme theme)
        {
            return theme switch
            {
                // Vibrant blue - bright and appealing
                CLGFBaseEditor.CLGFTheme.Event => (new Color(0.2f, 0.6f, 1.0f, 0.15f), new Color(0.1f, 0.5f, 0.95f, 0.8f)),
                
                // Vibrant orange - warm and energetic  
                CLGFBaseEditor.CLGFTheme.Action => (new Color(1.0f, 0.6f, 0.1f, 0.15f), new Color(0.95f, 0.5f, 0.0f, 0.8f)),
                
                // Vibrant green - fresh and lively
                CLGFBaseEditor.CLGFTheme.ObjectControl => (new Color(0.2f, 0.8f, 0.3f, 0.15f), new Color(0.1f, 0.7f, 0.2f, 0.8f)),
                
                // Vibrant purple - rich and elegant
                CLGFBaseEditor.CLGFTheme.Character => (new Color(0.7f, 0.3f, 0.9f, 0.15f), new Color(0.6f, 0.2f, 0.8f, 0.8f)),
                
                // Vibrant teal - modern and sophisticated
                CLGFBaseEditor.CLGFTheme.Camera => (new Color(0.2f, 0.8f, 0.7f, 0.15f), new Color(0.1f, 0.7f, 0.6f, 0.8f)),
                
                // Vibrant pink - playful and attractive
                CLGFBaseEditor.CLGFTheme.UI => (new Color(1.0f, 0.4f, 0.6f, 0.15f), new Color(0.9f, 0.3f, 0.5f, 0.8f)),
                
                // Vibrant red - bold and attention-grabbing
                CLGFBaseEditor.CLGFTheme.System => (new Color(1.0f, 0.3f, 0.2f, 0.15f), new Color(0.9f, 0.2f, 0.1f, 0.8f)),
                
                _ => (Color.gray, Color.white)
            };
        }
        
        private void DrawColoredReviewSection(string title, CLGFBaseEditor.CLGFTheme theme, System.Action drawContent)
        {
            // Get theme colors from CLGF
            var (backgroundColor, borderColor) = GetCLGFThemeColors(theme);
            
            // Extract icon from title (assumes format like "⚡ Trigger GameObject:")
            string icon = "📋"; // Default icon
            string displayTitle = title;
            
            if (title.Length > 2 && char.IsSymbol(title[0]))
            {
                icon = title.Substring(0, 2).Trim();
                displayTitle = title.Substring(2).Trim();
                // Remove trailing colon if present
                if (displayTitle.EndsWith(":"))
                    displayTitle = displayTitle.Substring(0, displayTitle.Length - 1);
            }
            
            // Draw header in the same style as step headers
            var headerRect = GUILayoutUtility.GetRect(0, 40);
            headerRect.x += 10;
            headerRect.width -= 20;
            
            // Draw background
            EditorGUI.DrawRect(headerRect, borderColor);
            
            // Create styles for larger icons like step headers
            GUIStyle iconStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 24, // Large emoji icon like step headers
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(8, 0, 0, 0)
            };
            
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18, // Large title text like step headers
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(50, 0, 0, 0) // Indent for icon space
            };
            
            // Draw icon and title without CLGF prefix
            GUI.Label(new Rect(headerRect.x, headerRect.y, 50, headerRect.height), icon, iconStyle);
            GUI.Label(headerRect, displayTitle.ToUpper(), titleStyle);
            
            EditorGUILayout.Space(10);
            
            // Draw content with themed background and border
            Rect contentStart = GUILayoutUtility.GetRect(0, 0);
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.Space(8);
            
            // Draw content with consistent indentation
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(8);
            EditorGUILayout.BeginVertical();
            
            drawContent();
            
            EditorGUILayout.EndVertical();
            GUILayout.Space(8);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(8);
            EditorGUILayout.EndVertical();
            
            // Draw colored background on top of the box
            if (Event.current.type == EventType.Repaint)
            {
                Rect contentEnd = GUILayoutUtility.GetLastRect();
                Rect backgroundRect = new Rect(
                    contentEnd.x,
                    contentStart.y,
                    contentEnd.width,
                    contentEnd.height + (contentEnd.y - contentStart.y)
                );
                
                // Draw semi-transparent background over the box
                Color contentBackground = backgroundColor;
                contentBackground.a = 0.05f; // Match foldout background alpha
                EditorGUI.DrawRect(backgroundRect, contentBackground);
                
                // Draw colored border
                float borderWidth = 2f;
                EditorGUI.DrawRect(new Rect(backgroundRect.x, backgroundRect.y, backgroundRect.width, borderWidth), borderColor);
                EditorGUI.DrawRect(new Rect(backgroundRect.x, backgroundRect.y + backgroundRect.height - borderWidth, backgroundRect.width, borderWidth), borderColor);
                EditorGUI.DrawRect(new Rect(backgroundRect.x, backgroundRect.y, borderWidth, backgroundRect.height), borderColor);
                EditorGUI.DrawRect(new Rect(backgroundRect.x + backgroundRect.width - borderWidth, backgroundRect.y, borderWidth, backgroundRect.height), borderColor);
            }
        }
        
        
        private void DrawReview()
        {
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16
            };
            EditorGUILayout.LabelField("📝 Complete Interaction Review", titleStyle);
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.HelpBox("Review your complete interaction system below. Click 'Apply Setup' to create all components and event connections.", MessageType.Info);
            
            EditorGUILayout.Space(10);
            
            // *** INTERACTION FLOW USING BASESETUPWIZARD SYSTEM ***
            DrawFlowDiagram();
            
            EditorGUILayout.Space(15);
            
            // Trigger object & configuration
            DrawColoredReviewSection("⚡ Trigger GameObject:", CLGFBaseEditor.CLGFTheme.Action, () => {
                EditorGUILayout.LabelField(triggerObject ? triggerObject.name : "None");
            });
            
            EditorGUILayout.Space(8);
            
            DrawColoredReviewSection("⚙️ Trigger Configuration:", CLGFBaseEditor.CLGFTheme.Action, () => {
                EditorGUILayout.LabelField($"Type: {triggerConfig.triggerType}");
                
                // Show collision type for collision triggers
                if (triggerConfig.triggerType == TriggerType.Collision)
                {
                    EditorGUILayout.LabelField($"Collider: {triggerConfig.colliderType}");
                }
                
                EditorGUILayout.LabelField($"Can Repeat: {canRepeat}");
                if (canRepeat && cooldownTime > 0)
                {
                    EditorGUILayout.LabelField($"Cooldown: {cooldownTime}s");
                }
            });
            
            EditorGUILayout.Space(8);
            
            // Event channels
            DrawColoredReviewSection("📡 Event Channels:", CLGFBaseEditor.CLGFTheme.Event, () => {
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
            });
            
            EditorGUILayout.Space(8);
            
            // Conditions
            DrawColoredReviewSection("🔍 Conditions:", CLGFBaseEditor.CLGFTheme.Action, () => {
                if (conditionConfigs.Count == 0)
                {
                    EditorGUILayout.LabelField("None (always trigger)");
                }
                else
                {
                    EditorGUILayout.LabelField($"{conditionConfigs.Count} condition(s) - {(requireAllConditions ? "All must be met" : "Any can be met")}");
                }
            });
            
            EditorGUILayout.Space(8);
            
            // Response objects
            DrawColoredReviewSection("🎬 Response Objects:", CLGFBaseEditor.CLGFTheme.ObjectControl, () => {
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
                            EditorGUILayout.LabelField($"  → Action: {action.ActionId}" + (action.executionDelay > 0 ? $" (delay: {action.executionDelay}s)" : ""));
                        }
                    }
                }
            });
        }
        
        
        private void DrawComplete()
        {
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16
            };
            EditorGUILayout.LabelField("✅ Complete Interaction Setup Complete!", titleStyle);
            
            EditorGUILayout.HelpBox("Your complete interaction system has been successfully created with event-driven architecture spanning multiple GameObjects.", MessageType.Info);
            
            if (triggerObject != null)
            {
                EditorGUILayout.Space(10);
                
                GUIStyle subsectionStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14
                };
                EditorGUILayout.LabelField("🔧 Created Components:", subsectionStyle);
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
        
        private void LoadExistingProjectFolders()
        {
            var projectFolders = new List<string>();
            
            // Look for existing project folders in Events directory
            string eventsPath = "Assets/Events";
            if (AssetDatabase.IsValidFolder(eventsPath))
            {
                string[] subFolders = AssetDatabase.GetSubFolders(eventsPath);
                foreach (string folder in subFolders)
                {
                    // Extract just the folder name from the full path
                    string folderName = System.IO.Path.GetFileName(folder);
                    projectFolders.Add(folderName);
                }
            }
            
            // Look for existing project folders in other common directories that might contain organized GameFramework assets
            string[] commonBasePaths = { "Assets/GameFramework/Events", "Assets/Content/Events" };
            
            foreach (string basePath in commonBasePaths)
            {
                if (AssetDatabase.IsValidFolder(basePath))
                {
                    string[] subFolders = AssetDatabase.GetSubFolders(basePath);
                    foreach (string folder in subFolders)
                    {
                        string folderName = System.IO.Path.GetFileName(folder);
                        if (!projectFolders.Contains(folderName))
                        {
                            projectFolders.Add(folderName);
                        }
                    }
                }
            }
            
            existingProjectFolders = projectFolders.ToArray();
            
            // If we have existing folders and no selection, default to the first one
            if (existingProjectFolders.Length > 0 && string.IsNullOrEmpty(selectedProjectFolderPath))
            {
                selectedProjectFolderPath = existingProjectFolders[0];
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
            // Use action discovery system to get action information
            var actionDef = ActionDiscoveryService.GetAction(actionConfig.ActionId);
            if (actionDef != null)
            {
                string description = string.IsNullOrEmpty(actionDef.Description) 
                    ? "Action-specific settings will be configured on the component."
                    : actionDef.Description;
                EditorGUILayout.HelpBox(description, MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"Unknown action type: {actionConfig.ActionId}. Action-specific settings will be configured on the component.", MessageType.Warning);
            }
        }
        
        private bool CanProceedToNextStep()
        {
            return GetCurrentWizardStep() switch
            {
                WizardStep.ProjectSetup => (useExistingProjectFolder && !string.IsNullOrEmpty(selectedProjectFolderPath)) || (!useExistingProjectFolder && !string.IsNullOrEmpty(projectName.Trim())),
                WizardStep.TemplateSelection => !useTemplate || selectedTemplate != null,
                WizardStep.TriggerSetup => triggerObject != null || (createNewTriggerObject && !string.IsNullOrEmpty(newTriggerObjectName.Trim())),
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
            if (currentStepIndex < GetWizardSteps().Length - 1)
            {
                NextStep();
            }
        }
        
        private void PreviousStep()
        {
            if (currentStepIndex > 1) // Template Selection is index 1
            {
                PreviousStep();
            }
        }
        
        private void ResetWizard()
        {
            NavigateToStep(1); // Template Selection
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
        
        /// <summary>
        /// Draws the interaction flow diagram using the BaseSetupWizard framework
        /// </summary>
        private void DrawFlowDiagram()
        {
            // Now we can use DrawFlowDiagram directly since we inherit from BaseSetupWizard
            
            // Build flow steps using the new BaseSetupWizard.FlowStep system
            var flowSteps = new List<FlowStep>();
            int stepNum = 1;
            
            // Step 1: Trigger
            flowSteps.Add(new FlowStep(
                stepNum++, "⚡", 
                $"{triggerConfig.triggerType} trigger on '{(triggerObject ? triggerObject.name : "TriggerObject")}'", 
                CLGFBaseEditor.CLGFTheme.Action
            ));
            
            // Step 2: Events (always present)
            foreach (var eventConfig in eventChannelConfigs)
            {
                flowSteps.Add(new FlowStep(
                    stepNum++, "📡", 
                    $"Raise event: {eventConfig.eventName}",
                    CLGFBaseEditor.CLGFTheme.Event
                ));
                
                // Step 3: Responses for this event (organized by hierarchy)
                var directListeners = responseObjectConfigs.Where(r => IsListeningToEvent(r, eventConfig.eventName)).ToArray();
                
                // Find parent containers that have children listening to this event (even if parent doesn't listen)
                var parentsWithListeningChildren = responseObjectConfigs
                    .Where(parent => parent.isParentObject && 
                           responseObjectConfigs.Any(child => child.isChildObject && 
                                                   child.parentObjectName == parent.objectName && 
                                                   IsListeningToEvent(child, eventConfig.eventName)))
                    .ToArray();
                
                // Combine direct listeners with parents of listening children
                var allRelevantParents = directListeners.Where(l => l.isParentObject)
                    .Union(parentsWithListeningChildren)
                    .Distinct()
                    .ToArray();
                
                // Get standalone listeners (not parents, not children)
                var standaloneListeners = directListeners.Where(l => !l.isParentObject && !l.isChildObject).ToArray();
                
                // Add parent containers (including those with listening children)
                foreach (var parent in allRelevantParents)
                {
                    // Count children listening to this specific event
                    var listeningChildren = responseObjectConfigs
                        .Where(l => l.isChildObject && l.parentObjectName == parent.objectName && IsListeningToEvent(l, eventConfig.eventName))
                        .ToArray();
                    
                    string parentDescription;
                    if (listeningChildren.Length > 0)
                    {
                        parentDescription = $"{parent.objectName} (contains {listeningChildren.Length} children)";
                    }
                    else if (IsListeningToEvent(parent, eventConfig.eventName))
                    {
                        parentDescription = $"{parent.objectName} responds";
                    }
                    else
                    {
                        parentDescription = $"{parent.objectName} (container)";
                    }
                    
                    string[] parentDetails = parent.actions.Take(2).Select(a => $"• {a.ActionId}").ToArray();
                    flowSteps.Add(new FlowStep(
                        stepNum++, "🏗️", 
                        parentDescription,
                        CLGFBaseEditor.CLGFTheme.ObjectControl, 
                        parentDetails, 
                        true, // Indented
                        1 // Indent level 1 for parents
                    ));
                    
                    // Add child objects under this parent that are listening to this event
                    foreach (var child in listeningChildren.Take(3)) // Limit children shown
                    {
                        string[] childDetails = child.actions.Take(2).Select(a => $"• {a.ActionId}").ToArray();
                        flowSteps.Add(new FlowStep(
                            stepNum++, "🎬", 
                            $"└─ {child.objectName}",
                            CLGFBaseEditor.CLGFTheme.ObjectControl, 
                            childDetails, 
                            true, // Indented
                            2 // Indent level 2 for children (deeper)
                        ));
                    }
                }
                
                // Add standalone (non-hierarchical) listeners
                foreach (var listener in standaloneListeners.Take(2))
                {
                    string[] actionDetails = listener.actions.Take(2).Select(a => $"• {a.ActionId}").ToArray();
                    flowSteps.Add(new FlowStep(
                        stepNum++, "🎬", 
                        $"{listener.objectName} responds",
                        CLGFBaseEditor.CLGFTheme.ObjectControl, 
                        actionDetails, 
                        true // Indented
                    ));
                }
            }
            
            // If no response objects were added through events, show all response objects
            if (responseObjectConfigs.Count > 0 && !responseObjectConfigs.Any(r => eventChannelConfigs.Any(e => r.listenToEvents.Contains(e.eventName))))
            {
                // Add a generic "Response Objects" section
                foreach (var responseConfig in responseObjectConfigs.Take(5)) // Show up to 5 response objects
                {
                    string icon = responseConfig.isParentObject ? "🏗️" : 
                                 responseConfig.isChildObject ? "🔧" : "🎬";
                    
                    string[] actionDetails = responseConfig.actions.Take(2).Select(a => $"• {a.ActionId}").ToArray();
                    
                    int indentLevel = responseConfig.isChildObject ? 2 : 1;
                    string description = responseConfig.isChildObject ? 
                        $"└─ {responseConfig.objectName}" : 
                        responseConfig.objectName;
                    
                    flowSteps.Add(new FlowStep(
                        stepNum++, icon, 
                        description,
                        CLGFBaseEditor.CLGFTheme.ObjectControl, 
                        actionDetails, 
                        true, // Indented
                        indentLevel
                    ));
                }
            }
            
            // Use the BaseSetupWizard flow diagram system (only if we have steps)
            if (flowSteps.Count > 1) // More than just the trigger step
            {
                DrawFlowDiagram(flowSteps);
            }
            else
            {
                // Show a message when no response objects are configured
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("🔄 Flow diagram will appear here once you add response objects in the previous steps.", MessageType.Info);
                EditorGUILayout.Space(10);
            }
        }
        
        #region Setup Application
        
        private void ApplySetup()
        {
            // Create new GameObject if requested
            if (createNewTriggerObject && triggerObject == null)
            {
                triggerObject = new GameObject(newTriggerObjectName.Trim());
                
                // Place the new object at the origin or scene view center
                var sceneView = SceneView.lastActiveSceneView;
                if (sceneView != null)
                {
                    triggerObject.transform.position = sceneView.pivot;
                }
                
                // Register undo for the new GameObject creation
                Undo.RegisterCreatedObjectUndo(triggerObject, "Create Trigger GameObject");
                
                // Select the new object in the hierarchy
                Selection.activeGameObject = triggerObject;
            }
            
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
                
                // Connect trigger events to raise actions - CRITICAL MISSING STEP
                ConnectTriggerToRaiseActions();
                
                // Create response objects and their components
                CreateResponseObjects();
                
                // Mark scene as dirty
                EditorSceneManager.MarkSceneDirty(triggerObject.scene);
                
                // Move to complete step
                NavigateToStep(7); // Complete step
                
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
        
        // Store created assets and objects for use in other methods
        private System.Collections.Generic.Dictionary<string, GameEvent> createdGameEvents = new System.Collections.Generic.Dictionary<string, GameEvent>();
        private System.Collections.Generic.Dictionary<string, GameObject> createdResponseObjects = new System.Collections.Generic.Dictionary<string, GameObject>();
        
        private void CreateEventChannels()
        {
            createdGameEvents.Clear();
            createdResponseObjects.Clear();
            
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
                        // Store reference for later use
                        eventConfig.gameEventAsset = gameEvent;
                    }
                    else
                    {
                        Debug.LogError($"Failed to create GameEvent asset: {eventConfig.eventName}");
                    }
                }
                else
                {
                    // Use existing GameEvent asset (direct reference or path)
                    if (eventConfig.gameEventAsset != null)
                    {
                        // Direct ScriptableObject reference
                        gameEvent = eventConfig.gameEventAsset;
                        Debug.Log($"Using existing GameEvent: {gameEvent.ChannelName} from ObjectField");
                        createdGameEvents[eventConfig.eventName] = gameEvent;
                    }
                    else if (!string.IsNullOrEmpty(eventConfig.existingEventPath))
                    {
                        // Legacy path-based loading (for templates)
                        gameEvent = LoadExistingGameEvent(eventConfig);
                        if (gameEvent != null)
                        {
                            Debug.Log($"Loaded existing GameEvent: {eventConfig.eventName} from {eventConfig.existingEventPath}");
                            createdGameEvents[eventConfig.eventName] = gameEvent;
                            // Store reference for consistency
                            eventConfig.gameEventAsset = gameEvent;
                        }
                        else
                        {
                            Debug.LogError($"Failed to load existing GameEvent: {eventConfig.existingEventPath}");
                        }
                    }
                    else
                    {
                        Debug.LogError($"No GameEvent specified for event config: {eventConfig.eventName}");
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
        
        private void ConnectTriggerToRaiseActions()
        {
            if (triggerObject == null)
            {
                Debug.LogError("Cannot connect trigger to raise actions - trigger object is null");
                return;
            }
            
            // Get the trigger component and raise action component
            var trigger = triggerObject.GetComponent<BaseTrigger>();
            var raiseAction = triggerObject.GetComponent<RaiseGameEventAction>();
            
            if (trigger == null)
            {
                Debug.LogError("No BaseTrigger component found on trigger object - cannot connect events");
                return;
            }
            
            if (raiseAction == null)
            {
                Debug.LogError("No RaiseGameEventAction component found on trigger object - cannot connect events");
                return;
            }
            
            // Check if OnTriggeredEvent property is accessible
            var triggeredEvent = trigger.OnTriggeredEvent;
            if (triggeredEvent == null)
            {
                Debug.LogError("OnTriggeredEvent property returned null - cannot connect events");
                return;
            }
            
            // Connect trigger's OnTriggeredEvent UnityEvent to RaiseGameEventAction.Execute()
            try
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(
                    triggeredEvent, 
                    raiseAction.Execute
                );
                
                Debug.Log($"Connected {trigger.GetType().Name}.OnTriggeredEvent to RaiseGameEventAction.Execute()");
                
                // Mark the trigger component as dirty for undo/redo
                UnityEditor.EditorUtility.SetDirty(trigger);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception while connecting trigger events: {e.Message}");
                Debug.LogException(e);
            }
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
                
                if (creationMode == CreationMode.SceneObjects)
                {
                    // Handle parent-child hierarchy
                    if (responseConfig.isChildObject && !string.IsNullOrEmpty(responseConfig.parentObjectName))
                    {
                        // Find the parent object
                        GameObject parentObject = FindCreatedResponseObject(responseConfig.parentObjectName);
                        if (parentObject != null)
                        {
                            responseObject.transform.SetParent(parentObject.transform);
                            responseObject.transform.localPosition = Vector3.zero; // Reset to parent origin
                            Debug.Log($"Created child response object '{responseConfig.objectName}' under parent '{responseConfig.parentObjectName}'");
                        }
                        else
                        {
                            Debug.LogWarning($"Parent object '{responseConfig.parentObjectName}' not found for child '{responseConfig.objectName}'. Creating as root object.");
                        }
                    }
                    
                    // Position it near the trigger object if possible (only for non-child objects)
                    if (!responseConfig.isChildObject && triggerObject != null)
                    {
                        Vector3 offset = new Vector3(2f, 0f, 0f); // Place 2 units to the right of trigger
                        responseObject.transform.position = triggerObject.transform.position + offset;
                    }
                    
                    // Register undo for the creation
                    Undo.RegisterCreatedObjectUndo(responseObject, $"Create Response Object: {responseConfig.objectName}");
                    
                    Debug.Log($"Created new response object in scene: {responseConfig.objectName}");
                }
                else // CreationMode.Prefabs
                {
                    // Create as prefab asset
                    string prefabPath = GetResponseObjectPrefabPath(responseConfig.objectName);
                    
                    // Ensure the directory exists
                    string directory = System.IO.Path.GetDirectoryName(prefabPath);
                    if (!System.IO.Directory.Exists(directory))
                    {
                        System.IO.Directory.CreateDirectory(directory);
                    }
                    
                    // Create prefab asset
                    GameObject prefab = PrefabUtility.SaveAsPrefabAsset(responseObject, prefabPath);
                    
                    // Destroy the temporary scene object and use the prefab reference
                    UnityEngine.Object.DestroyImmediate(responseObject);
                    responseObject = prefab;
                    
                    Debug.Log($"Created new response object as prefab: {prefabPath}");
                }
                
                // Store the created object in the dictionary for hierarchy tracking
                createdResponseObjects[responseConfig.objectName] = responseObject;
                
                return responseObject;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception creating response object '{responseConfig.objectName}': {e.Message}");
                return null;
            }
        }
        
        private GameObject FindCreatedResponseObject(string objectName)
        {
            // Search through already created response objects
            // This assumes objects are created in the order they appear in responseObjectConfigs
            foreach (var kvp in createdResponseObjects)
            {
                if (kvp.Value != null && kvp.Value.name == objectName)
                {
                    return kvp.Value;
                }
            }
            
            // Fallback: search in scene
            GameObject found = GameObject.Find(objectName);
            if (found != null)
            {
                Debug.Log($"Found existing response object in scene: {objectName}");
                return found;
            }
            
            Debug.LogWarning($"Could not find created response object: {objectName}");
            return null;
        }
        
        private string GetCurrentProjectFolderName()
        {
            // Use existing project folder or new project name
            if (useExistingProjectFolder && !string.IsNullOrEmpty(selectedProjectFolderPath))
            {
                return selectedProjectFolderPath.Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
            }
            else if (!string.IsNullOrEmpty(projectName))
            {
                return projectName.Trim().Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
            }
            
            // Fallback to default organization
            return "Default";
        }
        
        private string GetResponseObjectPrefabPath(string objectName)
        {
            // Clean the object name for use as filename
            string cleanObjectName = objectName.Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
            
            // Create path with project-level organization: ResponseObjects > Project > ObjectName
            string basePath = "Assets/Content/ResponseObjects";
            
            // Add project-level organization
            string projectFolderName = GetCurrentProjectFolderName();
            if (!string.IsNullOrEmpty(projectFolderName))
            {
                basePath = $"{basePath}/{projectFolderName}";
            }
            
            return $"{basePath}/{cleanObjectName}.prefab";
        }
        
        private GameObject FindExistingResponseObject(ResponseObjectConfig responseConfig)
        {
            try
            {
                GameObject responseObject = null;
                
                // Try direct GameObject reference first (new approach)
                if (responseConfig.targetGameObject != null)
                {
                    responseObject = responseConfig.targetGameObject;
                    Debug.Log($"Using existing response object from ObjectField: {responseObject.name}");
                }
                // Fall back to legacy name-based lookup (for templates)
                else if (!string.IsNullOrEmpty(responseConfig.targetObjectId))
                {
                    // Try finding by exact name first
                    responseObject = GameObject.Find(responseConfig.targetObjectId);
                    
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
                        Debug.Log($"Found existing response object by name: {responseObject.name}");
                    }
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
            // Handle both string events and GameEvent assets
            for (int i = 0; i < responseConfig.listenToEvents.Count; i++)
            {
                string eventName = responseConfig.listenToEvents[i];
                GameEvent gameEvent = null;
                
                // First try to get GameEvent from direct asset reference
                if (i < responseConfig.gameEventAssets.Count && responseConfig.gameEventAssets[i] != null)
                {
                    gameEvent = responseConfig.gameEventAssets[i];
                    Debug.Log($"Using direct GameEvent asset: {gameEvent.ChannelName} for {responseObject.name}");
                }
                // Fallback to wizard-created events
                else if (!string.IsNullOrEmpty(eventName) && createdGameEvents.TryGetValue(eventName, out gameEvent))
                {
                    Debug.Log($"Using wizard-created GameEvent: {eventName} for {responseObject.name}");
                }
                else
                {
                    Debug.LogWarning($"GameEvent '{eventName}' not found for response object '{responseObject.name}'. Skipping listener creation.");
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
                        Debug.Log($"Using existing GameEventListener for event: {gameEvent.ChannelName} on {responseObject.name}");
                    }
                    else
                    {
                        gameEventListener = responseObject.AddComponent<GameEventListener>();
                        Debug.Log($"Added new GameEventListener for event: {gameEvent.ChannelName} on {responseObject.name}");
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
                        Debug.Log($"Created and configured {actionConfig.ActionId} on {responseObject.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to create action component: {actionConfig.ActionId} on {responseObject.name}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Exception creating action '{actionConfig.ActionId}' on '{responseObject.name}': {e.Message}");
                }
            }
        }
        
        private void ConnectActionToEventListeners(GameObject responseObject, BaseTriggerAction action, ResponseObjectConfig responseConfig)
        {
            try
            {
                var listeners = responseObject.GetComponents<GameEventListener>();
                Debug.Log($"Found {listeners.Length} GameEventListener(s) on {responseObject.name}");
                
                if (listeners.Length == 0)
                {
                    Debug.LogWarning($"No GameEventListener components found on {responseObject.name} - actions will not be connected");
                    return;
                }
                
                int connectionsCount = 0;
                
                foreach (var listener in listeners)
                {
                    if (listener.GameEvent == null)
                    {
                        Debug.LogWarning($"GameEventListener on {responseObject.name} has null GameEvent - skipping");
                        continue;
                    }
                    
                    // Check if this listener is for an event this response object should listen to
                    string eventName = GetEventNameFromGameEvent(listener.GameEvent);
                    Debug.Log($"Checking listener for event '{eventName}' against required events: [{string.Join(", ", responseConfig.listenToEvents)}]");
                    
                    if (responseConfig.listenToEvents.Contains(eventName))
                    {
                        // Get and validate the OnEventRaised UnityEvent
                        var onEventRaised = listener.OnEventRaised;
                        if (onEventRaised == null)
                        {
                            Debug.LogError($"OnEventRaised UnityEvent is null on GameEventListener for {eventName}");
                            continue;
                        }
                        
                        // Add a persistent call to the action's Execute method
                        UnityEditor.Events.UnityEventTools.AddPersistentListener(onEventRaised, action.Execute);
                        connectionsCount++;
                        
                        Debug.Log($"✅ Connected {action.GetType().Name}.Execute() to GameEventListener.OnEventRaised for event: {eventName}");
                        
                        // Mark listener as dirty for undo/redo
                        UnityEditor.EditorUtility.SetDirty(listener);
                    }
                    else
                    {
                        Debug.Log($"Skipping listener for event '{eventName}' - not in required events list");
                    }
                }
                
                if (connectionsCount == 0)
                {
                    Debug.LogWarning($"No connections made for {action.GetType().Name} on {responseObject.name} - check event name matching");
                }
                else
                {
                    Debug.Log($"Made {connectionsCount} connection(s) for {action.GetType().Name} on {responseObject.name}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception connecting action to event listeners: {e.Message}");
                Debug.LogException(e);
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
            // Use the new action discovery system
            return ActionDiscoveryService.CreateActionComponent(targetObj, actionConfig.ActionId);
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
        
        /// <summary>
        /// Helper method to check if a response object is listening to a specific event,
        /// considering both string event names and GameEvent ScriptableObject references.
        /// </summary>
        private bool IsListeningToEvent(ResponseObjectConfig responseConfig, string eventName)
        {
            // Check string event names
            if (responseConfig.listenToEvents.Contains(eventName))
                return true;
            
            // Check GameEvent ScriptableObject references
            for (int i = 0; i < responseConfig.gameEventAssets.Count; i++)
            {
                var gameEventAsset = responseConfig.gameEventAssets[i];
                if (gameEventAsset != null && gameEventAsset.ChannelName == eventName)
                    return true;
            }
            
            return false;
        }
        
        private bool HasValidTriggerConfiguration()
        {
            if (createNewTriggerObject)
                return !string.IsNullOrEmpty(newTriggerObjectName?.Trim());
            else
                return triggerObject != null;
        }
        
        private bool HasValidSetupConfiguration()
        {
            return HasValidTriggerConfiguration() && 
                   eventChannelConfigs.Count > 0 && 
                   responseObjectConfigs.Count > 0;
        }
        
        private void CompleteSetup()
        {
            // Apply the complete interaction system setup
            ApplySetup();
        }
        
        private CLGFBaseEditor.CLGFTheme GetStepThemeForEnum(WizardStep step)
        {
            return step switch
            {
                WizardStep.ProjectSetup => CLGFBaseEditor.CLGFTheme.System,
                WizardStep.TemplateSelection => CLGFBaseEditor.CLGFTheme.System,
                WizardStep.TriggerSetup => CLGFBaseEditor.CLGFTheme.Action,
                WizardStep.EventChannelSetup => CLGFBaseEditor.CLGFTheme.Event,
                WizardStep.ConditionSetup => CLGFBaseEditor.CLGFTheme.Action,
                WizardStep.ResponseObjectSetup => CLGFBaseEditor.CLGFTheme.ObjectControl,
                WizardStep.Review => CLGFBaseEditor.CLGFTheme.Character,
                WizardStep.Complete => CLGFBaseEditor.CLGFTheme.System,
                _ => CLGFBaseEditor.CLGFTheme.System
            };
        }
        
        // Placeholder helper methods removed - now using comprehensive implementations
        
        private void ApplyTemplate(TriggerResponseTemplate template)
        {
            // TODO: Implement template application
            if (template != null)
            {
                // Apply template configuration to current wizard state
                EditorUtility.DisplayDialog("Template Applied", $"Template '{template.TemplateName}' has been applied.", "OK");
            }
        }
        
        #region Foldout System
        
        /// <summary>
        /// Draws a collapsible foldout section with themed styling using BaseSetupWizard's foldout system.
        /// Returns true if the section is expanded and content should be drawn.
        /// </summary>
        private new bool DrawFoldoutSection(string id, string title, string icon = "📁", CLGFBaseEditor.CLGFTheme theme = CLGFBaseEditor.CLGFTheme.System, System.Action drawContent = null, bool defaultExpanded = true, bool showItemCount = false, int itemCount = 0, string tooltip = "", bool drawBackground = true, FoldoutUtility.FoldoutButton[] headerButtons = null)
        {
            return base.DrawFoldoutSection(id, title, icon, theme, drawContent, defaultExpanded, showItemCount, itemCount, tooltip, drawBackground, headerButtons);
        }
        
        
        
        #endregion

        #region BaseSetupWizard Implementation
        // Duplicate methods removed - implementations are earlier in the file
        
        #endregion

        #endregion
    }
}
#endif