#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using GameFramework.Events.Triggers;

namespace GameFramework.Events.Templates
{
    /// <summary>
    /// Utility class for creating predefined trigger response templates.
    /// </summary>
    public static class TemplateCreator
    {
        [MenuItem("GameFramework/Events/Create Template Presets")]
        public static void CreateTemplatePresets()
        {
            CreatePickupItemTemplate();
            CreateProximityDetectorTemplate();
            CreateTimedEventTemplate();
            CreateButtonPressTemplate();
            CreateDoorTriggerTemplate();
            
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Templates Created", "Predefined trigger response templates have been created in Assets/Content/Templates/", "OK");
        }
        
        private static void CreatePickupItemTemplate()
        {
            var template = ScriptableObject.CreateInstance<TriggerResponseTemplate>();
            
            // Template info
            SetTemplateField(template, "templateName", "Pickup Item");
            SetTemplateField(template, "description", "Complete pickup interaction: Player touches item → ItemCollected event → UI updates, audio plays, item disappears.");
            SetTemplateField(template, "category", "Items");
            SetTemplateField(template, "difficulty", 1);
            
            // Trigger config
            var triggerConfig = new TriggerConfig
            {
                triggerType = TriggerType.Collision,
                collisionEvent = CollisionTrigger.TriggerEvent.OnEnter,
                triggerLayers = LayerMask.GetMask("Player"),
                requiredTag = "Player",
                requireRigidbody = false
            };
            SetTemplateField(template, "triggerSettings", triggerConfig);
            
            // Event channels
            var eventChannels = new List<EventChannelConfig>
            {
                new EventChannelConfig { eventName = "ItemCollected", description = "Triggered when player picks up an item", createNewEvent = true }
            };
            SetTemplateField(template, "eventChannels", eventChannels);
            
            // Response objects
            var responseObjects = new List<ResponseObjectConfig>
            {
                new ResponseObjectConfig 
                { 
                    objectName = "AudioFeedback", 
                    description = "Plays pickup sound",
                    createNewObject = true,
                    listenToEvents = new List<string> { "ItemCollected" },
                    actions = new List<ActionConfig> { new ActionConfig { ActionId = "audio-action", executionDelay = 0f } }
                },
                new ResponseObjectConfig 
                { 
                    objectName = "ItemVisual", 
                    description = "Deactivates the item visual",
                    createNewObject = false,
                    listenToEvents = new List<string> { "ItemCollected" },
                    actions = new List<ActionConfig> { new ActionConfig { ActionId = "gameobject-activate", executionDelay = 0.1f } }
                }
            };
            SetTemplateField(template, "responseObjects", responseObjects);
            
            // General settings
            SetTemplateField(template, "canRepeat", false);
            SetTemplateField(template, "cooldownTime", 0f);
            SetTemplateField(template, "debugMode", false);
            
            SaveTemplate(template, "PickupItem");
        }
        
        private static void CreateProximityDetectorTemplate()
        {
            var template = ScriptableObject.CreateInstance<TriggerResponseTemplate>();
            
            // Template info
            SetTemplateField(template, "templateName", "Proximity Detector");
            SetTemplateField(template, "description", "Detects when player enters a specific range. Useful for area triggers, ambushes, or environmental effects.");
            SetTemplateField(template, "category", "Detection");
            SetTemplateField(template, "difficulty", 2);
            
            // Trigger config
            var triggerConfig = new TriggerConfig
            {
                triggerType = TriggerType.Proximity,
                proximityEvent = ProximityTrigger.ProximityEvent.OnEnterRange,
                triggerDistance = 5f,
                checkInterval = 0.2f,
                use3DDistance = true,
                targetMode = ProximityTrigger.TargetMode.FindByTag,
                targetTag = "Player"
            };
            SetTemplateField(template, "triggerSettings", triggerConfig);
            
            // Actions
            var actions = new List<ActionConfig>
            {
                new ActionConfig { ActionId = "audio-action", executionDelay = 0f }
            };
            SetTemplateField(template, "actions", actions);
            
            // General settings
            SetTemplateField(template, "canRepeat", true);
            SetTemplateField(template, "cooldownTime", 2f);
            SetTemplateField(template, "debugMode", true);
            
            SaveTemplate(template, "ProximityDetector");
        }
        
        private static void CreateTimedEventTemplate()
        {
            var template = ScriptableObject.CreateInstance<TriggerResponseTemplate>();
            
            // Template info
            SetTemplateField(template, "templateName", "Timed Event");
            SetTemplateField(template, "description", "Triggers events after a specified time delay. Perfect for cutscenes, delayed reactions, or timed sequences.");
            SetTemplateField(template, "category", "Timing");
            SetTemplateField(template, "difficulty", 1);
            
            // Trigger config
            var triggerConfig = new TriggerConfig
            {
                triggerType = TriggerType.Timer,
                timerDuration = 3f,
                startOnAwake = true,
                autoReset = false
            };
            SetTemplateField(template, "triggerSettings", triggerConfig);
            
            // Actions
            var actions = new List<ActionConfig>
            {
                new ActionConfig { ActionId = "audio-action", executionDelay = 0f },
                new ActionConfig { ActionId = "gameobject-activate", executionDelay = 0.5f }
            };
            SetTemplateField(template, "actions", actions);
            
            // General settings
            SetTemplateField(template, "canRepeat", false);
            SetTemplateField(template, "cooldownTime", 0f);
            SetTemplateField(template, "debugMode", false);
            
            SaveTemplate(template, "TimedEvent");
        }
        
        private static void CreateButtonPressTemplate()
        {
            var template = ScriptableObject.CreateInstance<TriggerResponseTemplate>();
            
            // Template info
            SetTemplateField(template, "templateName", "Interactive Button");
            SetTemplateField(template, "description", "Player-activated button or switch. Requires player collision and can trigger doors, elevators, or other mechanisms.");
            SetTemplateField(template, "category", "Interactive");
            SetTemplateField(template, "difficulty", 2);
            
            // Trigger config
            var triggerConfig = new TriggerConfig
            {
                triggerType = TriggerType.Collision,
                collisionEvent = CollisionTrigger.TriggerEvent.OnEnter,
                triggerLayers = LayerMask.GetMask("Player"),
                requiredTag = "Player",
                requireRigidbody = true
            };
            SetTemplateField(template, "triggerSettings", triggerConfig);
            
            // Actions
            var actions = new List<ActionConfig>
            {
                new ActionConfig { ActionId = "audio-action", executionDelay = 0f },
                new ActionConfig { ActionId = "component-toggle", executionDelay = 0.2f }
            };
            SetTemplateField(template, "actions", actions);
            
            // General settings
            SetTemplateField(template, "canRepeat", true);
            SetTemplateField(template, "cooldownTime", 1f);
            SetTemplateField(template, "debugMode", false);
            
            SaveTemplate(template, "InteractiveButton");
        }
        
        private static void CreateDoorTriggerTemplate()
        {
            var template = ScriptableObject.CreateInstance<TriggerResponseTemplate>();
            
            // Template info
            SetTemplateField(template, "templateName", "Automatic Door");
            SetTemplateField(template, "description", "Door that opens when player approaches and closes when they leave. Uses proximity detection with enter/exit events.");
            SetTemplateField(template, "category", "Interactive");
            SetTemplateField(template, "difficulty", 3);
            
            // Trigger config
            var triggerConfig = new TriggerConfig
            {
                triggerType = TriggerType.Proximity,
                proximityEvent = ProximityTrigger.ProximityEvent.OnEnterRange,
                triggerDistance = 3f,
                checkInterval = 0.1f,
                use3DDistance = true,
                targetMode = ProximityTrigger.TargetMode.FindByTag,
                targetTag = "Player"
            };
            SetTemplateField(template, "triggerSettings", triggerConfig);
            
            // Actions
            var actions = new List<ActionConfig>
            {
                new ActionConfig { ActionId = "audio-action", executionDelay = 0f },
                new ActionConfig { ActionId = "component-toggle", executionDelay = 0.1f }
            };
            SetTemplateField(template, "actions", actions);
            
            // General settings
            SetTemplateField(template, "canRepeat", true);
            SetTemplateField(template, "cooldownTime", 0.5f);
            SetTemplateField(template, "debugMode", false);
            
            SaveTemplate(template, "AutomaticDoor");
        }
        
        private static void SetTemplateField(TriggerResponseTemplate template, string fieldName, object value)
        {
            var field = typeof(TriggerResponseTemplate).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(template, value);
        }
        
        private static void SaveTemplate(TriggerResponseTemplate template, string fileName)
        {
            string directoryPath = "Assets/Content/Templates/TriggerResponse";
            
            // Create directory if it doesn't exist
            if (!AssetDatabase.IsValidFolder(directoryPath))
            {
                string[] pathParts = directoryPath.Split('/');
                string currentPath = pathParts[0];
                
                for (int i = 1; i < pathParts.Length; i++)
                {
                    string newPath = currentPath + "/" + pathParts[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, pathParts[i]);
                    }
                    currentPath = newPath;
                }
            }
            
            string assetPath = $"{directoryPath}/{fileName}_Template.asset";
            AssetDatabase.CreateAsset(template, assetPath);
        }
    }
}
#endif