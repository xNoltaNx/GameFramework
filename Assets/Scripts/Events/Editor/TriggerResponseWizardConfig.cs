#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Events.Templates;

namespace GameFramework.Events.Editor
{
    /// <summary>
    /// Configuration class for the TriggerResponseSetupWizard.
    /// Contains all the wizard state and settings.
    /// </summary>
    [Serializable]
    public class TriggerResponseWizardConfig
    {
        [Header("Project Setup")]
        public string projectName = "NewInteractionProject";
        public CreationMode creationMode = CreationMode.SceneObjects;
        public bool useExistingProjectFolder = false;
        public string selectedProjectFolderPath = "";
        
        [Header("Template Configuration")]
        public bool useTemplate = true;
        public TriggerResponseTemplate selectedTemplate;
        
        [Header("Trigger Configuration")]
        public GameObject triggerObject;
        public bool createNewTriggerObject = false;
        public string newTriggerObjectName = "New Trigger";
        public TriggerConfig triggerConfig = new TriggerConfig();
        
        [Header("Event Channels")]
        public List<EventChannelConfig> eventChannelConfigs = new List<EventChannelConfig>();
        
        [Header("Conditions")]
        public List<ConditionConfig> conditionConfigs = new List<ConditionConfig>();
        public bool requireAllConditions = true;
        
        [Header("Response Objects")]
        public List<ResponseObjectConfig> responseObjectConfigs = new List<ResponseObjectConfig>();
        
        [Header("Advanced Settings")]
        public bool canRepeat = true;
        public float cooldownTime = 0f;
        public bool debugMode = false;
        
        [Header("UI State")]
        public string searchFilter = "";
        public string selectedCategory = "All";
        public bool showAdvancedSettings = false;
    }
    
    /// <summary>
    /// Creation mode for wizard objects.
    /// </summary>
    public enum CreationMode
    {
        SceneObjects,
        Prefabs
    }
}
#endif