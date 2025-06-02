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
            SetTemplateField(template, "gameObjectName", "PickupItem");
            
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
            
            // Organization settings - Pickup items benefit from shared parent
            SetTemplateField(template, "useSharedParent", true);
            SetTemplateField(template, "preferCreateNew", true);
            
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
            SetTemplateField(template, "gameObjectName", "ProximityDetector");
            
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
            
            // Event channels
            var eventChannels = new List<EventChannelConfig>
            {
                new EventChannelConfig { eventName = "PlayerDetected", description = "Triggered when player enters detection range", createNewEvent = true }
            };
            SetTemplateField(template, "eventChannels", eventChannels);
            
            // Response objects
            var responseObjects = new List<ResponseObjectConfig>
            {
                new ResponseObjectConfig 
                { 
                    objectName = "DetectionFeedback", 
                    description = "Provides audio/visual feedback for detection",
                    createNewObject = true,
                    listenToEvents = new List<string> { "PlayerDetected" },
                    actions = new List<ActionConfig> { new ActionConfig { ActionId = "audio-action", executionDelay = 0f } }
                }
            };
            SetTemplateField(template, "responseObjects", responseObjects);
            
            // General settings
            SetTemplateField(template, "canRepeat", true);
            SetTemplateField(template, "cooldownTime", 2f);
            SetTemplateField(template, "debugMode", true);
            
            // Organization settings - Proximity detectors don't need shared parent (separate objects)
            SetTemplateField(template, "useSharedParent", false);
            SetTemplateField(template, "preferCreateNew", true);
            
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
            SetTemplateField(template, "gameObjectName", "TimedEvent");
            
            // Trigger config
            var triggerConfig = new TriggerConfig
            {
                triggerType = TriggerType.Timer,
                timerDuration = 3f,
                startOnAwake = true,
                autoReset = false
            };
            SetTemplateField(template, "triggerSettings", triggerConfig);
            
            // Event channels
            var eventChannels = new List<EventChannelConfig>
            {
                new EventChannelConfig { eventName = "TimerExpired", description = "Triggered when timer reaches zero", createNewEvent = true }
            };
            SetTemplateField(template, "eventChannels", eventChannels);
            
            // Response objects
            var responseObjects = new List<ResponseObjectConfig>
            {
                new ResponseObjectConfig 
                { 
                    objectName = "AudioFeedback", 
                    description = "Plays audio when timer expires",
                    createNewObject = true,
                    listenToEvents = new List<string> { "TimerExpired" },
                    actions = new List<ActionConfig> { new ActionConfig { ActionId = "audio-action", executionDelay = 0f } }
                },
                new ResponseObjectConfig 
                { 
                    objectName = "TargetObject", 
                    description = "Object to activate/deactivate after timer",
                    createNewObject = true,
                    listenToEvents = new List<string> { "TimerExpired" },
                    actions = new List<ActionConfig> { new ActionConfig { ActionId = "gameobject-activate", executionDelay = 0.5f } }
                }
            };
            SetTemplateField(template, "responseObjects", responseObjects);
            
            // General settings
            SetTemplateField(template, "canRepeat", false);
            SetTemplateField(template, "cooldownTime", 0f);
            SetTemplateField(template, "debugMode", false);
            
            // Organization settings - Timed events don't typically need shared parent
            SetTemplateField(template, "useSharedParent", false);
            SetTemplateField(template, "preferCreateNew", true);
            
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
            SetTemplateField(template, "gameObjectName", "InteractiveButton");
            
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
            
            // Event channels
            var eventChannels = new List<EventChannelConfig>
            {
                new EventChannelConfig { eventName = "ButtonPressed", description = "Triggered when button is activated by player", createNewEvent = true }
            };
            SetTemplateField(template, "eventChannels", eventChannels);
            
            // Response objects
            var responseObjects = new List<ResponseObjectConfig>
            {
                new ResponseObjectConfig 
                { 
                    objectName = "ButtonFeedback", 
                    description = "Provides audio/visual feedback for button press",
                    createNewObject = true,
                    listenToEvents = new List<string> { "ButtonPressed" },
                    actions = new List<ActionConfig> { new ActionConfig { ActionId = "audio-action", executionDelay = 0f } }
                },
                new ResponseObjectConfig 
                { 
                    objectName = "MechanismTarget", 
                    description = "The mechanism activated by the button (door, elevator, etc.)",
                    createNewObject = true,
                    listenToEvents = new List<string> { "ButtonPressed" },
                    actions = new List<ActionConfig> { new ActionConfig { ActionId = "component-toggle", executionDelay = 0.2f } }
                }
            };
            SetTemplateField(template, "responseObjects", responseObjects);
            
            // General settings
            SetTemplateField(template, "canRepeat", true);
            SetTemplateField(template, "cooldownTime", 1f);
            SetTemplateField(template, "debugMode", false);
            
            // Organization settings - Interactive buttons benefit from shared parent
            SetTemplateField(template, "useSharedParent", true);
            SetTemplateField(template, "preferCreateNew", true);
            
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
            SetTemplateField(template, "gameObjectName", "AutomaticDoor");
            
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
            
            // Event channels
            var eventChannels = new List<EventChannelConfig>
            {
                new EventChannelConfig { eventName = "DoorOpen", description = "Triggered when player approaches door", createNewEvent = true },
                new EventChannelConfig { eventName = "DoorClose", description = "Triggered when player leaves door area", createNewEvent = true }
            };
            SetTemplateField(template, "eventChannels", eventChannels);
            
            // Response objects
            var responseObjects = new List<ResponseObjectConfig>
            {
                new ResponseObjectConfig 
                { 
                    objectName = "DoorSounds", 
                    description = "Plays door opening/closing sounds",
                    createNewObject = true,
                    listenToEvents = new List<string> { "DoorOpen", "DoorClose" },
                    actions = new List<ActionConfig> { new ActionConfig { ActionId = "audio-action", executionDelay = 0f } }
                },
                new ResponseObjectConfig 
                { 
                    objectName = "DoorMechanism", 
                    description = "Controls door opening/closing animation or movement",
                    createNewObject = true,
                    listenToEvents = new List<string> { "DoorOpen", "DoorClose" },
                    actions = new List<ActionConfig> { new ActionConfig { ActionId = "component-toggle", executionDelay = 0.1f } }
                }
            };
            SetTemplateField(template, "responseObjects", responseObjects);
            
            // General settings
            SetTemplateField(template, "canRepeat", true);
            SetTemplateField(template, "cooldownTime", 0.5f);
            SetTemplateField(template, "debugMode", false);
            
            // Organization settings - Automatic doors benefit from shared parent
            SetTemplateField(template, "useSharedParent", true);
            SetTemplateField(template, "preferCreateNew", true);
            
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