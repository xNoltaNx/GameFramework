using System;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Events.Triggers;
using GameFramework.Events.Actions;
using GameFramework.Events.Conditions;

namespace GameFramework.Events.Templates
{
    /// <summary>
    /// Template for complete interaction patterns spanning multiple GameObjects.
    /// Used by the Complete Interaction Setup Wizard to configure event-driven game systems.
    /// </summary>
    [CreateAssetMenu(fileName = "New Interaction Template", menuName = "GameFramework/Events/Complete Interaction Template")]
    public class TriggerResponseTemplate : ScriptableObject
    {
        [Header("Template Info")]
        [SerializeField] private string templateName = "New Interaction";
        [SerializeField, TextArea(3, 6)] private string description = "Describe what this interaction does...";
        [SerializeField] private Texture2D templateIcon;
        [SerializeField] private string category = "General";
        [SerializeField] private int difficulty = 1; // 1 = Beginner, 2 = Intermediate, 3 = Advanced
        [SerializeField] private string gameObjectName = "NewInteraction"; // Logical name for the main GameObject
        
        [Header("Trigger Configuration")]
        [SerializeField] private TriggerConfig triggerSettings;
        
        [Header("Event Channels")]
        [SerializeField] private List<EventChannelConfig> eventChannels = new List<EventChannelConfig>();
        
        [Header("Conditions")]
        [SerializeField] private List<ConditionConfig> conditions = new List<ConditionConfig>();
        [SerializeField] private bool requireAllConditions = true;
        
        [Header("Response Objects")]
        [SerializeField] private List<ResponseObjectConfig> responseObjects = new List<ResponseObjectConfig>();
        
        [Header("Advanced Settings")]
        [SerializeField] private bool canRepeat = true;
        [SerializeField] private float cooldownTime = 0f;
        [SerializeField] private bool debugMode = false;
        
        [Header("Organization")]
        [SerializeField] private bool useSharedParent = false;
        [SerializeField] private bool preferCreateNew = true;
        
        // Properties
        public string TemplateName => templateName;
        public string Description => description;
        public Texture2D TemplateIcon => templateIcon;
        public string Category => category;
        public int Difficulty => difficulty;
        public string GameObjectName => gameObjectName;
        public TriggerConfig TriggerSettings => triggerSettings;
        public List<EventChannelConfig> EventChannels => eventChannels;
        public List<ConditionConfig> Conditions => conditions;
        public bool RequireAllConditions => requireAllConditions;
        public List<ResponseObjectConfig> ResponseObjects => responseObjects;
        public bool CanRepeat => canRepeat;
        public float CooldownTime => cooldownTime;
        public bool DebugMode => debugMode;
        public bool UseSharedParent => useSharedParent;
        public bool PreferCreateNew => preferCreateNew;
        
#if UNITY_EDITOR
        /// <summary>
        /// Updates template to use new organization features.
        /// Call this from editor scripts to migrate old templates.
        /// </summary>
        public void UpdateToNewFeatures(bool setUseSharedParent = false, bool setPreferCreateNew = true)
        {
            useSharedParent = setUseSharedParent;
            preferCreateNew = setPreferCreateNew;
            
            // Update all response objects to create new
            if (preferCreateNew)
            {
                foreach (var responseConfig in responseObjects)
                {
                    responseConfig.createNewObject = true;
                }
                
                foreach (var eventConfig in eventChannels)
                {
                    eventConfig.createNewEvent = true;
                }
            }
            
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
    
    /// <summary>
    /// Configuration for trigger setup
    /// </summary>
    [Serializable]
    public class TriggerConfig
    {
        public TriggerType triggerType = TriggerType.Collision;
        
        // Collision Trigger Settings
        public CollisionTrigger.TriggerEvent collisionEvent = CollisionTrigger.TriggerEvent.OnEnter;
        public LayerMask triggerLayers = -1;
        public string requiredTag = "";
        public bool requireRigidbody = false;
        public ColliderType colliderType = ColliderType.BoxCollider;
        
        // Proximity Trigger Settings
        public ProximityTrigger.ProximityEvent proximityEvent = ProximityTrigger.ProximityEvent.OnEnterRange;
        public float triggerDistance = 5f;
        public float checkInterval = 0.1f;
        public bool use3DDistance = true;
        public ProximityTrigger.TargetMode targetMode = ProximityTrigger.TargetMode.FindByTag;
        public string targetTag = "Player";
        
        // Timer Trigger Settings
        public float timerDuration = 1f;
        public bool startOnAwake = true;
        public bool autoReset = false;
    }
    
    /// <summary>
    /// Configuration for condition setup
    /// </summary>
    [Serializable]
    public class ConditionConfig
    {
        public ConditionType conditionType = ConditionType.Custom;
        public bool invertResult = false;
        public string conditionData = ""; // JSON or serialized data for condition-specific settings
    }
    
    /// <summary>
    /// Configuration for action setup
    /// </summary>
    [Serializable]
    public class ActionConfig
    {
        [SerializeField] private string actionId = "audio-action";
        public float executionDelay = 0f;
        public string actionData = ""; // JSON or serialized data for action-specific settings
        
        /// <summary>
        /// The action ID used by the action discovery system.
        /// </summary>
        public string ActionId 
        { 
            get => actionId;
            set => actionId = value;
        }
    }
    
    /// <summary>
    /// Supported trigger types
    /// </summary>
    public enum TriggerType
    {
        Collision,
        Proximity,
        Timer,
        Custom
    }
    
    /// <summary>
    /// Supported condition types
    /// </summary>
    public enum ConditionType
    {
        TagCondition,
        LayerCondition,
        DistanceCondition,
        Custom
    }
    
    
    /// <summary>
    /// Supported collider types for collision triggers
    /// </summary>
    public enum ColliderType
    {
        BoxCollider,
        SphereCollider,
        CapsuleCollider,
        MeshCollider
    }
    
    /// <summary>
    /// Configuration for event channels
    /// </summary>
    [Serializable]
    public class EventChannelConfig
    {
        public string eventName = "NewEvent";
        public string description = "";
        public bool createNewEvent = true;
        public string existingEventPath = ""; // Path to existing GameEvent asset (for templates)
        
        // Direct ScriptableObject reference for Unity's ObjectField
        public GameFramework.Events.Channels.GameEvent gameEventAsset;
    }
    
    /// <summary>
    /// Event subscription modes for response objects
    /// </summary>
    public enum EventSubscriptionMode
    {
        WizardEvents,  // Use events configured in the current wizard
        Manual         // Manual entry via GameEvent assets or text input
    }
    
    /// <summary>
    /// Configuration for response objects
    /// </summary>
    [Serializable]
    public class ResponseObjectConfig
    {
        public string objectName = "ResponseObject";
        public string description = "";
        public bool createNewObject = true;
        public string targetObjectId = ""; // For existing objects (legacy)
        
        // Direct GameObject reference for Unity's ObjectField
        public GameObject targetGameObject;
        
        public List<string> listenToEvents = new List<string>(); // Event names this object responds to
        public List<GameFramework.Events.Channels.GameEvent> gameEventAssets = new List<GameFramework.Events.Channels.GameEvent>(); // Direct GameEvent ScriptableObject references
        public List<ActionConfig> actions = new List<ActionConfig>();
        
        [Header("Event Subscription Settings")]
        public EventSubscriptionMode eventSubscriptionMode = EventSubscriptionMode.WizardEvents; // Mode for event subscription
        
        [Header("Hierarchy Settings")]
        public bool isParentObject = false; // Whether this is a parent container
        public bool isChildObject = false; // Whether this is a child of another response object
        public string parentObjectName = ""; // Name of the parent object (for child objects)
        public List<ResponseObjectConfig> childObjects = new List<ResponseObjectConfig>(); // Child response objects
    }
}