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
        
        // Properties
        public string TemplateName => templateName;
        public string Description => description;
        public Texture2D TemplateIcon => templateIcon;
        public string Category => category;
        public int Difficulty => difficulty;
        public TriggerConfig TriggerSettings => triggerSettings;
        public List<EventChannelConfig> EventChannels => eventChannels;
        public List<ConditionConfig> Conditions => conditions;
        public bool RequireAllConditions => requireAllConditions;
        public List<ResponseObjectConfig> ResponseObjects => responseObjects;
        public bool CanRepeat => canRepeat;
        public float CooldownTime => cooldownTime;
        public bool DebugMode => debugMode;
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
        public List<ActionConfig> actions = new List<ActionConfig>();
    }
}